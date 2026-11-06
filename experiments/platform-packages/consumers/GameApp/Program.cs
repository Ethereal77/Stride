#if !STRIDE_PLATFORM_WINDOWS
#error GameApp must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WIN32
#error GameApp must resolve the Win32 windowing system.
#endif

#if !STRIDE_GRAPHICS_API_DIRECT3D11
#error GameApp must resolve the Direct3D11 graphics API.
#endif

using Stride.AppModel;
using Stride.Core.IO;
using Stride.Games;
using Stride.Graphics;
using Stride.Platform;

var host = new PlatformHost();
var services = host.CreateServices();

using var device = new GraphicsDevice();
var game = new DemoGame(services, device)
{
    SubmitCommands = device.Submit,
};

game.Run();

Console.WriteLine($"frames={game.FrameCount}");
Console.WriteLine($"elapsed={game.Elapsed.TotalMilliseconds:0}ms");
Console.WriteLine($"submitted={device.SubmittedCommands}");
Console.WriteLine($"device-surface={device.Surface.Kind}");
Console.WriteLine($"device-api={device.Api}");
Console.WriteLine($"device-native={device.NativeLibrary}");

var fileSystem = new FileSystem();
Console.WriteLine($"io-root={fileSystem.Info.Root}");
Console.WriteLine($"io-separator={fileSystem.Info.Separator}");
Console.WriteLine($"io-case-sensitive={fileSystem.Info.IsCaseSensitive}");
Console.WriteLine($"io-native={fileSystem.NativeLibrary}");

internal sealed class DemoGame : Game
{
    private readonly GraphicsDevice _device;

    public DemoGame(PlatformServices services, GraphicsDevice device)
        : base(services)
    {
        _device = device;
    }

    protected override void Initialize()
    {
        _device.Initialize(Services.CreateSurface());
        Console.WriteLine($"platform={Platform.Name}");
        Console.WriteLine($"windowing={WindowSystem}");
    }

    protected override void Draw()
    {
        base.Draw();
        Console.WriteLine($"frame={FrameCount} surface={Surface.Kind}");
    }
}
