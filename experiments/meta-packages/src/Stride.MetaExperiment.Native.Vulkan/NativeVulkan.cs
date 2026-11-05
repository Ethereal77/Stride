namespace Stride.MetaExperiment.Native.Vulkan;

/// <summary>
/// Managed face of the simulated native Vulkan package.
/// </summary>
public static class NativeVulkan
{
    /// <summary>Package that owns the native assets.</summary>
    public const string PackageId = "Stride.MetaExperiment.Native.Vulkan";

    /// <summary>Native library shipped in <c>runtimes/win-x64/native</c>.</summary>
    public const string WindowsLibraryFileName = "stride_vulkan.dll";

    /// <summary>Native library shipped in <c>runtimes/linux-x64/native</c>.</summary>
    public const string LinuxLibraryFileName = "libstride_vulkan.so";

    /// <summary>Native library the current OS needs.</summary>
    public static string GetLibraryFileName() =>
        OperatingSystem.IsWindows() ? WindowsLibraryFileName : LinuxLibraryFileName;
}
