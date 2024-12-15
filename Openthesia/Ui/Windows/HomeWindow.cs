using ImGuiNET;
using Openthesia.Core;
using Openthesia.Settings;
using Openthesia.Ui.Helpers;
using System.Diagnostics;
using System.Numerics;

namespace Openthesia.Ui.Windows;

public class HomeWindow : ImGuiWindow
{
    private const string _title = "OPENTHESIA";
    private Vector2 _logoSize = new(250, 250);
    private Vector2 _btnHoverOffset = new(2);
    private Vector2 _titleShadowOffset = new(3);
    private Vector2 _buttonsShadowOffset = new(4);
    private Vector2 _buttonsSize = new(300, 50);
    private uint _titleShadowColor = ImGui.GetColorU32(new Vector4(0.13f, 0.83f, 0.93f, 0.5f));
    private bool _isPlayMidiHovered;
    private bool _isPlayModeHovered;
    private bool _isSettingsHovered;
    private bool _isExitHovered;

    public HomeWindow()
    {
        _id = Enums.Windows.Home.ToString();
        _active = true;
    }

    private void DrawTitle()
    {
        if (_io.DisplaySize.Y < 1079)
            return; // don't render on screen size lower than 1079 px

        float alpha = 0.5f * (1.0f + MathF.Sin(2.0f * MathF.PI * _timer));
        if (_timer >= 1f)
            _timer -= 1f;

        using (AutoFont titleFont = new(FontController.Title))
        {
            var textPos = new Vector2(ImGui.GetIO().DisplaySize.X / 2 - ImGui.CalcTextSize(_title).X / 2, ImGui.GetIO().DisplaySize.Y / 10);
            ImGui.SetCursorPos(textPos + _titleShadowOffset);
            ImGui.GetWindowDrawList().AddText(textPos + _titleShadowOffset, _titleShadowColor, _title);
            ImGui.SetCursorPos(textPos);
            ImGui.TextColored(new Vector4(1, 1, 1, alpha), _title);
        }
    }

    private void DrawLogo()
    {
        ImGui.SetCursorPos(ImGui.GetIO().DisplaySize / 2 - ImGuiUtils.FixedSize(new Vector2(_logoSize.X / 2, 300)));
        ImGui.Image(ProgramData.LogoImage, ImGuiUtils.FixedSize(_logoSize));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left, false))
            {
                Process.Start(new ProcessStartInfo("https://openthesia.pages.dev/") { UseShellExecute = true });
            }
        }
    }

    private void DrawButton(string label, (string idle, string hover, string active) htmlColor, ref bool btnHoverRef, Action onClick)
    {
        var drawList = ImGui.GetWindowDrawList();
        ImGuiTheme.PushButton(
            ImGuiTheme.HtmlToVec4(htmlColor.idle),
            ImGuiTheme.HtmlToVec4(htmlColor.hover),
            ImGuiTheme.HtmlToVec4(htmlColor.active));

        if (btnHoverRef)
        {
            ImGui.SetCursorPos(ImGui.GetCursorPos() + _btnHoverOffset);
            // Draw shadow rectangle
            Vector2 buttonPosScreen = ImGui.GetCursorScreenPos();
            Vector2 shadowPosScreen = buttonPosScreen + _buttonsShadowOffset;
            drawList.AddRectFilled(shadowPosScreen, shadowPosScreen + ImGuiUtils.FixedSize(_buttonsSize),
                ImGui.GetColorU32(ImGuiTheme.HtmlToVec4(htmlColor.idle)), 5.0f);
        }

        if (ImGui.Button(label, ImGuiUtils.FixedSize(_buttonsSize)))
            onClick.Invoke();

        btnHoverRef = ImGui.IsItemHovered();
        ImGuiTheme.PopButton();
    }

    private void RenderButtonsContainer()
    {
        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - ImGuiUtils.FixedSize(new Vector2(150, 0)));
        if (ImGui.BeginChild("Buttons container", ImGuiUtils.FixedSize(new Vector2(400, 300))))
        {
            DrawButton("PLAY MIDI FILE", ("#31CB15", "#20870E", "#31CB15"), ref _isPlayMidiHovered, () =>
            {
                WindowsManager.SetWindow(Enums.Windows.MidiBrowser);
            });

            ImGuiUtils.Spacing(2);

            DrawButton("PLAY MODE", ("#0EA5E9", "#096E9B", "#0EA5E9"), ref _isPlayModeHovered, () =>
            {
                WindowsManager.SetWindow(Enums.Windows.PlayMode);
            });

            ImGuiUtils.Spacing(2);

            DrawButton("SETTINGS", ("#464748", "#2E2F30", "#464748"), ref _isSettingsHovered, () =>
            {
                WindowsManager.SetWindow(Enums.Windows.Settings);
            });

            ImGuiUtils.Spacing(2);

            DrawButton("EXIT", ("#B33838", "#772525", "#B33838"), ref _isExitHovered, () =>
            {
                Application.AppInstance.Quit();
            });

            ImGui.EndChild();
        }
    }

    protected override void OnImGui()
    {
        using (AutoFont font22 = new(FontController.GetFontOfSize(22)))
        {
            if (CoreSettings.AnimatedBackground)
                Drawings.RenderMatrixBackground();

            DrawTitle();
            DrawLogo();
            RenderButtonsContainer();
        }
    }
}
