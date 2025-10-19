using ImGuiNET;
using Openthesia.Core;
using System.Numerics;

namespace Openthesia.Ui.Helpers;

public static class ImGuiUtils
{
    public static Vector2 FixedSize(Vector2 size)
    {
        return size * FontController.DSF;
    }

    public static void Spacing(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            ImGui.Spacing();
        }
    }
    /// <summary>
    /// Darkens a color by the specified factor.
    /// </summary>
    /// <param name="color">The original color to darken.</param>
    /// <param name="factor">The amount to darken by (0.0 = no change, 1.0 = completely black). 
    /// This represents the darkness amount, not the final multiplier values.</param>
    /// <returns>The darkened color with the alpha channel preserved.</returns>
    public static Vector4 DarkenColor(Vector4 color, float factor)
    {
        var multiplier = 1 - factor;
        return new Vector4(
            color.X * multiplier,
            color.Y * multiplier,
            color.Z * multiplier,
            color.W  // Preserve alpha
        );
    }
}
