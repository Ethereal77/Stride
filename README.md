# Stride build-system test bed

A replica of the project structure and build system of Stride, **without source code**.
It validates changes to `.csproj`, `.props`, `.targets`, `.slnx` and build scripts
in seconds, without building the real Stride.

The real repository content is mirrored with `tools/mirror-stride-structure.ps1`.
Everything that is not mirrored (skeleton layer, scripts and this guide) lives only
here and is never deleted.

## Current status

| Item | Result |
| --- | --- |
| `build/Stride.Runtime.slnf` | ✅ 0 errors (`dotnet build`) |
| `build/Stride.Tests.Simple.slnf` | ✅ 0 errors |
| `sources/sdk/Stride.Build.Sdk.slnx` | ✅ 0 errors |
| `build/Stride.slnx` (119 projects) | ✅ 0 errors |
| `build/Stride.AssemblyProcessor.slnx` | ✅ 0 errors |
| `build-windows-full-msbuild.yml` (`msbuild build\Stride.slnx`) | ✅ with Visual Studio + MSBuild + C++ workload |
| `sources/native/WindowsProjects/*.vcxproj` | ✅ with Visual Studio + MSBuild + C++ workload |
| `.github` (52 workflows + 6 actions) | ✅ static verification, 0 errors |

Expected warnings:
- NuGet vulnerability advisories for packages already pinned by Stride.
- MSBuild cannot resolve native assemblies that were not mirrored
  (`BulletSharp`, `Stride.Core.AssemblyProcessor`).

Approximate times with a warm NuGet cache:
- `Runtime` ~6 s
- `Full` ~12 s
- `Sdk` ~2 s.

The first restore takes longer because it downloads packages.

## Building

```powershell
# Runtime smoke test (default)
pwsh build/build-skeleton.ps1

# Other shortcuts
pwsh build/build-skeleton.ps1 -Target Tests
pwsh build/build-skeleton.ps1 -Target Sdk
pwsh build/build-skeleton.ps1 -Target Full
pwsh build/build-skeleton.ps1 -Target Full -Pack   # enables NuGet packaging

# A specific project or solution
pwsh build/build-skeleton.ps1 -Solution sources/core/Stride.Core/Stride.Core.csproj
```

Options: `-Configuration Release`, `-Clean`, `-NoRestore`, `-Verbosity normal`.

Calling `dotnet` directly from the repository root also works:

```powershell
dotnet build build/Stride.Runtime.slnf
dotnet build build/Stride.slnx -p:StrideSkeletonPack=true
```

`-Pack` enables auto-pack during `Build` (as in Stride).
To pack a single project with `dotnet pack`, add `-p:StrideSkipAutoPack=true` to avoid the
`Pack`/`GeneratePackageOnBuild` recursion, exactly like in the real repository:

```powershell
dotnet pack sources/core/Stride.Core/Stride.Core.csproj -p:StrideSkipAutoPack=true
```

## Re-synchronizing with Stride

```powershell
pwsh tools/mirror-stride-structure.ps1
```

- Copies projects (`.csproj`, `.vcxproj`, `.shproj`, `.projitems`),
  build files (`.props`, `.targets`, `.build`, `.proj`, `.slnx`, `.slnf`),
  scripts (`.bat`, `.cmd`, `.ps1`, `.sh`),
  packaging files (`.nuspec`, `.runsettings`, `.ruleset`, `.template`)
  and support files (`.config`, `.manifest`, `.ico`, `.snk`).

- Also copies `docs/build` and `.github` in full
  (workflows, composite actions, templates and `repo-file-guard`),
  plus the NuGet icons.

- **Does not** copy code (`.cs`), XAML, shaders, assets or `.resx`.

- **Does not** copy the upstream root `README.md`: in this branch this guide is the README.

- Overwrites mirrored files, but does not delete files that only exist in the skeleton.

- Preserves each file's original path, so `StrideRoot`, relative imports
  and `ProjectReference` entries keep working.

To update a specific `.csproj`/`.targets`, just run the script again.

## GitHub Actions

`.github` is mirrored in full:
52 workflows, 6 composite actions and the support files (`repo-file-guard`, templates).

Nothing is executed; the definitions and everything they reference are validated against the skeleton:

```powershell
python tools/verify-github-workflows.py
```

Requires Python 3 and PyYAML. It exits with code 0 when there are no errors and writes
the report to `build/skeleton/github-workflows-report.md`.

What it checks:

- Valid YAML without duplicate keys, and the correct workflow/job/step shape.

- Local `uses: ./...` entries:
  + Reusable workflows and composite actions exist,
  + Their `with:` keys are declared,
  + and no required input/secret is missing.

- Internal wiring:
  `needs`, `needs.*.outputs.*`, `steps.*.outputs.*`, `matrix.*`,
  `inputs.*` and cycles between reusable workflows.

- Targets of `dotnet build|pack|restore|test|run|publish`, `msbuild`
  and invoked scripts (`pwsh`, `bash`, `python`, `node`).

- Repository paths mentioned in `run:` blocks:
  if they exist in Stride but not here they are reported as an expected mirror gap;
  if they exist in neither tree and the workflow does not create them
  (`git clone`, `mkdir`, `checkout path:`), they are reported as errors.

Current result: **52 workflows + 6 actions, 0 errors, 0 warnings**. The CI entry points
(`build/Stride.Runtime.slnf`, `build/Stride.Tests.*.slnf`, `build/Stride.slnx`,
`build/Stride.iOS.slnf`, `build/Stride.Android.slnf`, `tests/android|ios` scripts, etc.)
exist in the skeleton and the referenced solutions build.

Each unit is also classified by what it would additionally need to run for real:

| Profile | Meaning |
| --- | --- |
| `dotnet-build` | Builds here (verified with `build-skeleton.ps1`). |
| `tests` | Builds, but there is no code or assets to execute. |
| `native` | Needs cmake/ninja/xcodebuild/NDK and sources cloned at runtime. |
| `visual-studio` | Needs Visual Studio MSBuild (`build-windows-full-msbuild.yml`). |
| `publish` | Needs NuGet/OIDC/gh release and deployment environments. |
| `secrets`, `environment` | Requires secrets and environments from the real repository. |
| `git`, `git-lfs` | Requires a Stride git checkout with history/LFS. |

Execution limitations:
- Test workflows have nothing to execute,
- `dep-*` workflows clone native sources that are not part of the repository,
- `release-*` workflows need secrets,
- and `repo-file-guard.yml` inspects the git/LFS history.

None of that can be exercised locally, but the structure and local references are validated.

## Skeleton layer

No mirrored file is modified. The adaptation is additive:

| File | Purpose |
| --- | --- |
| `build/skeleton/Skeleton.targets` | The only adaptation layer (properties + targets). |
| `build/skeleton/StrideSkeletonEntryPoint.cs` | Empty `Main` linked into `Exe`/`WinExe` projects. |
| `Directory.Build.targets` | Imports the layer from the root. |
| `sources/Directory.Build.targets` | Imports the layer for `/sources`. |
| `tests/Directory.Build.targets` | Imports the layer for `/tests`. |
| `samples/Directory.Build.targets` | Imports the layer for `/samples`. |

MSBuild only imports the nearest `Directory.Build.targets`, hence the four shims.
The original repository has none, so the mirror never overwrites them.

What `Skeleton.targets` does:

1. **Disables everything that needs content which does not exist**:
   auto-pack, signing, assembly processor, asset compilation, default shaders,
   multi-API hosts, localization, the UI NuGet resolver and native compilation.

2. **Removes items that point to files that were not mirrored**
   (`Compile`, `Page`, `EmbeddedResource`, `Content`, `None`, etc.) right before
   they are consumed by `CoreCompile`, `MarkupCompilePass1`, `GenerateResource`,
   `AssignTargetPaths`...
   The filtering runs in a target rather than during evaluation because MSBuild
   does not allow item metadata in `Remove` conditions (`MSB4190`).

3. **Removes missing `StrideNativeLib` entries** before `Stride.Dependencies.targets`
   tries to copy them.

4. **Refreshes the `Stride.Core.AssemblyProcessor` output list** before its
   `CopyFiles`/`GenerateHash` targets run.
   `BuiltItems` is expanded during evaluation, before the assembly exists,
   so a clean tree fails with `MSB3954` ("cannot calculate the hash of the generated
   file because it does not exist").

5. **Adds the empty entry point** and forces `GenerateProgramFile=false` +
   `StartupObject=StrideSkeleton.Program`.

6. **Redirects the worktree-version ledger** to `build/skeleton/.worktree-ids.txt`
   and disables the git query (the skeleton is not a repository).

## Required stubs

- `build/skeleton/StrideSkeletonEntryPoint.cs`:
  `Main` for executables and tests.

- `sources/shared/Stride.NuGetResolver.Targets/NuGetResolverModuleInitializer.cs`:
  the `.projitems` reads it with `ReadAllText()` during **evaluation**,
  before any target can filter it.
  The mirror script creates an inert placeholder when it does not exist.

Neither of them ends up in a real binary: they only let the MSBuild graph run.

## Known limitations

- **Native**:
  `.vcxproj` projects need Visual Studio (desktop MSBuild) and the C++ workload.

- **`.resx`**:
  not mirrored because they reference images that are not copied either,
  and MSBuild fails while generating resources (`MSB3103`).
  Projects that declare them build without resources.

- **Empty code**:
  the resulting assemblies do not contain the real implementation.
  They cannot run anything; they only validate evaluation, dependencies
  and target ordering.

- **Tests**:
  they compile but are not executed (`dotnet test` has nothing to run).

- **NuGet**:
  the global cache and the local `bin/packages` feed are used exactly as in Stride.
  Outside `slim-sdk`, only that global cache is written.

- `build/Stride.Local.props` is generated by the SDK on the first build;
  this is expected.

## Layout

```
slim-sdk/
├─ build/            # Solutions, .build, scripts and build-skeleton.ps1
│  └─ skeleton/      # Adaptation layer (only here)
├─ sources/          # Engine, core, editor, sdk, tools projects...
├─ tests/            # End-user / editor tests
├─ samples/          # Sample projects
├─ docs/build/       # Mirrored build documentation
├─ deps/             # Native checkout scripts (no binaries)
├─ .github/          # Workflows, composite actions and templates
├─ tools/            # Mirror-stride-structure.ps1 + workflow verifier
├─ bin/packages/     # Local NuGet feed
└─ README.md         # This guide
```
