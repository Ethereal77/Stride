using Stride.Graphics;

namespace ConsumerLibrary;

/// <summary>
/// Library consumer: it references the meta-package and exposes the API the build selected.
/// </summary>
public static class GraphicsLibrary
{
    /// <summary>API the library was compiled against.</summary>
    public static GraphicsApi CompileTimeApi
    {
        get
        {
#if STRIDE_GRAPHICS_API_DIRECT3D11
            return GraphicsApi.Direct3D11;
#elif STRIDE_GRAPHICS_API_DIRECT3D12
            return GraphicsApi.Direct3D12;
#elif STRIDE_GRAPHICS_API_VULKAN
            return GraphicsApi.Vulkan;
#else
#error The Stride.Graphics meta-package did not define STRIDE_GRAPHICS_API_*.
#endif
        }
    }

    /// <summary>Creates a device through the meta-package and describes it.</summary>
    public static string CreateDeviceDescription()
    {
        using var device = new GraphicsDevice();
        device.Initialize();
        return $"{device.Api} / {device.BackendName} / {device.Description.Profile}";
    }

    /// <summary>Profiling package available to the library for the selected family.</summary>
    public static string ProfilingBackend
    {
        get
        {
#if STRIDE_GRAPHICS_FAMILY_DIRECT3D
            return $"PIX:{Stride.MetaExperiment.Pix.PixMarkers.NativeLibrary}";
#elif STRIDE_GRAPHICS_FAMILY_VULKAN
            return $"Lavapipe:{Stride.MetaExperiment.Lavapipe.LavapipeDevice.GetLibraryFileName()}";
#else
#error The Stride.Graphics meta-package did not define a graphics family.
#endif
        }
    }
}
