# Stride.Graphics

**Estado:** público. Es el meta-paquete recomendado para el eje gráfico.

Selecciona una única variante de API (`Direct3D11` o `Vulkan`) y las
composiciones pairwise disponibles (`Direct3D11.WinUI`). Define las constantes
`STRIDE_GRAPHICS_*`, valida la combinación con la plataforma y poda las
variantes no seleccionadas. No lleva ensamblado propio.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <!-- Direct3D11 es el valor por defecto en Windows. -->
  <StrideGraphicsApi>Direct3D11</StrideGraphicsApi>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Graphics" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Graphics;

using var device = new GraphicsDevice();
Console.WriteLine(device.Api); // Direct3D11
```

En Linux usa `net10.0` con RID o `StridePlatform=Linux`, y `Vulkan`. Un mismo
binario no puede ver las dos variantes a la vez.
