using Stride.Graphics;

// References Stride.Graphics.Direct3D11 directly, without the meta-package.
// The concrete GraphicsDevice type comes from that variant assembly.
using var device = new GraphicsDevice();
device.Initialize();

var nativeLibraryPresent = File.Exists(Path.Combine(AppContext.BaseDirectory, device.NativeLibrary));

Console.WriteLine($"api={device.Api}");
Console.WriteLine($"native={device.NativeLibrary}");
Console.WriteLine($"native-present={nativeLibraryPresent}");

if (device.Api != GraphicsApi.Direct3D11)
{
    Console.Error.WriteLine($"FAIL: expected Direct3D11, got {device.Api}.");
    return 1;
}

if (!nativeLibraryPresent)
{
    Console.Error.WriteLine($"FAIL: native library {device.NativeLibrary} was not copied to the output.");
    return 2;
}

Console.WriteLine("OK");
return 0;
