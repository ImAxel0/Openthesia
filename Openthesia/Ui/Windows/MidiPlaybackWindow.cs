using ImGuiNET;
using Openthesia.Core;
using Openthesia.Settings;
using System.Numerics;

namespace Openthesia.Ui.Windows;

public class MidiPlaybackWindow : ImGuiWindow
{
    public MidiPlaybackWindow()
    {
        _id = Enums.Windows.MidiPlayback.ToString();
        _active = false;
    }

    protected override void OnImGui()
    {
        Vector2 canvasSize = new(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y * 75 / 100);
        if (ImGui.BeginChild("Screen", canvasSize))
        {
            ScreenCanvas.RenderScreen();
            ImGui.EndChild();
        }

        Vector2 lineStart = new(0, ImGui.GetCursorPos().Y);
        Vector2 lineEnd = new(ImGui.GetContentRegionAvail().X, ImGui.GetCursorPos().Y);
        uint lineColor = ImGui.GetColorU32(ThemeManager.RightHandCol);
        const float lineThickness = 2f;
        ImGui.GetForegroundDrawList().AddLine(lineStart, lineEnd, lineColor, lineThickness);

        if (ImGui.BeginChild("Keyboard", ImGui.GetContentRegionAvail()))
        {
            PianoRenderer.RenderKeyboard();
            ImGui.EndChild();
        }
    }
}
