using Stride.Graphics;

// Intentionally touches a type that only exists in the Direct3D11 variant.
// Building this project with StrideGraphicsApi=Vulkan must fail with CS0234
// because the meta-package prunes the Direct3D11 variant from the compile graph.
Console.WriteLine(Stride.Graphics.Direct3D11.Direct3D11Features.ShaderModel);
