// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

// Dev-packages staleness check
//   Compiled at runtime by RoslynCodeTaskFactory (no prebuilt task assembly).
//   Referenced from Stride.DevPackages.targets via <Code Source>.

public class StrideCheckDevPackageInputs : Task
{
    [Required] public string ManifestPath { get; set; }
    [Required] public string StampPath { get; set; }
    [Required] public string StubsPath { get; set; }
    [Required] public ITaskItem[] FixedInputs { get; set; }
    [Required] public string OwnStub { get; set; }

    [Output] public ITaskItem[] Changed { get; set; }
    [Output] public bool OwnStubFresh { get; set; }

    public override bool Execute()
    {
        // The generator rewrites the stamp files under this mutex
        // (a full run, or another project's adopt running in parallel with this check),
        // so they are read under it too
        using var mutex = new Mutex(false, @"Global\StrideDevPackages");

        try
        {
            mutex.WaitOne();
        }
        catch (AbandonedMutexException) { /* The holder died; the files are whole lines, so proceed */ }

        try
        {
            Check();
        }
        finally
        {
            mutex.ReleaseMutex();
        }

        return true;
    }

    // Two questions about one project's stub, answered from the generator's files (see build/dev-packages.cs):
    //
    //  * OwnStubFresh:
    //      The stubs file lists, per stub, the fingerprint of the fixed inputs
    //      (the script, the central package versions, the solution) it was generated with;
    //      the project's stub is fresh when its line carries the fingerprint of those files as they are now.
    //      Every stub depends on the fixed inputs, so each project judges its own stub instead of
    //      a shared "everything is stale" that the first refresh would hide from the others.
    private void Check()
    {
        var changed = new List<ITaskItem>();
        var stampTime = File.GetLastWriteTimeUtc(StampPath);

        string[] manifestLines = File.Exists(ManifestPath) ? File.ReadAllLines(ManifestPath) : []

        // Changed:
        //   The inputs of the manifest ("<sha256>\t<path>" per line: csprojs and packed build/ assets)
        //   whose content changed since the stubs were generated.
        //   - A file older than the stamp is trusted without being read;
        //   - A newer one is confirmed by content hash, so modified time churn alone (touch, checkout, rebase)
        //     is not considered a change.
        //   - A missing file counts as changed. The targets keep the ones under the project's own directory.
        foreach (var rawLine in manifestLines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
                continue;

            var tab = line.IndexOf('\t');
            var expectedHash = tab >= 0 ? line.Substring(0, tab) : null;
            var path = tab >= 0 ? line.Substring(tab + 1) : line;

            if (!File.Exists(path))
            {
                // New file
                changed.Add(new TaskItem(path));
                continue;
            }

            if (File.GetLastWriteTimeUtc(path) <= stampTime)
                // Untouched since generation: trusted without reading
                continue;

            if (expectedHash is null ||
                !string.Equals(ContentHash(path), expectedHash, StringComparison.OrdinalIgnoreCase))
                // Content changed
                changed.Add(new TaskItem(path));
        }
        Changed = changed.ToArray();

        // OwnStubFresh:
        //   The stubs file lists, per stub, the fingerprint of the fixed inputs
        //   (the script, the central package versions, the solution) it was generated with;
        //   the project's stub is fresh when its line carries the fingerprint of those files as they are now.
        //   Every stub depends on the fixed inputs, so each project judges its own stub instead of
        //   a shared "everything is stale" that the first refresh would hide from the others.

        var fingerprint = Fingerprint(FixedInputs);
        OwnStubFresh = false;

        // Fresh = the stub file is in the feed (next to the stamp) and its recorded fingerprint is the current one
        if (File.Exists(Path.Combine(Path.GetDirectoryName(StampPath), OwnStub)))
        {
            string[] stubLines = File.Exists(StubsPath) ? File.ReadAllLines(StubsPath) : [];

            foreach (var rawLine in stubLines)
            {
                var tab = rawLine.IndexOf('\t');
                if (tab < 0)
                    continue;

                if (string.Equals(rawLine.Substring(0, tab), OwnStub, StringComparison.OrdinalIgnoreCase))
                    OwnStubFresh = string.Equals(rawLine.Substring(tab + 1).Trim(), fingerprint, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    //
    // Computes the content hashes of the fixed inputs, joined by '\n', hashed again.
    // Same as the generator's Fingerprint.
    //
    private static string Fingerprint(ITaskItem[] files)
    {
        var hashes = new List<string>();

        foreach (var file in files)
        {
            var contentHash = File.Exists(file.ItemSpec) ? ContentHash(file.ItemSpec) : "missing";
            hashes.Add(contentHash);
        }

        return HexSha256(System.Text.Encoding.UTF8.GetBytes(string.Join("\n", hashes)));
    }

    //
    // Computes the hash of the contents of a file.
    // Same normalization as the generator's ContentHash: CRLF folded to LF, SHA-256, lowercase hex.
    //
    private static string ContentHash(string path)
    {
        var bytes = File.ReadAllBytes(path);

        var normalized = new byte[bytes.Length];
        var length = 0;

        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == (byte)'\r' && i + 1 < bytes.Length && bytes[i + 1] == (byte)'\n')
                continue;

            normalized[length++] = bytes[i];
        }
        var trimmed = new byte[length];
        Array.Copy(normalized, trimmed, length);

        return HexSha256(trimmed);
    }

    //
    // Computes the SHA-256 digest of a byte span and returns it as a lowercase hexadecimal string.
    //
    private static string HexSha256(byte[] data)
    {
        using var sha = SHA256.Create();

        var hash = sha.ComputeHash(data);

        var hex = new char[hash.Length * 2];
        for (var i = 0; i < hash.Length; i++)
        {
            hex[i * 2] = "0123456789abcdef"[hash[i] >> 4];
            hex[i * 2 + 1] = "0123456789abcdef"[hash[i] & 0xF];
        }
        return new string(hex);
    }
}
