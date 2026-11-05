# Stride.Graphics como meta-paquete (experimento)

Prototipo aislado para validar un rediseño del build de gráficos de Stride: en
lugar de compilar `Stride.Graphics` una vez por API gráfica dentro de un único
paquete multi-API, publicar un **meta-paquete** `Stride.Graphics` que elige en
**tiempo de compilación** entre `Stride.Graphics.Direct3D11`,
`Stride.Graphics.Direct3D12` y `Stride.Graphics.Vulkan`.

Además de la variante, el meta-paquete incluye **paquetes condicionales por
familia**: PIX y WARP son comunes a todo Direct3D, mientras que Lavapipe solo
tiene sentido en Vulkan. La misma selección de compilación decide qué paquetes
condicionales sobreviven y cuáles se podan.

La restricción principal del diseño es el camino caliente del `main loop`: no hay
interfaces de backend, ni clases base abstractas, ni `virtual`, ni
`Assembly.Load`/reflexión. El consumidor compila contra **una sola variante** y las
llamadas a `GraphicsDevice` se enlazan directamente a su tipo concreto.

Todo el experimento vive en `slim-sdk/experiments/meta-packages/`. No se ha
modificado nada fuera de esta carpeta y no se importa `Stride.Build.Sdk`: los
proyectos usan `Microsoft.NET.Sdk` puro, así que el prototipo se construye en
segundos y sirve para validar comportamiento de MSBuild/NuGet, no para renderizar.

Requisitos: .NET SDK 10 y PowerShell 7 (`pwsh`).

## El contrato que se quería validar

Un consumidor escribe solo esto:

```xml
<ItemGroup>
  <!-- En el experimento la versión la inyecta test.ps1 con -p:StrideMetaVersion=... -->
  <PackageReference Include="Stride.Graphics" Version="$(StrideMetaVersion)" />
</ItemGroup>
```

y obtiene:

- los tipos compartidos de `Stride.Graphics.Common` (`GraphicsApi`, `PixelFormat`,
  `GraphicsProfile`, `Color`, `GraphicsDeviceDescription`, `GraphicsApiInfo`);
- **una** variante compilada: `Stride.Graphics.Direct3D11`, `.Direct3D12` o
  `.Vulkan`;
- constantes de compilación para código condicional:
  `STRIDE_GRAPHICS_API_<API>`, `STRIDE_GRAPHICS_FAMILY_<FAMILIA>` y
  `STRIDE_PLATFORM_<PLATAFORMA>`;
- el tipo concreto `Stride.Graphics.GraphicsDevice` de esa variante, con la misma
  superficie pública en las tres;
- los paquetes condicionales que aplican a esa API: PIX y WARP en Direct3D,
  Lavapipe en Vulkan;
- **solo** el ensamblado y los assets nativos de la variante y de los paquetes
  condicionales elegidos en la salida.

La selección se puede forzar por proyecto o por línea de comandos:

```xml
<PropertyGroup>
  <StrideGraphicsApi>Vulkan</StrideGraphicsApi>
</PropertyGroup>
```

```powershell
dotnet build -p:StrideGraphicsApi=Direct3D12
```

## Por qué no hay despacho en runtime

`Stride.Graphics.Common` contiene **solo** tipos compartidos y sin jerarquía:

- enums (`GraphicsApi`, `PixelFormat`, `GraphicsProfile`);
- structs de valor (`Color`, `GraphicsDeviceDescription`);
- helpers estáticos (`GraphicsApiInfo`).

No define `GraphicsDevice`, ni `IGraphicsBackend`, ni ningún tipo abstracto. El
test estructural comprueba que todos sus tipos son sellados y que no declara
métodos virtuales sobrescribibles.

Cada variante declara su propio `Stride.Graphics.GraphicsDevice`, `sealed`, sin
clase base (salvo `object`), que solo implementa `IDisposable`. El método del
camino caliente es deliberadamente no virtual:

```csharp
public sealed class GraphicsDevice : IDisposable
{
    // ...
    public void Submit(int commandCount) => submittedCommands += commandCount;
}
```

El consumidor compila contra exactamente una variante, así que el compilador ve un
único tipo `GraphicsDevice` y puede aplicar inlining y devirtualización normales.
No hay ningún salto indirecto añadido por el meta-paquete.

## Estructura

```
experiments/meta-packages/
├─ Directory.Build.props          # defaults + feed/caché locales en artifacts/
├─ Directory.Build.targets        # vacío a propósito: bloquea el skeleton de slim-sdk
├─ nuget.config                   # feed local artifacts/packages + package source mapping
├─ Stride.MetaPackages.slnx       # proyectos empaquetables (sin consumidores)
├─ test.ps1                       # pack + matriz de consumidores + comprobaciones
├─ src/
│  ├─ Stride.Graphics.Common/              # tipos compartidos, sin GraphicsDevice
│  ├─ Stride.MetaExperiment.Native.*/      # nativos simulados (runtimes/<rid>/native)
│  ├─ Stride.MetaExperiment.Pix/           # PIX: condicional, familia Direct3D
│  ├─ Stride.MetaExperiment.Warp/          # WARP: condicional, familia Direct3D
│  ├─ Stride.MetaExperiment.Lavapipe/      # Lavapipe: condicional, Vulkan
│  ├─ Stride.Graphics.Direct3D11|12|Vulkan # una variante por API
│  └─ Stride.Graphics/                     # meta-paquete sin ensamblado propio
└─ consumers/
   ├─ ConsumerApp/     # solo PackageReference Stride.Graphics
   ├─ VulkanApp/       # override en el cuerpo del .csproj
   ├─ MultiApiHostApp/ # StrideMultiGraphicsApiHost=true: despliega las tres variantes
   ├─ Library/         # biblioteca que referencia el meta-paquete
   ├─ LibraryApp/      # app que solo referencia la biblioteca (flujo transitivo)
   ├─ VariantApp/      # referencia directa a una variante (escape hatch)
   └─ WrongApiApp/     # usa un tipo de una variante podada: debe fallar al compilar
```

Cada paquete variante tiene un nombre de ensamblado distinto
(`Stride.Graphics.Direct3D11.dll`, etc.). Esa decisión permite que un host multi-API
mantenga las tres variantes planas en el mismo directorio, sin subcarpetas por
identidad como hace hoy el paquete multi-DLL de Stride.

## Cómo funciona la selección

### 1. Defaults (props, antes del cuerpo del proyecto)

`src/Stride.Graphics/build/Stride.Graphics.props` importa
`Stride.Graphics.Defaults.props`, que:

- deriva `StridePlatform` de `RuntimeIdentifier`, o del SO anfitrión si no hay RID;
- aplica la tabla de defaults de Stride (`Windows`/`UWP` → `Direct3D11`, resto → `Vulkan`);
- resuelve `StrideGraphicsApi` con el orden: valor explícito → primer `StrideGraphicsApis` → default.

El cuerpo del `.csproj` todavía puede sobrescribir `StrideGraphicsApi` porque su
asignación es posterior. Lo que **no** puede cambiar a tiempo es el
`RuntimeIdentifier` que leen los defaults: hay que pasarlo por línea de comandos o
en `Directory.Build.props` (igual que en el Stride actual).

### 2. Constantes de compilación

`Stride.Graphics.Selection.targets` añade a `DefineConstants`:

| Selección | Constantes |
| --- | --- |
| `Direct3D11` en Windows | `STRIDE_GRAPHICS_API_DIRECT3D11`, `STRIDE_GRAPHICS_FAMILY_DIRECT3D`, `STRIDE_PLATFORM_WINDOWS` |
| `Direct3D12` en Windows | `STRIDE_GRAPHICS_API_DIRECT3D12`, `STRIDE_GRAPHICS_FAMILY_DIRECT3D`, `STRIDE_PLATFORM_WINDOWS` |
| `Vulkan` en Windows | `STRIDE_GRAPHICS_API_VULKAN`, `STRIDE_GRAPHICS_FAMILY_VULKAN`, `STRIDE_PLATFORM_WINDOWS` |
| `Vulkan` en Linux | `STRIDE_GRAPHICS_API_VULKAN`, `STRIDE_GRAPHICS_FAMILY_VULKAN`, `STRIDE_PLATFORM_LINUX` |

Las constantes de familia permiten escribir código que comparte dependencias sin
duplicar el `#if` por API:

```csharp
#if STRIDE_GRAPHICS_FAMILY_DIRECT3D
PixMarkers.BeginEvent("frame");
Console.WriteLine(WarpDevice.AdapterName);
#elif STRIDE_GRAPHICS_FAMILY_VULKAN
Console.WriteLine(LavapipeDevice.GetLibraryFileName());
#endif
```

No se escribe nada en `runtimeconfig.json`: la variante ya está decidida en
compilación y no hay selector que consultar al arrancar.

### 3. Podado de assets

NuGet restaura de forma estática: el meta-paquete declara los tres paquetes
variante y los paquetes condicionales como dependencias, así que **siempre** se
restauran todos. El build poda lo que no corresponde en
`_StrideMetaFilterGraphicsAssets`, enganchado a
`AfterTargets="ResolvePackageAssets"` (antes de que esos items se conviertan en
`Reference` y `ReferenceCopyLocalPaths`):

| Item | Qué elimina | ¿Cuándo? |
| --- | --- | --- |
| `ResolvedCompileFileDefinitions` | referencias de compilación de variantes y paquetes condicionales no elegidos | **siempre** |
| `RuntimeCopyLocalItems` | ensamblados gestionados | salvo host multi-API |
| `NativeCopyLocalItems` / `RuntimeTargetsCopyLocalItems` | nativos por RID | salvo host multi-API |
| `ResourceCopyLocalItems` | recursos satélite | salvo host multi-API |
| `ReferenceCopyLocalPaths` | red de seguridad para el copiado | salvo host multi-API |

El podado de compilación es incondicional porque las tres variantes exponen el
mismo tipo `Stride.Graphics.GraphicsDevice`: dos variantes en el grafo de
compilación serían un `CS0433` inmediato. Incluso un host multi-API compila contra
una sola variante.

Cada paquete declara a qué APIs aplica con una lista:

```xml
<StrideMetaGraphicsApiSpecificPackage Include="Stride.MetaExperiment.Pix"
                                      Apis="Direct3D11;Direct3D12" />
<StrideMetaGraphicsApiSpecificPackage Include="Stride.MetaExperiment.Lavapipe"
                                      Apis="Vulkan" />
```

El meta-paquete solo comprueba si `$(StrideGraphicsApi)` aparece en `Apis`; no
conoce nombres de paquetes ni de plataformas. Eso permite que PIX y WARP sean
comunes a todo Direct3D sin duplicar lógica en D3D11 y D3D12, y que Lavapipe
desaparezca de cualquier build Direct3D.

El criterio es `%(NuGetPackageId)` contra la lista de paquetes marcados como
exclusivos de otra API, con dos salvaguardas:

- un paquete referenciado **directamente** por el consumidor nunca se poda
  (escape hatch: referenciar `Stride.Graphics.Vulkan` a mano);
- si falta el metadato `NuGetPackageId`, se compara el nombre del ensamblado.

### 4. Contrato de las variantes

Cada paquete variante publica sus `build/` y `buildTransitive/` con
`Stride.Graphics.<Api>.targets`, que declara:

```xml
<StrideMetaGraphicsVariant Include="Stride.Graphics.Vulkan"
                           GraphicsApi="Vulkan"
                           Platforms="Windows;Linux;macOS;Android;iOS"
                           AssemblyName="Stride.Graphics.Vulkan.dll" />

<StrideMetaGraphicsApiSpecificPackage Include="Stride.Graphics.Vulkan" Apis="Vulkan" />
<StrideMetaGraphicsApiSpecificPackage Include="Stride.MetaExperiment.Native.Vulkan" Apis="Vulkan" />
```

El meta-paquete solo declara las dependencias y filtra por `Apis`: no conoce
plataformas ni nombres de ensamblado. Añadir una variante nueva (p. ej. Metal)
consiste en publicar su paquete con esos items, añadirlo como dependencia del
meta-paquete y darle su propia implementación de `GraphicsDevice` con la misma
superficie pública.

Los paquetes condicionales siguen el mismo contrato: publican su propio
`buildTransitive/<Paquete>.targets` con la lista `Apis` que les corresponde. No
necesitan saber si se usan desde D3D11 o D3D12; basta con que la lista incluya
ambos.

### 5. Paquetes condicionales por familia

Tres paquetes del experimento demuestran el mecanismo:

| Paquete | `Apis` | Contenido |
| --- | --- | --- |
| `Stride.MetaExperiment.Pix` | `Direct3D11;Direct3D12` | Marcadores PIX + `WinPixEventRuntime.dll` |
| `Stride.MetaExperiment.Warp` | `Direct3D11;Direct3D12` | Bindings WARP + `d3d10warp.dll` |
| `Stride.MetaExperiment.Lavapipe` | `Vulkan` | Bindings Lavapipe + `vulkan_lvp.dll` / `libvulkan_lvp.so` |

El meta-paquete los declara como dependencias (con `PrivateAssets="none"` al
empaquetar) y el podado decide cuáles sobreviven. El consumidor puede usar las
constantes de familia para escribir código que solo compile con los paquetes
disponibles; si intenta usar un tipo de la familia contraria, el build falla
porque el ensamblado no está en el grafo de compilación.

En un host multi-API se despliegan los tres paquetes condicionales, igual que las
tres variantes, aunque el código siga compilando contra una sola familia.

### 6. Host multi-API

`StrideMultiGraphicsApiHost=true` desactiva el podado de runtime y nativos, de modo
que las tres variantes, los tres paquetes condicionales y sus nativos quedan
desplegados. La compilación sigue eligiendo una sola variante y una sola familia.

Cambiar de API **en runtime** queda fuera del prototipo: requeriría carga dinámica
(`Assembly.Load`/reflexión), que es justo lo que este diseño evita. Un host real
tendría que arrancar el proceso con la variante ya compilada, o asumir
explícitamente el coste de un mecanismo dinámico separado del camino caliente.

## Empaquetado: dos fases y `PrivateAssets="none"`

Éste es el hallazgo más importante del prototipo.

1. **Las dependencias de un meta-paquete no arrastran `build`/`buildTransitive`
   por defecto.** `PackageReference` tiene `PrivateAssets` por defecto igual a
   `contentfiles;analyzers;build`, así que el `.nuspec` sale con
   `exclude="Build,Analyzers"` y los targets de las variantes **no** se importan
   en el consumidor. La solución es declarar las dependencias del meta-paquete con
   `PrivateAssets="none"`, que produce `include="All"`.
2. **`ProjectReference` siempre se empaqueta con `exclude="Build,Analyzers"`** y
   no hay metadato para evitarlo. Por eso el meta-proyecto usa `ProjectReference`
   para compilar normalmente, pero al empaquetar cambia a `PackageReference`
   (`-p:StrideMetaPacking=true`). El empaquetado es en dos fases: primero las
   variantes, después el meta-paquete. `test.ps1` lo hace en ese orden.

Además, el meta-paquete incluye `lib/net10.0/_._` y `IncludeBuildOutput=false`:
no publica ensamblado propio, pero sigue siendo un paquete compatible.

## Matriz de pruebas

`test.ps1` empaqueta todo, construye los consumidores, ejecuta las apps e inspecciona
los ensamblados resultantes. Estas son las comprobaciones (171 en total, todas en
verde):

| Escenario | Qué demuestra |
| --- | --- |
| `ConsumerApp` en `win-x64` | un único `PackageReference` basta; define de API, familia y plataforma, variante Direct3D11, PIX y WARP presentes, Lavapipe podado y `Submit` no virtual |
| `-p:StrideGraphicsApi=Direct3D12` | override por línea de comandos; se poda Direct3D11 y Vulkan, pero PIX y WARP siguen presentes |
| `VulkanApp` con `<StrideGraphicsApi>Vulkan</StrideGraphicsApi>` | override en el cuerpo del proyecto; Lavapipe presente y PIX/WARP podados |
| `-r linux-x64` sin API explícita | default por plataforma (Vulkan), `libstride_vulkan.so` y `libvulkan_lvp.so` |
| `MultiApiHostApp` | `StrideMultiGraphicsApiHost=true` despliega las tres variantes y los tres paquetes condicionales, pero compila contra Direct3D11 |
| `LibraryApp` → `Library` | la selección, los defines y el podado fluyen de forma transitiva por `buildTransitive`; la biblioteca usa PIX |
| `VariantApp` | referencia directa a `Stride.Graphics.Direct3D11`, sin meta-paquete ni paquetes condicionales |
| `-p:StrideGraphicsApi=Metal` | error claro: la API no la provee ninguna variante |
| `-r linux-x64 -p:StrideGraphicsApi=Direct3D12` | error claro: API no soportada en la plataforma |
| `WrongApiApp` + `-p:StrideGraphicsApi=Vulkan` | usar un tipo de una variante podada falla con `CS0234` |
| Inspección de `.nupkg` | `lib/net10.0/_._`, `buildTransitive/*.targets`, nativos por RID y dependencias con `include="All"` |
| Reflexión **solo en el test** | `Common` no define `GraphicsDevice` ni `IGraphicsBackend`, no tiene clases abstractas ni virtuales sobrescribibles; las tres variantes exponen la misma superficie pública, `sealed`, sin clase base y sin `Submit` virtual |

La reflexión aparece únicamente en `test.ps1` para auditar los ensamblados. No hay
reflexión en el código que consume el meta-paquete ni en el camino caliente.

Ejecución:

```powershell
cd slim-sdk/experiments/meta-packages
pwsh ./test.ps1              # empaqueta y prueba todo
pwsh ./test.ps1 -SkipPack    # reutiliza el último paquete del feed local
pwsh ./test.ps1 -Clean       # borra artifacts/ y empieza de cero
```

Todo lo que escribe el experimento queda en `artifacts/` (feed, caché de NuGet y
salidas de build), que se puede borrar en cualquier momento.

## Qué implica para el rediseño real

- **La restauración estática no es un obstáculo, pero el podado es obligatorio.**
  Un meta-paquete siempre restaura las tres variantes y los paquetes condicionales;
  el trabajo real es filtrar compilación y copiado, que es exactamente lo que ya
  hace `Stride.GraphicsApiSpecificPackages.targets` para los paquetes de interop.
- **Las dependencias compartidas por una familia necesitan una lista de APIs.**
  PIX y WARP no son "de D3D11" ni "de D3D12": son de Direct3D. Modelarlos con
  `Apis="Direct3D11;Direct3D12"` evita duplicar la dependencia y deja que el
  meta-paquete decida con una sola comprobación.
- **Las variantes necesitan identidades distintas** para poder convivir planas en
  el output. Eso simplifica mucho `Stride.MultiGraphicsApiHost.targets` frente al
  esquema actual de subcarpetas por API.
- **El flujo de `buildTransitive` depende de `PrivateAssets`.** Cualquier paquete
  de Stride que dependa del meta-paquete y espere que sus targets lleguen al
  consumidor final (por ejemplo `Stride.Assets` añadiendo Vulkan a la lista de
  paquetes que sobreviven) tiene que declararlo con `PrivateAssets="none"` o
  equivalentemente no excluir `build`.
- **El empaquetado tiene que ser en orden de dependencias.** Un meta-paquete no
  puede empaquetarse en la misma pasada que sus variantes si quiere dependencias
  con `include="All"`.
- **El código común separado (`Stride.Graphics.Common`) es lo que hace posible el
  contrato.** Sin él, cada variante expondría tipos distintos y el consumidor
  tendría que elegir ensamblado en compilación.
- **El tipo concreto compartido es la clave para el rendimiento.** Las tres
  variantes deben mantener exactamente la misma superficie pública de
  `GraphicsDevice`; el test lo verifica comparando los miembros declarados.

## Limitaciones y siguientes pasos

- **Prototipo simulado**: los backends y los paquetes condicionales solo comprueban
  que su nativo esté copiado; no hay interop real ni shaders. El objetivo era
  MSBuild/NuGet y el contrato de tipos.
- **El host multi-API no cambia de API en runtime**: despliega las tres variantes y
  los tres paquetes condicionales, pero la app compilada usa la variante y la
  familia seleccionadas. El cambio dinámico se dejó fuera a propósito.
- **La referencia directa a una variante no arrastra los condicionales**: el escape
  hatch `PackageReference Stride.Graphics.Direct3D11` evita el meta-paquete y, por
  tanto, también PIX/WARP. Si se necesitan, hay que referenciarlos explícitamente o
  usar el meta-paquete.
- **Los paquetes condicionales son dependencias del meta-paquete, no de la
  variante**: eso mantiene una sola declaración para PIX/WARP y evita duplicar la
  dependencia en D3D11 y D3D12. En un rediseño real, las herramientas puramente de
  test (Lavapipe/WARP) podrían activarse con una propiedad opt-in en lugar de
  restaurarse siempre.
- **El `RuntimeIdentifier` debe venir de fuera del cuerpo del `.csproj`** para que
  los defaults lo vean a tiempo. Si se necesita soportar `<RuntimeIdentifier>`
  dentro del proyecto, la detección de plataforma tendría que moverse a un target
  temprano y repetirse en `DefineConstants`.
- **Un build sin RID no copia nativos por RID.** Los consumidores de prueba usan
  `-r win-x64`/`-r linux-x64`, igual que un proyecto Stride real.
- **Propagación entre proyectos**: `-p:StrideGraphicsApi=...` se propaga a las
  referencias de proyecto porque es una propiedad global. Un proyecto que fije la
  API en su cuerpo no la propaga; en Stride real esto se resuelve con la propiedad
  global del SDK o con un `.props` compartido.
- **Diseño/IDE**: no se ha probado la experiencia en Visual Studio/Rider ni
  design-time builds; el filtrado depende de `ResolvePackageAssets`, que también
  corre en design-time, pero conviene validarlo.
- **Siguiente paso natural**: repetir el ejercicio con los paquetes reales de
  interop (`Silk.NET.*`, `Vortice.Vulkan`, `WinPixEventRuntime`) y medir cuánto se
  reduce `Stride.GraphicsApi.InnerBuild.targets` y
  `Stride.MultiGraphicsApiHost.targets`.
