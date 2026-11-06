namespace Stride.Platform;

/// <summary>
/// Kind of native surface a windowing package can produce. Kept as data so the
/// graphics variants never need to reference a windowing package.
/// </summary>
public enum SurfaceKind
{
    Headless = 0,
    Win32Hwnd,
    WinUISwapChainPanel,
    X11Window,
    AndroidWindow,
    IOSView,
}
