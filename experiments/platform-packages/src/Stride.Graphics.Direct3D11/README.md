# Stride.Graphics.Direct3D11

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es una variante gráfica que selecciona `Stride.Graphics`. No lo
> referencies directamente desde una aplicación.

**Estado:** interno.

Aporta la implementación simulada de `GraphicsDevice` para Direct3D 11 y el
activo nativo `stride_d3d11.dll`. Es la API por defecto en Windows. Una
referencia directa que no coincida con `StrideGraphicsApi` hace fallar el build;
si es compatible, emite `STRIDE0001` salvo opt-out.

## Consumo recomendado

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
