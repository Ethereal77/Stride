# Stride.Graphics.Common

**Estado:** público de soporte. No es un meta-paquete ni implementa un device.

Contiene los tipos compartidos del eje gráfico: `GraphicsApi` y
`GraphicsDeviceDescription`. No selecciona ninguna variante; el `GraphicsDevice`
concreto lo aporta la implementación elegida por `Stride.Graphics`.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Graphics.Common" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Graphics;

public static class GraphicsProbe
{
    public static GraphicsApi PreferredApi => GraphicsApi.Vulkan;

    public static GraphicsDeviceDescription Describe(GraphicsApi api) =>
        new(api, "portable-probe", "none");
}
```
