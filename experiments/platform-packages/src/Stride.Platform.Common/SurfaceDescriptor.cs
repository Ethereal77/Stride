namespace Stride.Platform;

/// <summary>
/// Opaque description of a native surface. This is the only seam between a
/// windowing package and a graphics variant: no interface, no virtual dispatch.
/// </summary>
public readonly record struct SurfaceDescriptor(
    SurfaceKind Kind,
    nint Handle,
    uint Width,
    uint Height,
    string DebugName)
{
    /// <summary>A surface that presents nowhere, used by tests and headless hosts.</summary>
    public static SurfaceDescriptor Headless { get; } = new(SurfaceKind.Headless, 0, 0, 0, "headless");
}
