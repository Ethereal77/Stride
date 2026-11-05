#Requires -Version 7.0
<#
.SYNOPSIS
    Packs the Stride.Graphics meta-package experiment and runs the consumer matrix.

.DESCRIPTION
    Everything the script writes stays inside experiments/meta-packages:
      artifacts/packages     local NuGet feed
      artifacts/nuget-cache  local NuGet package cache
      artifacts/build        consumer build outputs

    The script prints one line per check and exits non-zero when any check fails.

.PARAMETER Configuration
    Build configuration, Release by default.

.PARAMETER SkipPack
    Reuse the packages already in artifacts/packages instead of packing again.

.PARAMETER KeepArtifacts
    Keep the previous consumer build outputs instead of deleting artifacts/build.

.PARAMETER Clean
    Delete the whole artifacts directory (packages, cache and build outputs) first.
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [switch]$SkipPack,

    [switch]$KeepArtifacts,

    [switch]$Clean
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = $PSScriptRoot
$artifacts = Join-Path $root 'artifacts'
$packagesDir = Join-Path $artifacts 'packages'
$buildDir = Join-Path $artifacts 'build'
$version = '0.1.0-meta.' + (Get-Date -Format 'yyyyMMddHHmmssfff')

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
        "-p:StrideMetaVersion=$version"
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
        "-p:StrideMetaVersion=$version"
        "-p:BaseOutputPath=$outRoot"
        "-r", $RuntimeIdentifier
    ) + $ExtraProperties

    return Invoke-Dotnet -Arguments $arguments
}

function Get-OutputDirectory {
    param([string]$Scenario, [string]$RuntimeIdentifier)

    return Join-Path $buildDir "$Scenario\$Configuration\net10.0\$RuntimeIdentifier"
}

function Invoke-App {
    param(
        [string]$Scenario,
        [string]$RuntimeIdentifier,
        [string]$AssemblyName,
        [string[]]$AppArguments = @()
    )

    $dll = Join-Path (Get-OutputDirectory -Scenario $Scenario -RuntimeIdentifier $RuntimeIdentifier) "$AssemblyName.dll"
    if (-not (Test-Path $dll)) {
        throw "Missing build output: $dll"
    }

    return Invoke-Dotnet -Arguments (@($dll) + $AppArguments)
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

function Get-PackageNuspec {
    param([string]$PackageId)

    $path = Join-Path $packagesDir "$PackageId.$version.nupkg"
    $zip = [System.IO.Compression.ZipFile]::OpenRead($path)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -like '*.nuspec' } | Select-Object -First 1
        $reader = New-Object System.IO.StreamReader($entry.Open())
        try {
            return $reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
    }
    finally {
        $zip.Dispose()
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

function Import-ExperimentAssembly {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        throw "Missing assembly: $Path"
    }

    return [System.Reflection.Assembly]::LoadFrom($Path)
}

function Get-GraphicsDeviceType {
    param([System.Reflection.Assembly]$Assembly)

    return $Assembly.GetType('Stride.Graphics.GraphicsDevice', $false)
}

function Format-ReflectedMember {
    param([System.Reflection.MemberInfo]$Member)

    if ($Member -is [System.Reflection.MethodInfo]) {
        $parameters = ($Member.GetParameters() | ForEach-Object { $_.ParameterType.FullName }) -join ','
        return "method $($Member.Name)($parameters):$($Member.ReturnType.FullName)"
    }

    if ($Member -is [System.Reflection.ConstructorInfo]) {
        $parameters = ($Member.GetParameters() | ForEach-Object { $_.ParameterType.FullName }) -join ','
        return "ctor($parameters)"
    }

    if ($Member -is [System.Reflection.PropertyInfo]) {
        return "property $($Member.Name):$($Member.PropertyType.FullName):get=$($Member.CanRead):set=$($Member.CanWrite)"
    }

    if ($Member -is [System.Reflection.FieldInfo]) {
        return "field $($Member.Name):$($Member.FieldType.FullName)"
    }

    return "$($Member.MemberType) $($Member.Name)"
}

function Get-GraphicsDeviceSurface {
    param([System.Reflection.Assembly]$Assembly)

    $type = Get-GraphicsDeviceType -Assembly $Assembly
    if ($null -eq $type) {
        return @()
    }

    $flags = [System.Reflection.BindingFlags]::Public -bor
        [System.Reflection.BindingFlags]::Instance -bor
        [System.Reflection.BindingFlags]::Static -bor
        [System.Reflection.BindingFlags]::DeclaredOnly

    return @($type.GetMembers($flags) | ForEach-Object { Format-ReflectedMember -Member $_ } | Sort-Object)
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

if ($Clean -and (Test-Path $artifacts)) {
    Remove-Item -Recurse -Force $artifacts
}

if ($SkipPack) {
    $latestPackage = Get-ChildItem $packagesDir -Filter 'Stride.Graphics.*.nupkg' -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -match '^Stride\.Graphics\.\d' } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if (-not $latestPackage) {
        throw "SkipPack was set but $packagesDir has no Stride.Graphics package. Run without -SkipPack first."
    }
    $version = $latestPackage.BaseName.Substring('Stride.Graphics.'.Length)
}

if (-not $KeepArtifacts -and (Test-Path $buildDir)) {
    Remove-Item -Recurse -Force $buildDir
}

if (-not $SkipPack) {
    if (Test-Path $packagesDir) {
        Remove-Item -Recurse -Force $packagesDir
    }
    New-Item -ItemType Directory -Path $packagesDir -Force | Out-Null

    Write-Step "Packing base packages ($version)"
    $baseProjects = @(
        'src/Stride.Graphics.Common/Stride.Graphics.Common.csproj'
        'src/Stride.MetaExperiment.Native.Direct3D11/Stride.MetaExperiment.Native.Direct3D11.csproj'
        'src/Stride.MetaExperiment.Native.Direct3D12/Stride.MetaExperiment.Native.Direct3D12.csproj'
        'src/Stride.MetaExperiment.Native.Vulkan/Stride.MetaExperiment.Native.Vulkan.csproj'
        'src/Stride.MetaExperiment.Pix/Stride.MetaExperiment.Pix.csproj'
        'src/Stride.MetaExperiment.Warp/Stride.MetaExperiment.Warp.csproj'
        'src/Stride.MetaExperiment.Lavapipe/Stride.MetaExperiment.Lavapipe.csproj'
        'src/Stride.Graphics.Direct3D11/Stride.Graphics.Direct3D11.csproj'
        'src/Stride.Graphics.Direct3D12/Stride.Graphics.Direct3D12.csproj'
        'src/Stride.Graphics.Vulkan/Stride.Graphics.Vulkan.csproj'
    )
    foreach ($project in $baseProjects) {
        Invoke-Pack -Project $project
        Write-Host "  packed $(Split-Path $project -Leaf)"
    }

    Write-Step "Packing the Stride.Graphics meta-package"
    # Two-phase pack: the meta-package depends on the packages packed above.
    Invoke-Pack -Project 'src/Stride.Graphics/Stride.Graphics.csproj' -ExtraProperties @('-p:StrideMetaPacking=true')
    Write-Host "  packed Stride.Graphics.csproj"
}

Write-Step 'Package layout'
$metaEntries = Get-PackageEntries -PackageId 'Stride.Graphics'
Assert-True ($metaEntries -contains 'lib/net10.0/_._') 'Stride.Graphics ships a lib placeholder, not an assembly'
Assert-True ($metaEntries -contains 'buildTransitive/Stride.Graphics.targets') 'Stride.Graphics ships buildTransitive targets'
Assert-True (-not ($metaEntries -contains 'lib/net10.0/Stride.Graphics.dll')) 'Stride.Graphics ships no assembly of its own'

$metaNuspec = Get-PackageNuspec -PackageId 'Stride.Graphics'
$metaDependencies = @(
    'Stride.Graphics.Common'
    'Stride.Graphics.Direct3D11'
    'Stride.Graphics.Direct3D12'
    'Stride.Graphics.Vulkan'
    'Stride.MetaExperiment.Pix'
    'Stride.MetaExperiment.Warp'
    'Stride.MetaExperiment.Lavapipe'
)
foreach ($dependency in $metaDependencies) {
    Assert-Contains $metaNuspec "id=`"$dependency`" version=`"$version`" include=`"All`"" "Stride.Graphics depends on $dependency with all assets"
}

$commonEntries = Get-PackageEntries -PackageId 'Stride.Graphics.Common'
Assert-True ($commonEntries -contains 'lib/net10.0/Stride.Graphics.Common.dll') 'Common ships its shared-types assembly'
Assert-True (-not ($commonEntries -contains 'buildTransitive/Stride.Graphics.Common.targets')) 'Common ships no selection targets'

$d3d11Entries = Get-PackageEntries -PackageId 'Stride.Graphics.Direct3D11'
Assert-True ($d3d11Entries -contains 'lib/net10.0/Stride.Graphics.Direct3D11.dll') 'Variant ships its own assembly'
Assert-True ($d3d11Entries -contains 'buildTransitive/Stride.Graphics.Direct3D11.targets') 'Variant ships buildTransitive targets'

$vulkanEntries = Get-PackageEntries -PackageId 'Stride.Graphics.Vulkan'
Assert-True ($vulkanEntries -contains 'buildTransitive/Stride.Graphics.Vulkan.targets') 'Vulkan variant ships buildTransitive targets'

$nativeVulkanEntries = Get-PackageEntries -PackageId 'Stride.MetaExperiment.Native.Vulkan'
Assert-True ($nativeVulkanEntries -contains 'runtimes/win-x64/native/stride_vulkan.dll') 'Native package ships its Windows asset'
Assert-True ($nativeVulkanEntries -contains 'runtimes/linux-x64/native/libstride_vulkan.so') 'Native package ships its Linux asset'

$pixEntries = Get-PackageEntries -PackageId 'Stride.MetaExperiment.Pix'
Assert-True ($pixEntries -contains 'lib/net10.0/Stride.MetaExperiment.Pix.dll') 'PIX package ships its assembly'
Assert-True ($pixEntries -contains 'buildTransitive/Stride.MetaExperiment.Pix.targets') 'PIX package ships its applicability targets'
Assert-True ($pixEntries -contains 'runtimes/win-x64/native/WinPixEventRuntime.dll') 'PIX package ships its native asset'

$warpEntries = Get-PackageEntries -PackageId 'Stride.MetaExperiment.Warp'
Assert-True ($warpEntries -contains 'lib/net10.0/Stride.MetaExperiment.Warp.dll') 'WARP package ships its assembly'
Assert-True ($warpEntries -contains 'buildTransitive/Stride.MetaExperiment.Warp.targets') 'WARP package ships its applicability targets'
Assert-True ($warpEntries -contains 'runtimes/win-x64/native/d3d10warp.dll') 'WARP package ships its native asset'

$lavapipeEntries = Get-PackageEntries -PackageId 'Stride.MetaExperiment.Lavapipe'
Assert-True ($lavapipeEntries -contains 'lib/net10.0/Stride.MetaExperiment.Lavapipe.dll') 'Lavapipe package ships its assembly'
Assert-True ($lavapipeEntries -contains 'buildTransitive/Stride.MetaExperiment.Lavapipe.targets') 'Lavapipe package ships its applicability targets'
Assert-True ($lavapipeEntries -contains 'runtimes/win-x64/native/vulkan_lvp.dll') 'Lavapipe package ships its Windows asset'
Assert-True ($lavapipeEntries -contains 'runtimes/linux-x64/native/libvulkan_lvp.so') 'Lavapipe package ships its Linux asset'

Write-Step 'Default Windows consumer (Direct3D11)'
$build = Invoke-Build -Project 'consumers/ConsumerApp/ConsumerApp.csproj' -Scenario 'default-win' -RuntimeIdentifier 'win-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with only <PackageReference Include="Stride.Graphics" />'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'default-win' -RuntimeIdentifier 'win-x64' -AssemblyName 'ConsumerApp'
Assert-True ($run.ExitCode -eq 0) 'Application runs'
Assert-OutputContains $run 'platform=Windows' 'The meta-package defines STRIDE_PLATFORM_WINDOWS'
Assert-OutputContains $run 'compile-time=Direct3D11' 'Compile-time define selects Direct3D11'
Assert-OutputContains $run 'api=Direct3D11' 'The compiled device is the Direct3D11 variant'
Assert-OutputContains $run 'native-present=True' 'Native asset of the selected variant is copied'
Assert-OutputContains $run 'pix=WinPixEventRuntime.dll' 'PIX is available for the Direct3D family'
Assert-OutputContains $run 'warp=d3d10warp.dll' 'WARP is available for the Direct3D family'
Assert-OutputContains $run 'submitted=1000' 'The non-virtual Submit hot path works'

$outputDir = Get-OutputDirectory -Scenario 'default-win' -RuntimeIdentifier 'win-x64'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Common.dll')) 'Common assembly is present'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll')) 'Direct3D11 variant assembly is present'
Assert-True (Test-Path (Join-Path $outputDir 'stride_d3d11.dll')) 'Direct3D11 native asset is present'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll'))) 'Direct3D12 variant assembly is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll'))) 'Vulkan variant assembly is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'stride_d3d12.dll'))) 'Direct3D12 native asset is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'stride_vulkan.dll'))) 'Vulkan native asset is pruned'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll')) 'PIX assembly is present for Direct3D'
Assert-True (Test-Path (Join-Path $outputDir 'WinPixEventRuntime.dll')) 'PIX native asset is present for Direct3D'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll')) 'WARP assembly is present for Direct3D'
Assert-True (Test-Path (Join-Path $outputDir 'd3d10warp.dll')) 'WARP native asset is present for Direct3D'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll'))) 'Lavapipe assembly is pruned for Direct3D'
Assert-True (-not (Test-Path (Join-Path $outputDir 'vulkan_lvp.dll'))) 'Lavapipe native asset is pruned for Direct3D'

Write-Step 'Command-line override (Direct3D12)'
$build = Invoke-Build -Project 'consumers/ConsumerApp/ConsumerApp.csproj' -Scenario 'd3d12-cli' -RuntimeIdentifier 'win-x64' -ExtraProperties @('-p:StrideGraphicsApi=Direct3D12')
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with -p:StrideGraphicsApi=Direct3D12'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'd3d12-cli' -RuntimeIdentifier 'win-x64' -AssemblyName 'ConsumerApp'
Assert-True ($run.ExitCode -eq 0) 'Application runs'
Assert-OutputContains $run 'platform=Windows' 'Platform define stays Windows'
Assert-OutputContains $run 'compile-time=Direct3D12' 'Compile-time define follows the override'
Assert-OutputContains $run 'api=Direct3D12' 'The compiled device is the Direct3D12 variant'
Assert-OutputContains $run 'pix=WinPixEventRuntime.dll' 'PIX is also available for Direct3D 12'
Assert-OutputContains $run 'warp=d3d10warp.dll' 'WARP is also available for Direct3D 12'

$outputDir = Get-OutputDirectory -Scenario 'd3d12-cli' -RuntimeIdentifier 'win-x64'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll')) 'Direct3D12 variant assembly is present'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll'))) 'Direct3D11 variant assembly is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll'))) 'Vulkan variant assembly is pruned'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll')) 'PIX assembly is present for Direct3D 12'
Assert-True (Test-Path (Join-Path $outputDir 'WinPixEventRuntime.dll')) 'PIX native asset is present for Direct3D 12'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll')) 'WARP assembly is present for Direct3D 12'
Assert-True (Test-Path (Join-Path $outputDir 'd3d10warp.dll')) 'WARP native asset is present for Direct3D 12'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll'))) 'Lavapipe assembly is pruned for Direct3D 12'
Assert-True (-not (Test-Path (Join-Path $outputDir 'vulkan_lvp.dll'))) 'Lavapipe native asset is pruned for Direct3D 12'

Write-Step 'Project-level override (Vulkan in the csproj body)'
$build = Invoke-Build -Project 'consumers/VulkanApp/VulkanApp.csproj' -Scenario 'vulkan-body' -RuntimeIdentifier 'win-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with <StrideGraphicsApi>Vulkan</StrideGraphicsApi>'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'vulkan-body' -RuntimeIdentifier 'win-x64' -AssemblyName 'VulkanApp'
Assert-True ($run.ExitCode -eq 0) 'Application runs'
Assert-OutputContains $run 'platform=Windows' 'Vulkan on Windows still gets STRIDE_PLATFORM_WINDOWS'
Assert-OutputContains $run 'compile-time=Vulkan' 'Compile-time define selects Vulkan'
Assert-OutputContains $run 'api=Vulkan' 'The compiled device is the Vulkan variant'
Assert-OutputContains $run 'native=stride_vulkan.dll' 'Vulkan Windows native asset is used'
Assert-OutputContains $run 'lavapipe=vulkan_lvp.dll' 'Lavapipe is available for the Vulkan family'

$outputDir = Get-OutputDirectory -Scenario 'vulkan-body' -RuntimeIdentifier 'win-x64'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll')) 'Vulkan variant assembly is present'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll'))) 'Direct3D11 variant assembly is pruned'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll'))) 'Direct3D12 variant assembly is pruned'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll')) 'Lavapipe assembly is present for Vulkan'
Assert-True (Test-Path (Join-Path $outputDir 'vulkan_lvp.dll')) 'Lavapipe native asset is present for Vulkan'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll'))) 'PIX assembly is pruned for Vulkan'
Assert-True (-not (Test-Path (Join-Path $outputDir 'WinPixEventRuntime.dll'))) 'PIX native asset is pruned for Vulkan'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll'))) 'WARP assembly is pruned for Vulkan'
Assert-True (-not (Test-Path (Join-Path $outputDir 'd3d10warp.dll'))) 'WARP native asset is pruned for Vulkan'

Write-Step 'Linux default selection (Vulkan, build only)'
$build = Invoke-Build -Project 'consumers/ConsumerApp/ConsumerApp.csproj' -Scenario 'linux-default' -RuntimeIdentifier 'linux-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds for linux-x64 without an explicit API'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$outputDir = Get-OutputDirectory -Scenario 'linux-default' -RuntimeIdentifier 'linux-x64'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll')) 'Linux build keeps the Vulkan variant'
Assert-True (Test-Path (Join-Path $outputDir 'libstride_vulkan.so')) 'Linux build keeps the Vulkan native asset'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll'))) 'Linux build prunes Direct3D11'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll'))) 'Linux build prunes Direct3D12'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll')) 'Linux build keeps the Lavapipe assembly'
Assert-True (Test-Path (Join-Path $outputDir 'libvulkan_lvp.so')) 'Linux build keeps the Lavapipe Linux asset'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll'))) 'Linux build prunes PIX'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll'))) 'Linux build prunes WARP'

Write-Step 'Multi-API host'
$build = Invoke-Build -Project 'consumers/MultiApiHostApp/MultiApiHostApp.csproj' -Scenario 'multi-api' -RuntimeIdentifier 'win-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with StrideMultiGraphicsApiHost=true'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$outputDir = Get-OutputDirectory -Scenario 'multi-api' -RuntimeIdentifier 'win-x64'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll')) 'Multi-API host keeps Direct3D11'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll')) 'Multi-API host keeps Direct3D12'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll')) 'Multi-API host keeps Vulkan'
Assert-True (Test-Path (Join-Path $outputDir 'stride_d3d11.dll')) 'Multi-API host keeps the Direct3D11 native asset'
Assert-True (Test-Path (Join-Path $outputDir 'stride_d3d12.dll')) 'Multi-API host keeps the Direct3D12 native asset'
Assert-True (Test-Path (Join-Path $outputDir 'stride_vulkan.dll')) 'Multi-API host keeps the Vulkan native asset'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll')) 'Multi-API host keeps PIX'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll')) 'Multi-API host keeps WARP'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll')) 'Multi-API host keeps Lavapipe'

$run = Invoke-App -Scenario 'multi-api' -RuntimeIdentifier 'win-x64' -AssemblyName 'MultiApiHostApp'
Assert-True ($run.ExitCode -eq 0) 'Multi-API host runs'
Assert-OutputContains $run 'compile-time=Direct3D11' 'Multi-API host still compiles against one variant'
Assert-OutputContains $run 'deployed=Direct3D11,Direct3D12,Vulkan' 'Multi-API host deploys every variant'
Assert-OutputContains $run 'conditional=WinPixEventRuntime.dll,d3d10warp.dll,vulkan_lvp.dll' 'Multi-API host deploys every conditional package'
Assert-OutputContains $run 'api=Direct3D11' 'Multi-API host uses the compile-time selected variant'

Write-Step 'Structure: compile-time only, no runtime dispatch'
$commonAssembly = Import-ExperimentAssembly (Join-Path $outputDir 'Stride.Graphics.Common.dll')
$d3d11Assembly = Import-ExperimentAssembly (Join-Path $outputDir 'Stride.Graphics.Direct3D11.dll')
$d3d12Assembly = Import-ExperimentAssembly (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll')
$vulkanAssembly = Import-ExperimentAssembly (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll')

Assert-True ($null -eq $commonAssembly.GetType('Stride.Graphics.GraphicsDevice', $false)) 'Common does not define GraphicsDevice'
Assert-True ($null -eq $commonAssembly.GetType('Stride.Graphics.IGraphicsBackend', $false)) 'Common does not define an IGraphicsBackend'

$commonFlags = [System.Reflection.BindingFlags]::Public -bor
    [System.Reflection.BindingFlags]::NonPublic -bor
    [System.Reflection.BindingFlags]::Instance -bor
    [System.Reflection.BindingFlags]::Static -bor
    [System.Reflection.BindingFlags]::DeclaredOnly

$commonTypes = @($commonAssembly.GetTypes())
$commonAbstract = @($commonTypes | Where-Object { -not $_.IsInterface -and $_.IsAbstract -and -not $_.IsSealed })
Assert-True ($commonAbstract.Count -eq 0) 'Common declares no abstract classes'

$commonOpenTypes = @($commonTypes | Where-Object { -not $_.IsSealed })
Assert-True ($commonOpenTypes.Count -eq 0) 'Common only declares sealed types (no inheritance hierarchy)'

$commonVirtuals = @($commonTypes | ForEach-Object { $_.GetMethods($commonFlags) } | Where-Object { $_.IsVirtual -and -not $_.IsFinal -and -not $_.DeclaringType.IsSealed })
Assert-True ($commonVirtuals.Count -eq 0) 'Common declares no overridable virtual methods'

$commonReferences = @($commonAssembly.GetReferencedAssemblies() | ForEach-Object Name)
Assert-True (-not ($commonReferences | Where-Object { $_ -like 'Stride.Graphics.*' })) 'Common does not reference any variant'

$variants = @(
    [pscustomobject]@{ Name = 'Direct3D11'; Assembly = $d3d11Assembly },
    [pscustomobject]@{ Name = 'Direct3D12'; Assembly = $d3d12Assembly },
    [pscustomobject]@{ Name = 'Vulkan'; Assembly = $vulkanAssembly }
)

foreach ($variant in $variants) {
    $type = Get-GraphicsDeviceType -Assembly $variant.Assembly
    Assert-True ($null -ne $type) "$($variant.Name) defines Stride.Graphics.GraphicsDevice"
    Assert-True ($type.IsSealed) "$($variant.Name) GraphicsDevice is sealed"
    Assert-True (-not $type.IsAbstract) "$($variant.Name) GraphicsDevice is concrete"
    Assert-True ($type.BaseType -eq [object]) "$($variant.Name) GraphicsDevice has no base class"

    $interfaces = @($type.GetInterfaces() | ForEach-Object FullName)
    Assert-True ($interfaces.Count -eq 1 -and $interfaces[0] -eq 'System.IDisposable') "$($variant.Name) GraphicsDevice only implements IDisposable"

    $virtuals = @($type.GetMethods($commonFlags) | Where-Object { $_.IsVirtual -and -not $_.IsFinal })
    Assert-True ($virtuals.Count -eq 0) "$($variant.Name) GraphicsDevice declares no overridable virtual methods"

    $submit = $type.GetMethod('Submit', [type[]]@([int]))
    Assert-True ($null -ne $submit -and -not $submit.IsVirtual) "$($variant.Name) Submit is a non-virtual hot-path call"
    Assert-True ($null -eq $variant.Assembly.GetType('Stride.Graphics.IGraphicsBackend', $false)) "$($variant.Name) has no IGraphicsBackend"

    $references = @($variant.Assembly.GetReferencedAssemblies() | ForEach-Object Name)
    $otherVariants = @('Stride.Graphics.Direct3D11', 'Stride.Graphics.Direct3D12', 'Stride.Graphics.Vulkan') |
        Where-Object { $_ -ne "Stride.Graphics.$($variant.Name)" }
    Assert-True ($references -contains 'Stride.Graphics.Common') "$($variant.Name) references the shared Common assembly"
    Assert-True (-not ($otherVariants | Where-Object { $references -contains $_ })) "$($variant.Name) does not reference another variant"
}

$d3d11Surface = Get-GraphicsDeviceSurface -Assembly $d3d11Assembly
$d3d12Surface = Get-GraphicsDeviceSurface -Assembly $d3d12Assembly
$vulkanSurface = Get-GraphicsDeviceSurface -Assembly $vulkanAssembly

Assert-True ($d3d11Surface.Count -gt 0) 'GraphicsDevice exposes a public surface'
Assert-True (($d3d11Surface -join "`n") -eq ($d3d12Surface -join "`n")) 'Direct3D11 and Direct3D12 expose identical GraphicsDevice surfaces'
Assert-True (($d3d11Surface -join "`n") -eq ($vulkanSurface -join "`n")) 'Direct3D11 and Vulkan expose identical GraphicsDevice surfaces'

Write-Step 'Library consumer (transitive selection)'
$build = Invoke-Build -Project 'consumers/LibraryApp/LibraryApp.csproj' -Scenario 'library-app' -RuntimeIdentifier 'win-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds when only a library references Stride.Graphics'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'library-app' -RuntimeIdentifier 'win-x64' -AssemblyName 'LibraryApp'
Assert-True ($run.ExitCode -eq 0) 'Application runs'
Assert-OutputContains $run 'platform=Windows' 'The app gets the platform define through the library'
Assert-OutputContains $run 'library-compile-time=Direct3D11' 'Library gets the compile-time define'
Assert-OutputContains $run 'api=Direct3D11' 'App and library compile against the same variant'
Assert-OutputContains $run 'library-device=Direct3D11 / Direct3D11 (simulated) / Level_11_0' 'Library device uses the Direct3D11 variant'
Assert-OutputContains $run 'library-profiling=PIX:WinPixEventRuntime.dll' 'Library gets the Direct3D conditional package transitively'
Assert-OutputContains $run 'native-present=True' 'Transitive selection copies the native asset'

$outputDir = Get-OutputDirectory -Scenario 'library-app' -RuntimeIdentifier 'win-x64'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Direct3D12.dll'))) 'Transitive selection prunes Direct3D12'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.Graphics.Vulkan.dll'))) 'Transitive selection prunes Vulkan'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll')) 'Transitive selection keeps PIX for Direct3D'
Assert-True (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll')) 'Transitive selection keeps WARP for Direct3D'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll'))) 'Transitive selection prunes Lavapipe for Direct3D'

Write-Step 'Direct variant reference (escape hatch)'
$build = Invoke-Build -Project 'consumers/VariantApp/VariantApp.csproj' -Scenario 'variant-direct' -RuntimeIdentifier 'win-x64'
Assert-True ($build.ExitCode -eq 0) 'Build succeeds with a direct Stride.Graphics.Direct3D11 reference'
if ($build.ExitCode -ne 0) { Write-Host $build.Output }

$run = Invoke-App -Scenario 'variant-direct' -RuntimeIdentifier 'win-x64' -AssemblyName 'VariantApp'
Assert-True ($run.ExitCode -eq 0) 'Application runs'
Assert-OutputContains $run 'api=Direct3D11' 'Direct variant reference works without the meta-package'
Assert-OutputContains $run 'native-present=True' 'Direct variant reference copies its native asset'

$outputDir = Get-OutputDirectory -Scenario 'variant-direct' -RuntimeIdentifier 'win-x64'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Pix.dll'))) 'Direct variant reference bypasses meta-package conditionals (PIX)'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Warp.dll'))) 'Direct variant reference bypasses meta-package conditionals (WARP)'
Assert-True (-not (Test-Path (Join-Path $outputDir 'Stride.MetaExperiment.Lavapipe.dll'))) 'Direct variant reference bypasses meta-package conditionals (Lavapipe)'

Write-Step 'Validation: unknown API'
$build = Invoke-Build -Project 'consumers/ConsumerApp/ConsumerApp.csproj' -Scenario 'invalid-api' -RuntimeIdentifier 'win-x64' -ExtraProperties @('-p:StrideGraphicsApi=Metal')
Assert-True ($build.ExitCode -ne 0) 'Build fails for an unknown API'
Assert-OutputContains $build 'is not provided by any Stride.Graphics variant' 'Error explains the unknown API'

Write-Step 'Validation: API not supported on the target platform'
$build = Invoke-Build -Project 'consumers/ConsumerApp/ConsumerApp.csproj' -Scenario 'unsupported-platform' -RuntimeIdentifier 'linux-x64' -ExtraProperties @('-p:StrideGraphicsApi=Direct3D12')
Assert-True ($build.ExitCode -ne 0) 'Build fails for Direct3D12 on Linux'
Assert-OutputContains $build "is not supported on platform 'Linux'" 'Error explains the platform mismatch'

Write-Step 'Pruning: wrong variant type does not compile'
$build = Invoke-Build -Project 'consumers/WrongApiApp/WrongApiApp.csproj' -Scenario 'wrong-api' -RuntimeIdentifier 'win-x64' -ExtraProperties @('-p:StrideGraphicsApi=Vulkan')
Assert-True ($build.ExitCode -ne 0) 'Build fails when source code uses a pruned variant'
Assert-OutputContains $build 'error CS0234' 'Compiler error reports the missing variant namespace'
Assert-OutputContains $build 'Direct3D11' 'Compiler error points at the pruned variant'

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
