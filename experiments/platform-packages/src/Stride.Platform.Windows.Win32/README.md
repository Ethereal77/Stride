# Stride.Platform.Windows.Win32

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es el host de ventanas Win32 que selecciona `Stride.Platform` en
> Windows. No lo referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `PlatformHost` para Win32: `WindowSystemKind.Win32`, superficies
`Win32Hwnd` y el activo nativo `stride_platform_win32.dll`. Es el windowing por
defecto de Windows.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

var host = new PlatformHost();
Console.WriteLine($"{host.WindowSystem} {host.CreateSurface().Kind}"); // Win32 Win32Hwnd
```

El pin directo es una vía de escape: emite `STRIDE0001` salvo opt-out y falla si
no coincide con la plataforma o el windowing resueltos.
