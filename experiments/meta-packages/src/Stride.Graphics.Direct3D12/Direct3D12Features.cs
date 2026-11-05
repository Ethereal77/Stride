namespace Stride.Graphics.Direct3D12;

/// <summary>
/// Direct3D 12-only capabilities. This type exists in no other variant.
/// </summary>
public static class Direct3D12Features
{
    /// <summary>Highest shader model compiled by DXC.</summary>
    public const string ShaderModel = "6.7";

    /// <summary>Whether the backend can expose ray tracing.</summary>
    public const bool SupportsRayTracing = true;

    /// <summary>Whether command lists are recorded in bundles.</summary>
    public const bool SupportsBundles = true;
}
