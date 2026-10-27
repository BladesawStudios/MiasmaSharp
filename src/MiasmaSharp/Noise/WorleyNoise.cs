using static MiasmaSharp.Noise.NoiseHash;

namespace MiasmaSharp.Noise;

/// <summary>agl's <c>noise_worley</c> program in its simple form: one minus the distance to the nearest feature point.</summary>
/// <remarks>
/// Each of the 27 cells around the point holds one feature point, placed by a sine hash of the wrapped cell. A
/// second sine hash of that point gives each cell a value that <see cref="FeatureIntensity"/> adds to its distance,
/// and the result is one minus the smallest, clamped to 0..1. Checked against the decompiled program; the cells
/// agree as closely as the sines do (see <see cref="NoiseHash"/>).
/// </remarks>
public sealed record WorleyNoise(float FeatureIntensity) : INoise3D
{
    public float Sample(float x, float y, float z, float period)
    {
        float ix = MathF.Floor(x), iy = MathF.Floor(y), iz = MathF.Floor(z);
        float fx = x - ix, fy = y - iy, fz = z - iz;
        float best = float.MaxValue;

        for (int dz = -1; dz <= 1; dz++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    float n = Sine(Wrap(ix + dx, period), Wrap(iy + dy, period), Wrap(iz + dz, period), 17f, 59.4f, 15f);
                    float px = Fract(n * 262144f), py = Fract(n * 32768f), pz = Fract(n * 2097152f);
                    float ex = fx - (px + dx), ey = fy - (py + dy), ez = fz - (pz + dz);
                    float value = Fract(4096f * Sine(px + 74.2f, py - 124.5f, pz + 99.4f, 17f, 59.4f, 15f));
                    best = MathF.Min(best, MathF.Sqrt(ex * ex + ey * ey + ez * ez) + FeatureIntensity * value);
                }

        return 1f - Math.Clamp(best, 0f, 1f);
    }
}
