namespace Stride.Graphics;

/// <summary>
/// Graphics feature level requested by the application. Shared by every variant.
/// </summary>
public enum GraphicsProfile
{
    /// <summary>Direct3D 9.1 / OpenGL ES 2.0 class hardware.</summary>
    Level_9_1 = 0,

    /// <summary>Direct3D 10.0 class hardware.</summary>
    Level_10_0,

    /// <summary>Direct3D 11.0 class hardware.</summary>
    Level_11_0,

    /// <summary>Direct3D 11.1 class hardware.</summary>
    Level_11_1,
}
