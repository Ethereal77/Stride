using Stride.Platform;

namespace EditorApp;

/// <summary>
/// Simulated WPF -> Win32 adapter. A real WPF editor would obtain the HWND from
/// HwndHost and expose it as a SurfaceDescriptor to the graphics device.
/// </summary>
internal sealed class WpfWin32SurfaceAdapter
{
    public SurfaceDescriptor CreateSurface() =>
        new(SurfaceKind.Win32Hwnd, 0xE001, 1280, 720, "WPF HwndHost");
}
