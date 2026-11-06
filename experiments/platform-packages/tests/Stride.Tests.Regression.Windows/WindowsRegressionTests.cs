#if !STRIDE_PLATFORM_WINDOWS || !STRIDE_WINDOWSYSTEM_WIN32
#error The Windows regression project must resolve Windows/Win32.
#endif

#if STRIDE_WINDOWSYSTEM_WINUI
#error WinUI must not be selected in the Windows regression build.
#endif

using Stride.Platform;

namespace Stride.Tests.Regression.Windows;

/// <summary>
/// Regression test for a concrete platform implementation. The project pins
/// <c>Stride.Platform.Windows.Win32</c> directly on purpose, with
/// <c>StridePlatformAllowDirectReferences=true</c>, to prove that a pinned
/// implementation still resolves, runs, and coexists with pruning.
/// </summary>
[Trait("Category", "PlatformRegression")]
public sealed class WindowsRegressionTests
{
    [Fact]
    public void Direct_win32_pin_keeps_the_win32_assembly()
    {
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Windows.Win32.dll")));
    }

    [Fact]
    public void Direct_win32_pin_prunes_the_other_windowing_package()
    {
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Windows.WinUI.dll")));
        Assert.False(File.Exists(Path.Combine(AppContext.BaseDirectory, "Stride.Platform.Linux.X11.dll")));
    }

    [Fact]
    public void Pinned_win32_host_regression_behaviour_is_stable()
    {
        var host = new PlatformHost();
        var surface = host.CreateSurface();

        Assert.Equal(PlatformKind.Windows, host.Platform.Kind);
        Assert.Equal(WindowSystemKind.Win32, host.WindowSystem);
        Assert.Equal(SurfaceKind.Win32Hwnd, surface.Kind);
        Assert.Equal("stride_platform_win32.dll", host.NativeLibrary);
        Assert.Equal(0x10001, surface.Handle);
    }
}
