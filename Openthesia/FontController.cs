using ImGuiNET;

namespace Openthesia;

public class FontController
{
    public static ImFontPtr Title;
    public static ImFontPtr BigIcon;
    public static ImFontPtr Font16_Icon12;
    public static ImFontPtr Font16_Icon16;
    public static List<ImFontPtr> FontSizes = new();

    public static ImFontPtr GetFontOfSize(int size)
    {
        int cSize = Math.Clamp(size, 17, 25);
        return FontSizes[cSize - 17];
    }
}
