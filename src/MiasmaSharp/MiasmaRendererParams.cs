using System.Numerics;
using BymlSharp;

namespace MiasmaSharp;

/// <summary>The gloom renderer's settings: <c>MiasmaRendererParam</c> of the game's <c>ModelSceneExtensionParam</c>.</summary>
/// <remarks>
/// The game keeps them in <c>System/GameGraphics/ModelSceneExtensionParam/Default.game__gfx__ModelSceneExtensionParam.bgyml</c>
/// in <c>Pack/Bootup.Nin_NX_NVN.pack.zs</c>; <see cref="Read"/> takes that document's root, or the
/// <c>MiasmaRendererParam</c> map itself. Colours are RGBA. A value the file leaves out keeps the default the game's
/// constructor gives it. The dump distances fade the gloom with distance from the camera, by day and in the dark.
/// </remarks>
public sealed record MiasmaRendererParams
{
    public float AlbedoIntensity { get; init; } = 1f;
    public Vector4 BaseColor { get; init; } = new(0.3f, 0f, 0f, 1f);
    public Vector4 BlackColor { get; init; } = new(0f, 0f, 0f, 1f);
    public float BlackPatternDensity { get; init; } = 3f;
    public float EmissionIntensity { get; init; } = 1f;
    public float StartDumpDist { get; init; }
    public float EndDumpDist { get; init; }
    public float StartDumpDistDark { get; init; }
    public float EndDumpDistDark { get; init; }
    public Vector4 GrainPatternColor { get; init; } = new(0f, 0f, 0f, 1f);
    public float GrainPatternParallaxIntensity { get; init; }
    public float GrainPatternRepeatRatio { get; init; }
    public float Intensity { get; init; }
    public float IntensityDark { get; init; }
    public float MinIntensity { get; init; }
    public float MinIntensityDark { get; init; }
    public Vector4 MixColor0 { get; init; } = new(1.035f, 0.038f, 0.062f, 1f);
    public Vector4 MixColor1 { get; init; } = new(1.5f, 0.1f, 0f, 1f);
    public float RedPatternParallaxIntensity { get; init; } = 1f;
    public float RedPatternScrollSpeed { get; init; } = 1f;
    public IReadOnlyList<MiasmaAreaParam> Areas { get; init; } = [];

    // The row for a kind of area, or null when the file has none; the renderer draws such an area at density zero.
    public MiasmaAreaParam? AreaOf(MiasmaAreaType type) => Areas.FirstOrDefault(a => a.Type == type);

    public static MiasmaRendererParams Read(Byml document)
    {
        Byml p = document.TryGet("MiasmaRendererParam", out Byml? inner) ? inner : document;
        MiasmaRendererParams d = new();
        return new()
        {
            AlbedoIntensity = MiasmaAreaParam.Float(p, "AlbedoIntensiy", d.AlbedoIntensity),
            BaseColor = Color(p, "BaseColor", d.BaseColor),
            BlackColor = Color(p, "BlackColor", d.BlackColor),
            BlackPatternDensity = MiasmaAreaParam.Float(p, "BlackPatternDensity", d.BlackPatternDensity),
            EmissionIntensity = MiasmaAreaParam.Float(p, "EmissionIntensity", d.EmissionIntensity),
            StartDumpDist = MiasmaAreaParam.Float(p, "StartDumpDist", 0f),
            EndDumpDist = MiasmaAreaParam.Float(p, "EndDumpDist", 0f),
            StartDumpDistDark = MiasmaAreaParam.Float(p, "StartDumpDistDark", 0f),
            EndDumpDistDark = MiasmaAreaParam.Float(p, "EndDumpDistDark", 0f),
            GrainPatternColor = Color(p, "GrainPatternColor", d.GrainPatternColor),
            GrainPatternParallaxIntensity = MiasmaAreaParam.Float(p, "GrainPatternParallaxIntensity", 0f),
            GrainPatternRepeatRatio = MiasmaAreaParam.Float(p, "GrainPatternRepeatRatio", 0f),
            Intensity = MiasmaAreaParam.Float(p, "Intensity", 0f),
            IntensityDark = MiasmaAreaParam.Float(p, "IntensityDark", 0f),
            MinIntensity = MiasmaAreaParam.Float(p, "MinIntensity", 0f),
            MinIntensityDark = MiasmaAreaParam.Float(p, "MinIntensityDark", 0f),
            MixColor0 = Color(p, "MixColor0", d.MixColor0),
            MixColor1 = Color(p, "MixColor1", d.MixColor1),
            RedPatternParallaxIntensity = MiasmaAreaParam.Float(p, "RedPatternParallaxIntensity", d.RedPatternParallaxIntensity),
            RedPatternScrollSpeed = MiasmaAreaParam.Float(p, "RedPatternScrollSpeed", d.RedPatternScrollSpeed),
            Areas = p.TryGet("MiasmaAreaParam", out Byml? rows) && rows.IsArray
                ? [.. rows.AsArray.Select(MiasmaAreaParam.Read).OfType<MiasmaAreaParam>()]
                : [],
        };
    }

    private static Vector4 Color(Byml map, string key, Vector4 fallback) =>
        map.TryGet(key, out Byml? c)
            ? new(MiasmaAreaParam.Float(c, "R", fallback.X), MiasmaAreaParam.Float(c, "G", fallback.Y),
                  MiasmaAreaParam.Float(c, "B", fallback.Z), MiasmaAreaParam.Float(c, "A", fallback.W))
            : fallback;
}
