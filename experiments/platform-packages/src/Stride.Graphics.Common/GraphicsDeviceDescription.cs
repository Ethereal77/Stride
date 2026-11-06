namespace Stride.Graphics;

/// <summary>Describes the concrete device exposed by the selected variant.</summary>
public sealed record GraphicsDeviceDescription(GraphicsApi Api, string BackendName, string NativeLibrary);
