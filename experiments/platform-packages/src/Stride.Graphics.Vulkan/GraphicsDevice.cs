using Stride.Platform;

namespace Stride.Graphics;

/// <summary>Concrete Vulkan device. Same public surface as every variant.</summary>
public sealed class GraphicsDevice : IDisposable
{
    public GraphicsApi Api => GraphicsApi.Vulkan;

    public string BackendName => "Vulkan (simulated)";

    public string NativeLibrary => OperatingSystem.IsWindows() ? "stride_vulkan.dll" : "libstride_vulkan.so";

    public SurfaceDescriptor Surface { get; private set; } = SurfaceDescriptor.Headless;

    public GraphicsDeviceDescription Description => new(Api, BackendName, NativeLibrary);

    public int SubmittedCommands { get; private set; }

    public void Initialize(SurfaceDescriptor surface) => Surface = surface;

    public void Submit(int commandCount) => SubmittedCommands += commandCount;

    public void Dispose()
    {
    }
}
