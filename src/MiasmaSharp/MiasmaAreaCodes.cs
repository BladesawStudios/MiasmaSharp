namespace MiasmaSharp;

/// <summary>The code a gloom actor carries in its <c>Graphics_MiasmaAreaType</c> blackboard value, and the area type it stands for.</summary>
/// <remarks>
/// The code's tens are the shape (1 box, 2 sphere, 8 deep hole, 9 the world-wide altitude gloom) and its units the
/// strength (0 sparse, 1 normal, 2 dense, 3 the Hyrule Castle chasm). The table is the game's own, built once in the
/// executable and read both ways: by type when a <c>MiasmaRendererParam</c> row is applied to an area, and by code
/// when an actor's area is matched to its row. A code the table does not hold has no type.
/// </remarks>
public static class MiasmaAreaCodes
{
    private static readonly int[] Codes = [0, 1, 10, 11, 12, 20, 21, 22, 80, 81, 82, 83, 90];

    public static int CodeOf(MiasmaAreaType type) => Codes[(int)type];

    public static MiasmaAreaType? TypeOf(int code)
    {
        int index = Array.IndexOf(Codes, code);
        return index < 0 ? null : (MiasmaAreaType)index;
    }
}
