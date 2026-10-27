namespace MiasmaSharp.Noise;

/// <summary>The five 3D noise textures the gloom renderer samples, built as the game builds them at boot.</summary>
/// <remarks>
/// The game's <c>game::gfx::NoiseMgr::initialize</c> renders them once with agl's noise programs into single-level
/// R8 unorm volumes, one slice at a time with the slice at <c>(z + 0.5) / depth</c>. <see cref="All"/> lists them by
/// the sampler the renderer binds each to, with the settings the game gives them; <see cref="Build"/> renders one
/// the same way into bytes, a slice after another, rows of X within each slice.
/// </remarks>
public static class MiasmaNoiseTextures
{
    public sealed record Texture(string Sampler, int Size, int CoordScale, INoise3D Noise);

    public static IReadOnlyList<Texture> All { get; } =
    [
        new("cVoronoiseCloud", 64, 3, new VoronoiseNoise(ValueScale: 1.3f, FeatureIntensity: 2f, Jitter: 1f, Smoothness: 0.7f)),
        new("cGradientSimple", 64, 2, new GradientNoise(GradientNoiseKind.Simple, ValueScale: 2.2f, FeatureIntensity: 1f)),
        new("cGradientFbm", 64, 2, new GradientNoise(GradientNoiseKind.Fbm, ValueScale: 2.2f, FeatureIntensity: 1f)),
        new("cGradientRidge", 64, 2, new GradientNoise(GradientNoiseKind.Ridge, ValueScale: 2.2f, FeatureIntensity: 1f)),
        new("cWorleySimple", 32, 2, new WorleyNoise(FeatureIntensity: 0f)),
    ];

    public static byte[] Build(Texture texture)
    {
        int size = texture.Size;
        float period = MathF.Pow(2f, texture.CoordScale);
        byte[] texels = new byte[size * size * size];
        Parallel.For(0, size, z =>
        {
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float value = texture.Noise.Sample(period * (x + 0.5f) / size, period * (y + 0.5f) / size, period * (z + 0.5f) / size, period);
                    texels[(z * size + y) * size + x] = (byte)MathF.Round(Math.Clamp(value, 0f, 1f) * 255f);
                }
        });
        return texels;
    }
}
