#if !STRIDE_GRAPHICS_API_VULKAN
#error The Vulkan test project must resolve the Vulkan API.
#endif

#if !STRIDE_PLATFORM_LINUX
#error The Vulkan test project must resolve the Linux platform.
#endif

using Stride.Graphics;
using Stride.Platform;

namespace Stride.Tests.Graphics.Linux.Vulkan;

/// <summary>
/// Graphics contract test on Linux/Vulkan. Same abstractions as the Direct3D11
/// project, different build: the matrix, not a runtime switch, covers both APIs.
/// </summary>
[Trait("Category", "GraphicsContract")]
public sealed class VulkanContractTests
{
    [Fact]
    public void Device_reports_the_vulkan_variant()
    {
        using var device = new GraphicsDevice();

        // The variant ships native assets for both OSes; the runtime picks the
        // matching one, while the API axis is fixed by the build.
        var expectedNative = OperatingSystem.IsWindows() ? "stride_vulkan.dll" : "libstride_vulkan.so";

        Assert.Equal(GraphicsApi.Vulkan, device.Api);
        Assert.Equal(expectedNative, device.NativeLibrary);
        Assert.Contains("Vulkan", device.BackendName);
    }

    [Fact]
    public void Device_accepts_an_x11_surface()
    {
        using var device = new GraphicsDevice();
        var surface = new SurfaceDescriptor(SurfaceKind.X11Window, 0x5678, 800, 600, "test");

        device.Initialize(surface);

        Assert.Equal(surface, device.Surface);
    }

    [Fact]
    public void Device_submits_commands_and_describes_itself()
    {
        using var device = new GraphicsDevice();

        device.Submit(5);

        Assert.Equal(5, device.SubmittedCommands);
        Assert.Equal(GraphicsApi.Vulkan, device.Description.Api);
        Assert.Equal(device.NativeLibrary, device.Description.NativeLibrary);
    }
}
