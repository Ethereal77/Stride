// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

// Build version generators
//   Compiled at runtime by RoslynCodeTaskFactory (no prebuilt task assembly).
//   Referenced from both Stride.WorktreeVersion.targets and Stride.GitVersion.targets via <Code Source>.

// The version is the committed value in SharedAssemblyInfo.cs (MajorMinor.Patch + NuGetVersionSuffix)
// It is bumped per release rather than derived from git tags.
//
// Both generators overlay that value into a generated copy; they differ only in their wrapper concerns:
//
// * ResolveStrideWorktreeVersion (dev and CI builds):
//     "MajorMinor.Patch" without the committed suffix (so the betas of one version share one dev version),
//     per-checkout ledger -> -devN, cached, no metadata.
//
// * StrideGitVersion (release/package builds):
//     committed suffix + "+g<sha>" build metadata, no ledger.

// Shared version logic for both the dev and release generators.
internal static class StrideVersionUtil
{
    //
    // Reads the editable inputs from SharedAssemblyInfo.cs.
    // The version is composed MajorMinor.Patch
    // (Patch is the only patch source; MajorMinor the only major.minor source — no duplication to drift).
    //
    public static void ReadInputs(string sourceFile, out string majorMinor, out string version, out string suffix)
    {
        string data = File.ReadAllText(sourceFile);

        majorMinor = GetFieldValue(data, "MajorMinor");
        string patch = GetFieldValue(data, "Patch");
        suffix = GetFieldValue(data, "NuGetVersionSuffix");

        // Fail loudly on an unparseable version rather than emit a malformed one
        // (e.g. "." or "4.4.") that would ship a weird package or compile a garbage const.
        // GetFieldValue() returns "" when a line's shape changed.
        if (!Regex.IsMatch(majorMinor, @"^[0-9]+\.[0-9]+$") ||
            !Regex.IsMatch(patch, @"^[0-9]+$"))
            throw new Exception($"Could not parse a valid version from {sourceFile} (MajorMinor='{majorMinor}', Patch='{patch}'). " +
                                "Check the 'MajorMinor = \"x.y\";' and 'Patch = \"n\";' lines.");

            version = majorMinor + "." + patch;
    }

    //
    // Parses the value of a field initialization.
    // It expects the exact syntax, including spaces: FieldName = "Value";
    //
    private static string GetFieldValue(string data, string name)
        => Regex.Match(data, name + " = \"([^\"]*)\";").Groups[1].Value;

    //
    // Overlays the version + suffix (+ optional build metadata) into a copy of the source version file.
    // Rewrites:
    //  - Patch (read back by the package producers)
    //  - PublicVersion (the sentinel in the template -> the real version, read by the compiled consts).
    //
    public static string Overlay(string source, string majorMinor, string version, string suffix, string buildMetadata)
    {
        string patch = version.StartsWith(majorMinor + ".", StringComparison.Ordinal)
            ? version.Substring(majorMinor.Length + 1)
            : version;

        string patched = Regex.Replace(source, "Patch = \"[^\"]*\";", "Patch = \"" + patch + "\";");
        patched = Regex.Replace(patched, "const string PublicVersion = [^;]*;", "const string PublicVersion = \"" + version + "\";");
        patched = Regex.Replace(patched, "NuGetVersionSuffix = \"[^\"]*\";", "NuGetVersionSuffix = \"" + suffix + "\";");

        if (buildMetadata != null)
            patched = Regex.Replace(patched, "BuildMetadata = \"[^\"]*\";", "BuildMetadata = \"" + buildMetadata + "\";");

        return patched;
    }
}

// The per-project version generator:
//   Resolves this checkout's suffix and overlays the version into SharedAssemblyInfo.Generated.cs
//   (the single file the Stride SDK swaps in).
//   - On dev builds the suffix comes from a per-machine ledger (-devN) so multiple checkouts on one machine
//     stop clobbering each other in the shared NuGetDev feed / global cache.
//   - On CI (and when StrideSkipWorktreeVersion is set) NoLedgerSuffix gives the clean, unsuffixed version
//     with no ledger touch.
//   A stamp caches the result so the per-project invocations don't each rewrite the file.
public class ResolveStrideWorktreeVersion : Task
{
    [Required] public string StrideRoot { get; set; }

    // Explicit override (StrideWorktreeId property/env var); wins over the ledger when set.
    public string OverrideId { get; set; }

    // CI / StrideSkipWorktreeVersion: emit the overlay with an empty suffix and no ledger registration (but keep the
    // fast path, unlike an explicit OverrideId).
    public bool NoLedgerSuffix { get; set; }

    // Ledger location override (StrideWorktreeLedger property); defaults to LocalApplicationData/stride/worktree-ids.txt.
    public string LedgerPath { get; set; }

    [Required] public string SourceVersionFile { get; set; }
    [Required] public string GeneratedVersionFile { get; set; }

    [Output] public string WorktreeSuffix { get; set; }
    [Output] public bool Generated { get; set; }

    public override bool Execute()
    {
        WorktreeSuffix = string.Empty;

        try
        {
            string rootPath = NormalizePath(StrideRoot);
            string ledgerPath = !string.IsNullOrWhiteSpace(LedgerPath)
                ? LedgerPath
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Stride", "worktree-ids.txt");

            // Stamp = ledger modified time + source file hash
            // A manual worktree-ID remap or any edit to SharedAssemblyInfo.cs invalidates the generated overlay.
            string stamp = GetLedgerStamp(ledgerPath) + "|" + ComputeSourceVersionFileHash();

            if (string.IsNullOrWhiteSpace(OverrideId) && TryFastPath(stamp))
                return true;

            Directory.CreateDirectory(Path.GetDirectoryName(ledgerPath));

            for (int attempt = 0; attempt < 300; attempt++)
            {
                try
                {
                    // FileShare.None on a sibling lock file serializes the first-build race
                    // across MSBuild's parallel project nodes (cross-process, cross-platform).
                    using (new FileStream(ledgerPath + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                    {
                        string token = !string.IsNullOrWhiteSpace(OverrideId)
                            ? OverrideId.Trim()
                            : NoLedgerSuffix
                                ? "(empty)"
                                : ResolveOrRegisterWorktreeToken(ledgerPath, rootPath);

                        WorktreeSuffix = GetSuffixFromWorktreeToken(token);
                        WriteGeneratedFile(WorktreeSuffix, stamp);
                    }
                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(5);
                }
            }
            throw new IOException("Timed out acquiring the Stride worktree ledger lock at " + ledgerPath);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"Stride worktree version resolution skipped: {ex.Message}");

            WorktreeSuffix = string.Empty;
            return true;
        }
    }

    //
    // Resolves or registers a worktree token for the given ledger path and root directory.
    //
    private string ResolveOrRegisterWorktreeToken(string ledgerPath, string root)
    {
        List<string> lines = File.Exists(ledgerPath)
            ? [.. File.ReadAllLines(ledgerPath)]
            : [];

        // Steady state: Already registered —> Return the existing token, no pruning.
        foreach (string line in lines)
        {
            if (TryParseLine(line, out string token, out string dir) &&
                NormalizePath(dir).Equals(root, PathComparison))
                return token;
        }

        // Not registered:
        //   - Drop entries whose directory is gone
        //   - Allocate the lowest free slot (1 -> "dev", 2 -> "dev2", ...),
        //     i.e., deleting dev2 while dev3 is taken frees dev2 for the next checkout,
        //     instead of climbing to dev4 indefinitely.

        var keptLines = new List<string>(lines.Count);
        var liveDevNumbers = new HashSet<int>();

        foreach (string line in lines)
        {
            if (!TryParseLine(line, out string token, out string dir))
            {
                keptLines.Add(line);
                continue;
            }
            dir = NormalizePath(dir);

            // UNC entries (\\server\share\...) are kept regardless — a transient network failure
            // makes Directory.Exists() return false, which would silently delete a valid entry.
            bool isUnc = dir.StartsWith("\\\\", StringComparison.Ordinal)
                      || dir.StartsWith("//", StringComparison.Ordinal);

            if (!isUnc && !Directory.Exists(dir))
            {
                Log.LogMessage(MessageImportance.High, $"Pruning stale Stride worktree ledger entry: '{token}' -> '{dir}'");
                continue;
            }
            keptLines.Add(line);

            // Slot occupancy:
            // - "dev" (and the legacy "(primary)") = slot 1
            // - "devN" = slot N
            // - "(empty)" holds no slot — it's the explicit no-suffix opt-out.
            if (token.Equals("dev", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("(primary)", StringComparison.OrdinalIgnoreCase))
                liveDevNumbers.Add(1);
            else
            {
                // Match "devN" where N is a number, and add N to the set of live slots
                Match m = Regex.Match(token, "^dev([0-9]+)$");
                if (m.Success)
                    liveDevNumbers.Add(int.Parse(m.Groups[1].Value));
            }
        }

        // Lowest free slot: 1 -> "dev", 2 -> "dev2", ... So the first checkout is "-dev"
        int n = 1;
        while (liveDevNumbers.Contains(n)) n++;
        string newToken = n == 1 ? "dev" : $"dev{n}";

        // Ensure the ledger has a header if it's empty
        if (keptLines.Count == 0)
            keptLines.Add("# Stride checkout version-suffix ledger. Tokens: dev (-dev), dev2, dev3, ...; '(empty)' = no suffix (clean build).");

        // Add the new token to the ledger
        keptLines.Add(newToken + " = " + root);

        // Write the updated ledger back to disk
        File.WriteAllLines(ledgerPath, keptLines);
        Log.LogMessage(MessageImportance.High, $"Registered this checkout in the Stride worktree ledger as '{newToken}' ({ledgerPath})");

        return newToken;
    }

    //
    // Writes the generated version file with the given suffix and stamp.
    //
    private void WriteGeneratedFile(string suffix, string stamp)
    {
        StrideVersionUtil.ReadInputs(SourceVersionFile, out string majorMinor, out string version, out _);

        // The committed suffix (-beta1) is for release builds only:
        //   Here the suffix is the worktree one alone, as on the producer side (Stride.NuGetVersion.props),
        //   so the const matches the package.
        //   Always emit the overlay (even when it equals the committed version): the base file's PublicVersion
        //   is a sentinel, so leaving the base to compile would ship it.
        string source = File.ReadAllText(SourceVersionFile);
        string patched = StrideVersionUtil.Overlay(source, majorMinor, version, suffix, buildMetadata: null);

        // Cache key = stamp + resolved worktree suffix.
        // TryFastPath reuses the file while the stamp matches.
        patched += "\n// version-stamp: " + stamp + "|" + suffix + "\n";

        if (!File.Exists(GeneratedVersionFile) ||
            File.ReadAllText(GeneratedVersionFile) != patched)
        {
            File.WriteAllText(GeneratedVersionFile, patched);
        }

        Generated = true;
    }

    //
    // Computes the hash of the whole source file.
    // The hash is part of the cache stamp so any edit to it (version, suffix) invalidates the generated overlay.
    // Returns "" if unreadable.
    //
    private string ComputeSourceVersionFileHash()
    {
        try
        {
            var bytes = File.ReadAllBytes(SourceVersionFile);

            // FNV-1a: Intended to be a change detector, not a signature.
            // The task is compiled by RoslynCodeTaskFactory: only use what the MSBuild host ships (no System.IO.Hashing).
            ulong hash = 14695981039346656037;

            foreach (var b in bytes)
                hash = (hash ^ b) * 1099511628211;

            return hash.ToString("x16");
        }
        catch { return string.Empty; }
    }

    //
    // Tries the fast path:
    // The stamp is the combination of the ledger modification time and the source file hash.
    // When it is unchanged, the generated file can be reused.
    //
    private bool TryFastPath(string stamp)
    {
        if (!File.Exists(GeneratedVersionFile))
            // Can't reuse a file that does not exist
            return false;

        var generatedContent = File.ReadAllText(GeneratedVersionFile);

        var m = Regex.Match(generatedContent, "// version-stamp: (.*)");
        if (!m.Success)
            // Can't reuse a file that does not have a version-stamp marker
            return false;

        string key = m.Groups[1].Value;

        if (!key.StartsWith(stamp + "|", StringComparison.Ordinal))
            // The file marker is "stamp|suffix", so it must start with stamp + "|"
            return false;

        WorktreeSuffix = key.Substring(stamp.Length + 1).Trim();
        Generated = true;
        return true;
    }

    //
    // Returns the last modification time of the ledger file as a string, or "0" if the file does not exist.
    // The modification time is used as part of the stamp to detect changes in the ledger file.
    // A manual edit to the ledger file will change its modification time, invalidating the stamp,
    // re-triggering resolution instead of reusing a stale overlay.
    //
    private static string GetLedgerStamp(string ledgerPath)
    {
        return File.Exists(ledgerPath) ? File.GetLastWriteTimeUtc(ledgerPath).Ticks.ToString() : "0";
    }

    // Windows file paths are case-insensitive.
    // Ordinal correctly distinguishes truly-distinct case-sensitive checkouts
    // (where OrdinalIgnoreCase would falsely merge `/work/stride` and `/work/Stride` and silently clobber both).
    private static readonly StringComparison PathComparison =
        Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    //
    // Normalizes a file path to its full absolute form, trimming any trailing directory separators.
    //
    private static string NormalizePath(string path)
        => Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    //
    // Converts a worktree token into a suffix for the overlay directory.
    // Returns an empty string for "(empty)" and normalizes legacy aliases like "(primary)".
    //
    private static string GetSuffixFromWorktreeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            // No token specified, treat as empty
            return string.Empty;

        token = token.Trim();

        // Explicit opt-out: A clean build with no suffix (e.g., to reproduce a release)
        if (token.Equals("(empty)", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        // Legacy alias: Ledgers written before "dev" became the default first-checkout token
        if (token.Equals("(primary)", StringComparison.OrdinalIgnoreCase))
            return "-dev";

        // Default case: Prepend a dash if not already present
        return token[0] == '-' ? token : "-" + token;
    }

    //
    // Tries to parse a line from the ledger file into a token and directory.
    // Returns true if the line is valid, false otherwise.
    //
    private static bool TryParseLine(string line, out string token, out string dir)
    {
        token = null;
        dir = null;

        // Discard empty or whitespace-only lines
        if (string.IsNullOrWhiteSpace(line))
            return false;

        line = line.Trim();

        // Discard comment lines
        if (line[0] == '#')
            return false;
        // And without an '=' character
        int eq = line.IndexOf('=');
        if (eq <= 0)
            return false;

        token = line.Substring(0, eq).Trim();
        dir = line.Substring(eq + 1).Trim();

        return token.Length != 0 && dir.Length != 0;
    }
}

// Release/package builds:
//   Overlays the committed version + release suffix + "+g<sha>" build metadata
//   into "SharedAssemblyInfo.Generated.cs".
//   No ledger; the only git use is reading HEAD's short sha for the build metadata.
public class StrideGitVersion : Task
{
    [Required] public string RootDirectory { get; set; }
    [Required] public string VersionFile { get; set; }
    [Required] public string GeneratedVersionFile { get; set; }

    // A nightly build replaces the committed suffix with -nightly.YYYYMMDD.N (MajorMinor.Patch unchanged)
    public string NightlyLabel { get; set; }

    [Output] public string NuGetVersion { get; set; }

    public override bool Execute()
    {
        try
        {
            // Normalize paths to /
            VersionFile = VersionFile.Replace('\\', '/');
            string sourcePath = Path.Combine(RootDirectory, VersionFile);
            GeneratedVersionFile = GeneratedVersionFile.Replace('\\', '/');
            string generatedPath = Path.Combine(RootDirectory, GeneratedVersionFile);

            StrideVersionUtil.ReadInputs(sourcePath, out string majorMinor, out string version, out string suffix);

            if (!string.IsNullOrEmpty(NightlyLabel))
            {
                if (!Regex.IsMatch(NightlyLabel, @"^nightly\.[0-9]{8}\.[0-9]+$"))
                    throw new Exception($"NightlyLabel '{NightlyLabel}' is not of the form nightly.YYYYMMDD.N");

                // If a nightly label is provided, use it directly
                suffix = "-" + NightlyLabel;
            }

            string sha = Git(RootDirectory, "rev-parse HEAD").Substring(0, 8);

            string sourceVersionInfo = File.ReadAllText(sourcePath);
            string patchedVersionInfo = StrideVersionUtil.Overlay(sourceVersionInfo, majorMinor, version, suffix, "+g" + sha);

            File.WriteAllText(generatedPath, patchedVersionInfo);

            NuGetVersion = version + suffix;
            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"StrideGitVersion failed: {ex.Message}");
            return false;
        }
    }

    //
    // Invokes `git` on a directory with the provided arguments string.
    //
    private static string Git(string workingDir, string args)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        using var gitProcess = Process.Start(psi);

        string output = gitProcess.StandardOutput.ReadToEnd().Trim();
        gitProcess.WaitForExit();

        if (gitProcess.ExitCode != 0)
            throw new Exception($"git {args} exited with {gitProcess.ExitCode}");

        return output;
    }
}
