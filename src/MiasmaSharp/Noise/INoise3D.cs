namespace MiasmaSharp.Noise;

/// <summary>A tiling 3D noise, sampled in lattice units.</summary>
/// <remarks>
/// <see cref="Sample"/> takes a point in lattice cells and the period, in cells, after which the noise repeats on
/// every axis, and returns a value in 0..1. agl renders a texture of side <c>2^coord_scale</c> cells, so a texel at
/// <c>t</c> in 0..1 samples <c>t * period</c> with <c>period = 2^coord_scale</c>.
/// </remarks>
public interface INoise3D
{
    float Sample(float x, float y, float z, float period);
}
