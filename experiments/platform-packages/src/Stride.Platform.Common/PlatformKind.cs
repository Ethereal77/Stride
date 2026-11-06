namespace Stride.Platform;

/// <summary>Operating system a build was resolved for.</summary>
public enum PlatformKind
{
    /// <summary>No platform was resolved; only platform-neutral code is available.</summary>
    Portable = 0,

    Windows,
    Linux,
    MacOS,
    Android,
    IOS,
}
