#if !STRIDE_PLATFORM_WINDOWS
#error AssetTool must resolve the Windows platform.
#endif

#if !STRIDE_WINDOWSYSTEM_WIN32
#error AssetTool must resolve the Win32 windowing system (Windows default).
#endif

using Stride.Core.IO;
using Stride.Platform;

// A console tool owns no window. It only needs I/O, so it references
// Stride.Core.IO and states the platform explicitly instead of relying on the
// compile-host fallback that a distributed build must never use.
var host = new PlatformHost();
var fileSystem = new FileSystem();

var assetsRoot = fileSystem.Combine(fileSystem.Info.Root, "assets");
var cacheRoot = fileSystem.Combine(fileSystem.Info.Root, "cache");

Console.WriteLine("tool=asset");
Console.WriteLine($"platform={host.Platform.Name}");
Console.WriteLine($"io-root={fileSystem.Info.Root}");
Console.WriteLine($"io-separator={fileSystem.Info.Separator}");
Console.WriteLine($"io-case-sensitive={fileSystem.Info.IsCaseSensitive}");
Console.WriteLine($"io-native={fileSystem.NativeLibrary}");
Console.WriteLine($"assets={assetsRoot}");
Console.WriteLine($"cache={cacheRoot}");
