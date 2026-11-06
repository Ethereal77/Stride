# Stride.Core.IO.Windows

> **Advertencia — proyecto interno.** No está pensado para consumidores de
> Stride. Es la implementación de I/O que selecciona `Stride.Core.IO`. No lo
> referencies directamente desde una aplicación.

**Estado:** interno.

Aporta `FileSystem` para Windows: raíz `C:\ProgramData\Stride`, separador `\`,
rutas case-insensitive y el activo nativo `stride_io_windows.dll`.

## Consumo recomendado

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Core.IO" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Core.IO;

var fileSystem = new FileSystem();
Console.WriteLine(fileSystem.Combine(fileSystem.Info.Root, "assets"));
// C:\ProgramData\Stride\assets
```

Una referencia directa compatible emite `STRIDE0001` salvo opt-out; una
incompatible con la plataforma resuelta sigue siendo error.
