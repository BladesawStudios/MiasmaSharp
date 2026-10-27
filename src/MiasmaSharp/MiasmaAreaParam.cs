using BymlSharp;

namespace MiasmaSharp;

/// <summary>One row of <c>MiasmaRendererParam.MiasmaAreaParam</c>: how dense one kind of gloom area is.</summary>
/// <remarks>
/// A sphere or box area marks its surfaces at <see cref="Density"/>; a deep hole blends from
/// <see cref="LowestDensity"/> at its bottom to <see cref="HeightestDensity"/> at its top, and the inner ratios
/// thin it toward its axis. <see cref="NoiseScale"/> scales the noise that breaks up the area's edge. A value the
/// row leaves out keeps the default the game's constructor gives it: one for the density, the noise scale and the
/// inner ratios, zero for the two height densities.
/// </remarks>
public sealed record MiasmaAreaParam(MiasmaAreaType Type)
{
    public float Density { get; init; } = 1f;
    public float NoiseScale { get; init; } = 1f;
    public float HeightestDensity { get; init; }
    public float HeightestInnerDensityRatio { get; init; } = 1f;
    public float LowestDensity { get; init; }
    public float LowestInnerDensityRatio { get; init; } = 1f;

    public static MiasmaAreaParam? Read(Byml row) =>
        row.TryGet("MiasmaAreaType", out Byml? type) && Enum.TryParse(type.String, out MiasmaAreaType parsed)
            ? new(parsed)
            {
                Density = Float(row, "Density", 1f),
                NoiseScale = Float(row, "NoiseScale", 1f),
                HeightestDensity = Float(row, "HeightestDensity", 0f),
                HeightestInnerDensityRatio = Float(row, "HeightestInnerDensityRatio", 1f),
                LowestDensity = Float(row, "LowestDensity", 0f),
                LowestInnerDensityRatio = Float(row, "LowestInnerDensityRatio", 1f),
            }
            : null;

    internal static float Float(Byml map, string key, float fallback) => map.TryGet(key, out Byml? v) ? v.Float : fallback;
}
