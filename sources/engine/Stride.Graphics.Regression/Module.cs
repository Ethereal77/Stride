// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Stride.Graphics.Regression;

internal static class Module
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Crash-dialog suppression + (opt-in) SEH minidump capture. Shared with Stride.Games.AutoTesting.
        NativeCrashHandler.Install();

        // Crash-report popups must never interrupt a test run: unless the caller chose a mode, tools
        // spawned by tests (asset compiler etc.) capture silently. Inherited by child processes.
        if (Environment.GetEnvironmentVariable("STRIDE_CRASH_MODE") == null)
            Environment.SetEnvironmentVariable("STRIDE_CRASH_MODE", "save");

        // Default to software rendering unless STRIDE_TESTS_GPU=1 is set.
        // This ensures Test Explorer and dotnet test match the gold images out of the box.
        var gpu = Environment.GetEnvironmentVariable("STRIDE_TESTS_GPU");
        if (gpu != "1")
        {
            Environment.SetEnvironmentVariable("STRIDE_GRAPHICS_SOFTWARE_RENDERING", "1");

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIDE_MAX_PARALLELISM")))
                Environment.SetEnvironmentVariable("STRIDE_MAX_PARALLELISM", "8");

#if STRIDE_GRAPHICS_API_VULKAN && STRIDE_PLATFORM_DESKTOP
            ConfigureLavapipe();
#endif
        }
    }

#if STRIDE_GRAPHICS_API_VULKAN && STRIDE_PLATFORM_DESKTOP
    // Isolated so the JIT only loads Stride.Dependencies.Lavapipe (whose ModuleInitializer
    // sets VK_DRIVER_FILES) when this method actually runs. Inlining it into Initialize() would
    // resolve the Lavapipe type ref at Initialize's JIT time and load the assembly on the GPU
    // path too.
    // Desktop only: the Lavapipe package ships win/linux software-Vulkan (see csproj) and
    // throws from its ModuleInitializer on mobile; Android provide Vulkan via emulator/device.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ConfigureLavapipe()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VK_DRIVER_FILES")))
            Stride.Dependencies.Lavapipe.Lavapipe.TryConfigure();
    }
#endif
}
