namespace Stride.MetaExperiment.Lavapipe;

/// <summary>
/// Managed face of the simulated Lavapipe (llvmpipe) bindings. Lavapipe is a
/// software Vulkan implementation used to exercise graphics techniques without a
/// GPU and to catch regressions, so this package only makes sense for Vulkan.
/// </summary>
public static class LavapipeDevice
{
    /// <summary>Package that owns the native assets.</summary>
    public const string PackageId = "Stride.MetaExperiment.Lavapipe";

    /// <summary>Native library shipped in <c>runtimes/win-x64/native</c>.</summary>
    public const string WindowsLibrary = "vulkan_lvp.dll";

    /// <summary>Native library shipped in <c>runtimes/linux-x64/native</c>.</summary>
    public const string LinuxLibrary = "libvulkan_lvp.so";

    /// <summary>Native library the current OS needs.</summary>
    public static string GetLibraryFileName() =>
        OperatingSystem.IsWindows() ? WindowsLibrary : LinuxLibrary;
}
