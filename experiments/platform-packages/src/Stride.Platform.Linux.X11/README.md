# Stride.Platform.Linux.X11

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es el host de ventanas X11 que selecciona `Stride.Platform` en Linux.
> No lo referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `PlatformHost` para X11: `WindowSystemKind.X11`, superficies
`X11Window` y el activo nativo `libstride_platform_x11.so`. Es el windowing por
defecto de Linux.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

var host = new PlatformHost();
Console.WriteLine($"{host.WindowSystem} {host.CreateSurface().Kind}"); // X11 X11Window
```

El pin directo es una vía de escape: emite `STRIDE0001` salvo opt-out y falla si
no coincide con la plataforma o el windowing resueltos.
