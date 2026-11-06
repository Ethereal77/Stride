using Stride.Platform;

namespace MultiTargetProbe;

/// <summary>
/// Same source, two TFMs. net10.0 stays portable (only Common is visible);
/// net10.0-windows resolves Windows/Win32 and sees PlatformHost.
/// </summary>
public static class Probe
{
    public static string Describe()
    {
#if STRIDE_PLATFORM_WINDOWS
#if !STRIDE_WINDOWSYSTEM_WIN32
#error net10.0-windows should resolve Win32 by default.
#endif
        var host = new PlatformHost();
        return $"{host.Platform.Name}/{host.WindowSystem}/{host.CreateSurface().Kind}";
#else
#if !STRIDE_PLATFORM_PORTABLE
#error net10.0 should stay portable.
#endif
        return $"{PlatformInfo.Portable.Name}/none/{SurfaceKind.Headless}";
#endif
    }
}
