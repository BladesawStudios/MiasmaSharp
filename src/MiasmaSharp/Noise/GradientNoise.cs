using static MiasmaSharp.Noise.NoiseHash;

namespace MiasmaSharp.Noise;

/// <summary>agl's <c>noise_gradient</c> program: tiling 3D gradient noise in its simple, fbm and ridge forms.</summary>
/// <remarks>
/// Each lattice corner's gradient is <see cref="NoiseHash.Direction"/> of the wrapped corner, blended with the
/// quintic fade and scaled by <see cref="ValueScale"/>. <see cref="GradientNoiseKind.Simple"/> maps that to 0..1;
/// <see cref="GradientNoiseKind.Fbm"/> averages four octaves of it at weights 1, 1/2, 1/4, 1/8; and
/// <see cref="GradientNoiseKind.Ridge"/> takes each octave's ridge, <c>clamp(1 - |n|)</c> raised to
/// <see cref="FeatureIntensity"/>, and multiplies the first by the weighted mean of the other three. Every octave
/// doubles both the coordinate and the period, so all of them tile. Checked against the decompiled program to 1e-4.
/// </remarks>
public sealed record GradientNoise(GradientNoiseKind Kind, float ValueScale, float FeatureIntensity) : INoise3D
{
    public float Sample(float x, float y, float z, float period) => Kind switch
    {
        GradientNoiseKind.Simple => Octave(x, y, z, period) * 0.5f + 0.5f,
        GradientNoiseKind.Fbm => Fbm(x, y, z, period),
        GradientNoiseKind.Ridge => Ridge(x, y, z, period),
        _ => throw new NotSupportedException($"Gradient noise kind {Kind}"),
    };

    private float Fbm(float x, float y, float z, float period)
    {
        float total = 0f, weight = 0f, amplitude = 1f;
        for (int i = 0; i < 4; i++, x *= 2f, y *= 2f, z *= 2f, period *= 2f, amplitude *= 0.5f)
        {
            total += (Octave(x, y, z, period) * 0.5f + 0.5f) * amplitude;
            weight += amplitude;
        }
        return total / weight;
    }

    private float Ridge(float x, float y, float z, float period)
    {
        Span<float> r = stackalloc float[4];
        for (int i = 0; i < 4; i++, x *= 2f, y *= 2f, z *= 2f, period *= 2f)
            r[i] = MathF.Pow(Math.Clamp(1f - MathF.Abs(Octave(x, y, z, period)), 0f, 1f), FeatureIntensity);
        return (r[1] + 0.5f * r[2] + 0.25f * r[3]) * r[0] / 1.75f;
    }

    private float Octave(float x, float y, float z, float period)
    {
        float ix = MathF.Floor(x), iy = MathF.Floor(y), iz = MathF.Floor(z);
        float fx = x - ix, fy = y - iy, fz = z - iz;
        float ux = Fade(fx), uy = Fade(fy), uz = Fade(fz);

        float Corner(int dx, int dy, int dz)
        {
            var g = Direction(Wrap(ix + dx, period), Wrap(iy + dy, period), Wrap(iz + dz, period));
            return ValueScale * (g.X * (fx - dx) + g.Y * (fy - dy) + g.Z * (fz - dz));
        }

        float x00 = Lerp(Corner(0, 0, 0), Corner(1, 0, 0), ux), x10 = Lerp(Corner(0, 1, 0), Corner(1, 1, 0), ux);
        float x01 = Lerp(Corner(0, 0, 1), Corner(1, 0, 1), ux), x11 = Lerp(Corner(0, 1, 1), Corner(1, 1, 1), ux);
        return Lerp(Lerp(x00, x10, uy), Lerp(x01, x11, uy), uz);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    private static float Lerp(float a, float b, float t) => a + (b - a) * t;
}
