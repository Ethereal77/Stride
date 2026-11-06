#Requires -Version 7.0
<#
.SYNOPSIS
    Packs the platform meta-package experiment and runs the consumer matrix.

.DESCRIPTION
    Everything the script writes stays inside experiments/platform-packages:
      artifacts/packages     local NuGet feed
      artifacts/nuget-cache  local NuGet package cache
      artifacts/build        consumer build outputs

    The script prints one line per check and exits non-zero when any check fails.

    The MSBuild server and node reuse are disabled on purpose: a long-lived
    evaluation can keep an older copy of a just-packed targets file in memory.

.PARAMETER Configuration
    Build configuration, Release by default.

.PARAMETER SkipPack
    Reuse the newest packages already in artifacts/packages instead of packing.

.PARAMETER KeepArtifacts
    Keep the previous consumer build outputs instead of deleting artifacts/build.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$SkipPack,

    [switch]$KeepArtifacts
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$env:MSBUILDDISABLENODEREUSE = '1'
$env:DOTNET_CLI_USE_MSBUILD_SERVER = '0'

$root = $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$packagesDir = Join-Path $artifacts 'packages'
$buildDir = Join-Path $artifacts 'build'
$version = '0.1.0-platform.' + (Get-Date -Format 'yyyyMMddHHmmssfff')

$script:checks = 0
$script:failures = 0

function Write-Step([string]$Text) {
    Write-Host ''
    Write-Host "=== $Text" -ForegroundColor Cyan
}

function Assert-True([bool]$Condition, [string]$Message) {
    $script:checks++
    if ($Condition) {
        Write-Host "  [ok]   $Message" -ForegroundColor Green
    }
    else {
        $script:failures++
        Write-Host "  [FAIL] $Message" -ForegroundColor Red
    }
}

function Assert-Contains([string]$Text, [string]$Expected, [string]$Message) {
    $script:checks++
    if ($Text.Contains($Expected, [StringComparison]::Ordinal)) {
        Write-Host "  [ok]   $Message" -ForegroundColor Green
    }
    else {
        $script:failures++
        Write-Host "  [FAIL] $Message (expected '$Expected')" -ForegroundColor Red
    }
}

function Invoke-Dotnet {
    param([string[]]$Arguments)

    $lines = & dotnet @Arguments 2>&1 | ForEach-Object { "$_" -replace "`r", "" }
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output   = ($lines -join "`n")
    }
}

function Invoke-Pack {
    param([string]$Project, [string[]]$ExtraProperties = @())

    $arguments = @(
        'pack', (Join-Path $root $Project)
        '-c', $Configuration
        '-nologo'
        '-v:minimal'
        "-p:PackageVersion=$version"
        '-nodeReuse:false'
    ) + $ExtraProperties

    $result = Invoke-Dotnet -Arguments $arguments
    if ($result.ExitCode -ne 0) {
        Write-Host $result.Output
        throw "Pack failed: $Project"
    }
}

function Invoke-Build {
    param(
        [string]$Project,
        [string]$Scenario,
        [string]$RuntimeIdentifier,
        [string[]]$ExtraProperties = @()
    )

    $outRoot = Join-Path $buildDir "$Scenario\"
    $arguments = @(
        'build', (Join-Path $root $Project)
        '-c', $Configuration
        '-nologo'
        '-v:minimal'
        '--no-incremental'
        "-p:StridePlatformVersion=$version"
        "-p:BaseOutputPath=$outRoot"
        '-nodeReuse:false'
    )

    if ($RuntimeIdentifier) {
        $arguments += @('-r', $RuntimeIdentifier)
    }

    $arguments += $ExtraProperties
    return Invoke-Dotnet -Arguments $arguments
}

function Get-OutputDirectory {
    param([string]$Scenario, [string]$TargetFramework, [string]$RuntimeIdentifier)

    $path = Join-Path $buildDir "$Scenario\$Configuration\$TargetFramework"
    if ($RuntimeIdentifier) {
        $path = Join-Path $path $RuntimeIdentifier
    }

    return $path
}

function Invoke-App {
    param(
        [string]$Scenario,
        [string]$TargetFramework,
        [string]$RuntimeIdentifier,
        [string]$AssemblyName
    )

    $dll = Join-Path (Get-OutputDirectory -Scenario $Scenario -TargetFramework $TargetFramework -RuntimeIdentifier $RuntimeIdentifier) "$AssemblyName.dll"
    if (-not (Test-Path $dll)) {
        throw "Missing build output: $dll"
    }

    return Invoke-Dotnet -Arguments @($dll)
}

function Get-PackageEntries {
    param([string]$PackageId)

    $path = Join-Path $packagesDir "$PackageId.$version.nupkg"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        return @($zip.Entries | ForEach-Object { $_.FullName })
    }
    finally {
        $zip.Dispose()
    }
}

function Get-PackageNuspecXml {
    param([string]$PackageId)

    $path = Join-Path $packagesDir "$PackageId.$version.nupkg"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try {
            return [xml]$reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $zip.Dispose()
    }
}

function Get-DependencyGroup {
    param([xml]$Nuspec, [string]$TargetFramework)

    return @($Nuspec.package.metadata.dependencies.group) |
        Where-Object { $_.targetFramework -eq $TargetFramework } |
        Select-Object -First 1
}

function Get-DependencyIds {
    param([xml]$Nuspec, [string]$TargetFramework)

    $group = Get-DependencyGroup -Nuspec $Nuspec -TargetFramework $TargetFramework
    if ($null -eq $group) {
        return @()
    }

    return @($group.dependency | ForEach-Object { $_.id })
}

function Assert-DependencyIds {
    param(
        [xml]$Nuspec,
        [string]$TargetFramework,
        [string[]]$Expected,
        [string[]]$Forbidden,
        [string]$Message
    )

    $ids = Get-DependencyIds -Nuspec $Nuspec -TargetFramework $TargetFramework
    $missing = @($Expected | Where-Object { -not ($ids -contains $_) })
    $unexpected = @($Forbidden | Where-Object { $ids -contains $_ })

    Assert-True ($missing.Count -eq 0 -and $unexpected.Count -eq 0) $Message
    if ($missing.Count -gt 0 -or $unexpected.Count -gt 0) {
        Write-Host "         group $TargetFramework = $($ids -join ', ')"
        if ($missing.Count -gt 0) { Write-Host "         missing: $($missing -join ', ')" }
        if ($unexpected.Count -gt 0) { Write-Host "         forbidden: $($unexpected -join ', ')" }
    }
}

function Assert-OutputContains {
    param(
        [pscustomobject]$Result,
        [string]$Expected,
        [string]$Message
    )

    $script:checks++
    if ($Result.Output.Contains($Expected, [StringComparison]::Ordinal)) {
        Write-Host "  [ok]   $Message" -ForegroundColor Green
    }
    else {
        $script:failures++
        Write-Host "  [FAIL] $Message (expected '$Expected')" -ForegroundColor Red
        Write-Host ($Result.Output -split "`n" | Select-Object -First 30 | ForEach-Object { "         $_" })
    }
}

function Test-Asset {
    param([string]$OutputDirectory, [string]$Name)

    $candidates = @(
        (Join-Path $OutputDirectory $Name)
        (Join-Path $OutputDirectory "runtimes\win-x64\native\$Name")
        (Join-Path $OutputDirectory "runtimes\linux-x64\native\$Name")
    )

    return @($candidates | Where-Object { Test-Path $_ }).Count -gt 0
}

function Get-AssemblyReferencesFromPath {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        throw "Missing assembly: $Path"
    }

    # LoadFile gives each assembly its own context, so both TFMs can be inspected.
    $assembly = [System.Reflection.Assembly]::LoadFile((Resolve-Path $Path).Path)
    return @($assembly.GetReferencedAssemblies() | ForEach-Object Name)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

if (-not $SkipPack) {
    Write-Step 'Cleaning previous packages and build outputs'
    & dotnet build-server shutdown | Out-Null
    if (Test-Path $packagesDir) { Remove-Item -Recurse -Force $packagesDir }
    if (-not $KeepArtifacts -and (Test-Path $buildDir)) { Remove-Item -Recurse -Force $buildDir }
    New-Item -ItemType Directory -Path $packagesDir -Force | Out-Null
}

if ($SkipPack) {
    $latest = Get-ChildItem $packagesDir -Filter 'Stride.Platform.0.1.0-platform.*.nupkg' -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if (-not $latest) {
        throw "SkipPack was set but $packagesDir has no Stride.Platform package. Run without -SkipPack first."
    }
    $version = $latest.BaseName.Substring('Stride.Platform.'.Length)
}

if (-not $SkipPack) {
    Write-Step "Packing base packages ($version)"
    $baseProjects = @(
        'src/Stride.Platform.Common/Stride.Platform.Common.csproj'
        'src/Stride.Platform.Windows/Stride.Platform.Windows.csproj'
        'src/Stride.Platform.Windows.Win32/Stride.Platform.Windows.Win32.csproj'
        'src/Stride.Platform.Windows.WinUI/Stride.Platform.Windows.WinUI.csproj'
        'src/Stride.Platform.Linux/Stride.Platform.Linux.csproj'
        'src/Stride.Platform.Linux.X11/Stride.Platform.Linux.X11.csproj'
        'src/Stride.Core.IO.Common/Stride.Core.IO.Common.csproj'
        'src/Stride.Core.IO.Windows/Stride.Core.IO.Windows.csproj'
        'src/Stride.Core.IO.Linux/Stride.Core.IO.Linux.csproj'
        'src/Stride.Graphics.Common/Stride.Graphics.Common.csproj'
        'src/Stride.Graphics.Direct3D11/Stride.Graphics.Direct3D11.csproj'
        'src/Stride.Graphics.Vulkan/Stride.Graphics.Vulkan.csproj'
        'src/Stride.Graphics.Direct3D11.WinUI/Stride.Graphics.Direct3D11.WinUI.csproj'
    )
    foreach ($project in $baseProjects) {
        Invoke-Pack -Project $project
        Write-Host "  packed $(Split-Path $project -Leaf)"
    }

    Write-Step 'Packing the meta-packages'
    # Phase 2: platform first; every other meta-package depends on it.
    Invoke-Pack -Project 'src/Stride.Platform/Stride.Platform.csproj' -ExtraProperties @('-p:StridePlatformPacking=true')
    Write-Host '  packed Stride.Platform.csproj'

    # Phase 3: the other metas consume Stride.Platform from the local feed.
    $metaProjects = @(
        'src/Stride.Core.IO/Stride.Core.IO.csproj'
        'src/Stride.Graphics/Stride.Graphics.csproj'
        'src/Stride.AppModel/Stride.AppModel.csproj'
        'src/Stride.Games/Stride.Games.csproj'
    )
    foreach ($project in $metaProjects) {
        Invoke-Pack -Project $project -ExtraProperties @('-p:StridePlatformPacking=true')
        Write-Host "  packed $(Split-Path $project -Leaf)"
    }

    $expectedPackages = @(
        'Stride.Platform.Common'
        'Stride.Platform.Windows'
        'Stride.Platform.Windows.Win32'
        'Stride.Platform.Windows.WinUI'
        'Stride.Platform.Linux'
        'Stride.Platform.Linux.X11'
        'Stride.Platform'
        'Stride.Core.IO.Common'
        'Stride.Core.IO.Windows'
        'Stride.Core.IO.Linux'
        'Stride.Core.IO'
        'Stride.Graphics.Common'
        'Stride.Graphics.Direct3D11'
        'Stride.Graphics.Vulkan'
        'Stride.Graphics.Direct3D11.WinUI'
        'Stride.Graphics'
        'Stride.AppModel'
        'Stride.Games'
    )
    $missingPackages = @($expectedPackages | Where-Object { -not (Test-Path (Join-Path $packagesDir "$_.$version.nupkg")) })
    Assert-True ($missingPackages.Count -eq 0) 'All 18 packages were produced'
    if ($missingPackages.Count -gt 0) { Write-Host "         missing: $($missingPackages -join ', ')" }
}

Write-Step 'Package layout and dependency groups'
$platformEntries = Get-PackageEntries -PackageId 'Stride.Platform'
Assert-True ($platformEntries -contains 'lib/net10.0/_._') 'Stride.Platform ships the net10.0 placeholder'
Assert-True ($platformEntries -contains 'lib/net10.0-windows7.0/_._') 'Stride.Platform normalizes the Windows placeholder'
Assert-True ($platformEntries -contains 'buildTransitive/Stride.Platform.targets') 'Stride.Platform ships buildTransitive targets'
Assert-True (-not ($platformEntries -contains 'lib/net10.0/Stride.Platform.dll')) 'Stride.Platform ships no assembly of its own'

$platformNuspec = Get-PackageNuspecXml -PackageId 'Stride.Platform'
Assert-DependencyIds -Nuspec $platformNuspec -TargetFramework 'net10.0' `
    -Expected @('Stride.Platform.Common', 'Stride.Platform.Windows', 'Stride.Platform.Windows.Win32', 'Stride.Platform.Linux', 'Stride.Platform.Linux.X11') `
    -Forbidden @('Stride.Platform.Windows.WinUI') `
    -Message 'net10.0 group carries Windows, Win32, Linux and X11 (no WinUI)'
Assert-DependencyIds -Nuspec $platformNuspec -TargetFramework 'net10.0-windows7.0' `
    -Expected @('Stride.Platform.Common', 'Stride.Platform.Windows', 'Stride.Platform.Windows.Win32', 'Stride.Platform.Windows.WinUI') `
    -Forbidden @('Stride.Platform.Linux', 'Stride.Platform.Linux.X11') `
    -Message 'net10.0-windows group carries Windows, Win32 and WinUI (no Linux)'

$ioNuspec = Get-PackageNuspecXml -PackageId 'Stride.Core.IO'
Assert-DependencyIds -Nuspec $ioNuspec -TargetFramework 'net10.0' `
    -Expected @('Stride.Platform', 'Stride.Core.IO.Common', 'Stride.Core.IO.Windows', 'Stride.Core.IO.Linux') `
    -Forbidden @() `
    -Message 'Stride.Core.IO net10.0 group carries both OS implementations'
Assert-DependencyIds -Nuspec $ioNuspec -TargetFramework 'net10.0-windows7.0' `
    -Expected @('Stride.Platform', 'Stride.Core.IO.Common', 'Stride.Core.IO.Windows') `
    -Forbidden @('Stride.Core.IO.Linux') `
    -Message 'Stride.Core.IO Windows group only carries the Windows implementation'

$graphicsNuspec = Get-PackageNuspecXml -PackageId 'Stride.Graphics'
Assert-DependencyIds -Nuspec $graphicsNuspec -TargetFramework 'net10.0' `
    -Expected @('Stride.Platform', 'Stride.Graphics.Common', 'Stride.Graphics.Direct3D11', 'Stride.Graphics.Vulkan') `
    -Forbidden @('Stride.Graphics.Direct3D11.WinUI') `
    -Message 'Stride.Graphics net10.0 group carries both API variants, no WinUI composition'
Assert-DependencyIds -Nuspec $graphicsNuspec -TargetFramework 'net10.0-windows7.0' `
    -Expected @('Stride.Platform', 'Stride.Graphics.Common', 'Stride.Graphics.Direct3D11', 'Stride.Graphics.Vulkan', 'Stride.Graphics.Direct3D11.WinUI') `
    -Forbidden @() `
    -Message 'Stride.Graphics Windows group adds the WinUI composition'

$appModelNuspec = Get-PackageNuspecXml -PackageId 'Stride.AppModel'
$appModelGroup = Get-DependencyGroup -Nuspec $appModelNuspec -TargetFramework 'net10.0'
$appModelDependency = @($appModelGroup.dependency) | Where-Object { $_.id -eq 'Stride.Platform' } | Select-Object -First 1
Assert-True ($null -ne $appModelDependency) 'Stride.AppModel depends on Stride.Platform'
$appModelExclude = if ($null -ne $appModelDependency -and $appModelDependency.HasAttribute('exclude')) { $appModelDependency.GetAttribute('exclude') } else { '' }
Assert-True ($appModelExclude -eq '') 'Stride.AppModel keeps all Stride.Platform assets (no exclude)'

$gamesNuspec = Get-PackageNuspecXml -PackageId 'Stride.Games'
Assert-DependencyIds -Nuspec $gamesNuspec -TargetFramework 'net10.0' `
    -Expected @('Stride.AppModel') `
    -Forbidden @('Stride.Graphics.Direct3D11', 'Stride.Graphics.Vulkan') `
    -Message 'Stride.Games only depends on Stride.AppModel (no graphics variant)'

$vulkanEntries = Get-PackageEntries -PackageId 'Stride.Graphics.Vulkan'
Assert-True ($vulkanEntries -contains 'runtimes/win-x64/native/stride_vulkan.dll') 'Vulkan variant ships its Windows native asset'
Assert-True ($vulkanEntries -contains 'runtimes/linux-x64/native/libstride_vulkan.so') 'Vulkan variant ships its Linux native asset'

$winuiEntries = Get-PackageEntries -PackageId 'Stride.Graphics.Direct3D11.WinUI'
Assert-True ($winuiEntries -contains 'lib/net10.0-windows7.0/Stride.Graphics.Direct3D11.WinUI.dll') 'WinUI composition ships its assembly'
Assert-True ($winuiEntries -contains 'buildTransitive/Stride.Graphics.Direct3D11.WinUI.targets') 'WinUI composition ships its descriptor targets'

Write-Step 'Windows / Win32 consumer (default selection)'
$build = Invoke-Build -Project 'consumers/GameApp/GameApp.csproj' -Scenario 'game-win'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with only meta-package references'
Assert-True (-not $build.Output.Contains('STRIDE0001')) 'Meta-only build emits no direct-reference warning'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'game-win' -TargetFramework 'net10.0-windows' -RuntimeIdentifier '' -AssemblyName 'GameApp'
Assert-True ($run.ExitCode -eq 0) 'GameApp runs'
Assert-OutputContains $run 'platform=Windows' 'STRIDE_PLATFORM_WINDOWS is defined'
Assert-OutputContains $run 'windowing=Win32' 'Win32 is the default windowing system on Windows'
Assert-OutputContains $run 'device-api=Direct3D11' 'Direct3D11 is the default API on Windows'
Assert-OutputContains $run 'device-surface=Win32Hwnd' 'The surface comes from the Win32 host'
Assert-OutputContains $run 'io-separator=\' 'Stride.Core.IO selects the Windows implementation'
Assert-OutputContains $run 'io-case-sensitive=False' 'The Windows file system is case-insensitive'
Assert-OutputContains $run 'frames=2' 'Stride.Games runs the platform loop'
Assert-OutputContains $run 'submitted=2' 'The game loop reaches the graphics device'

$outputDir = Get-OutputDirectory -Scenario 'game-win' -TargetFramework 'net10.0-windows' -RuntimeIdentifier ''
foreach ($assembly in @('Stride.AppModel.dll', 'Stride.Games.dll', 'Stride.Platform.Common.dll', 'Stride.Platform.Windows.dll', 'Stride.Platform.Windows.Win32.dll', 'Stride.Core.IO.Common.dll', 'Stride.Core.IO.Windows.dll', 'Stride.Graphics.Common.dll', 'Stride.Graphics.Direct3D11.dll')) {
    Assert-True (Test-Path (Join-Path $outputDir $assembly)) "$assembly is present"
}
foreach ($assembly in @('Stride.Platform.Windows.WinUI.dll', 'Stride.Platform.Linux.dll', 'Stride.Platform.Linux.X11.dll', 'Stride.Core.IO.Linux.dll', 'Stride.Graphics.Vulkan.dll', 'Stride.Graphics.Direct3D11.WinUI.dll')) {
    Assert-True (-not (Test-Path (Join-Path $outputDir $assembly))) "$assembly is pruned"
}
Assert-True (Test-Asset -OutputDirectory $outputDir -Name 'stride_platform_win32.dll') 'The Win32 native asset is deployed'
Assert-True (Test-Asset -OutputDirectory $outputDir -Name 'stride_io_windows.dll') 'The Windows I/O native asset is deployed'
Assert-True (Test-Asset -OutputDirectory $outputDir -Name 'stride_d3d11.dll') 'The Direct3D11 native asset is deployed'

Write-Step 'Windows / WinUI consumer (explicit windowing + composition)'
$build = Invoke-Build -Project 'consumers/GameWinUIApp/GameWinUIApp.csproj' -Scenario 'game-winui'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with <StrideWindowSystem>WinUI</StrideWindowSystem>'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'game-winui' -TargetFramework 'net10.0-windows' -RuntimeIdentifier '' -AssemblyName 'GameWinUIApp'
Assert-True ($run.ExitCode -eq 0) 'GameWinUIApp runs'
Assert-OutputContains $run 'windowing=WinUI' 'WinUI is selected explicitly'
Assert-OutputContains $run 'surface=WinUISwapChainPanel' 'The surface comes from the WinUI host'
Assert-OutputContains $run 'composition=Stride.Graphics.Direct3D11.WinUI' 'The pairwise composition package is active'
Assert-OutputContains $run 'frames=2' 'The game loop still runs through the WinUI host'

$outputDir = Get-OutputDirectory -Scenario 'game-winui' -TargetFramework 'net10.0-windows' -RuntimeIdentifier ''
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Platform.Windows.WinUI.dll')) 'The WinUI host is present'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.WinUI.dll')) 'The WinUI composition is present'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Platform.Windows.Win32.dll'))) 'The Win32 host is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll'))) 'Vulkan is pruned'
Assert-True (Test-Asset -OutputDirectory $outputDir -Name 'stride_platform_winui.dll') 'The WinUI native asset is deployed'
Assert-True (Test-Asset -OutputDirectory $outputDir -Name 'stride_graphics_winui.dll') 'The composition native asset is deployed'

Write-Step 'Compatible direct package pin (warning + opt-out)'
$build = Invoke-Build -Project 'consumers/DirectPinLib/DirectPinLib.csproj' -Scenario 'direct-pin'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with a compatible direct WinUI reference'
Assert-OutputContains $build 'STRIDE0001' 'A warning discourages the direct reference'
Assert-OutputContains $build 'Reference the meta-packages' 'The warning recommends the meta-packages'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$build = Invoke-Build -Project 'consumers/DirectPinLib/DirectPinLib.csproj' -Scenario 'direct-pin-optout' -ExtraProperties @('-p:StridePlatformAllowDirectReferences=true')
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with StridePlatformAllowDirectReferences=true'
Assert-True (-not $build.Output.Contains('STRIDE0001')) 'The opt-out suppresses the direct-reference warning'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

Write-Step 'Linux / X11 consumer (explicit platform, Vulkan)'
$build = Invoke-Build -Project 'consumers/GameLinuxApp/GameLinuxApp.csproj' -Scenario 'game-linux' -RuntimeIdentifier 'linux-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds for net10.0 + StridePlatform=Linux + linux-x64'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$outputDir = Get-OutputDirectory -Scenario 'game-linux' -TargetFramework 'net10.0' -RuntimeIdentifier 'linux-x64'
foreach ($assembly in @('Stride.AppModel.dll', 'Stride.Games.dll', 'Stride.Platform.Common.dll', 'Stride.Platform.Linux.dll', 'Stride.Platform.Linux.X11.dll', 'Stride.Core.IO.Common.dll', 'Stride.Core.IO.Linux.dll', 'Stride.Graphics.Common.dll', 'Stride.Graphics.Vulkan.dll')) {
    Assert-True (Test-Path (Join-Path $outputDir $assembly)) "$assembly is present"
}
foreach ($assembly in @('Stride.Platform.Windows.dll', 'Stride.Platform.Windows.Win32.dll', 'Stride.Platform.Windows.WinUI.dll', 'Stride.Core.IO.Windows.dll', 'Stride.Graphics.Direct3D11.dll', 'Stride.Graphics.Direct3D11.WinUI.dll')) {
    Assert-True (-not (Test-Path (Join-Path $outputDir $assembly))) "$assembly is pruned"
}
Assert-True (Test-Path (Join-Path $outputDir 'libstride_platform_x11.so')) 'The X11 native asset is deployed'
Assert-True (Test-Path (Join-Path $outputDir 'libstride_io_linux.so')) 'The Linux I/O native asset is deployed'
Assert-True (Test-Path (Join-Path $outputDir 'libstride_vulkan.so')) 'The Vulkan Linux native asset is deployed'

$run = Invoke-App -Scenario 'game-linux' -TargetFramework 'net10.0' -RuntimeIdentifier 'linux-x64' -AssemblyName 'GameLinuxApp'
Assert-True ($run.ExitCode -eq 0) 'The simulated Linux build runs (no native calls)'
Assert-OutputContains $run 'platform=Linux' 'STRIDE_PLATFORM_LINUX is defined'
Assert-OutputContains $run 'windowing=X11' 'X11 is the default windowing system on Linux'
Assert-OutputContains $run 'surface=X11Window' 'The surface comes from the X11 host'
Assert-OutputContains $run 'platform-native=libstride_platform_x11.so' 'The X11 host reports its native library'
Assert-OutputContains $run 'io-separator=/' 'Stride.Core.IO selects the Linux implementation'
Assert-OutputContains $run 'io-case-sensitive=True' 'The Linux file system is case-sensitive'
Assert-OutputContains $run 'frames=2' 'The game loop runs through the X11 host'

Write-Step 'Portable library (net10.0, no platform)'
$build = Invoke-Build -Project 'consumers/PortableGameLib/PortableGameLib.csproj' -Scenario 'portable-lib'
Assert-True ($build.ExitCode -eq 0) 'PortableGameLib compiles with STRIDE_PLATFORM_PORTABLE and no platform constants'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$outputDir = Get-OutputDirectory -Scenario 'portable-lib' -TargetFramework 'net10.0' -RuntimeIdentifier ''
$portableReferences = Get-AssemblyReferencesFromPath (Join-Path $outputDir 'PortableGameLib.dll')
Assert-True ($portableReferences -contains 'Stride.Platform.Common') 'PortableGameLib references Stride.Platform.Common'
Assert-True (-not ($portableReferences | Where-Object { $_ -like 'Stride.Platform.*' -and $_ -ne 'Stride.Platform.Common' })) 'PortableGameLib references no platform-specific assembly'
Assert-True (-not ($portableReferences | Where-Object { $_ -like 'Stride.Graphics.*' })) 'PortableGameLib references no graphics variant'

Write-Step 'Multi-target library (net10.0;net10.0-windows)'
$build = Invoke-Build -Project 'consumers/MultiTargetProbe/MultiTargetProbe.csproj' -Scenario 'multi-target'
Assert-True ($build.ExitCode -eq 0) 'MultiTargetProbe compiles for both TFMs'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$portableDir = Get-OutputDirectory -Scenario 'multi-target' -TargetFramework 'net10.0' -RuntimeIdentifier ''
$windowsDir = Get-OutputDirectory -Scenario 'multi-target' -TargetFramework 'net10.0-windows' -RuntimeIdentifier ''
Assert-True (Test-Path (Join-Path $portableDir 'MultiTargetProbe.dll')) 'net10.0 output exists'
Assert-True (Test-Path (Join-Path $windowsDir 'MultiTargetProbe.dll')) 'net10.0-windows output exists'

$portableProbeReferences = Get-AssemblyReferencesFromPath (Join-Path $portableDir 'MultiTargetProbe.dll')
$windowsProbeReferences = Get-AssemblyReferencesFromPath (Join-Path $windowsDir 'MultiTargetProbe.dll')
$portableProbeDeps = Get-Content (Join-Path $portableDir 'MultiTargetProbe.deps.json') -Raw
$windowsProbeDeps = Get-Content (Join-Path $windowsDir 'MultiTargetProbe.deps.json') -Raw

Assert-True (-not ($portableProbeReferences | Where-Object { $_ -like 'Stride.Platform.*' -and $_ -ne 'Stride.Platform.Common' })) 'net10.0 only references Stride.Platform.Common'
Assert-True ($windowsProbeReferences -contains 'Stride.Platform.Windows.Win32') 'net10.0-windows references the Win32 host'
Assert-True ($windowsProbeDeps.Contains('"Stride.Platform.Windows/')) 'net10.0-windows restores the Windows OS package'
Assert-True (-not $windowsProbeDeps.Contains('Stride.Platform.Linux') -and -not $windowsProbeDeps.Contains('Stride.Platform.Windows.WinUI')) 'net10.0-windows prunes Linux and WinUI'
Assert-True (-not $portableProbeDeps.Contains('"Stride.Platform.Windows/') -and -not $portableProbeDeps.Contains('Stride.Platform.Linux')) 'net10.0 restores only Stride.Platform.Common'

Write-Step 'Validation: Direct3D11 on Linux'
$build = Invoke-Build -Project 'consumers/invalid/InvalidLinuxD3D11/InvalidLinuxD3D11.csproj' -Scenario 'invalid-linux-d3d11'
Assert-True ($build.ExitCode -ne 0) 'Build fails for Direct3D11 on Linux'
Assert-OutputContains $build "is not provided on platform 'Linux'" 'Error explains the API/platform mismatch'

Write-Step 'Validation: X11 on a Windows TFM'
$build = Invoke-Build -Project 'consumers/invalid/InvalidWindowsX11/InvalidWindowsX11.csproj' -Scenario 'invalid-win-x11'
Assert-True ($build.ExitCode -ne 0) 'Build fails for X11 on net10.0-windows'
Assert-OutputContains $build "StrideWindowSystem='X11' is not available on platform 'Windows'" 'Error explains the windowing/platform mismatch'

Write-Step 'Validation: WinUI without the Windows TFM'
$build = Invoke-Build -Project 'consumers/invalid/InvalidWinUIOnNet10/InvalidWinUIOnNet10.csproj' -Scenario 'invalid-winui-net10'
Assert-True ($build.ExitCode -ne 0) 'Build fails for WinUI on net10.0'
Assert-OutputContains $build "StrideWindowSystem='WinUI' is not available on platform 'Windows'" 'Error explains that WinUI needs the Windows TFM'
Assert-OutputContains $build 'target net10.0-windows' 'Error suggests the Windows TFM'

Write-Step 'Validation: direct platform package pins the windowing axis'
$build = Invoke-Build -Project 'consumers/invalid/InvalidDirectVariant/InvalidDirectVariant.csproj' -Scenario 'invalid-direct-platform'
Assert-True ($build.ExitCode -ne 0) 'Build fails for a direct WinUI reference while Win32 is resolved'
Assert-OutputContains $build 'Direct reference(s) incompatible' 'Error reports the incompatible direct reference'
Assert-OutputContains $build 'Reference the meta-packages instead' 'Error suggests the meta-packages'

Write-Step 'Validation: direct graphics variant pins the API axis'
$build = Invoke-Build -Project 'consumers/invalid/InvalidDirectGraphics/InvalidDirectGraphics.csproj' -Scenario 'invalid-direct-graphics'
Assert-True ($build.ExitCode -ne 0) 'Build fails for a direct Vulkan reference while Direct3D11 is resolved'
Assert-OutputContains $build 'Direct graphics reference(s) incompatible' 'Error reports the incompatible graphics reference'
Assert-OutputContains $build 'Reference Stride.Graphics instead' 'Error suggests the graphics meta-package'

Write-Step 'Summary'
Write-Host "  $($script:checks - $script:failures)/$($script:checks) checks passed"
Write-Host "  packages: $packagesDir"
Write-Host "  outputs:  $buildDir"

if ($script:failures -gt 0) {
    Write-Host ''
    Write-Host "$($script:failures) check(s) failed." -ForegroundColor Red
    exit 1
}

Write-Host ''
Write-Host 'All checks passed.' -ForegroundColor Green
exit 0
