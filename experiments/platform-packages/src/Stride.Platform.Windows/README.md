# Stride.Platform.Windows

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es una implementación del eje de plataforma, seleccionada por
> `Stride.Platform`. No lo referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `WindowsPlatform.Info` y el windowing por defecto de Windows (`Win32`).
No aporta un `PlatformHost`; eso lo hacen `Stride.Platform.Windows.Win32` o
`Stride.Platform.Windows.WinUI`.

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
Console.WriteLine(host.Platform.Name); // Windows
```

Una referencia directa compatible es solo una vía de escape: emite `STRIDE0001`
salvo que se active `StridePlatformAllowDirectReferences`; una incompatible
sigue siendo error.
