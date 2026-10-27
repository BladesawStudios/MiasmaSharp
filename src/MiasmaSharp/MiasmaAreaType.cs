namespace MiasmaSharp;

/// <summary>The kinds of gloom area the game's renderer knows, in the order of its <c>MiasmaAreaType</c> enum.</summary>
/// <remarks>
/// <c>MiasmaRendererParam</c> names its per-area rows by these names, and the renderer looks a row up by the
/// area's code (<see cref="MiasmaAreaCodes"/>). The spelling of <see cref="DeepHoleSparese"/> is the game's.
/// </remarks>
public enum MiasmaAreaType
{
    Primitive = 0,
    Swarm = 1,
    CubeSparse = 2,
    CubeNormal = 3,
    CubeDense = 4,
    SphereSparse = 5,
    SphereNormal = 6,
    SphereDense = 7,
    DeepHoleSparese = 8,
    DeepHoleNormal = 9,
    DeepHoleDense = 10,
    DeepHoleHyruleCastle = 11,
    Attitude = 12,
}
