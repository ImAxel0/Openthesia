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
}
