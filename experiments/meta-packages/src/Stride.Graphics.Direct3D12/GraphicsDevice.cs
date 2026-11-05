using Stride.MetaExperiment.Native.Direct3D12;

namespace Stride.Graphics;

/// <summary>
/// Direct3D 12 graphics device.
///
/// The namespace, the type name and the public surface are identical in every
/// variant: the consumer compiles against exactly one variant assembly, so there
/// is no interface, no base class and no runtime dispatch on this type.
/// </summary>
public sealed class GraphicsDevice : IDisposable
{
    private static readonly string LibraryFileName = NativeDirect3D12.LibraryFileName;

    private int submittedCommands;

    /// <summary>Creates the Direct3D 12 device.</summary>
    public GraphicsDevice()
    {
#if !STRIDE_GRAPHICS_API_DIRECT3D12
#error Stride.Graphics.Direct3D12 must be compiled with STRIDE_GRAPHICS_API_DIRECT3D12.
#endif
#if !STRIDE_PLATFORM_WINDOWS
#error Stride.Graphics.Direct3D12 only builds for Windows.
#endif
        Description = new GraphicsDeviceDescription(
            GraphicsApi.Direct3D12,
            "Direct3D12 (simulated)",
            LibraryFileName,
            GraphicsProfile.Level_11_1,
            maxTextureSize: 16384,
            supportsCompute: true);
    }

    /// <summary>Immutable description of the device.</summary>
    public GraphicsDeviceDescription Description { get; }

    /// <summary>API implemented by this variant.</summary>
    public GraphicsApi Api => Description.Api;

    /// <summary>Human readable backend name.</summary>
    public string BackendName => Description.BackendName;

    /// <summary>Native library required by this variant (file name only).</summary>
    public string NativeLibrary => Description.NativeLibrary;

    /// <summary>Number of commands passed to <see cref="Submit"/>.</summary>
    public int SubmittedCommands => submittedCommands;

    /// <summary>Loads the native library. Called once, outside the render loop.</summary>
    public void Initialize()
    {
        var path = Path.Combine(AppContext.BaseDirectory, LibraryFileName);
        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Native library '{LibraryFileName}' is missing from '{AppContext.BaseDirectory}'. " +
                $"The {NativeDirect3D12.PackageId} package assets were not copied to the output directory.");
        }
    }

    /// <summary>Hot path: deliberately non-virtual and allocation free.</summary>
    public void Submit(int commandCount) => submittedCommands += commandCount;

    /// <inheritdoc />
    public void Dispose()
    {
        // The simulated backend holds no unmanaged resources.
    }
}
