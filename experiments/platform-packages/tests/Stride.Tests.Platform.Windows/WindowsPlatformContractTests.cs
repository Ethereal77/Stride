#if !STRIDE_PLATFORM_WINDOWS
#error The Windows platform test project must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WIN32
#error The Windows platform test project must resolve Win32.
#endif

using Stride.Core.IO;
using Stride.Platform;

namespace Stride.Tests.Platform.Windows;

/// <summary>
/// Platform contract test: a normal consumer of the meta-packages. It asserts
/// the Windows implementation behind the abstractions, not Windows internals.
/// </summary>
[Trait("Category", "PlatformContract")]
public sealed class WindowsPlatformContractTests
{
    [Fact]
    public void Host_reports_windows_and_win32()
    {
        var host = new PlatformHost();

        Assert.Equal(PlatformKind.Windows, host.Platform.Kind);
        Assert.Equal(WindowSystemKind.Win32, host.WindowSystem);
        Assert.NotEqual(0, host.MainThreadId);
    }

    [Fact]
    public void Host_creates_a_win32_surface()
    {
        var surface = new PlatformHost().CreateSurface();

        Assert.Equal(SurfaceKind.Win32Hwnd, surface.Kind);
        Assert.NotEqual(0, surface.Handle);
        Assert.Equal(1280u, surface.Width);
        Assert.Equal(720u, surface.Height);
    }

    [Fact]
    public void Host_runs_the_frame_callback_once_per_frame()
    {
        var frames = 0;

        new PlatformHost().Run(() => frames++);

        Assert.Equal(2, frames);
    }

    [Fact]
    public void Services_bundle_matches_the_host()
    {
        var host = new PlatformHost();
        var services = host.CreateServices();

        Assert.Equal(PlatformKind.Windows, services.Platform.Kind);
        Assert.Equal(WindowSystemKind.Win32, services.WindowSystem);
        Assert.Equal(SurfaceKind.Win32Hwnd, services.CreateSurface().Kind);
    }

    [Fact]
    public void File_system_uses_windows_path_rules()
    {
        var fileSystem = new FileSystem();

        Assert.Equal('\\', fileSystem.Info.Separator);
        Assert.False(fileSystem.Info.IsCaseSensitive);
        Assert.Equal("stride_io_windows.dll", fileSystem.NativeLibrary);
        Assert.Equal(@"C:\ProgramData\Stride\assets", fileSystem.Combine(fileSystem.Info.Root, "assets"));
    }
}
