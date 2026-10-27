#version 330 core

// The gloom renderer's shape pass: miasma_renderer_shape with its noise on, the variant the game
// draws every volume with.
//
// A volume is drawn as the box around its unit volume, and each fragment rebuilds the surface point
// the scene shows there from the depth buffer. A point inside the volume gets a density: the shape's
// own falloff s, scaled by the area's density and broken up by world-space gradient noise, with a
// ridge pattern eating into where it is thin. The result goes to an RG target, R the density and G
// the volume's fade, which the host blends with MAX for a volume and MIN for a cancelling one, so a
// cancel sphere at fade 0 clears what the others laid down. The numbers are the program's own.

layout(location = 0) out vec2 FragDensity;

uniform sampler2D uDepth;
uniform float     uDepthClear;
// The view-space Z of a depth-buffer value d is (x d + y) / (z d + w), negative in front.
uniform vec4      uDepthToView;
uniform vec2      uViewport;
// tan of half the vertical field of view, times the aspect ratio in x.
uniform vec2      uTanHalfFov;
uniform mat4      uViewToWorld;
// Distances shrink for views narrower than the game's 50 degrees; see MiasmaView.DistanceScale.
uniform float     uDistanceScale;

// MiasmaShapeKind, or 3 for the world-wide altitude maps.
uniform int   uShape;
// View space to the volume's unit space, where the volume fills |p| <= 0.5.
uniform mat4  uViewToUnit;
// x the density, or a deep hole's density at its top with y the one at its bottom.
uniform vec2  uDensity;
// A cylinder's inner density ratio at its top (x) and bottom (y).
uniform vec2  uInnerRatio;
// A cylinder's noise scale; the other shapes sample at the program's fixed scales.
uniform float uNoiseScale;
uniform float uFade;

uniform sampler3D uGradientSimple;
uniform sampler3D uGradientFbm;
uniform sampler3D uGradientRidge;
uniform sampler2D uAttitude;
uniform sampler2D uAttitude2;

// The game's approximation of acos, kept so the rim pattern lands where the game's does.
float gameAcos(float c)
{
    float a = abs(c);
    float r = (((-0.0187293 * a + 0.074261) * a - 0.2121144) * a + 1.5707288) * sqrt(1.0 - a);
    return c < 0.0 ? 3.1415927 - r : r;
}

// Full detail within 0.6 of a range, fading out by its end.
float fadeIn(float d) { return d < 0.6 ? 1.0 : 1.0 - clamp((d - 0.6) * 2.5000002, 0.0, 1.0); }

float mark(float s, float density, vec3 world, float dist, float noiseScale, float ridgeScale)
{
    vec3 coord = world * noiseScale;
    float near = dist / 300.0;
    float detail = near < 1.0 ? (2.0 * texture(uGradientFbm, coord).x - 1.0) * fadeIn(near) : 0.0;
    float s2 = s * s;
    float base = s2 * density;
    float value = (texture(uGradientSimple, coord).x + detail) * clamp(s2 * 10.0, 0.0, 1.0);
    if (value <= -base) discard;

    float v = base + value;
    if (v < 0.3182)
    {
        float far = dist / 150.0;
        float ridge = far < 1.0 ? texture(uGradientRidge, world * ridgeScale).x * fadeIn(far) : 0.0;
        float k = (1.0 + 0.5 * clamp((dist - 30.0) * 0.02, 0.0, 1.0)) * -0.23;
        v *= clamp((v + s * clamp((ridge - 0.25) * 1.3333334, 0.0, 1.0) + k) / (k + 0.5), 0.0, 1.0);
    }
    return v;
}

float cylinder(vec3 l, vec3 world, float dist)
{
    float r = length(l.xz);
    if (abs(l.y) > 0.5 || r > 0.5) discard;
    float angle = gameAcos(l.x / r) * sign(l.z);
    float s0 = clamp(1.0 - 2.0 * r, 0.0, 1.0);
    float rim = (0.5 + 0.5 * sin(angle * 5.0)) * clamp((0.5 - s0) * 2.0, 0.0, 1.0);
    float profile = (s0 - rim * (0.5 + 0.5 * cos(angle * 7.0))) * (1.0 - clamp((abs(l.y) - 0.49) * 100.0001, 0.0, 1.0));
    float h = l.y + 0.5;
    float inner = mix(uInnerRatio.y, uInnerRatio.x, h);
    float s = profile * (inner + (1.0 - inner) * (1.0 - s0 * s0));
    if (s < 0.0) discard;
    return mark(s, mix(uDensity.y, uDensity.x, h), world, dist, 0.05 * uNoiseScale, 0.25 * uNoiseScale);
}

float altitude(vec3 world, float dist)
{
    vec2 uv = vec2(world.x * 0.0001 + 0.5, world.z * 0.000125 + 0.5);
    float depth = texture(uAttitude, uv).x * -2000.0 - world.y;
    vec2 shape = texture(uAttitude2, uv).xw;
    float thick = clamp((shape.y * 20.0 - 6.0) * 0.125, 0.0, 1.0);
    float rise = (texture(uGradientSimple, world * 0.0135).x * 3.4 - thick * 0.2 - 1.7) * (1.0 - clamp(depth * -0.027027028, 0.0, 1.0));
    float lip = (shape.x * 2.0 - 0.8) * 2.0 * (1.0 - clamp(depth * -0.04, 0.0, 1.0));
    float below = clamp(depth, -1.0, 11.0);
    float height = mix(below, clamp(rise + lip, -1.0, 11.0), thick);
    if (height < 0.0) discard;
    float s = 2.0 - 2.0 * clamp((0.1 - min(height * 0.02, 1.0)) * 10.0, 0.0, 1.0);
    return mark(s, uDensity.x, world, dist, 0.1, 0.5);
}

void main()
{
    float d = texelFetch(uDepth, ivec2(gl_FragCoord.xy), 0).r;
    if (d == uDepthClear) discard;

    float viewZ = (uDepthToView.x * d + uDepthToView.y) / (uDepthToView.z * d + uDepthToView.w);
    vec2 ndc = gl_FragCoord.xy / uViewport * 2.0 - 1.0;
    vec3 view = vec3(ndc * uTanHalfFov * -viewZ, viewZ);
    vec3 world = (uViewToWorld * vec4(view, 1.0)).xyz;
    float dist = -viewZ * uDistanceScale;
    vec3 l = (uViewToUnit * vec4(view, 1.0)).xyz;

    float v;
    if (uShape == 0)
    {
        // The game's sphere ends at its radius: it stores the radius over 0.88 and keeps s >= 0.12.
        float s = 1.0 - 2.0 * length(l) * 0.88;
        if (s < 0.12) discard;
        v = mark(s, uDensity.x, world, dist, 0.05, 0.25);
    }
    else if (uShape == 1)
    {
        if (any(greaterThan(abs(l), vec3(0.5)))) discard;
        v = mark(max(0.0, 1.0 - 2.0 * length(l)), uDensity.x, world, dist, 0.05, 0.25);
    }
    else if (uShape == 2) v = cylinder(l, world, dist);
    else v = altitude(world, dist);

    FragDensity = vec2(v, uShape == 3 ? 1.0 : uFade);
}
