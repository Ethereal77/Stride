# Stride.Platform.Common

**Estado:** público de soporte. No es un meta-paquete ni selecciona plataforma.

Contiene los contratos compartidos del eje de plataforma: `PlatformInfo`,
`PlatformKind`, `WindowSystemKind`, `SurfaceDescriptor`, `SurfaceKind`,
`PlatformServices`. No define constantes de plataforma ni importa paquetes de
eje, así que es seguro en una biblioteca portable. Normalmente llega de forma
transitiva desde `Stride.Platform`.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform.Common" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

public static class PortableProbe
{
    public static PlatformInfo Platform => PlatformInfo.Portable;
    public static SurfaceDescriptor Surface => SurfaceDescriptor.Headless;
}
```
