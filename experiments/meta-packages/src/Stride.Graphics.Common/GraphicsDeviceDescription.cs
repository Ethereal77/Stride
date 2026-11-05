namespace Stride.Graphics;

/// <summary>
/// Immutable description of the device created by a variant. A shared value type:
/// the variants fill it in, consumers read it without any indirection.
/// </summary>
public readonly struct GraphicsDeviceDescription
{
    /// <summary>Creates a device description.</summary>
    public GraphicsDeviceDescription(
        GraphicsApi api,
        string backendName,
        string nativeLibrary,
        GraphicsProfile profile,
        int maxTextureSize,
        bool supportsCompute)
    {
        Api = api;
        BackendName = backendName;
        NativeLibrary = nativeLibrary;
        Profile = profile;
        MaxTextureSize = maxTextureSize;
        SupportsCompute = supportsCompute;
    }

    /// <summary>API implemented by the variant.</summary>
    public GraphicsApi Api { get; }

    /// <summary>Human readable backend name.</summary>
    public string BackendName { get; }

    /// <summary>Native library the backend needs at runtime (file name only).</summary>
    public string NativeLibrary { get; }

    /// <summary>Feature level reported by the backend.</summary>
    public GraphicsProfile Profile { get; }

    /// <summary>Largest supported texture edge, in pixels.</summary>
    public int MaxTextureSize { get; }

    /// <summary>Whether the backend exposes compute shaders.</summary>
    public bool SupportsCompute { get; }

    /// <inheritdoc />
    public override string ToString() =>
        $"{BackendName} ({Api}, {Profile}, max texture {MaxTextureSize}, compute {SupportsCompute})";
}
