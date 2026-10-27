namespace MiasmaSharp;

/// <summary>The three shapes a gloom volume can take.</summary>
/// <remarks>The values are the renderer's own <c>SHAPE_TYPE</c>; its fourth, the altitude maps, is no volume.</remarks>
public enum MiasmaShapeKind
{
    Sphere = 0,
    Box = 1,
    Cylinder = 2,
}
