using Stride.Platform;

namespace Stride.Graphics;

/// <summary>Concrete Direct3D 11 device. Same public surface as every variant.</summary>
public sealed class GraphicsDevice : IDisposable
{
    public GraphicsApi Api => GraphicsApi.Direct3D11;

    public string BackendName => "Direct3D11 (simulated)";

    public string NativeLibrary => "stride_d3d11.dll";

    public SurfaceDescriptor Surface { get; private set; } = SurfaceDescriptor.Headless;

    public GraphicsDeviceDescription Description => new(Api, BackendName, NativeLibrary);

    public int SubmittedCommands { get; private set; }

    public void Initialize(SurfaceDescriptor surface) => Surface = surface;

    public void Submit(int commandCount) => SubmittedCommands += commandCount;

    public void Dispose()
    {
    }
}
