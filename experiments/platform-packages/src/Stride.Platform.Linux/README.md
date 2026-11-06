# Stride.Platform.Linux

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es una implementación del eje de plataforma, seleccionada por
> `Stride.Platform`. No lo referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `LinuxPlatform.Info` y el windowing por defecto de Linux (`X11`). No
aporta un `PlatformHost`; eso lo hace `Stride.Platform.Linux.X11`. Linux se
representa con `net10.0` más un RID (`linux-x64`) o `StridePlatform=Linux`.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
  <!-- Opcional si no se usa RID: -->
  <!-- <StridePlatform>Linux</StridePlatform> -->
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

var host = new PlatformHost();
Console.WriteLine(host.Platform.Name); // Linux
```

Una referencia directa compatible es solo una vía de escape: emite `STRIDE0001`
salvo opt-out; una incompatible sigue siendo error.
