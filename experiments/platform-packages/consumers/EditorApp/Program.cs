#if !STRIDE_PLATFORM_WINDOWS
#error EditorApp must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WIN32
#error EditorApp must resolve the Win32 windowing system.
#endif

#if !STRIDE_GRAPHICS_API_DIRECT3D11
#error EditorApp must resolve the Direct3D11 graphics API.
#endif

using EditorApp;
using EditorShared;
using Stride.Core.IO;
using Stride.Graphics;
using Stride.Platform;

var host = new PlatformHost();
var adapter = new WpfWin32SurfaceAdapter();

// The editor owns the UI loop and supplies surfaces from WPF, not from the
// platform host's game-window factory.
var services = new PlatformServices
{
    Platform = host.Platform,
    WindowSystem = host.WindowSystem,
    CreateSurface = adapter.CreateSurface,
    Run = host.Run,
};

using var device = new GraphicsDevice();
device.Initialize(adapter.CreateSurface());

var scene = new SceneViewGame(services, device);
var preview = new AssetPreviewGame(services, device);

scene.Run();
preview.Run();

var fileSystem = new FileSystem();

Console.WriteLine($"host={host.Platform.Name}/{host.WindowSystem}");
Console.WriteLine($"api={device.Api}");
Console.WriteLine($"surface={device.Surface.Kind}");
Console.WriteLine($"adapter={device.Surface.DebugName}");
Console.WriteLine($"scene-frames={scene.FrameCount} draws={scene.DrawCount}");
Console.WriteLine($"preview-frames={preview.FrameCount} draws={preview.DrawCount}");
Console.WriteLine($"submitted={device.SubmittedCommands}");
Console.WriteLine($"assets={fileSystem.Combine(fileSystem.Info.Root, "assets")}");
