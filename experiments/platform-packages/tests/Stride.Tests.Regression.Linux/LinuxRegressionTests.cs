#if !STRIDE_PLATFORM_LINUX || !STRIDE_WINDOWSYSTEM_X11
#error The Linux regression project must resolve Linux/X11.
#endif

#if STRIDE_WINDOWSYSTEM_WIN32 || STRIDE_WINDOWSYSTEM_WINUI
#error Windows windowing must not be selected in the Linux regression build.
#endif

using Stride.Platform;

namespace Stride.Tests.Regression.Linux;

/// <summary>
/// Regression test for the concrete X11 implementation. It pins
/// <c>Stride.Platform.Linux.X11</c> directly, with the documented opt-out, and
/// verifies that the Windows implementations are pruned from the same build.
/// </summary>
[Trait("Category", "PlatformRegression")]
public sealed class LinuxRegressionTests
{
    [Fact]
    public void Direct_x11_pin_keeps_the_linux_assemblies()
    {
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Linux.dll")));
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Linux.X11.dll")));
    }

    [Fact]
    public void Direct_x11_pin_prunes_the_windows_implementations()
    {
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Windows.dll")));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Windows.Win32.dll")));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Windows.WinUI.dll")));
    }

    [Fact]
    public void Pinned_x11_host_regression_behaviour_is_stable()
    {
        var host = new PlatformHost();
        var surface = host.CreateSurface();

        Assert.Equal(PlatformKind.Linux, host.Platform.Kind);
        Assert.Equal(WindowSystemKind.X11, host.WindowSystem);
        Assert.Equal(SurfaceKind.X11Window, surface.Kind);
        Assert.Equal("libstride_platform_x11.so", host.NativeLibrary);
        Assert.Equal(0x30003, surface.Handle);
    }
}
