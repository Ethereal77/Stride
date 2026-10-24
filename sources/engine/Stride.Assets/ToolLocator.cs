using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Stride.Core.IO;

namespace Stride.Assets;

static class ToolLocator
{
    /// <summary>
    /// Finds an executable tool by name. Pass <paramref name="ensureExecutable"/> = true for native
    /// CLI binaries shipped via nupkg.
    /// </summary>
    public static UFile LocateTool(string toolName, bool ensureExecutable = false)
    {
        // Bin-dir lookups use the platform-correct extension
        var exeExt = ".exe";

        // On non-Windows, prefer system PATH (apt/brew). Falls through to the bin-dir
        // lookup below, which a future Stride.Dependencies.FFmpeg-style NuGet can satisfy.
        if (!OperatingSystem.IsWindows())
        {
            var pathDirectories = Environment.GetEnvironmentVariable("PATH")?.Split(':') ?? Array.Empty<string>();

            foreach (var directory in pathDirectories)
            {
                var toolLocation = Path.Combine(directory, toolName);
                if (File.Exists(toolLocation))
                    return new UFile(toolLocation);
            }
        }

        var asmDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // RID-keyed bundled binary: runtimes/<rid>/native/<tool>{.exe}. Two layouts to cover:
        //  * Source-tree build / single-flat publish — runtimes/ sits alongside the assembly.
        //  * NuGet package cache — assembly is at <pkgRoot>/lib/<tfm>/<dll>, natives at
        //    <pkgRoot>/runtimes/<rid>/native/ (standard NuGet layout, two levels up).
        var rid = GetCurrentRid();
        if (rid != null)
        {
            var nativeRel = new UFile($"runtimes/{rid}/native/{toolName}{exeExt}");
            var ridTool = UPath.Combine(asmDir, nativeRel);
            if (File.Exists(ridTool))
                return ridTool;

            var pkgRoot = Path.GetDirectoryName(Path.GetDirectoryName(asmDir));
            if (pkgRoot != null)
            {
                var pkgRidTool = UPath.Combine(pkgRoot, nativeRel);
                if (File.Exists(pkgRidTool))
                    return pkgRidTool;
            }
        }

        // Legacy locations kept for backward compatibility with older deps/ layouts.
        var tool = UPath.Combine(asmDir, new UFile($"{toolName}{exeExt}"));
        if (File.Exists(tool))
            return tool;

        tool = UPath.Combine(asmDir, new UFile($"../../content/{toolName}{exeExt}"));
        if (File.Exists(tool))
            return tool;

        return null;
    }

    private static string GetCurrentRid()
    {
        string os;
        if (OperatingSystem.IsWindows()) os = "win";
        else return null;

        var arch = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "x86",
            _ => null
        };
        return arch == null ? null : $"{os}-{arch}";
    }
}
