# Cross-platform notes

The launcher is in the middle of a Windows → Avalonia cross-platform port (`xplat-launcher` stream). It targets `net10.0` with RIDs `win-x64`; WPF has been fully replaced by Avalonia 12. This file lists the code paths that still behave differently per OS and what the remaining gaps are.

## Executable shape

| Platform | Game Studio file | How it is started |
|---|---|---|
| Windows | `Stride.GameStudio.Avalonia.Desktop.exe` (with fallback to `Stride.GameStudio.exe`) | `Process.Start(exe, args)` |

The choice is in `StrideVersionViewModel.UpdateAvailableEditors` and `DotNetHostSelector.NativeStartInfo`, which invokes `dotnet` when there is no apphost next to the dll.

## Windows-only code paths

Searching for `OperatingSystem.IsWindows()` in the launcher shows the remaining divergences.

### Prerequisites installer

`StrideStoreVersionViewModel.RunPrerequisitesInstaller` runs `{InstallPath}/Bin/Prerequisites/install-prerequisites.exe`. That binary is a Windows installer — it is simply skipped on non-Windows since the DirectX / .NET prerequisites it ships don't apply.

### Visual Studio integration

`VsixVersionViewModel` relies on `Stride.Core.CodeEditorSupport.VisualStudio.VisualStudioVersions` to find installed VS instances. This uses `vswhere` internally.

### Installers

Advanced Installer projects ([Prerequisites/](../../sources/launcher/Prerequisites/), [Setup/](../../sources/launcher/Setup/)) are Windows-only by construction. The MSBuild `PackageInstaller` target silently skips when `AdvancedInstaller.com` is not on `PATH`. See [packaging.md](packaging.md).

## Telemetry and privacy policy

Both `Stride.Metrics` / `MetricsClient` (telemetry) and `PrivacyPolicyHelper` (first-run consent prompt, uninstall-time revoke) have been **intentionally and permanently removed** from the launcher. They will not be ported. No cleanup of legacy privacy-policy state is performed on uninstall — telemetry was removed, so any residual registry keys or settings files from the old WPF launcher are harmless orphans and do not need to be scrubbed.

## Launcher ↔ GameStudio IPC

When `AutoCloseLauncher` is on, the launcher passes IPC information to Game Studio via a CLI argument so Game Studio can signal back once it has started.

| Platform | Argument | Mechanism |
|---|---|---|
| Windows | `/LauncherWindowHandle <hwnd>` | Game Studio sends a Win32 `WM_CLOSE` message to the launcher's HWND. `MainWindow.OnOpened` captures the HWND via `TryGetPlatformHandle()`. |

Both paths are implemented:
- **Launcher side:** `MainViewModel.StartStudio` — branches on `OperatingSystem.IsWindows()`. Non-Windows path starts `WaitForGameStudioPipeSignalAsync` and injects `/LauncherPipe <name>`. `MainWindow.axaml.cs` wires `CloseRequested → OnClosingAsync`.
- **Game Studio side:** `Program.ParseLauncherArgs` (in `Stride.GameStudio.Avalonia.Desktop`) parses both argument styles and sets `App.LauncherNotifier`. `MainWindow.OnLoaded` fires it once.

## Settings paths

All config paths go through `EditorPath` (linked file from `Stride.Core.Assets.Editor`). `EditorPath.UserDataPath` resolves to:

- `%LocalAppData%\Stride\` on Windows

The launcher writes `LauncherSettings.conf` and the `launcher.lock` single-instance marker under these paths. No OS-specific branching is needed.

## Icons

Window icons (`Launcher.ico`) are served through Avalonia's resource system. The `.ico` format is used on every platform; Avalonia picks the best-matching size at runtime.

## Recent-project "Show in Explorer"

`RecentProjectViewModel.Explore` reveals the selected recent project in the platform's native file manager:

- **Windows:** `explorer.exe /select,{path}` (unchanged from master).

All failures are swallowed silently (no dialog, no crash) since they are not actionable from inside the launcher.

## Testing surface

When exercising changes, run the launcher on Windows.

- Self-update through a reinstall (`reinstall-below`: downloads a `StrideSetup.exe`, Windows only).
- First-install VSIX prompt (no VS instances).
- Prerequisites installer on first run.
