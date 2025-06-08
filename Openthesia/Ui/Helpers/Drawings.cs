using ImGuiNET;
using Melanchall.DryWetMidi.Interaction;
using Openthesia.Enums;
using Openthesia.Settings;
using System.Numerics;

namespace Openthesia.Ui.Helpers;

public class Drawings
{
    public static IntPtr C;
    public static IntPtr CSharp;
    public static IntPtr CSharpWhite;
    public static IntPtr SustainPedalOff;
    public static IntPtr SustainPedalOn;

    public static void RenderMatrixBackground()
    {
        var drawList = ImGui.GetWindowDrawList();
        var io = ImGui.GetIO();
        var time = ImGui.GetTime();

        var screenWidth = (int)io.DisplaySize.X;
        var screenHeight = (int)io.DisplaySize.Y;

        var random = new Random(100);

        for (int i = 0; i < 20; i++)
        {
            int baseX = random.Next(0, screenWidth);
            int startY = random.Next(0, screenHeight);
            int length = random.Next(10, 50);
            float speed = random.Next(250, 500); // pixels/sec

            float y = (startY + (float)(time * speed)) % (screenHeight + length);

            if (CoreSettings.NeonFx)
            {
                for (int j = 0; j < 3; j++)
                {
                    float thickness = j * 2;
                    float alpha = 0.2f + (3 - j) * 0.2f;
                    uint color = ImGui.GetColorU32(new Vector4(
                        ThemeManager.RightHandCol.X,
                        ThemeManager.RightHandCol.Y,
                        ThemeManager.RightHandCol.Z,
                        alpha * 0.5f));

                    drawList.AddRect(
                        new Vector2(baseX - 1, y - 1),
                        new Vector2(baseX + 20 + 1, y + length + 1),
                        color,
                        5f,
                        0,
                        thickness
                    );
                }
            }

            drawList.AddRectFilled(
                new Vector2(baseX, y),
                new Vector2(baseX + 20, y + length),
                ImGui.GetColorU32(ThemeManager.RightHandCol),
                5,
                ImDrawFlags.RoundCornersAll
            );
        }
    }

    public static void Tooltip(string description)
    {
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    public static void NoteTooltip(string description)
    {
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
        ImGui.TextUnformatted(description);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    public static string GetNoteTextAs(TextTypes textType, Note note)
    {
        switch (textType)
        {
            case TextTypes.NoteName:
                return note.NoteName.ToString();
            case TextTypes.Velocity:
                return note.Velocity.ToString();
            case TextTypes.Octave:
                return note.Octave.ToString();
            default:
                return note.NoteName.ToString();
        }
    }
}
