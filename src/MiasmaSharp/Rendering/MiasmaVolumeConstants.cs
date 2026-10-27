using System.Numerics;

namespace MiasmaSharp.Rendering;

/// <summary>The shape pass's per-volume uniforms, as the game's renderer fills them for one area.</summary>
/// <remarks>
/// A sphere or box marks its surfaces at its area's density. A cylinder that is a deep hole blends from one less than
/// its lowest density at the bottom to one less than its highest at the top, thinning toward its axis by the inner
/// ratios; any other cylinder takes its density throughout. Only a cylinder scales its noise by the area's
/// <c>NoiseScale</c>. An area type with no row in the parameters draws at density zero, as the game's does.
/// <see cref="UnitToView"/> is the matrix whose inverse the shader wants as <c>uViewToUnit</c>.
/// </remarks>
public sealed record MiasmaVolumeConstants(MiasmaShapeKind Shape, Vector2 Density, Vector2 InnerRatio, float NoiseScale)
{
    public static MiasmaVolumeConstants For(MiasmaShapeKind shape, MiasmaAreaType type, MiasmaRendererParams parameters)
    {
        MiasmaAreaParam? row = parameters.AreaOf(type);
        if (row is null) return new(shape, Vector2.Zero, Vector2.One, 0f);

        bool deepHole = type is >= MiasmaAreaType.DeepHoleSparese and <= MiasmaAreaType.DeepHoleHyruleCastle;
        if (shape == MiasmaShapeKind.Cylinder && deepHole)
            return new(shape, new(row.HeightestDensity - 1f, row.LowestDensity - 1f),
                new(row.HeightestInnerDensityRatio, row.LowestInnerDensityRatio), row.NoiseScale);

        return new(shape, new(row.Density, row.Density), Vector2.One, row.NoiseScale);
    }

    // The volume's unit space to view space: its unit matrix, its placement and the camera's view.
    public static Matrix4x4 UnitToView(MiasmaShape shape, Matrix4x4 placement, Matrix4x4 worldToView) => shape.Unit * placement * worldToView;
}
