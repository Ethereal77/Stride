namespace Stride.Graphics;

/// <summary>
/// Graphics backend exposed by a <c>Stride.Graphics.*</c> variant package.
/// </summary>
public enum GraphicsApi
{
    /// <summary>No API requested: let the runtime pick the platform default.</summary>
    None = 0,

    /// <summary>Direct3D 11 (Windows only).</summary>
    Direct3D11,

    /// <summary>Direct3D 12 (Windows only).</summary>
    Direct3D12,

    /// <summary>Vulkan (Windows, Linux, macOS, Android, iOS).</summary>
    Vulkan,
}
