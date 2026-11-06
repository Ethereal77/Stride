#if !STRIDE_GRAPHICS_API_DIRECT3D11
#error The Direct3D11 test project must resolve the Direct3D11 API.
#endif

#if !STRIDE_PLATFORM_WINDOWS
#error The Direct3D11 test project must resolve the Windows platform.
#endif

using Stride.Graphics;
using Stride.Platform;

namespace Stride.Tests.Graphics.Windows.D3D11;

/// <summary>
/// Graphics contract test on Windows/Direct3D11. One API per test binary: the
/// meta-package prunes Vulkan, so the test cannot accidentally see both.
/// </summary>
[Trait("Category", "GraphicsContract")]
public sealed class Direct3D11ContractTests
{
    [Fact]
    public void Device_reports_the_direct3d11_variant()
    {
        using var device = new GraphicsDevice();

        Assert.Equal(GraphicsApi.Direct3D11, device.Api);
        Assert.Equal("stride_d3d11.dll", device.NativeLibrary);
        Assert.Contains("Direct3D11", device.BackendName);
    }

    [Fact]
    public void Device_accepts_a_win32_surface()
    {
        using var device = new GraphicsDevice();
        var surface = new SurfaceDescriptor(SurfaceKind.Win32Hwnd, 0x1234, 640, 480, "test");

        device.Initialize(surface);

        Assert.Equal(surface, device.Surface);
    }

    [Fact]
    public void Device_submits_commands_and_describes_itself()
    {
        using var device = new GraphicsDevice();

        device.Submit(3);
        device.Submit(4);

        Assert.Equal(7, device.SubmittedCommands);
        Assert.Equal(GraphicsApi.Direct3D11, device.Description.Api);
        Assert.Equal("stride_d3d11.dll", device.Description.NativeLibrary);
    }
}
