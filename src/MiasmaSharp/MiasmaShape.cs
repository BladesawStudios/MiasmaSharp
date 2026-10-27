using System.Numerics;
using BymlSharp;

namespace MiasmaSharp;

/// <summary>The solid a gloom actor fills: its Phive shape, as the renderer draws it.</summary>
/// <remarks>
/// The renderer marks the surfaces inside a sphere, a box or an upright cylinder, the three shapes gloom actors
/// carry in <c>Phive/ShapeParam</c>. <see cref="Unit"/> takes the shape's unit volume to the actor's local space:
/// the unit sphere of radius one half, or the unit cube and cylinder whose every coordinate lies within one half of
/// the origin, so a volume's world matrix is the placement's transform times this one. A shape file holding none of
/// the three is not a gloom shape.
/// </remarks>
public sealed record MiasmaShape(MiasmaShapeKind Kind, Matrix4x4 Unit)
{
    public static MiasmaShape? Read(Byml shapeParam)
    {
        if (First(shapeParam, "Sphere") is { } sphere)
            return new(MiasmaShapeKind.Sphere, Matrix4x4.CreateScale(2f * sphere["Radius"]!.Float));

        if (First(shapeParam, "Box") is { } box)
        {
            Vector3 size = 2f * Vec(box["HalfExtents"]!);
            return new(MiasmaShapeKind.Box, Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation(Vec(box["Center"]!)));
        }

        if (First(shapeParam, "Cylinder") is { } cylinder)
        {
            Vector3 a = Vec(cylinder["CenterA"]!), b = Vec(cylinder["CenterB"]!);
            float diameter = 2f * cylinder["Radius"]!.Float;
            Vector3 size = new(diameter, Vector3.Distance(a, b), diameter);
            return new(MiasmaShapeKind.Cylinder, Matrix4x4.CreateScale(size) * Matrix4x4.CreateTranslation((a + b) * 0.5f));
        }

        return null;
    }

    private static Byml? First(Byml shapeParam, string key) =>
        shapeParam.TryGet(key, out Byml? list) && list.IsArray && list.Count > 0 ? list[0] : null;

    private static Vector3 Vec(Byml v) => new(v["X"]!.Float, v["Y"]!.Float, v["Z"]!.Float);
}
