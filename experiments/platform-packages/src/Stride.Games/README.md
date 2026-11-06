# Stride.Games

**Estado:** público. Es el punto de entrada habitual para juegos.

Añade `Game`, un bucle sobre `Stride.AppModel`, con `Title`, `Elapsed` y
`SubmitCommands`. No posee ni referencia ninguna variante gráfica: la aplicación
crea el dispositivo concreto y lo conecta.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Games" Version="..." />
  <PackageReference Include="Stride.Graphics" Version="..." />
  <PackageReference Include="Stride.Core.IO" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Games;
using Stride.Graphics;
using Stride.Platform;

var host = new PlatformHost();
using var device = new GraphicsDevice();

var game = new Game(host.CreateServices())
{
    SubmitCommands = device.Submit,
};

game.Run();
```

Para un editor con su propio bucle de UI, usa los componentes internos de escena
y preview en lugar de convertir el editor completo en un `Game`.
