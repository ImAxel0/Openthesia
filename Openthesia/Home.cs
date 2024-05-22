using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace Openthesia;

public class Home
{
    private static Vector2 _btnHoverOffset = new(2);
    private static Vector2 _titleShadowOffset = new(3);
    private static Vector2 _buttonsShadowOffset = new(4);
    private static Vector2 _buttonsSize = new(300, 50);
    private static uint _titleShadowColor = ImGui.GetColorU32(new Vector4(0.13f, 0.83f, 0.93f, 0.5f));
    private static bool _playMidi;
    private static bool _playMode;
    private static bool _settings;
    private static bool _exit;
    private static float _timer = 0;

    public static void Render()
    {
        ImGui.BeginChild("Home", ImGui.GetContentRegionAvail());
        ImGui.PushFont(FontController.GetFontOfSize(22));

        _timer += ImGui.GetIO().DeltaTime;
        float alpha = 0.5f * (1.0f + MathF.Sin(2.0f * MathF.PI * _timer));
        if (_timer >= 1f)
        {
            _timer -= 1f;
        }

        if (Settings.AnimatedBackground)
        {
            Drawings.RenderMatrixBackground();
        }

        if (ImGui.GetIO().DisplaySize.Y > 950)
        {
            ImGui.PushFont(FontController.Title);
            var textPos = new Vector2(ImGui.GetIO().DisplaySize.X / 2 - ImGui.CalcTextSize("OPENTHESIA").X / 2, ImGui.GetIO().DisplaySize.Y / 10);
            ImGui.SetCursorPos(textPos + _titleShadowOffset);
            ImGui.GetWindowDrawList().AddText(textPos + _titleShadowOffset, _titleShadowColor, "OPENTHESIA");
            ImGui.SetCursorPos(textPos);
            ImGui.TextColored(new Vector4(1, 1, 1, alpha), "OPENTHESIA");
            ImGui.PopFont();
        }

        ImGui.SetCursorPos(ImGui.GetIO().DisplaySize / 2 - new Vector2(125, 300));
        ImGui.Image(ProgramData.LogoImage, new(250, 250));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left, false))
            {
                Process.Start(new ProcessStartInfo("https://openthesia.pages.dev/") { UseShellExecute = true });
            }
        }

        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - new Vector2(150, 0));
        if (ImGui.BeginChild("Home buttons", new(400, 300)))
        {
            ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#31CB15"), ImGuiTheme.HtmlToVec4("#20870E"), ImGuiTheme.HtmlToVec4("#31CB15"));
            var drawList = ImGui.GetWindowDrawList();

            if (_playMidi)
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + _btnHoverOffset);
                // Draw shadow rectangle
                Vector2 buttonPosScreen = ImGui.GetCursorScreenPos();
                Vector2 shadowPosScreen = buttonPosScreen + _buttonsShadowOffset;
                drawList.AddRectFilled(shadowPosScreen, shadowPosScreen + _buttonsSize, ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#31CB15")), 5.0f);
            }
            if (ImGui.Button($"PLAY MIDI FILE", _buttonsSize))
            {
                Router.SetRoute(Router.Routes.MidiList);
            }
            _playMidi = ImGui.IsItemHovered();

            ImGui.Dummy(new(5));

            ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0EA5E9"), ImGuiTheme.HtmlToVec4("#096E9B"), ImGuiTheme.HtmlToVec4("#0EA5E9"));
            if (_playMode)
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + _btnHoverOffset);
                // Draw shadow rectangle
                Vector2 buttonPosScreen = ImGui.GetCursorScreenPos();
                Vector2 shadowPosScreen = buttonPosScreen + _buttonsShadowOffset;
                drawList.AddRectFilled(shadowPosScreen, shadowPosScreen + _buttonsSize, ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#0EA5E9")), 5.0f);
            }
            if (ImGui.Button($"PLAY MODE", _buttonsSize))
            {
                Router.SetRoute(Router.Routes.PlayMode);
            }
            _playMode = ImGui.IsItemHovered();

            ImGuiTheme.PopButton();
            ImGui.Dummy(new(5));

            if (_settings)
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + _btnHoverOffset);
                // Draw shadow rectangle
                Vector2 buttonPosScreen = ImGui.GetCursorScreenPos();
                Vector2 shadowPosScreen = buttonPosScreen + _buttonsShadowOffset;
                drawList.AddRectFilled(shadowPosScreen, shadowPosScreen + _buttonsSize, ImGui.GetColorU32(ImGuiTheme.Style.Colors[(int)ImGuiCol.Button]), 5.0f);
            }
            if (ImGui.Button($"SETTINGS", _buttonsSize))
            {
                Router.SetRoute(Router.Routes.Settings);
            }
            _settings = ImGui.IsItemHovered();

            ImGui.Dummy(new(5));

            ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#B33838"), ImGuiTheme.HtmlToVec4("#772525"), ImGuiTheme.HtmlToVec4("#B33838"));
            if (_exit)
            {
                ImGui.SetCursorPos(ImGui.GetCursorPos() + _btnHoverOffset);
                // Draw shadow rectangle
                Vector2 buttonPosScreen = ImGui.GetCursorScreenPos();
                Vector2 shadowPosScreen = buttonPosScreen + _buttonsShadowOffset;
                drawList.AddRectFilled(shadowPosScreen, shadowPosScreen + _buttonsSize, ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#B33838")), 5.0f);
            }
            if (ImGui.Button($"EXIT", _buttonsSize))
            {
                Program.IsRunning = false;
            }
            _exit = ImGui.IsItemHovered();

            ImGuiTheme.PopButton();
            ImGui.EndChild();
        }

        ImGui.EndChild();
        ImGui.PopFont();
    }
}
