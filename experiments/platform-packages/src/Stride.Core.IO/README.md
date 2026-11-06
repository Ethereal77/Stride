# Stride.Core.IO

**Estado:** público. Es el meta-paquete recomendado para el eje de I/O.

Selecciona la implementación de `FileSystem` para Windows o Linux y poda la que
no corresponde. Publica dependencias por TFM (Windows y Linux en `net10.0`, solo
Windows en `net10.0-windows`) y no lleva ensamblado propio.

## Consumo

```xml
<!-- Windows -->
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Core.IO" Version="..." />
</ItemGroup>
```

```xml
<!-- Linux: net10.0 + RID o StridePlatform explícito -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
```

```csharp
using Stride.Core.IO;

var fileSystem = new FileSystem();
Console.WriteLine($"{fileSystem.Info.Separator} {fileSystem.Info.IsCaseSensitive}");
// Windows: \ False
// Linux:   / True
```
