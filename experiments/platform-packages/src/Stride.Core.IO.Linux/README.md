# Stride.Core.IO.Linux

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es la implementación de I/O que selecciona `Stride.Core.IO`. No lo
> referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `FileSystem` para Linux: raíz `/var/lib/stride`, separador `/`, rutas
case-sensitive y el activo nativo `libstride_io_linux.so`.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <RuntimeIdentifier>linux-x64</RuntimeIdentifier>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Core.IO" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Core.IO;

var fileSystem = new FileSystem();
Console.WriteLine(fileSystem.Combine(fileSystem.Info.Root, "assets"));
// /var/lib/stride/assets
```

Una referencia directa compatible emite `STRIDE0001` salvo opt-out; una
incompatible con la plataforma resuelta sigue siendo error.
