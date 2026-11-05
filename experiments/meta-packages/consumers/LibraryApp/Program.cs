using ConsumerLibrary;
using Stride.Graphics;

Console.WriteLine($"platform={GetCompileTimePlatform()}");
Console.WriteLine($"library-compile-time={GraphicsLibrary.CompileTimeApi}");
Console.WriteLine($"library-device={GraphicsLibrary.CreateDeviceDescription()}");
Console.WriteLine($"library-profiling={GraphicsLibrary.ProfilingBackend}");

using var device = new GraphicsDevice();
device.Initialize();

var nativeLibraryPresent = File.Exists(Path.Combine(AppContext.BaseDirectory, device.NativeLibrary));

Console.WriteLine($"api={device.Api}");
Console.WriteLine($"native-present={nativeLibraryPresent}");

if (device.Api != GraphicsLibrary.CompileTimeApi)
{
    Console.Error.WriteLine("FAIL: the app and the library disagree on the selected graphics API.");
    return 1;
}

if (!nativeLibraryPresent)
{
    Console.Error.WriteLine($"FAIL: native library {device.NativeLibrary} was not copied to the output.");
    return 2;
}

Console.WriteLine("OK");
return 0;

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
