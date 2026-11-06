#if !STRIDE_PLATFORM_LINUX
#error The Linux platform test project must resolve the Linux platform.
#endif

#if !STRIDE_WINDOWSYSTEM_X11
#error The Linux platform test project must resolve X11.
#endif

using Stride.Core.IO;
using Stride.Platform;

namespace Stride.Tests.Platform.Linux;

/// <summary>
/// Platform contract test: the same abstractions as the Windows project, but
/// resolved to the Linux implementation. It runs as a library on any test host;
/// the build, not the host OS, selects the implementation.
/// </summary>
[Trait("Category", "PlatformContract")]
public sealed class LinuxPlatformContractTests
{
    [Fact]
    public void Host_reports_linux_and_x11()
    {
        var host = new PlatformHost();

        Assert.Equal(PlatformKind.Linux, host.Platform.Kind);
        Assert.Equal(WindowSystemKind.X11, host.WindowSystem);
        Assert.Equal("libstride_platform_x11.so", host.NativeLibrary);
    }

    [Fact]
    public void Host_creates_an_x11_surface()
    {
        var surface = new PlatformHost().CreateSurface();

        Assert.Equal(SurfaceKind.X11Window, surface.Kind);
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

        Assert.Equal(PlatformKind.Linux, services.Platform.Kind);
        Assert.Equal(WindowSystemKind.X11, services.WindowSystem);
        Assert.Equal(SurfaceKind.X11Window, services.CreateSurface().Kind);
    }

    [Fact]
    public void File_system_uses_linux_path_rules()
    {
        var fileSystem = new FileSystem();

        Assert.Equal('/', fileSystem.Info.Separator);
        Assert.True(fileSystem.Info.IsCaseSensitive);
        Assert.Equal("libstride_io_linux.so", fileSystem.NativeLibrary);
        Assert.Equal("/var/lib/stride/assets", fileSystem.Combine(fileSystem.Info.Root, "assets"));
    }
}
