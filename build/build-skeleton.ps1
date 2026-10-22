<#
.SYNOPSIS
    Builds the code-free Stride skeleton to validate the build system quickly.

.DESCRIPTION
    Runs ordinary `dotnet build` against the mirrored Stride build graph. There is no
    application source code here: the build/skeleton layer links a stub entry point and
    removes items that point at files which were intentionally not mirrored.

    Use it after changing something in a real Stride checkout: mirror the structure with
    tools/mirror-stride-structure.ps1, then build one of the targets below. A failure here
    means the change broke project evaluation or the MSBuild target graph, not the runtime.

.PARAMETER Target
    Runtime  (default) build/Stride.Runtime.slnf         engine + core, fast smoke test
    Tests              build/Stride.Tests.Simple.slnf    all desktop test projects
    Sdk                sources/sdk/Stride.Build.Sdk.slnx the MSBuild SDK itself
    Full               build/Stride.slnx                 every managed project (119)
    Pack               like Full, but packaging is enabled (StrideSkeletonPack=true)

.PARAMETER Solution
    Explicit .slnx/.slnf/.csproj to build. Overrides -Target.

.PARAMETER Configuration
    Build configuration. Defaults to Debug.

.PARAMETER Pack
    Enable the Stride packing targets (NuGet package generation). Off by default because
    packing a code-free tree is only useful when validating the packaging graph.

.PARAMETER Clean
    Delete bin/obj directories under the skeleton before building.

.PARAMETER NoRestore
    Skip `dotnet restore` (passed as --no-restore).

.PARAMETER Verbosity
    MSBuild verbosity. Defaults to minimal.

.EXAMPLE
    pwsh -File build\build-skeleton.ps1

.EXAMPLE
    pwsh -File build\build-skeleton.ps1 -Target Full -Pack -Verbosity normal
#>
[CmdletBinding()]
param(
    [ValidateSet('Runtime', 'Tests', 'Sdk', 'Full', 'Pack')]
    [string] $Target = 'Runtime',

    [string] $Solution,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [switch] $Pack,

    [switch] $Clean,

    [switch] $NoRestore,

    [string] $Verbosity = 'minimal'
)

$ErrorActionPreference = 'Stop'

$root = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))

$targets = [ordered]@{
    Runtime = 'build\Stride.Runtime.slnf'
    Tests   = 'build\Stride.Tests.Simple.slnf'
    Sdk     = 'sources\sdk\Stride.Build.Sdk.slnx'
    Full    = 'build\Stride.slnx'
    Pack    = 'build\Stride.slnx'
}

if ($Pack -or $Target -eq 'Pack') {
    $pack = $true
}

$relativeSolution = if ($Solution) { $Solution } else { $targets[$Target] }
$solutionPath = if ([System.IO.Path]::IsPathRooted($relativeSolution)) {
    $relativeSolution
} else {
    Join-Path $root $relativeSolution
}

if (-not (Test-Path -LiteralPath $solutionPath)) {
    throw "Build target not found: $solutionPath"
}

if ($Clean) {
    Write-Host 'Removing project bin/obj directories...' -ForegroundColor DarkGray
    # Keep the local NuGet feed (bin/packages) created by the mirror script.
    $rootBin = Join-Path $root 'bin'
    Get-ChildItem -LiteralPath $root -Recurse -Directory -Force |
        Where-Object {
            $_.Name -in @('bin', 'obj') -and
            $_.FullName -ne $rootBin -and
            -not $_.FullName.StartsWith($rootBin + [System.IO.Path]::DirectorySeparatorChar, [System.StringComparison]::OrdinalIgnoreCase)
        } |
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}

$arguments = @(
    'build', $solutionPath,
    '--configuration', $Configuration,
    '--verbosity', $Verbosity,
    '--nologo'
)

if ($NoRestore) { $arguments += '--no-restore' }
if ($pack) { $arguments += '-p:StrideSkeletonPack=true' }

$display = [System.IO.Path]::GetRelativePath($root, $solutionPath)
Write-Host ''
Write-Host "slim-sdk build: $display ($Configuration)" -ForegroundColor Cyan
if ($pack) { Write-Host '  packaging enabled (StrideSkeletonPack=true)' -ForegroundColor DarkGray }
Write-Host ''

& dotnet @arguments
$exitCode = $LASTEXITCODE

Write-Host ''
if ($exitCode -eq 0) {
    Write-Host 'slim-sdk build succeeded.' -ForegroundColor Green
} else {
    Write-Host "slim-sdk build failed (exit code $exitCode)." -ForegroundColor Red
}

exit $exitCode
