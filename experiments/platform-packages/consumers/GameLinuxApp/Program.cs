#if !STRIDE_PLATFORM_LINUX
#error GameLinuxApp must resolve the Linux platform.
#endif

#if !STRIDE_WINDOWSYSTEM_X11
#error GameLinuxApp must resolve the X11 windowing system.
#endif

#if !STRIDE_GRAPHICS_API_VULKAN
#error GameLinuxApp must resolve the Vulkan graphics API.
#endif

using Stride.AppModel;
using Stride.Core.IO;
using Stride.Games;
using Stride.Graphics;
using Stride.Platform;

var host = new PlatformHost();
var services = host.CreateServices();

using var device = new GraphicsDevice();

Console.WriteLine($"platform={host.Platform.Name}");
Console.WriteLine($"windowing={host.WindowSystem}");
Console.WriteLine($"surface={host.CreateSurface().Kind}");
Console.WriteLine($"platform-native={host.NativeLibrary}");
Console.WriteLine($"device-native={device.NativeLibrary}");

var fileSystem = new FileSystem();
Console.WriteLine($"io-root={fileSystem.Info.Root}");
Console.WriteLine($"io-separator={fileSystem.Info.Separator}");
Console.WriteLine($"io-case-sensitive={fileSystem.Info.IsCaseSensitive}");
Console.WriteLine($"io-native={fileSystem.NativeLibrary}");

var game = new Game(services)
{
    Title = "Linux game",
    SubmitCommands = device.Submit,
};

game.Run();

Console.WriteLine($"frames={game.FrameCount}");
Console.WriteLine($"submitted={device.SubmittedCommands}");
