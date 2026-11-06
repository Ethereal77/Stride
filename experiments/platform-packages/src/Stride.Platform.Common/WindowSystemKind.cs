namespace Stride.Platform;

/// <summary>Windowing framework a build was resolved for.</summary>
public enum WindowSystemKind
{
    /// <summary>No windowing system was resolved (portable or headless).</summary>
    None = 0,

    Win32,
    WinUI,
    X11,
    Wayland,
}
