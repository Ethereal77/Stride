namespace Stride.Platform;

/// <summary>
/// Concrete platform host for X11. This is what Stride.AppModel binds to on the
/// net10.0 (Linux) build.
/// </summary>
public sealed class PlatformHost
{
    public PlatformInfo Platform => LinuxPlatform.Info;

    public WindowSystemKind WindowSystem => WindowSystemKind.X11;

    public int MainThreadId => Environment.CurrentManagedThreadId;

    public string NativeLibrary => "libstride_platform_x11.so";

    public SurfaceDescriptor CreateSurface() =>
        new(SurfaceKind.X11Window, 0x30003, 1280, 720, "X11 window");

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
