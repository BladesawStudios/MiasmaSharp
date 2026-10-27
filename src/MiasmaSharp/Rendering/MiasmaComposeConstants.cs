using System.Numerics;

namespace MiasmaSharp.Rendering;

/// <summary>The compose pass's uniforms: the renderer's parameters as the game packs them into cCompose, at a moment of its clock.</summary>
/// <remarks>
/// The game keeps three running clocks that wrap at 900, 300 and 30000 of its time steps; the pattern's warp and
/// grain read the first, the mix colour swings with a triangle wave of the second, and the near wobble reads the
/// third. <see cref="At"/> takes the steps elapsed. <see cref="BaseColor1"/> is the triangle wave's mix of the two
/// mix colours, doubled; the grain colour is premultiplied by its alpha. <see cref="DistanceScale"/> is the
/// shaders' <c>uDistanceScale</c> for a vertical field of view: one at 50 degrees and wider, less when narrower.
/// </remarks>
public sealed record MiasmaComposeConstants(
    Vector4 DarkDump, Vector4 BaseColor0, Vector3 BaseColor1, Vector3 BlackColor, Vector2 Intensity,
    Vector2 RedPattern, Vector3 GrainColor, Vector2 GrainPattern, Vector3 Time)
{
    public static MiasmaComposeConstants At(MiasmaRendererParams p, double steps)
    {
        float cycle = (float)(steps % 900.0), swing = (float)(steps % 300.0), slow = (float)(steps % 30000.0);
        float triangle = 2f * MathF.Abs(swing / 300f - MathF.Floor(swing / 300f) - 0.5f);
        Vector4 mix = Vector4.Lerp(p.MixColor0, p.MixColor1, Math.Clamp(triangle, 0f, 1f)) * 2f;
        return new(
            new(p.StartDumpDistDark, p.EndDumpDistDark, p.IntensityDark, p.MinIntensityDark),
            new(p.BaseColor.X, p.BaseColor.Y, p.BaseColor.Z, p.BlackPatternDensity),
            new(mix.X, mix.Y, mix.Z),
            new(p.BlackColor.X, p.BlackColor.Y, p.BlackColor.Z),
            new(p.AlbedoIntensity, p.EmissionIntensity),
            new(p.RedPatternScrollSpeed, p.RedPatternParallaxIntensity),
            new Vector3(p.GrainPatternColor.X, p.GrainPatternColor.Y, p.GrainPatternColor.Z) * p.GrainPatternColor.W,
            new(p.GrainPatternRepeatRatio, p.GrainPatternParallaxIntensity),
            new(cycle / 900f, triangle, slow / 30000f));
    }

    public static float DistanceScale(float verticalFovRadians)
    {
        float k = MathF.Max(0.87266463f / verticalFovRadians - 1f, 0f) * 0.3f + 1f;
        return 1f / k;
    }
}
