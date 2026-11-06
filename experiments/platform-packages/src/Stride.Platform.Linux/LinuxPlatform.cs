namespace Stride.Platform;

/// <summary>Operating-system services for Linux.</summary>
public static class LinuxPlatform
{
    public const string Name = "Linux";

    public static PlatformInfo Info { get; } = new(PlatformKind.Linux, Name);

    /// <summary>Default windowing framework when the project does not pick one.</summary>
    public const WindowSystemKind DefaultWindowSystem = WindowSystemKind.X11;
}
