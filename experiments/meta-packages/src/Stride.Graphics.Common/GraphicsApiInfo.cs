namespace Stride.Graphics;

/// <summary>
/// Static facts about the graphics APIs the meta-package knows about.
/// Pure compile-time/static helpers: nothing here is used by the render loop.
/// </summary>
public static class GraphicsApiInfo
{
    private static readonly GraphicsApi[] AllApis =
    [
        GraphicsApi.Direct3D11,
        GraphicsApi.Direct3D12,
        GraphicsApi.Vulkan,
    ];

    private static readonly string[] WindowsOnly = ["Windows"];
    private static readonly string[] Everywhere = ["Windows", "Linux", "macOS", "Android", "iOS"];

    /// <summary>Every API the meta-package knows about.</summary>
    public static IReadOnlyList<GraphicsApi> All => AllApis;

    /// <summary>Platforms on which <paramref name="api"/> can run.</summary>
    public static IReadOnlyList<string> GetSupportedPlatforms(GraphicsApi api) => api switch
    {
        GraphicsApi.Direct3D11 or GraphicsApi.Direct3D12 => WindowsOnly,
        GraphicsApi.Vulkan => Everywhere,
        _ => [],
    };

    /// <summary>Parses an API name (case-insensitive, a few aliases are accepted).</summary>
    public static bool TryParse(string? value, out GraphicsApi api)
    {
        api = GraphicsApi.None;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        switch (value.Trim().ToUpperInvariant())
        {
            case "DIRECT3D11":
            case "D3D11":
            case "DX11":
                api = GraphicsApi.Direct3D11;
                return true;
            case "DIRECT3D12":
            case "D3D12":
            case "DX12":
                api = GraphicsApi.Direct3D12;
                return true;
            case "VULKAN":
            case "VK":
                api = GraphicsApi.Vulkan;
                return true;
            default:
                return false;
        }
    }

    /// <summary>Parses an API name or throws with the list of valid values.</summary>
    public static GraphicsApi Parse(string value) =>
        TryParse(value, out var api)
            ? api
            : throw new ArgumentException(
                $"Unknown graphics API '{value}'. Valid values: {string.Join(", ", AllApis)}.", nameof(value));
}
