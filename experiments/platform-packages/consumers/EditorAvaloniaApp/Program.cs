#if !STRIDE_PLATFORM_LINUX
#error EditorAvaloniaApp must resolve the Linux platform.
#endif

#if !STRIDE_WINDOWSYSTEM_X11
#error EditorAvaloniaApp must resolve the X11 windowing system.
#endif

#if !STRIDE_GRAPHICS_API_VULKAN
#error EditorAvaloniaApp must resolve the Vulkan graphics API.
#endif

using EditorShared;
using Stride.Core.IO;
using Stride.Graphics;
using Stride.Platform;

// The Avalonia head owns the UI loop and supplies the native top level through
// its adapter. The internal scene and preview games stay in shared components.
var host = new PlatformHost();
var adapter = new AvaloniaSurfaceAdapter();

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
Console.WriteLine($"platform-native={host.NativeLibrary}");
Console.WriteLine($"device-native={device.NativeLibrary}");
Console.WriteLine($"scene-frames={scene.FrameCount} draws={scene.DrawCount}");
Console.WriteLine($"preview-frames={preview.FrameCount} draws={preview.DrawCount}");
Console.WriteLine($"submitted={device.SubmittedCommands}");
Console.WriteLine($"assets={fileSystem.Combine(fileSystem.Info.Root, "assets")}");
