# Stride.Graphics.Direct3D11.WinUI

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es una composición pairwise que selecciona `Stride.Graphics`; no es un
> eje independiente. No lo referencies directamente desde una aplicación.

**Estado:** interno.

Aporta la integración entre Direct3D 11 y WinUI: `WinUIGraphics.Initialize`
prepara un `GraphicsDevice` contra un `SwapChainPanel`, una superficie
`WinUISwapChainPanel` y el activo nativo `stride_graphics_winui.dll`. Existe como
composición porque el panel de WinUI necesita preparación específica de D3D11.
Una referencia directa compatible emite `STRIDE0001` salvo opt-out; si no
coincide con API, plataforma o windowing, el build falla.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <StrideWindowSystem>WinUI</StrideWindowSystem>
  <StrideGraphicsApi>Direct3D11</StrideGraphicsApi>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Graphics" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Graphics;
using Stride.Graphics.Direct3D11.WinUI;

using var device = new GraphicsDevice();
WinUIGraphics.Initialize(device, swapChainPanel);

Console.WriteLine(device.Surface.Kind); // WinUISwapChainPanel
```

`Stride.Graphics` solo restaura esta composición en `net10.0-windows` con
WinUI y Direct3D 11 seleccionados.
