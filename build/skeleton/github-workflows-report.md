# GitHub Actions verification against slim-sdk

- Root: `H:\Stride-Blaze-new\slim-sdk`
- Reference checkout: `H:\Stride-Blaze-new\stride`
- Workflows: 52  |  Composite actions: 6
- Errors: 0  |  Warnings: 0  |  Info: 23

## Per-unit result

| Unit | Kind | Jobs/Steps | Runners | Executing it would need | Status |
| --- | --- | --- | --- | --- | --- |
| `actions/ai-release-notes/action.yml` | action | 1 steps | - | publish, git | OK |
| `actions/set-display-resolution-vdd/action.yml` | action | 4 steps | - | - | OK |
| `actions/set-display-resolution/action.yml` | action | 1 steps | - | - | OK |
| `actions/set-monitor-dpi/action.yml` | action | 1 steps | - | - | OK |
| `actions/stride-checkout/action.yml` | action | 3 steps | - | git-lfs, git | OK |
| `actions/stride-workload/action.yml` | action | 2 steps | - | workloads | OK |
| `workflows/build-android.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-assembly-processor.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-ios.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-launcher.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-linux-runtime.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-vspackage.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-windows-full-msbuild.yml` | workflow | 1 jobs | windows-2025-vs2026 | visual-studio | OK |
| `workflows/build-windows-full.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/build-windows-runtime.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/changes.yml` | workflow | 1 jobs | ubuntu-latest | - | OK |
| `workflows/check-parallel-build.yml` | workflow | 1 jobs | windows-2025-vs2026 | dotnet-build | OK |
| `workflows/contributors-add.yml` | workflow | 1 jobs | ubuntu-latest | secrets, git | OK |
| `workflows/dep-astcenc.yml` | workflow | 4 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-bullet.yml` | workflow | 5 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-celt.yml` | workflow | 1 jobs | macos-15 | git, native | OK |
| `workflows/dep-dxtwrapper.yml` | workflow | 4 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-ffmpeg.yml` | workflow | 4 jobs | macos-15, ubuntu-24.04, windows-11-arm, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-freetype.yml` | workflow | 6 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-gettext.yml` | workflow | 2 jobs | ubuntu-latest, windows-2025-vs2026 | publish, secrets, environment, dotnet-build | OK |
| `workflows/dep-lavapipe.yml` | workflow | 6 jobs | macos-15, ubuntu-24.04, windows-11-arm, windows-2025-vs2026 | native, publish, visual-studio, secrets, git, environment, dotnet-build | OK |
| `workflows/dep-llvm-windows.yml` | workflow | 2 jobs | windows-2025-vs2026 | native, publish, secrets, environment | OK |
| `workflows/dep-moltenvk.yml` | workflow | 1 jobs | macos-15 | git | OK |
| `workflows/dep-msdfgen.yml` | workflow | 4 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/dep-nativepath.yml` | workflow | 1 jobs | macos-15 | git, native | OK |
| `workflows/dep-spirv-cross.yml` | workflow | 3 jobs | windows-2025-vs2026 | native, publish, secrets, git, environment | OK |
| `workflows/dep-spirv-to-dxil.yml` | workflow | 1 jobs | windows-2025-vs2026 | native, visual-studio, git | OK |
| `workflows/dep-spirv-tools.yml` | workflow | 9 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, publish, visual-studio, secrets, git, environment, dotnet-build | OK |
| `workflows/dep-vhacd.yml` | workflow | 4 jobs | macos-15, ubuntu-24.04, windows-2025-vs2026 | native, git | OK |
| `workflows/main.yml` | workflow | 21 jobs | ubuntu-latest | - | OK |
| `workflows/pr-draft-welcome.yml` | workflow | 1 jobs | ubuntu-latest | - | OK |
| `workflows/pr-label-suites.yml` | workflow | 6 jobs | ubuntu-latest | secrets | OK |
| `workflows/pr-screenshot-vision-gate.yml` | workflow | 1 jobs | ubuntu-24.04 | secrets | OK |
| `workflows/release-cli.yml` | workflow | 4 jobs | ubuntu-latest, windows-2025-vs2026 | publish, secrets, git, environment, dotnet-build | OK |
| `workflows/release-deploy.yml` | workflow | 1 jobs | ubuntu-latest | publish, secrets, git, environment | OK |
| `workflows/release-launcher.yml` | workflow | 3 jobs | ubuntu-latest, windows-latest | publish, visual-studio, secrets, git, environment | OK |
| `workflows/release-notes.yml` | workflow | 1 jobs | ubuntu-latest | publish, secrets | OK |
| `workflows/release-rollback.yml` | workflow | 2 jobs | ubuntu-latest | publish, secrets, git, environment | OK |
| `workflows/release-vspackage.yml` | workflow | 3 jobs | ubuntu-latest, windows-2025-vs2026 | publish, visual-studio, secrets, git, environment, dotnet-build | OK |
| `workflows/release.yml` | workflow | 4 jobs | ubuntu-latest, windows-2025-vs2026 | visual-studio, secrets, git, environment, dotnet-build | OK |
| `workflows/repo-file-guard.yml` | workflow | 1 jobs | ubuntu-latest | git | OK |
| `workflows/test-android-game.yml` | workflow | 1 jobs | ubuntu-24.04 | tests | OK |
| `workflows/test-enduser.yml` | workflow | 2 jobs | ${{ matrix.os }} | tests, secrets | OK |
| `workflows/test-gold-gen.yml` | workflow | 8 jobs | ubuntu-latest | tests, git | OK |
| `workflows/test-ios-game.yml` | workflow | 1 jobs | macos-26 | tests, native | OK |
| `workflows/test-linux-game.yml` | workflow | 1 jobs | ubuntu-24.04 | tests | OK |
| `workflows/test-linux-simple.yml` | workflow | 1 jobs | ubuntu-24.04 | tests | OK |
| `workflows/test-macos-game.yml` | workflow | 1 jobs | macos-26 | tests, native | OK |
| `workflows/test-samples-baselines.yml` | workflow | 1 jobs | ubuntu-latest | - | OK |
| `workflows/test-windows-editor.yml` | workflow | 1 jobs | windows-2025-vs2026 | tests, secrets | OK |
| `workflows/test-windows-game-api-neutral.yml` | workflow | 1 jobs | windows-2025-vs2026 | tests | OK |
| `workflows/test-windows-game.yml` | workflow | 1 jobs | windows-2025-vs2026 | tests | OK |
| `workflows/test-windows-simple.yml` | workflow | 1 jobs | windows-2025-vs2026 | tests | OK |

## Info

- **workflows/dep-ffmpeg.yml** `jobs.linux-x64.steps[3]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.linux-x64.steps[4]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.osx-arm64.steps[3]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.osx-arm64.steps[4]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.win-x64.steps[4]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.win-x64.steps[5]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.win-arm64.steps[3]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-ffmpeg.yml** `jobs.win-arm64.steps[4]`: working-directory is created at runtime: 'ffmpeg-src'
- **workflows/dep-gettext.yml** `jobs.pack.steps[2]`: 'gettextnet-src/GNU.Getopt/GNU.Getopt.csproj' is created at runtime
- **workflows/dep-gettext.yml** `jobs.pack.steps[2]`: 'gettextnet-src/GNU.Gettext/GNU.Gettext/GNU.Gettext.csproj' is created at runtime
- **workflows/dep-lavapipe.yml** `jobs.build-windows.steps[8]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-windows.steps[9]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-windows-arm64.steps[8]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-windows-arm64.steps[9]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-linux.steps[3]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-linux.steps[6]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-lavapipe.yml** `jobs.build-macos.steps[3]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-nativepath.yml** `jobs.build.steps[1]`: working-directory is created at runtime: 'NativePath'
- **workflows/dep-spirv-to-dxil.yml** `jobs.build-windows.steps[2]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-spirv-to-dxil.yml** `jobs.build-windows.steps[5]`: working-directory is created at runtime: 'mesa-src'
- **workflows/dep-spirv-to-dxil.yml** `jobs.build-windows.steps[6]`: working-directory is created at runtime: 'mesa-src'
- **workflows/test-enduser.yml** `on.workflow_call.secrets.ANTHROPIC_API_KEY`: secret without 'description' (GitHub tolerates it)
- **workflows/test-windows-editor.yml** `on.workflow_call.secrets.ANTHROPIC_API_KEY`: secret without 'description' (GitHub tolerates it)

## Local references resolved against the skeleton

### Local actions and reusable workflows

- `.github/actions/ai-release-notes`
- `.github/actions/set-display-resolution-vdd`
- `.github/actions/set-monitor-dpi`
- `.github/actions/stride-checkout`
- `.github/actions/stride-workload`
- `workflows/build-android.yml`
- `workflows/build-assembly-processor.yml`
- `workflows/build-ios.yml`
- `workflows/build-linux-runtime.yml`
- `workflows/build-vspackage.yml`
- `workflows/build-windows-full.yml`
- `workflows/build-windows-runtime.yml`
- `workflows/changes.yml`
- `workflows/release-deploy.yml`
- `workflows/test-android-game.yml`
- `workflows/test-enduser.yml`
- `workflows/test-ios-game.yml`
- `workflows/test-linux-game.yml`
- `workflows/test-linux-simple.yml`
- `workflows/test-macos-game.yml`
- `workflows/test-windows-editor.yml`
- `workflows/test-windows-game-api-neutral.yml`
- `workflows/test-windows-game.yml`
- `workflows/test-windows-simple.yml`

### Build/test/script entry points

- `build/Stride.Android.slnf`
- `build/Stride.AssemblyProcessor.slnx`
- `build/Stride.Launcher.slnx`
- `build/Stride.Runtime.slnf`
- `build/Stride.Tests.Game.GPU.slnf`
- `build/Stride.Tests.Game.Runtime.slnf`
- `build/Stride.Tests.Game.slnf`
- `build/Stride.Tests.Simple.Linux.slnf`
- `build/Stride.Tests.Simple.slnf`
- `build/Stride.VisualStudio.slnx`
- `build/Stride.build`
- `build/Stride.iOS.slnf`
- `build/Stride.slnx`
- `build/deps/lavapipe/Stride.Dependencies.Lavapipe.csproj`
- `build/deps/spirv-tools/Stride.Dependencies.SPIRVTools.csproj`
- `build/tools/ParallelBuildAuditor/ParallelBuildAuditor.csproj`
- `build/tools/Stride.CompareGold`
- `deps/Celt/buildApple.sh`
- `sources/editor/Stride.GameStudio.AutoTesting/Stride.GameStudio.AutoTesting.csproj`
- `sources/engine/Stride.Games.AutoTesting/Stride.Games.AutoTesting.csproj`
- `tests/editor/Stride.Editor.Tests.csproj`
- `tests/enduser/Stride.Packaging.Tests/Stride.Packaging.Tests.csproj`
- `tests/enduser/Stride.Samples.Tests/Stride.Samples.Tests.csproj`

### Files present in the real checkout but not mirrored

- none

## How to read this

- **Errors** are structural problems in the mirrored configuration itself: invalid YAML,
  missing local actions/workflows, unknown inputs/outputs, broken command targets.
- **Warnings** are CI references that cannot resolve in the skeleton because the target
  is source code, an asset or a native binary. These are expected and do not indicate a
  broken workflow.
- **Executing it would need** lists what a real run requires beyond the structure:
  `dotnet-build` (verified to compile here), `tests` (compiles, but there is nothing to
  run), `native` (cmake/xcodebuild/NDK), `publish` (NuGet/OIDC), `secrets`,
  `environment`, `git-lfs` (needs a git repository).
- This script never executes a workflow. It validates definitions and local paths only;
  runner availability, secrets, cloud services and `actions/*` versions are out of scope.

### Skeleton-specific notes

- Build workflows (`dotnet build build/Stride.*.sln*`) are structurally valid here and
  the referenced solutions compile locally with `build/build-skeleton.ps1`.
- Test workflows reference mirrored test projects, but the skeleton has no test code or
  assets, so they would compile and then fail at run time.
- `dep-*` workflows build third-party natives from freshly cloned sources; the clones are
  not part of the repository and are not expected in the skeleton.
- `release-*` workflows need repository environments, secrets and OIDC permissions.
- `repo-file-guard.yml` inspects the git tree/LFS metadata, so it needs a real checkout.

