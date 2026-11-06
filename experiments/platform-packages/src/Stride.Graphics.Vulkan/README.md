# Stride.Graphics.Vulkan

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es una variante gráfica que selecciona `Stride.Graphics`. No lo
> referencies directamente desde una aplicación.

**Estado:** interno.

Aporta la implementación simulada de `GraphicsDevice` para Vulkan y activos
nativos para Windows (`stride_vulkan.dll`) y Linux (`libstride_vulkan.so`). Es la
API por defecto en Linux. Una referencia directa que no coincida con
`StrideGraphicsApi` o la plataforma resuelta hace fallar el build; si es
compatible, emite `STRIDE0001` salvo opt-out.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <StrideGraphicsApi>Vulkan</StrideGraphicsApi>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Graphics" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Graphics;

using var device = new GraphicsDevice();
Console.WriteLine(device.Api); // Vulkan
```

También puede seleccionarse en Windows con `<StrideGraphicsApi>Vulkan</StrideGraphicsApi>`.
