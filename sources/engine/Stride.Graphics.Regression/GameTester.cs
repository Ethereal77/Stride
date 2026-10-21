// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Diagnostics;
using Stride.Core.Diagnostics;
using Stride.Engine;
using Stride.Games;

namespace Stride.Graphics.Regression
{
    /// <summary>
    ///   Provides functionality to execute game tests in a platform-specific environment.
    /// </summary>
    public class GameTester
    {
        public static readonly Logger Logger = GlobalLogger.GetLogger("GameTester");


        /// <summary>
        ///   Runs the specified game test in a platform-specific environment.
        /// </summary>
        /// <param name="game">The <see cref="Game"/> instance to run. This parameter cannot be <see langword="null"/>.</param>
        /// <remarks>
        ///   This method handles the execution of the <paramref name="game"/> differently depending
        ///   on the target platform:
        ///   <list type="bullet">
        ///     <item>
        ///       On desktop platforms, the game is executed in a blocking manner.
        ///     </item>
        ///   </list>
        /// </remarks>
        public static void RunGameTest(Game game)
        {
#if STRIDE_PLATFORM_DESKTOP

            using (game)
            {
                try
                {
                    // Use headless context when not in interactive mode (no window/display needed)
                    var context = game is GameTestBase { ScreenShotAutomationEnabled: true }
                        ? GameContextFactory.NewGameContext(AppContextType.Headless)
                        : null; // null = auto-detect (creates a window)
                    game.Run(context);
                }
                finally
                {
                    // End/Discard RenderDoc capture while the device is still alive (before Dispose/Destroy)
                    if (game is GameTestBase testGame)
                        testGame.EndOrDiscardRenderDocCapture();
                }
            }

            // Log process memory for diagnostics (helps detect resource leaks across tests)
            using var process = Process.GetCurrentProcess();
            Logger.Info($"Process memory: working set={process.WorkingSet64 / 1024 / 1024}MB, private={process.PrivateMemorySize64 / 1024 / 1024}MB, GC={GC.GetTotalMemory(false) / 1024 / 1024}MB");

#endif
        }
    }
}
