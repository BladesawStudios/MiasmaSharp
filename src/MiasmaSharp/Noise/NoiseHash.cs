namespace MiasmaSharp.Noise;

/// <summary>The hashes agl's procedural noise programs draw their randomness from.</summary>
/// <remarks>
/// <see cref="Direction"/> is the sine-free <c>hash33</c> the gradient and voronoise programs share, turned into a
/// unit vector; <see cref="Sine"/> is the classic <c>fract(sin(dot))</c> hash Worley and voronoise also use. Both
/// take a cell already wrapped by the texture's period, so the noise tiles. The arithmetic is single precision, as
/// on the GPU, but a sine hash at these magnitudes magnifies the last bit of the sine, so cells hashed by
/// <see cref="Sine"/> land where the game's do only as closely as this machine's sine matches the Switch's.
/// </remarks>
internal static class NoiseHash
{
    public static (float X, float Y, float Z) Direction(float cx, float cy, float cz)
    {
        float x = Fract(cx * 0.1031f), y = Fract(cy * 0.11369f), z = Fract(cz * 0.13787f);
        float d = x * (y + 19.19f) + y * (x + 19.19f) + z * (z + 19.19f);
        x += d; y += d; z += d;
        float gx = 2f * Fract((x + y) * z) - 1f, gy = 2f * Fract((x + z) * y) - 1f, gz = 2f * Fract((y + z) * x) - 1f;
        float n = 1f / MathF.Sqrt(gx * gx + gy * gy + gz * gz);
        return (gx * n, gy * n, gz * n);
    }

    public static float Sine(float x, float y, float z, float kx, float ky, float kz) => MathF.Sin(x * kx + y * ky + z * kz);

    public static float Fract(float v) => v - MathF.Floor(v);

    // A cell index wrapped into [0, period), the texture's tiling.
    public static float Wrap(float cell, float period) => cell - MathF.Floor(cell / period) * period;
}
