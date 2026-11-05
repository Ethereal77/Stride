namespace Stride.MetaExperiment.Pix;

/// <summary>
/// Managed face of the simulated PIX event markers. Real PIX integration is a
/// native WinPixEventRuntime plus a thin managed wrapper.
/// </summary>
public static class PixMarkers
{
    /// <summary>Package that owns the native asset.</summary>
    public const string PackageId = "Stride.MetaExperiment.Pix";

    /// <summary>Native library shipped in <c>runtimes/win-x64/native</c>.</summary>
    public const string NativeLibrary = "WinPixEventRuntime.dll";

    /// <summary>Opens a PIX event scope.</summary>
    public static void BeginEvent(string name)
    {
        // The simulated marker does not call into the native runtime.
    }

    /// <summary>Closes the current PIX event scope.</summary>
    public static void EndEvent()
    {
        // The simulated marker does not call into the native runtime.
    }
}
