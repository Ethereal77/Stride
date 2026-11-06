#if STRIDE_PLATFORM_WINDOWS || STRIDE_PLATFORM_LINUX || STRIDE_PLATFORM_MACOS || STRIDE_PLATFORM_ANDROID || STRIDE_PLATFORM_IOS
#error A portable library must not resolve a platform, even through Stride.Games.
#endif

#if !STRIDE_PLATFORM_PORTABLE
#error A portable library should define STRIDE_PLATFORM_PORTABLE.
#endif

#if STRIDE_GRAPHICS_API_DIRECT3D11 || STRIDE_GRAPHICS_API_VULKAN
#error A portable library must not resolve a graphics API.
#endif

using Stride.AppModel;
using Stride.Games;
using Stride.Platform;

namespace PortableGameLib;

/// <summary>
/// Reusable, platform-neutral game code. The host application supplies the
/// platform services; this library never mentions PlatformHost.
/// </summary>
public sealed class HeadlessGame : Game
{
    public HeadlessGame(PlatformServices services)
        : base(services)
    {
    }

    public int RecordedFrames { get; private set; }

    protected override void Draw()
    {
        base.Draw();
        RecordedFrames++;
    }
}
