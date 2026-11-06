# Stride.AppModel

**Estado:** público, pero no es un meta-paquete.

Define el ciclo de vida neutral de una aplicación (`App`) sobre
`PlatformServices`. No elige plataforma ni API, y no referencia ningún host
concreto. Normalmente se consume a través de `Stride.Games`.

## Consumo

Necesita una plataforma resuelta (TFM, RID o `StridePlatform`) y un host
concreto, que aporta el meta-paquete de plataforma:

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.AppModel" Version="..." />
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.AppModel;
using Stride.Platform;

var host = new PlatformHost();
var app = new MyApp(host.CreateServices());
app.Run();

sealed class MyApp(PlatformServices services) : App(services)
{
    protected override void Draw() { }
}
```

`Stride.AppModel` ya depende de `Stride.Platform`; la referencia explícita solo
hace visible la intención.
