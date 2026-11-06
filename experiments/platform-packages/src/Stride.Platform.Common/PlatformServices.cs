namespace Stride.Platform;

/// <summary>
/// Platform primitives handed to platform-neutral code (Stride.AppModel).
/// Delegates only: no reflection, no interfaces, no virtual dispatch.
/// The application binds to the concrete PlatformHost at compile time and
/// passes this bundle to the library.
/// </summary>
public sealed class PlatformServices
{
    public required PlatformInfo Platform { get; init; }

    public required WindowSystemKind WindowSystem { get; init; }

    public required Func<SurfaceDescriptor> CreateSurface { get; init; }

    public required Action<Action> Run { get; init; }
}
