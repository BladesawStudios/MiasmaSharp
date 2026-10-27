namespace MiasmaSharp.Noise;

/// <summary>The forms of agl's gradient noise, by the program's <c>gradient_noise_type</c>.</summary>
/// <remarks>Type 2 exists in the program but nothing in the gloom renderer uses it, so it is not ported.</remarks>
public enum GradientNoiseKind
{
    Simple = 0,
    Fbm = 1,
    Ridge = 3,
}
