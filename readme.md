# MiasmaSharp

The gloom ("Miasma") renderer of Tears of the Kingdom, taken apart for tools that draw the game's world.

Gloom is not painted into the terrain or the models. The game places model-less actors (`Miasma_Sphere_5`,
`Miasma_Box_2x2x2`, `Miasma_DeepHole_*`, ...) whose blackboard value `Graphics_MiasmaAreaType` gives each an area
code, and its `MiasmaRenderer` draws the gloom over the finished scene wherever a visible surface falls inside one
of them.

It depends on BymlSharp (`lib/BymlSharp`, or `$(BymlLibRoot)` when a host already has a copy) to read the
renderer's parameters and the volumes' shapes.

## Reading

- `MiasmaRendererParams.Read` takes `System/GameGraphics/ModelSceneExtensionParam/Default.game__gfx__ModelSceneExtensionParam.bgyml`
  from `Pack/Bootup.Nin_NX_NVN.pack.zs`, and keeps the game's defaults for what it leaves out.
- `MiasmaShape.Read` takes an actor's `Phive/ShapeParam` file: a sphere, a box or an upright cylinder, as the
  matrix that takes the shape's unit volume to the actor's local space.

## Rendering

`Rendering/MiasmaShaders` holds GLSL 3.30 ports of the renderer's two passes, for a host to compile with its own GL:

1. **Shape** (`MiasmaVolume.vert`, `MiasmaShape.frag`): each volume's box is drawn inside out, and every pixel whose
   surface point lies inside the volume gets a density from the volume's falloff, world-space noise and a ridge
   pattern, written to an RG target as (density, fade). The game blends it with MAX, and with MIN for cancelling
   volumes, whose fade going to zero clears the gloom. A fourth shape is the world-wide altitude gloom of the Depths.
2. **Compose** (`MiasmaFullscreen.vert`, `MiasmaCompose.frag`): wherever there is density and fade, the surface's
   albedo and emission are replaced by the gloom pattern, in three tiers by distance, before lighting.

`MiasmaVolumeConstants` and `MiasmaComposeConstants` give the uniforms as the game fills them. Both passes were
checked against the game's programs run on the CPU: every shape and every compose tier agree to 1e-4.

## Noise

The renderer samples five 3D noise textures the game does not ship: `game::gfx::NoiseMgr` renders them at boot
with agl's procedural noise programs (`Lib/agl/agl_resource.Nin_NX_NVN.release.sarc.zs`, `agl_technique_proc.sharcb`).
`Noise/MiasmaNoiseTextures` builds the same five, R8 and single-level as the game's:

| Sampler | Size | Noise |
|---|---|---|
| `cVoronoiseCloud` | 64³ | domain-warped voronoise, 8 cells across |
| `cGradientSimple` | 64³ | gradient noise, 4 cells across |
| `cGradientFbm` | 64³ | four octaves of it |
| `cGradientRidge` | 64³ | its ridged form |
| `cWorleySimple` | 32³ | Worley distance, 4 cells across |

Each noise was checked against its decompiled program run on the CPU. The gradient noises agree to 1e-4. Worley and
voronoise hash their cells with `fract(sin(x) * big)`, which magnifies the last bit of the sine, so their structure
is the game's but which value a cell gets depends on the machine's sine.

## Area codes

`MiasmaAreaCodes` holds the game's table between an actor's code and the renderer's `MiasmaAreaType`:

| Code | Type | Actors |
|---|---|---|
| 1 | Swarm | `MiasmaSwarm`, Ganondorf's gloom |
| 10, 11, 12 | Cube Sparse, Normal, Dense | `Miasma_Box_2x2x2` (11) |
| 20, 21, 22 | Sphere Sparse, Normal, Dense | `Miasma_Sphere_5`, `Miasma_Sphere_150`, `MiasmaCancel_*` (21, the `_Danse` one 22) |
| 80 - 83 | DeepHole Sparse, Normal, Dense, HyruleCastle | `Miasma_DeepHole_*` |
| 90 | Attitude | the world-wide gloom of `TexToGo/miasma_altitude{,2}.bntx` |
