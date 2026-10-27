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

## Area codes

`MiasmaAreaCodes` holds the game's table between an actor's code and the renderer's `MiasmaAreaType`:

| Code | Type | Actors |
|---|---|---|
| 1 | Swarm | `MiasmaSwarm`, Ganondorf's gloom |
| 10, 11, 12 | Cube Sparse, Normal, Dense | `Miasma_Box_2x2x2` (11) |
| 20, 21, 22 | Sphere Sparse, Normal, Dense | `Miasma_Sphere_5`, `Miasma_Sphere_150`, `MiasmaCancel_*` (21, the `_Danse` one 22) |
| 80 - 83 | DeepHole Sparse, Normal, Dense, HyruleCastle | `Miasma_DeepHole_*` |
| 90 | Attitude | the world-wide gloom of `TexToGo/miasma_altitude{,2}.bntx` |
