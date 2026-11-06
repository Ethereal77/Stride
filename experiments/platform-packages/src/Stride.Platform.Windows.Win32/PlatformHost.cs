namespace Stride.Platform;

/// <summary>
/// Concrete platform host for Win32. This is the type Stride.AppModel binds to
/// on net10.0-windows; every windowing package exposes the same public surface.
/// </summary>
public sealed class PlatformHost
{
    public PlatformInfo Platform => WindowsPlatform.Info;

    public WindowSystemKind WindowSystem => WindowSystemKind.Win32;

    public int MainThreadId => Environment.CurrentManagedThreadId;

    public string NativeLibrary => "stride_platform_win32.dll";

    public SurfaceDescriptor CreateSurface() =>
        new(SurfaceKind.Win32Hwnd, 0x10001, 1280, 720, "Win32 HWND");

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
