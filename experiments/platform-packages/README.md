# Experimento: meta-paquetes de plataforma

Este directorio contiene un experimento aislado del build de Stride. Su objetivo
es comprobar si la plataforma, el sistema de ventanas y la API gráfica pueden
seleccionarse **en tiempo de compilación** mediante meta-paquetes de NuGet, sin
producto cartesiano de paquetes y sin despacho dinámico en tiempo de ejecución.

No modifica nada fuera de `slim-sdk/experiments/platform-packages/` y no toca el
experimento anterior de `Stride.Graphics`.

## Objetivo

Un consumidor declara únicamente meta-paquetes:

```xml
<ItemGroup>
  <PackageReference Include="Stride.Games" Version="..." />
  <PackageReference Include="Stride.Graphics" Version="..." />
  <PackageReference Include="Stride.Core.IO" Version="..." />
</ItemGroup>
```

y el build resuelve:

- el sistema operativo, a partir del TFM (`net10.0-windows`, `net10.0-macos`,
  `net10.0-android`, …), de un RID, o de `StridePlatform` explícito;
- el sistema de ventanas, explícito o con un valor por defecto adecuado;
- la API gráfica, explícita o con un valor por defecto adecuado.

## Estructura

```text
platform-packages/
  Directory.Build.props        defaults del experimento y feed local
  Directory.Build.targets      vacío: aísla del Directory.Build.targets de slim-sdk
  nuget.config                 feed local + source mapping
  Stride.PlatformPackages.slnx solución con los proyectos de src/
  test.ps1                     empaquetado, builds, ejecución y auditoría
  src/                         paquetes y meta-paquetes
  consumers/                   aplicaciones y bibliotecas de prueba
```

### Paquetes por eje

No hay paquetes combinados salvo una composición pairwise explícita.

| Eje | Paquetes |
| --- | --- |
| Tipos compartidos de plataforma | `Stride.Platform.Common` |
| Sistema operativo | `Stride.Platform.Windows`, `Stride.Platform.Linux` |
| Ventanas | `Stride.Platform.Windows.Win32`, `Stride.Platform.Windows.WinUI`, `Stride.Platform.Linux.X11` |
| Meta de plataforma | `Stride.Platform` |
| I/O | `Stride.Core.IO.Common`, `Stride.Core.IO.Windows`, `Stride.Core.IO.Linux`, `Stride.Core.IO` |
| Gráficos | `Stride.Graphics.Common`, `Stride.Graphics.Direct3D11`, `Stride.Graphics.Vulkan`, `Stride.Graphics.Direct3D11.WinUI`, `Stride.Graphics` |
| Aplicación | `Stride.AppModel`, `Stride.Games` |

`Stride.Graphics.Direct3D11.WinUI` es la única composición pairwise: el
`SwapChainPanel` de WinUI necesita preparación específica de Direct3D 11 que un
`SurfaceDescriptor` genérico no puede expresar.

## Modelo de resolución

La resolución vive en `src/Stride.Platform/build/Stride.Platform.targets`.

1. **Plataforma desde el TFM.** `TargetPlatformIdentifier` decide
   `Windows`, `macOS`, `Android` o `iOS`.
2. **Plataforma desde el RID.** Si no hay TFM de plataforma, `RuntimeIdentifier`
   decide `win*`, `linux*`, `osx*`, `android*` o `ios*`.
3. **Fallback de host.** Solo para `OutputType` `Exe`/`WinExe`: una aplicación
   `net10.0` sin plataforma toma la plataforma del host. Una **biblioteca**
   `net10.0` permanece portable.
4. **Valor explícito.** `StridePlatform` gana, pero no puede contradecir al TFM.
5. **Windowing.** `StrideWindowSystem` explícito; si falta, `Windows → Win32` y
   `Linux → X11`.
6. **API gráfica.** `StrideGraphicsApi` explícito; si falta,
   `Windows → Direct3D11` y `Linux → Vulkan`.

`net10.0-linux` no es un TFM válido (`NETSDK1139`). Linux se representa con
`net10.0` más `StridePlatform=Linux` o un RID `linux-x64`.

Para una biblioteca portable solo son visibles los tipos de
`Stride.Platform.Common`; no se define ninguna constante de plataforma y los
paquetes específicos se podan.

## Constantes

`Stride.Platform` es el único dueño de las constantes de plataforma y ventanas:

| Constante | Cuándo |
| --- | --- |
| `STRIDE_PLATFORM_WINDOWS` | plataforma Windows |
| `STRIDE_PLATFORM_LINUX` | plataforma Linux |
| `STRIDE_PLATFORM_MACOS` | plataforma macOS |
| `STRIDE_PLATFORM_ANDROID` | plataforma Android |
| `STRIDE_PLATFORM_IOS` | plataforma iOS |
| `STRIDE_PLATFORM_PORTABLE` | sin plataforma resuelta |
| `STRIDE_WINDOWSYSTEM_WIN32` | windowing Win32 |
| `STRIDE_WINDOWSYSTEM_WINUI` | windowing WinUI |
| `STRIDE_WINDOWSYSTEM_X11` | windowing X11 |
| `STRIDE_WINDOWSYSTEM_WAYLAND` | windowing Wayland |

Los RID no generan constantes: `win-x64` no define `STRIDE_PLATFORM_WINDOWS` por
sí solo; la constante depende de la plataforma resuelta.

`Stride.Graphics` es el único dueño de las constantes gráficas:

| Constante | Cuándo |
| --- | --- |
| `STRIDE_GRAPHICS_API_DIRECT3D11` | API Direct3D 11 |
| `STRIDE_GRAPHICS_API_VULKAN` | API Vulkan |
| `STRIDE_GRAPHICS_FAMILY_DIRECT3D` | familia Direct3D |
| `STRIDE_GRAPHICS_FAMILY_VULKAN` | familia Vulkan |

## Grupos de dependencias por TFM

NuGet sí puede expresar el eje de sistema operativo y las composiciones mediante
grupos de dependencias distintos por TFM:

| Paquete | `net10.0` | `net10.0-windows` |
| --- | --- | --- |
| `Stride.Platform` | Common, Windows, Win32, Linux, X11 | Common, Windows, Win32, WinUI |
| `Stride.Core.IO` | Common, Windows, Linux, Platform | Common, Windows, Platform |
| `Stride.Graphics` | Common, D3D11, Vulkan, Platform | Common, D3D11, Vulkan, D3D11.WinUI, Platform |

El podado se usa solo para el eje que NuGet no puede expresar: el **windowing**.
Los grupos por TFM resuelven el sistema operativo y deciden si una composición
está disponible.

## Podado

`Stride.Platform` poda ensamblados, recursos y activos nativos de los paquetes de
plataforma que no coinciden con la resolución. `Stride.Graphics` poda variantes
de API y composiciones con los mismos valores.

Una **referencia directa** a un paquete específico actúa como pin: si no coincide
con la plataforma, el windowing o la API resueltos, el build falla con un error
explícito. Es una vía de escape deliberada, no la forma recomendada de consumir
el experimento.

## Ejemplos de resolución

La plataforma, el windowing y la API se resuelven antes de validar las
referencias directas. Un paquete de eje no cambia esas propiedades: solo aporta
metadatos y assets. Por eso una referencia directa funciona como **pin**, no
como selector.

### Pin gráfico incompatible

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Graphics" Version="..." />
  <PackageReference Include="Stride.Graphics.Vulkan" Version="..." />
</ItemGroup>
```

| Valor | Resolución |
| --- | --- |
| Plataforma | `Windows` (TFM) |
| Windowing | `Win32` (default) |
| API | `Direct3D11` (default) |

`Stride.Graphics.Vulkan` declara `GraphicsApi=Vulkan`, pero no modifica
`StrideGraphicsApi`. La validación compara el pin con la API resuelta y falla:

> Direct graphics reference(s) incompatible with api 'Direct3D11' ... Reference
> Stride.Graphics instead, or fix the selection.

No significa que Vulkan no exista en Windows, sino que el pin no coincide con
`Direct3D11`. Añadir `<StrideGraphicsApi>Vulkan</StrideGraphicsApi>` haría
compatible el pin, pero la forma recomendada es consumir solo el meta-paquete.

### Pin de windowing incompatible

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
  <PackageReference Include="Stride.Platform.Windows.WinUI" Version="..." />
</ItemGroup>
```

| Valor | Resolución |
| --- | --- |
| Plataforma | `Windows` (TFM) |
| Windowing | `Win32` (default) |

`Stride.Platform.Windows.WinUI` declara `Platforms=Windows` y
`WindowSystems=WinUI`, pero no modifica `StrideWindowSystem`. La validación
detecta que el pin no coincide con `Win32` y falla:

> Direct reference(s) incompatible with the resolved platform 'Windows' /
> windowing 'Win32' ... Reference the meta-packages instead.

WinUI sí está disponible en `net10.0-windows`; el problema es únicamente el pin
directo contra el valor por defecto `Win32`. Con
`<StrideWindowSystem>WinUI</StrideWindowSystem>` el pin sería compatible, pero
la vía recomendada es no referenciar el paquete de eje directamente.

### `net10.0` con WinUI explícito

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <StridePlatform>Windows</StridePlatform>
  <StrideWindowSystem>WinUI</StrideWindowSystem>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Platform" Version="..." />
</ItemGroup>
```

| Valor | Resolución |
| --- | --- |
| Plataforma | `Windows` (explícita) |
| Windowing | `WinUI` (explícito) |
| Grupo de dependencias | `net10.0`, sin WinUI |

Aquí no hay contradicción entre `StridePlatform=Windows` y `net10.0`: ese TFM
puede representar Windows. El problema es que el grupo `net10.0` de
`Stride.Platform` solo restaura Common, Windows, Win32, Linux y X11; WinUI solo
está en el grupo `net10.0-windows`. Al no existir un componente Windows/WinUI
restaurado, la validación falla y sugiere apuntar a `net10.0-windows`:

> StrideWindowSystem='WinUI' is not available on platform 'Windows' with the
> restored packages. ... If you need WinUI, target net10.0-windows.

### Uso correcto: WinUI en Windows

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <StrideWindowSystem>WinUI</StrideWindowSystem>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Games" Version="..." />
  <PackageReference Include="Stride.Graphics" Version="..." />
</ItemGroup>
```

Resuelve `Windows` / `WinUI` / `Direct3D11`. El meta-paquete restaura WinUI y la
composición pairwise `Stride.Graphics.Direct3D11.WinUI`, y poda Win32.

### Uso correcto: Vulkan en Windows

```xml
<PropertyGroup>
  <TargetFramework>net10.0-windows</TargetFramework>
  <StrideGraphicsApi>Vulkan</StrideGraphicsApi>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="Stride.Games" Version="..." />
  <PackageReference Include="Stride.Graphics" Version="..." />
</ItemGroup>
```

Resuelve `Windows` / `Win32` / `Vulkan`. `Stride.Graphics` conserva la variante
Vulkan y poda Direct3D11 y la composición de WinUI.

La regla práctica es elegir la plataforma, el windowing y la API con propiedades
(`StridePlatform`, `StrideWindowSystem`, `StrideGraphicsApi`) y dejar que los
meta-paquetes restauren y poden los paquetes de eje. Las referencias directas
solo son una vía de escape para forzar un paquete concreto.

## `Stride.AppModel` y `Stride.Games`

Ambos son neutrales: apuntan a `net10.0`, no mencionan `PlatformHost` y no
referencian ninguna variante gráfica.

- `Stride.AppModel` define el ciclo de vida (`Initialize`, `LoadContent`,
  `Update`, `Draw`, `UnloadContent`, `Run`).
- `Stride.Games` añade el bucle de juego y un `Action<int>? SubmitCommands` que
  la aplicación conecta al dispositivo gráfico que ella misma posee.

La aplicación crea el `PlatformHost` concreto en tiempo de compilación y le pasa
un `PlatformServices` (delegados: crear superficie y ejecutar el bucle). No hay
reflexión, interfaces ni despacho virtual entre la biblioteca y la plataforma.

## Matriz de consumidores

| Consumidor | Escenario | Qué valida |
| --- | --- | --- |
| `GameApp` | `net10.0-windows`, sin overrides | Windows/Win32/Direct3D11 por defecto, `Core.IO.Windows` |
| `GameWinUIApp` | `net10.0-windows`, `StrideWindowSystem=WinUI` | WinUI explícito, poda de Win32 y composición `Direct3D11.WinUI` |
| `GameLinuxApp` | `net10.0`, `StridePlatform=Linux`, `StrideWindowSystem=X11`, Vulkan, `linux-x64` | Linux/X11/Vulkan, `Core.IO.Linux`, activos `.so` |
| `PortableGameLib` | biblioteca `net10.0` | `STRIDE_PLATFORM_PORTABLE`, sin variantes ni plataforma |
| `MultiTargetProbe` | `net10.0;net10.0-windows` | `net10.0` portable y `net10.0-windows` Windows/Win32 |
| `invalid/InvalidLinuxD3D11` | Linux + Direct3D11 | error de API no disponible |
| `invalid/InvalidWindowsX11` | Windows + X11 | error de windowing no disponible |
| `invalid/InvalidWinUIOnNet10` | `net10.0` + WinUI | error con sugerencia de usar `net10.0-windows` |
| `invalid/InvalidDirectVariant` | Win32 resuelto + `Stride.Platform.Windows.WinUI` directo | error de pin incompatible |
| `invalid/InvalidDirectGraphics` | Direct3D11 resuelto + `Stride.Graphics.Vulkan` directo | error de pin gráfico incompatible |

Los casos inválidos se compilan esperando un fallo y comprobando el mensaje.

## Cómo ejecutar

```powershell
pwsh ./test.ps1
```

El script:

1. desactiva el servidor de MSBuild y la reutilización de nodos (una evaluación
   persistente puede conservar en memoria un `targets` recién empaquetado);
2. limpia `artifacts/packages` y `artifacts/build`;
3. empaqueta en fases: bases → `Stride.Platform` → resto de metas;
4. inspecciona los `.nupkg` (placeholders, `buildTransitive`, grupos por TFM,
   ausencia de `exclude` en las dependencias de `AppModel`/`Games`);
5. compila y ejecuta los consumidores válidos;
6. comprueba ensamblados presentes, ensamblados podados y activos nativos;
7. compila los casos inválidos y verifica sus diagnósticos.

Opciones:

- `-SkipPack`: reutiliza los paquetes más recientes de `artifacts/packages`.
- `-KeepArtifacts`: no borra `artifacts/build`.
- `-Configuration Debug|Release`.

Resultado esperado: `110/110 checks passed`.

## Empaquetado

Los meta-paquetes no llevan ensamblado propio: publican `lib/<tfm>/_._` y sus
`build`/`buildTransitive` targets.

El empaquetado usa `-p:StridePlatformPacking=true`, que cambia las
`ProjectReference` por `PackageReference` con `PrivateAssets="none"`. Así el
nuspec conserva todos los assets de los paquetes referenciados, incluidos los
targets transitivos, en lugar de escribir dependencias con
`exclude="Build,Analyzers"`.

Las rutas de los placeholders están normalizadas por NuGet
(`lib/net10.0-windows7.0/_._`); no se condicionan por `$(TargetFramework)` dentro
del `ItemGroup`, porque la evaluación exterior del proyecto multi-TFM no los ve.

## Limitaciones

- Todo es simulado: no hay WinUI, X11 ni Vulkan reales. Los hosts y dispositivos
  solo demuestran la selección de ensamblados, activos y constantes.
- macOS, Android, iOS y Wayland están reconocidos en la resolución, pero no
  tienen paquetes en este experimento.
- Solo existen las variantes gráficas Direct3D 11 y Vulkan.
- `Stride.Games` no referencia ninguna variante gráfica: el dispositivo lo posee
  la aplicación. Referenciar una variante desde la biblioteca rompería a los
  consumidores que eligen otra API.
