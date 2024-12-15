using ImGuiNET;

namespace Openthesia.Core;

public static class FontController
{
    /// <summary>
    /// dpi scale factor to handle ui size and position
    /// </summary>
    public static float DSF = 1f;
    public static ImFontPtr Title;
    public static ImFontPtr BigIcon;
    public static ImFontPtr Font16_Icon12;
    public static ImFontPtr Font16_Icon16;
    public static List<ImFontPtr> FontSizes = new();

    public static void PushFont(ImFontPtr font)
    {

    }

    public static ImFontPtr GetFontOfSize(int size)
    {
        int cSize = Math.Clamp(size, 17, 25);
        return FontSizes[cSize - 17];
    }
}
