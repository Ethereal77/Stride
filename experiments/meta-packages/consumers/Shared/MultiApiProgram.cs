using Stride.Graphics;

// A multi-API host deploys every variant so a launcher can pick one later.
// Compilation is still single-API: the concrete GraphicsDevice below comes
// from the variant selected by StrideGraphicsApi. Switching APIs at runtime
// would need dynamic loading, which this design deliberately avoids.
var expectedApi = GetCompileTimeApi();

var variantAssemblies = new[]
{
    "Stride.Graphics.Direct3D11.dll",
    "Stride.Graphics.Direct3D12.dll",
    "Stride.Graphics.Vulkan.dll",
};

var nativeLibraries = new[]
{
    "stride_d3d11.dll",
    "stride_d3d12.dll",
    "stride_vulkan.dll",
};

var conditionalAssets = new[]
{
    "WinPixEventRuntime.dll",
    "d3d10warp.dll",
    "vulkan_lvp.dll",
};

var deployed = variantAssemblies
    .Where(name => File.Exists(Path.Combine(AppContext.BaseDirectory, name)))
    .Select(name => name.Replace("Stride.Graphics.", string.Empty).Replace(".dll", string.Empty))
    .ToArray();

Console.WriteLine($"compile-time={expectedApi}");
Console.WriteLine($"deployed={string.Join(",", deployed)}");

using var device = new GraphicsDevice();
device.Initialize();

Console.WriteLine($"api={device.Api}");
Console.WriteLine($"native={device.NativeLibrary}");

if (device.Api != expectedApi)
{
    Console.Error.WriteLine($"FAIL: compiled for {expectedApi} but the device reports {device.Api}.");
    return 1;
}

if (deployed.Length != variantAssemblies.Length)
{
    Console.Error.WriteLine($"FAIL: a multi-API host must deploy every variant; found {string.Join(",", deployed)}.");
    return 2;
}

var missingNative = nativeLibraries
    .Where(name => !File.Exists(Path.Combine(AppContext.BaseDirectory, name)))
    .ToArray();

if (missingNative.Length != 0)
{
    Console.Error.WriteLine($"FAIL: a multi-API host must deploy every native asset; missing {string.Join(",", missingNative)}.");
    return 3;
}

var deployedConditional = conditionalAssets
    .Where(name => File.Exists(Path.Combine(AppContext.BaseDirectory, name)))
    .ToArray();

Console.WriteLine($"conditional={string.Join(",", deployedConditional)}");

if (deployedConditional.Length != conditionalAssets.Length)
{
    Console.Error.WriteLine($"FAIL: a multi-API host must deploy every conditional package; found {string.Join(",", deployedConditional)}.");
    return 4;
}

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
