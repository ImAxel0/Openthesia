using ImGuiNET;
using Openthesia.Settings;
using System.Numerics;

namespace Openthesia.Core;

public static class ImGuiTheme
{
    public static ImGuiStylePtr Style;
    public static Vector4 Button = new Vector4(0.29f, 0.29f, 0.29f, .9f);
    public static Vector4 ButtonHovered = new Vector4(0.29f, 0.29f, 0.29f, .9f) * 1.2f;
    public static Vector4 ButtonActive = new Vector4(0.29f, 0.29f, 0.29f, .9f) * 1.5f;

    public static Vector4 HtmlToVec4(string htmlColor, float alpha = 1f)
    {
        if (htmlColor == null || htmlColor.Length != 7 || htmlColor[0] != '#')
            throw new ArgumentException("Invalid HTML color code");

        int r = Convert.ToInt32(htmlColor.Substring(1, 2), 16);
        int g = Convert.ToInt32(htmlColor.Substring(3, 2), 16);
        int b = Convert.ToInt32(htmlColor.Substring(5, 2), 16);

        return new Vector4(r / 255f, g / 255f, b / 255f, alpha);
    }

    public static void PushTheme()
    {
        Style = ImGui.GetStyle();
        Style.FrameRounding = 4;
        Style.FramePadding = new Vector2(5, 7);
        Style.WindowPadding = Vector2.Zero;
        Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
        Style.Colors[(int)ImGuiCol.MenuBarBg] = HtmlToVec4("#1F2937");
        Style.Colors[(int)ImGuiCol.WindowBg] = ThemeManager.MainBgCol;
        Style.Colors[(int)ImGuiCol.Button] = Button;
        Style.Colors[(int)ImGuiCol.ButtonHovered] = ButtonHovered;
        Style.Colors[(int)ImGuiCol.ButtonActive] = ButtonActive;
        Style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(1, 0, 0, 1);
        Style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.29f, 0.29f, 0.29f, .9f);
        Style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.29f, 0.29f, 0.29f, .9f) * 1.2f;
        Style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.29f, 0.29f, 0.29f, .9f) * 1.5f;
        Style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.29f, 0.29f, 0.29f, .9f);
        Style.Colors[(int)ImGuiCol.PopupBg] = HtmlToVec4("#1F1F21");
        Style.Colors[(int)ImGuiCol.TableRowBg] = HtmlToVec4("#1F1F21");
        Style.Colors[(int)ImGuiCol.TableRowBgAlt] = HtmlToVec4("#1F1F21");
        Style.PopupRounding = 5;
        Style.CellPadding = new(10);
        Style.ScrollbarRounding = 0;
    }

    public static void PushButton(Vector4 col, Vector4 hCol, Vector4 aCol)
    {
        Style.Colors[(int)ImGuiCol.Button] = col;
        Style.Colors[(int)ImGuiCol.ButtonHovered] = hCol;
        Style.Colors[(int)ImGuiCol.ButtonActive] = aCol;
    }

    public static void PopButton()
    {
        Style.Colors[(int)ImGuiCol.Button] = Button;
        Style.Colors[(int)ImGuiCol.ButtonHovered] = ButtonHovered;
        Style.Colors[(int)ImGuiCol.ButtonActive] = ButtonActive;
    }
}
