using Stride.Games;
using Stride.Graphics;
using Stride.Platform;

namespace EditorShared;

/// <summary>
/// Simulated editor-side game component. The editor host owns the UI loop;
/// these are the internal games used for scene views and asset previews.
/// </summary>
internal abstract class EditorGame : Game
{
    protected EditorGame(PlatformServices services, GraphicsDevice device)
        : base(services)
    {
        Device = device;
        SubmitCommands = device.Submit;
    }

    public GraphicsDevice Device { get; }

    public int DrawCount { get; private set; }

    protected override void Draw()
    {
        base.Draw();
        DrawCount++;
    }
}

internal sealed class SceneViewGame : EditorGame
{
    public SceneViewGame(PlatformServices services, GraphicsDevice device)
        : base(services, device)
    {
        Title = "Scene view";
    }
}

internal sealed class AssetPreviewGame : EditorGame
{
    public AssetPreviewGame(PlatformServices services, GraphicsDevice device)
        : base(services, device)
    {
        Title = "Asset preview";
    }
}
