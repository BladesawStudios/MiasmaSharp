using System.Reflection;

namespace MiasmaSharp.Rendering;

/// <summary>The GLSL ports of the gloom renderer's two passes, for a host to compile with its own GL.</summary>
/// <remarks>
/// <see cref="ShapeVertex"/> and <see cref="ShapeFragment"/> draw one volume into an RG density target;
/// <see cref="ComposeVertex"/> and <see cref="ComposeFragment"/> turn that target into gloom albedo and emission over
/// the whole screen. Each file's header says what it expects bound; <see cref="MiasmaVolumeConstants"/> and
/// <see cref="MiasmaComposeConstants"/> give the values. They are GLSL 3.30, and were each checked against the game's
/// program run on the CPU before being written out.
/// </remarks>
public static class MiasmaShaders
{
    public static string ShapeVertex => Load("MiasmaVolume.vert");
    public static string ShapeFragment => Load("MiasmaShape.frag");
    public static string ComposeVertex => Load("MiasmaFullscreen.vert");
    public static string ComposeFragment => Load("MiasmaCompose.frag");

    private static string Load(string file)
    {
        Assembly assembly = typeof(MiasmaShaders).Assembly;
        using Stream stream = assembly.GetManifestResourceStream($"MiasmaSharp.Shaders.{file}")
            ?? throw new InvalidOperationException($"MiasmaSharp is missing its shader {file}");
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}
