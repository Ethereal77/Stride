using Stride.Platform;

namespace Stride.Graphics.Direct3D11.WinUI;

/// <summary>
/// Pairwise integration: WinUI's SwapChainPanel needs Direct3D 11-specific setup
/// that a generic SurfaceDescriptor cannot express. This is why the package
/// exists as a composition rather than as another axis.
/// </summary>
public static class WinUIGraphics
{
    public const string PackageId = "Stride.Graphics.Direct3D11.WinUI";

    public const string NativeLibrary = "stride_graphics_winui.dll";

    /// <summary>Initializes a Direct3D 11 device against a simulated WinUI panel.</summary>
    public static void Initialize(GraphicsDevice device, object swapChainPanel)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentNullException.ThrowIfNull(swapChainPanel);

        device.Initialize(new SurfaceDescriptor(
            SurfaceKind.WinUISwapChainPanel,
            0x40004,
            1920,
            1080,
            "WinUI SwapChainPanel"));
    }
}
