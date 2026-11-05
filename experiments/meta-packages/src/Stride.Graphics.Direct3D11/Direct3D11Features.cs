namespace Stride.Graphics.Direct3D11;

/// <summary>
/// Direct3D 11-only capabilities. This type exists in no other variant.
/// </summary>
public static class Direct3D11Features
{
    /// <summary>Shader model compiled by the legacy FXC path.</summary>
    public const string ShaderModel = "5.0";

    /// <summary>Whether the backend supports tessellation stages.</summary>
    public const bool SupportsTessellation = true;

    /// <summary>Native compiler library used by the simulated pipeline.</summary>
    public const string CompilerLibrary = "d3dcompiler_47.dll";
}
