using Stride.Platform;

namespace DirectPinLib;

/// <summary>
/// Compiles against a direct, compatible WinUI package reference. The build
/// succeeds but emits STRIDE0001; the meta-package is still the recommended path.
/// </summary>
public static class Pin
{
    public static string Describe()
    {
#if !STRIDE_PLATFORM_WINDOWS
#error DirectPinLib must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WINUI
#error DirectPinLib must resolve the WinUI windowing system.
#endif

        var host = new PlatformHost();
        return $"{host.Platform.Name}/{host.WindowSystem}/{host.CreateSurface().Kind}";
    }
}
