namespace Stride.MetaExperiment.Native.Direct3D11;

/// <summary>
/// Managed face of the simulated native Direct3D 11 package. A real Stride package
/// would ship P/Invoke bindings here; the experiment only needs the file name so the
/// backend can prove the native asset was copied next to the application.
/// </summary>
public static class NativeDirect3D11
{
    /// <summary>Package that owns the native asset.</summary>
    public const string PackageId = "Stride.MetaExperiment.Native.Direct3D11";

    /// <summary>Native library shipped in <c>runtimes/win-x64/native</c>.</summary>
    public const string LibraryFileName = "stride_d3d11.dll";
}
