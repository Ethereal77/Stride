# Stride.Platform

**Estado:** público. Es el meta-paquete recomendado para el eje de plataforma.

Resuelve en tiempo de compilación el sistema operativo, el sistema de ventanas y
los valores por defecto de la API gráfica. Define las constantes
`STRIDE_PLATFORM_*` y `STRIDE_WINDOWSYSTEM_*`, valida combinaciones imposibles y
poda los paquetes de plataforma que no coinciden. No lleva ensamblado propio.

## Consumo

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <!-- Opcional: Windows usa Win32 por defecto. -->
  <StrideWindowSystem>WinUI</StrideWindowSystem>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

```csharp
using Stride.Platform;

var host = new PlatformHost();
Console.WriteLine($"{host.Platform.Name}/{host.WindowSystem}"); // Windows/WinUI
```

Para Linux no existe `net10.0-linux`: usa `net10.0` con un RID (`linux-x64`) o
`StridePlatform=Linux`. Normalmente llega de forma transitiva desde
`Stride.Games`, `Stride.Graphics` o `Stride.Core.IO`.
