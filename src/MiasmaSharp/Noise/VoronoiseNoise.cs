using static MiasmaSharp.Noise.NoiseHash;

namespace MiasmaSharp.Noise;

/// <summary>agl's <c>noise_voronoise</c> program in its domain-warped form, <c>voronoise_noise_type</c> 2.</summary>
/// <remarks>
/// Voronoise blends the 125 cells around a point by <c>(1 - smootherstep(d / sqrt 3))^k</c>, with
/// <c>k = 1 + 63 (1 - Smoothness)^4</c>, each cell's point pushed along its <see cref="NoiseHash.Direction"/> by
/// <see cref="Jitter"/>. Five octaves on a grid twice as fine as the texture's, each doubling, take the weighted
/// mean offset to the points and add it, divided by the octave's frequency, to a warp; a last pass at
/// <c>p - warp * FeatureIntensity</c> blends each cell's sine-hashed value in -1..1 and scales it by
/// <see cref="ValueScale"/> into 0..1. Checked against the decompiled program to 1e-9 with its own constants.
/// </remarks>
public sealed record VoronoiseNoise(float ValueScale, float FeatureIntensity, float Jitter, float Smoothness) : INoise3D
{
    public float Sample(float x, float y, float z, float period)
    {
        float k = 1f + 63f * MathF.Pow(MathF.Abs(1f - Smoothness), 4f);
        float wx = 0f, wy = 0f, wz = 0f, frequency = 1f;

        for (int octave = 0; octave < 5; octave++, frequency *= 2f)
        {
            float sx = 0f, sy = 0f, sz = 0f, total = 0f;
            Blend(x * frequency * 2f + wx, y * frequency * 2f + wy, z * frequency * 2f + wz, period * 2f * frequency, k,
                (rx, ry, rz, _, _, _, w) => { sx += rx * w; sy += ry * w; sz += rz * w; total += w; });
            if (total > 0f)
            {
                wx += sx / total / frequency;
                wy += sy / total / frequency;
                wz += sz / total / frequency;
            }
        }

        float sum = 0f, weight = 0f;
        Blend(x - wx * FeatureIntensity, y - wy * FeatureIntensity, z - wz * FeatureIntensity, period, k, (_, _, _, cx, cy, cz, w) =>
        {
            sum += w * (2f * Fract(43758.543f * Sine(cx, cy, cz, 12.9898f, 78.233f, 497.19f)) - 1f);
            weight += w;
        });
        return (weight > 0f ? sum / weight * ValueScale : 0f) * 0.5f + 0.5f;
    }

    private delegate void CellVisitor(float rx, float ry, float rz, float cx, float cy, float cz, float weight);

    // Visits the 125 cells around a point on a grid whose cells are centred on the integers.
    private void Blend(float x, float y, float z, float period, float k, CellVisitor visit)
    {
        float ix = MathF.Floor(x + 0.5f), iy = MathF.Floor(y + 0.5f), iz = MathF.Floor(z + 0.5f);
        float fx = x - ix, fy = y - iy, fz = z - iz;

        for (int dz = -2; dz <= 2; dz++)
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    float cx = Wrap(ix + dx, period), cy = Wrap(iy + dy, period), cz = Wrap(iz + dz, period);
                    var g = Direction(cx, cy, cz);
                    float rx = dx - fx + g.X * Jitter, ry = dy - fy + g.Y * Jitter, rz = dz - fz + g.Z * Jitter;
                    float s = MathF.Sqrt(rx * rx + ry * ry + rz * rz) * 0.57735026f;
                    float smooth = Math.Clamp(s * s * s * (s * (s * 6f - 15f) + 10f), 0f, 1f);
                    visit(rx, ry, rz, cx, cy, cz, MathF.Pow(1f - smooth, k));
                }
    }
}
