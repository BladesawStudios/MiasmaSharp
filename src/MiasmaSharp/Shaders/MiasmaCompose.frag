#version 330 core

// The gloom renderer's compose pass: miasma_renderer_compose as the game draws it, at full resolution.
//
// Wherever the shape pass left both a density and a fade, the surface becomes gloom: the game
// overwrites its G-buffer albedo and emission there before lighting, so the gloom is lit like the
// ground and glows on top. The pattern is world-space noise in three tiers by distance: within 50 m
// two voronoise layers, a warped gradient and a red Worley grain with parallax; to 500 m a warped
// gradient over voronoise; beyond, fbm over coarse voronoise. Black where the pattern is low, the
// base colour where the density is, and the animated mix colour where the pattern peaks. The
// emission fades with the dark distance falloff and the environment mask. The numbers are the
// program's own; the uniforms are cCompose's, filled by MiasmaComposeConstants.

layout(location = 0) out vec4 GloomEmission;
// rgb the albedo; a the game's G-buffer flag byte, 1 plus whether the gloom is dense, over 255.
layout(location = 1) out vec4 GloomAlbedo;

uniform sampler2D uDensity;
uniform sampler2D uDepth;
uniform vec4      uDepthToView;
uniform vec2      uViewport;
uniform vec2      uTanHalfFov;
uniform mat4      uViewToWorld;
// The third row of the world-to-view matrix, which the game projects the view direction against.
uniform vec3      uViewRow2;
uniform float     uDistanceScale;
// The game's material ID buffer, its .y holding bit 32 on surfaces gloom must not cover; bind black where there is none.
uniform sampler2D uMaterialId;
// The game's environment mask, its .z bright where the surroundings are lit; bind black where there is none.
uniform sampler2D uEnvironmentMask;

uniform sampler3D uVoronoiseCloud;
uniform sampler3D uGradientSimple;
uniform sampler3D uGradientFbm;
uniform sampler3D uWorleySimple;

uniform vec4 uDarkDump;     // start, end, intensity, minimum intensity
uniform vec4 uBaseColor0;   // rgb, w the black pattern density
uniform vec3 uBaseColor1;
uniform vec3 uBlackColor;
uniform vec2 uIntensity;    // albedo, emission
uniform vec2 uRedPattern;   // scroll speed, parallax
uniform vec3 uGrainColor;
uniform vec2 uGrainPattern; // repeat ratio, parallax
uniform vec3 uTime;         // the clock over 900, its triangle wave over 300, over 30000

float powPositive(float b, float e) { return exp2(log2(b) * e); }

vec3 tint(float pattern, float black, float cells, float exponent, float density)
{
    float v = clamp((powPositive(exponent, pattern) * cells - 0.25) * 3.3333333, 0.0, 1.0);
    float v7 = powPositive(v * v, 3.5);
    float dark = powPositive(abs(1.0 - black), uBaseColor0.w);
    float t = clamp((density - 0.425) * 4.4444451, 0.0, 1.0);
    float edge = t * t * (3.0 - 2.0 * t);
    vec3 low = uBlackColor + edge * uBaseColor0.rgb * dark * (1.0 - uBlackColor);
    vec3 high = uBlackColor + edge * v7 * (uBaseColor1 - uBlackColor);
    return mix(low, high, v) * 2.5;
}

vec3 near(vec3 view, vec3 world, float dist, float density, float exponent)
{
    vec3 dir = normalize(view);
    vec3 parallax = dir - dot(dir, uViewRow2) * uViewRow2;
    vec3 grain = parallax * uGrainPattern.y * clamp((density - 0.3) * 1.4285715, 0.0, 1.0);
    float fbm = texture(uGradientFbm, world * 0.05).x;
    float swirl = sin(texture(uGradientFbm, world * 0.1 + grain + sin(uTime.z * 3.14)).x) * 0.075;
    float grainCells = texture(uVoronoiseCloud, world * 0.25 + grain).x;
    float warp = fbm * 1.2 + uTime.x;
    float closeUp = clamp((dist - 25.0) * 0.04, 0.0, 1.0);
    float simple = texture(uGradientSimple, world * 0.1 + warp - 0.6).x;
    float cells = texture(uVoronoiseCloud, world * 0.125 + parallax * uRedPattern.y * (1.0 - closeUp)).x;
    float u = grainCells * 0.01 + swirl - 0.005, w = grainCells * -0.01 + swirl + 0.005;
    float worley = texture(uWorleySimple, vec3(u, w, u) * uGrainPattern.x + grain).x;

    vec3 colour = tint(simple, simple, cells, exponent, density);
    float threshold = (sin(uTime.x * 6.28) * 0.5 + 0.5) * 0.075 + 0.5;
    float speck = clamp((worley * worley - threshold) / (1.0 - threshold), 0.0, 1.0);
    float weight = clamp((density * density - 0.07) * 2.3255813, 0.0, 1.0) * (1.0 - clamp((density - 0.65) * 2.8571427, 0.0, 1.0));
    return colour + speck * uGrainColor * weight * (1.0 - closeUp);
}

vec3 middle(vec3 world, float dist, float density, float exponent)
{
    float fbm = texture(uGradientFbm, world * 0.05).x;
    float simple = texture(uGradientSimple, world * 0.1 + fbm * 1.2 + uTime.x - 0.6).x;
    float far = clamp((dist * 0.002 - 0.5) * 2.0, 0.0, 1.0);
    float n = mix(simple, fbm, far);
    return tint(n * (1.0 - 0.4 * far), n, texture(uVoronoiseCloud, world * 0.125).x, exponent, density);
}

vec3 distant(vec3 world, float density, float exponent)
{
    float fbm = texture(uGradientFbm, world * 0.05).x;
    return tint(fbm * 0.6, fbm, texture(uVoronoiseCloud, world * 0.025).x, exponent, density);
}

void main()
{
    ivec2 pixel = ivec2(gl_FragCoord.xy);
    vec2 density = texelFetch(uDensity, pixel, 0).xy;
    if (density.x <= 0.0 || density.y <= 0.0) discard;
    if ((int(texelFetch(uMaterialId, pixel, 0).y * 255.0) & 32) != 0) discard;

    float d = texelFetch(uDepth, pixel, 0).r;
    float viewZ = (uDepthToView.x * d + uDepthToView.y) / (uDepthToView.z * d + uDepthToView.w);
    vec2 ndc = gl_FragCoord.xy / uViewport * 2.0 - 1.0;
    vec3 view = vec3(ndc * uTanHalfFov * -viewZ, viewZ);
    vec3 world = (uViewToWorld * vec4(view, 1.0)).xyz;
    float depth = -viewZ;
    float dist = depth * uDistanceScale;
    float exponent = clamp((200.0 - depth) * 0.0055555557, 0.0, 1.0) * 0.15 + 0.05;

    vec3 colour = dist < 50.0 ? near(view, world, dist, density.x, exponent)
                : dist < 500.0 ? middle(world, dist, density.x, exponent)
                : distant(world, density.x, exponent);

    float dark = max(uDarkDump.z * (1.0 - clamp((dist - uDarkDump.x) / (uDarkDump.y - uDarkDump.x), 0.0, 1.0)), uDarkDump.w);
    float environment = clamp(texelFetch(uEnvironmentMask, pixel, 0).z + 0.2, 0.0, 1.0);
    float glow = dark + environment * (1.0 - dark);
    int flags = (int(clamp((density.x - 0.4) * 4.9999995, 0.0, 1.0) * 255.0) & 248) | 1;

    GloomEmission = vec4(colour * uIntensity.y * glow * glow, 0.0);
    GloomAlbedo = vec4(colour * uIntensity.x, float(flags) / 255.0);
}
