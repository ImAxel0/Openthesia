using IconFonts;
using ImGuiNET;
using Openthesia.Core;
using Openthesia.Core.Midi;
using Openthesia.Settings;
using Openthesia.Ui.Helpers;
using System.Numerics;

namespace Openthesia.Ui.Windows;

public class ModeSelectionWindow : ImGuiWindow
{
    public ModeSelectionWindow()
    {
        _id = Enums.Windows.ModeSelection.ToString();
        _active = false;
    }

    public static void RenderContainer()
    {
        ImGui.PushStyleColor(ImGuiCol.ChildBg, ThemeManager.MainBgCol * 0.8f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 2f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10f);
        ImGui.SetNextWindowPos(new((ImGui.GetIO().DisplaySize.X - ImGui.GetIO().DisplaySize.X / 1.2f) / 2, ImGuiUtils.FixedSize(new Vector2(120)).Y));
        if (ImGui.BeginChild("Container", new Vector2(ImGui.GetIO().DisplaySize.X / 1.2f, ImGui.GetIO().DisplaySize.Y / 1.2f),
            ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.Border))
        {
            ImGui.PopStyleVar(2);

            if (CoreSettings.AnimatedBackground)
                Drawings.RenderMatrixBackground();

            RenderTitle(MidiFileData.FileName.Replace(".mid", string.Empty), 50 * FontController.DSF);

            RenderIconWithText(FontAwesome6.Music, "Peacefully listen and visualize the piece", 0.1f, 2.5f);
            RenderIconWithText(FontAwesome6.Gamepad, "Playback will wait for the right note input", 0.36f, 2.5f);
            RenderIconWithText(FontAwesome6.Hands, "Separate right and left hands with colors", 0.625f, 2.5f);

            RenderButton("View and listen", "#31CB15", 0.1f, 1.5f, () => SetupMode(false, false));
            RenderButton("Play along", "#0EA5E9", 0.36f, 1.5f, () => SetupMode(true, false));
            RenderButton("Edit mode", "#772525", 0.625f, 1.5f, () => SetupMode(false, true));

            ImGui.EndChild();
        }
        ImGui.PopStyleColor();
    }

    private static void RenderBackButton()
    {
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(ImGuiUtils.FixedSize(new Vector2(22, 50)));
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, ImGuiUtils.FixedSize(new Vector2(100, 50))))
        {
            WindowsManager.SetWindow(Enums.Windows.MidiBrowser);
        }
        ImGui.PopFont();
    }

    private static void RenderTitle(string text, float offsetY)
    {
        ImGui.PushFont(FontController.Title);
        ImGui.SetCursorPos(new Vector2((ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X) / 2, offsetY));
        ImGui.Text(text);
        ImGui.PopFont();
    }

    private static void RenderIconWithText(string icon, string text, float xFactor, float yFactor)
    {
        var io = ImGui.GetIO();
        ImGui.PushFont(FontController.BigIcon);
        float xPos = io.DisplaySize.X * xFactor + ImGuiUtils.FixedSize(new Vector2(125)).X - ImGui.CalcTextSize(icon).X / 2;
        float yPos = io.DisplaySize.Y / yFactor;
   
        ImGui.SetCursorPos(new Vector2(xPos, yPos));
        ImGui.Text(icon);
        ImGui.PopFont();

        ImGui.PushFont(FontController.GetFontOfSize(22));
        var drawList = ImGui.GetWindowDrawList();
        drawList.AddText(new Vector2(io.DisplaySize.X * xFactor + ImGuiUtils.FixedSize(new Vector2(125)).X, yPos),
            ImGui.GetColorU32(Vector4.One), text);
        ImGui.PopFont();
    }

    private static void RenderButton(string label, string color, float xFactor, float yFactor, Action onClick)
    {
        var io = ImGui.GetIO();
        ImGuiTheme.PushButton(
            ImGuiTheme.HtmlToVec4(color),
            ImGuiTheme.HtmlToVec4(color, 0.7f),
            ImGuiTheme.HtmlToVec4(color)
        );

        ImGui.PushFont(FontController.GetFontOfSize(22));
        ImGui.SetCursorPos(new Vector2(io.DisplaySize.X * xFactor, io.DisplaySize.Y / yFactor));
        if (ImGui.Button(label, ImGuiUtils.FixedSize(new Vector2(250, 100))))
        {
            onClick.Invoke();
        }
        ImGui.PopFont();
        ImGuiTheme.PopButton();
    }

    private static void SetupMode(bool learningMode, bool editMode)
    {
        ScreenCanvasControls.SetLearningMode(learningMode);
        ScreenCanvasControls.SetEditMode(editMode);
        LeftRightData.S_IsRightNote.Clear();
        foreach (var note in MidiFileData.Notes)
        {
            LeftRightData.S_IsRightNote.Add(true);
        }
        MidiEditing.ReadData();
        WindowsManager.SetWindow(Enums.Windows.MidiPlayback);
    }


    protected override void OnImGui()
    {
        RenderBackButton();
        RenderContainer();
    }
}
