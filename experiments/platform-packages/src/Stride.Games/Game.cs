using Stride.AppModel;
using Stride.Platform;

namespace Stride.Games;

/// <summary>
/// Game loop on top of <see cref="App"/>. It owns no graphics device: the
/// executable creates the concrete variant and wires it through
/// <see cref="SubmitCommands"/>, because a graphics variant is chosen by the
/// application, not by the library.
/// </summary>
public class Game : App
{
    public Game(PlatformServices services)
        : base(services)
    {
    }

    public string Title { get; set; } = "Stride Game";

    /// <summary>Simulated frame duration added by <see cref="Update"/>.</summary>
    public TimeSpan Elapsed { get; private set; }

    public TimeSpan TargetFrameTime { get; set; } = TimeSpan.FromMilliseconds(16);

    /// <summary>Optional sink for the graphics device owned by the executable.</summary>
    public Action<int>? SubmitCommands { get; set; }

    protected override void Update()
    {
        Elapsed += TargetFrameTime;
    }

    protected override void Draw()
    {
        SubmitCommands?.Invoke(1);
    }
}
