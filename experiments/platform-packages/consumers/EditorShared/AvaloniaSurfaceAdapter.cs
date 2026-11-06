using Stride.Platform;

namespace EditorShared;

/// <summary>
/// Simulated Avalonia -> native windowing adapter. Avalonia is the UI toolkit,
/// not a <see cref="WindowSystemKind"/>: each head maps its native top level to
/// the surface kind of the OS it was built for (HWND, X11 window, ...).
/// </summary>
internal sealed class AvaloniaSurfaceAdapter
{
    public SurfaceDescriptor CreateSurface()
    {
#if STRIDE_PLATFORM_WINDOWS
        return new(SurfaceKind.Win32Hwnd, 0xA001, 1280, 720, "Avalonia Win32 HWND");
#elif STRIDE_PLATFORM_LINUX
        return new(SurfaceKind.X11Window, 0xA002, 1280, 720, "Avalonia X11 window");
#else
#error The Avalonia adapter needs a platform-specific head (Windows or Linux).
#endif
    }
}
