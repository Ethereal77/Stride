# Stride.Platform.Windows.WinUI

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es el host de ventanas WinUI que selecciona `Stride.Platform`. No lo
> referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `PlatformHost` para WinUI: `WindowSystemKind.WinUI`, superficies
`WinUISwapChainPanel` y el activo nativo `stride_platform_winui.dll`. Solo
existe en el TFM `net10.0-windows`; no forma parte del grupo `net10.0`.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <StrideWindowSystem>WinUI</StrideWindowSystem>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

var host = new PlatformHost();
Console.WriteLine($"{host.WindowSystem} {host.CreateSurface().Kind}"); // WinUI WinUISwapChainPanel
```

El pin directo es una vía de escape: emite `STRIDE0001` salvo opt-out y falla si
no coincide con el windowing resuelto.
