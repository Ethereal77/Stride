namespace Stride.Platform;

/// <summary>Describes the platform a build was resolved for.</summary>
public sealed record PlatformInfo(PlatformKind Kind, string Name)
{
    /// <summary>Platform-neutral build.</summary>
    public static PlatformInfo Portable { get; } = new(PlatformKind.Portable, "Portable");

    public bool IsPortable => Kind == PlatformKind.Portable;
}
