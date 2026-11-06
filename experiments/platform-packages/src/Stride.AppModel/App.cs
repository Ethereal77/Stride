using Stride.Platform;

namespace Stride.AppModel;

/// <summary>
/// Platform-neutral application lifecycle. It never references a concrete
/// platform: the executable binds to the resolved platform host at compile time
/// and hands over a <see cref="PlatformServices"/> bundle.
/// </summary>
public abstract class App
{
    protected App(PlatformServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        Services = services;
    }

    /// <summary>Platform primitives supplied by the executable.</summary>
    public PlatformServices Services { get; }

    public PlatformInfo Platform => Services.Platform;

    public WindowSystemKind WindowSystem => Services.WindowSystem;

    /// <summary>Surface created for the current frame.</summary>
    public SurfaceDescriptor Surface { get; private set; } = SurfaceDescriptor.Headless;

    public bool IsRunning { get; private set; }

    public int FrameCount { get; private set; }

    /// <summary>Initializes, runs the platform loop, then unloads.</summary>
    public void Run()
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("The application is already running.");
        }

        IsRunning = true;

        try
        {
            Initialize();
            LoadContent();
            Services.Run(Tick);
        }
        finally
        {
            IsRunning = false;
            UnloadContent();
        }
    }

    private void Tick()
    {
        Surface = Services.CreateSurface();
        Update();
        Draw();
        FrameCount++;
    }

    protected virtual void Initialize()
    {
    }

    protected virtual void LoadContent()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void Draw()
    {
    }

    protected virtual void UnloadContent()
    {
    }
}
