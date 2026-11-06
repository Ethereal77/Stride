namespace Stride.Platform;

/// <summary>Operating-system services for Windows.</summary>
public static class WindowsPlatform
{
    public const string Name = "Windows";

    public static PlatformInfo Info { get; } = new(PlatformKind.Windows, Name);

    /// <summary>Default windowing framework when the project does not pick one.</summary>
    public const WindowSystemKind DefaultWindowSystem = WindowSystemKind.Win32;
}
