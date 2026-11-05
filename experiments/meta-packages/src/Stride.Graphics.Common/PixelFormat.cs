namespace Stride.Graphics;

/// <summary>
/// Format of a texture or buffer element. Shared by every graphics API variant.
/// </summary>
public enum PixelFormat
{
    /// <summary>Format not specified.</summary>
    Unknown = 0,

    /// <summary>8 bits per channel, unsigned normalized RGBA.</summary>
    R8G8B8A8_UNorm,

    /// <summary>8 bits per channel, unsigned normalized BGRA (the usual swap-chain format).</summary>
    B8G8R8A8_UNorm,

    /// <summary>16 bits per channel, half precision RGBA.</summary>
    R16G16B16A16_Float,

    /// <summary>32 bits per channel, single precision RGBA.</summary>
    R32G32B32A32_Float,

    /// <summary>Depth 24 bits, stencil 8 bits.</summary>
    D24_UNorm_S8_UInt,
}
