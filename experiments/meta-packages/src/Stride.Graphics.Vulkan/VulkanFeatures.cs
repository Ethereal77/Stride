namespace Stride.Graphics.Vulkan;

/// <summary>
/// Vulkan-only capabilities. This type exists in no other variant, which is the
/// point of splitting the API-specific code out of Stride.Graphics.
/// </summary>
public static class VulkanFeatures
{
    /// <summary>Shader binary format used by Vulkan.</summary>
    public const string ShaderBinaryFormat = "SPIR-V";

    /// <summary>Whether the backend can expose ray tracing extensions.</summary>
    public const bool SupportsRayTracing = true;

    /// <summary>Name of the instance extension used by the simulated swap chain.</summary>
    public const string SwapChainExtension = "VK_KHR_swapchain";
}
