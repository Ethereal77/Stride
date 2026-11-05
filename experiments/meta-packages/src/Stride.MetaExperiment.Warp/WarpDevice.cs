namespace Stride.MetaExperiment.Warp;

/// <summary>
/// Managed face of the simulated WARP (Windows Advanced Rasterization Platform)
/// bindings. WARP is a software Direct3D rasterizer, so this package only makes
/// sense for the Direct3D variants.
/// </summary>
public static class WarpDevice
{
    /// <summary>Package that owns the native asset.</summary>
    public const string PackageId = "Stride.MetaExperiment.Warp";

    /// <summary>Native library shipped in <c>runtimes/win-x64/native</c>.</summary>
    public const string NativeLibrary = "d3d10warp.dll";

    /// <summary>Adapter name reported by WARP.</summary>
    public const string AdapterName = "Microsoft Basic Render Driver";
}
