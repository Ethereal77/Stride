using Stride.Graphics;

#if STRIDE_GRAPHICS_FAMILY_DIRECT3D
using Stride.MetaExperiment.Pix;
using Stride.MetaExperiment.Warp;
#elif STRIDE_GRAPHICS_FAMILY_VULKAN
using Stride.MetaExperiment.Lavapipe;
#endif

// The meta-package selects one variant at compile time. There is no selector,
// no reflection and no runtime dispatch: this file binds to the concrete
// GraphicsDevice of the selected variant.
var expectedApi = GetCompileTimeApi();

Console.WriteLine($"platform={GetCompileTimePlatform()}");
Console.WriteLine($"compile-time={expectedApi}");

using var device = new GraphicsDevice();
device.Initialize();

var nativeLibraryPresent = File.Exists(Path.Combine(AppContext.BaseDirectory, device.NativeLibrary));

Console.WriteLine($"api={device.Api}");
Console.WriteLine($"backend={device.BackendName}");
Console.WriteLine($"native={device.NativeLibrary}");
Console.WriteLine($"profile={device.Description.Profile}");
Console.WriteLine($"native-present={nativeLibraryPresent}");

// Family-specific packages: PIX/WARP for Direct3D, Lavapipe for Vulkan.
// The #if blocks prove that the other family's assemblies were pruned from the
// compile graph, not merely hidden at runtime.
#if STRIDE_GRAPHICS_FAMILY_DIRECT3D
PixMarkers.BeginEvent("frame");
PixMarkers.EndEvent();
Console.WriteLine($"pix={PixMarkers.NativeLibrary}");
Console.WriteLine($"warp={WarpDevice.NativeLibrary}");
#elif STRIDE_GRAPHICS_FAMILY_VULKAN
Console.WriteLine($"lavapipe={LavapipeDevice.GetLibraryFileName()}");
#else
#error The Stride.Graphics meta-package did not define a graphics family.
#endif

// Hot path: a plain, non-virtual call. No interface, no backend lookup.
for (var i = 0; i < 1000; i++)
{
    device.Submit(1);
}

Console.WriteLine($"submitted={device.SubmittedCommands}");

if (device.Api != expectedApi)
{
    Console.Error.WriteLine($"FAIL: compiled for {expectedApi} but the device reports {device.Api}.");
    return 1;
}

if (!nativeLibraryPresent)
{
    Console.Error.WriteLine($"FAIL: native library {device.NativeLibrary} was not copied to the output.");
    return 2;
}

if (device.SubmittedCommands != 1000)
{
    Console.Error.WriteLine($"FAIL: expected 1000 submitted commands, got {device.SubmittedCommands}.");
    return 3;
}

#if STRIDE_GRAPHICS_FAMILY_DIRECT3D
if (!File.Exists(Path.Combine(AppContext.BaseDirectory, PixMarkers.NativeLibrary)))
{
    Console.Error.WriteLine($"FAIL: PIX asset {PixMarkers.NativeLibrary} was not copied to the output.");
    return 4;
}

if (!File.Exists(Path.Combine(AppContext.BaseDirectory, WarpDevice.NativeLibrary)))
{
    Console.Error.WriteLine($"FAIL: WARP asset {WarpDevice.NativeLibrary} was not copied to the output.");
    return 5;
}
#elif STRIDE_GRAPHICS_FAMILY_VULKAN
if (!File.Exists(Path.Combine(AppContext.BaseDirectory, LavapipeDevice.GetLibraryFileName())))
{
    Console.Error.WriteLine($"FAIL: Lavapipe asset {LavapipeDevice.GetLibraryFileName()} was not copied to the output.");
    return 4;
}
#endif

Console.WriteLine("OK");
return 0;

static GraphicsApi GetCompileTimeApi()
{
#if STRIDE_GRAPHICS_API_DIRECT3D11
    return GraphicsApi.Direct3D11;
#elif STRIDE_GRAPHICS_API_DIRECT3D12
    return GraphicsApi.Direct3D12;
#elif STRIDE_GRAPHICS_API_VULKAN
    return GraphicsApi.Vulkan;
#else
#error The Stride.Graphics meta-package did not define STRIDE_GRAPHICS_API_*.
#endif
}

static string GetCompileTimePlatform()
{
#if STRIDE_PLATFORM_WINDOWS
    return "Windows";
#elif STRIDE_PLATFORM_LINUX
    return "Linux";
#elif STRIDE_PLATFORM_MACOS
    return "macOS";
#elif STRIDE_PLATFORM_ANDROID
    return "Android";
#elif STRIDE_PLATFORM_IOS
    return "iOS";
#else
    return "Unknown";
#endif
}
