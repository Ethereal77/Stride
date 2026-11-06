# Stride.Core.IO.Common

**Estado:** público de soporte. No es un meta-paquete ni implementa I/O.

Contiene `StorageInfo`, la descripción compartida de cómo una plataforma
almacena y compara rutas. No incluye `FileSystem`; la implementación concreta la
selecciona `Stride.Core.IO`.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Core.IO.Common" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Core.IO;

var info = new StorageInfo("/work", '/', IsCaseSensitive: true);
Console.WriteLine($"{info.Root} separator={info.Separator}");
```
