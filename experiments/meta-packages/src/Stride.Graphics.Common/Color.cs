namespace Stride.Graphics;

/// <summary>
/// Straight-alpha RGBA color. A shared value type, used by every variant.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>Creates a color from its components.</summary>
    public Color(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>Red component.</summary>
    public float R { get; }

    /// <summary>Green component.</summary>
    public float G { get; }

    /// <summary>Blue component.</summary>
    public float B { get; }

    /// <summary>Alpha component.</summary>
    public float A { get; }

    /// <summary>Opaque white.</summary>
    public static Color White => new(1f, 1f, 1f);

    /// <summary>Opaque black.</summary>
    public static Color Black => new(0f, 0f, 0f);

    /// <summary>Transparent.</summary>
    public static Color Transparent => new(0f, 0f, 0f, 0f);

    /// <inheritdoc />
    public bool Equals(Color other) => R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    /// <inheritdoc />
    public override string ToString() => $"RGBA({R}, {G}, {B}, {A})";
}
