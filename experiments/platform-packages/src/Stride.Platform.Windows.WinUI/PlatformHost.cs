namespace Stride.Platform;

/// <summary>
/// Concrete platform host for WinUI. Same public surface as the Win32 host, but
/// a WinUI build binds to this one instead.
/// </summary>
public sealed class PlatformHost
{
    public PlatformInfo Platform => WindowsPlatform.Info;

    public WindowSystemKind WindowSystem => WindowSystemKind.WinUI;

    public int MainThreadId => Environment.CurrentManagedThreadId;

    public string NativeLibrary => "stride_platform_winui.dll";

    public SurfaceDescriptor CreateSurface() =>
        new(SurfaceKind.WinUISwapChainPanel, 0x20002, 1920, 1080, "WinUI SwapChainPanel");

    /// <summary>Simulated run loop: invokes the frame callback twice.</summary>
    public void Run(Action frame)
    {
        frame();
        frame();
    }

    /// <summary>Bundles this host's primitives for platform-neutral libraries.</summary>
    public PlatformServices CreateServices() => new()
    {
        Platform = Platform,
        WindowSystem = WindowSystem,
        CreateSurface = CreateSurface,
        Run = Run,
    };
}
