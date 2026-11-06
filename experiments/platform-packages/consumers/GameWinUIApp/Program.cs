#if !STRIDE_PLATFORM_WINDOWS
#error GameWinUIApp must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WINUI
#error GameWinUIApp must resolve the WinUI windowing system.
#endif

#if !STRIDE_GRAPHICS_API_DIRECT3D11
#error GameWinUIApp must resolve the Direct3D11 graphics API.
#endif

using Stride.AppModel;
using Stride.Games;
using Stride.Graphics;
using Stride.Graphics.Direct3D11.WinUI;
using Stride.Platform;

var host = new PlatformHost();
var services = host.CreateServices();

using var device = new GraphicsDevice();

// Pairwise composition: the SwapChainPanel setup is Direct3D11 + WinUI specific.
object swapChainPanel = new();
WinUIGraphics.Initialize(device, swapChainPanel);

Console.WriteLine($"platform={host.Platform.Name}");
Console.WriteLine($"windowing={host.WindowSystem}");
Console.WriteLine($"surface={device.Surface.Kind}");
Console.WriteLine($"composition={WinUIGraphics.PackageId}");
Console.WriteLine($"composition-native={WinUIGraphics.NativeLibrary}");

var game = new Game(services)
{
    SubmitCommands = device.Submit,
};

game.Run();

Console.WriteLine($"frames={game.FrameCount}");
Console.WriteLine($"submitted={device.SubmittedCommands}");
