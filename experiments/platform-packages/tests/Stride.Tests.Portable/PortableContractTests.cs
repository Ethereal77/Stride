#if STRIDE_PLATFORM_WINDOWS || STRIDE_PLATFORM_LINUX || STRIDE_PLATFORM_MACOS || STRIDE_PLATFORM_ANDROID || STRIDE_PLATFORM_IOS || STRIDE_PLATFORM_PORTABLE
#error A portable test project must not define platform constants.
#endif

#if STRIDE_GRAPHICS_API_DIRECT3D11 || STRIDE_GRAPHICS_API_VULKAN
#error A portable test project must not define graphics API constants.
#endif

using Stride.Core.IO;
using Stride.Graphics;
using Stride.Platform;

namespace Stride.Tests.Portable;

/// <summary>
/// Tests that only use the platform-neutral contracts. They must compile and
/// run against any OS: no meta-package, no host fallback, no native asset.
/// </summary>
public sealed class PortableContractTests
{
    [Fact]
    public void PlatformInfo_exposes_the_portable_kind()
    {
        Assert.Equal(PlatformKind.Portable, PlatformInfo.Portable.Kind);
        Assert.True(PlatformInfo.Portable.IsPortable);
        Assert.Equal("Portable", PlatformInfo.Portable.Name);
    }

    [Fact]
    public void Headless_surface_is_empty_and_named()
    {
        var surface = SurfaceDescriptor.Headless;

        Assert.Equal(SurfaceKind.Headless, surface.Kind);
        Assert.Equal(0, surface.Handle);
        Assert.Equal("headless", surface.DebugName);
    }

    [Fact]
    public void Graphics_device_description_round_trips()
    {
        var description = new GraphicsDeviceDescription(GraphicsApi.Vulkan, "Vulkan (portable probe)", "none");

        Assert.Equal(GraphicsApi.Vulkan, description.Api);
        Assert.Equal("Vulkan (portable probe)", description.BackendName);
        Assert.Equal("none", description.NativeLibrary);
    }

    [Fact]
    public void Storage_info_describes_a_root_without_touching_the_disk()
    {
        var info = new StorageInfo("/work", '/', IsCaseSensitive: true);

        Assert.Equal("/work", info.Root);
        Assert.Equal('/', info.Separator);
        Assert.True(info.IsCaseSensitive);
    }

    [Fact]
    public void Platform_services_is_a_delegate_bundle()
    {
        var runs = 0;
        var surface = new SurfaceDescriptor(SurfaceKind.Headless, 0, 320, 200, "test");

        var services = new PlatformServices
        {
            Platform = PlatformInfo.Portable,
            WindowSystem = WindowSystemKind.None,
            CreateSurface = () => surface,
            Run = callback =>
            {
                runs++;
                callback();
            },
        };

        services.Run(() => { });

        Assert.Equal(1, runs);
        Assert.Equal(surface, services.CreateSurface());
        Assert.True(services.Platform.IsPortable);
    }
}
