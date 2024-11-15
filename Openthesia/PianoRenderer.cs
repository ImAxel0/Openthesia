using ImGuiNET;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System.Numerics;

namespace Openthesia;

public class PianoRenderer
{
    static uint _black = ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#141414"));
    static uint _white = ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#FFFFFF"));
    static uint _whitePressed = ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#888888"));
    static uint _blackPressed = ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#555555"));

    public static float Width;
    public static float Height;
    public static Vector2 P;

    public static Dictionary<SevenBitNumber, int> WhiteNoteToKey = new();
    public static Dictionary<SevenBitNumber, int> BlackNoteToKey = new();

    public static void RenderKeyboard()
    {
        ImGui.PushFont(FontController.Font16_Icon12);
        ImDrawListPtr draw_list = ImGui.GetWindowDrawList();
        P = ImGui.GetCursorScreenPos();

        Width = ImGui.GetIO().DisplaySize.X * 1.9f / 100;
        Height = ImGui.GetIO().DisplaySize.Y - (ImGui.GetIO().DisplaySize.Y * 76f / 100);

        int cur_key = 21;
        int cCount = 1;
        for (int key = 0; key < 52; key++)
        {
            uint col = _white;

            if (ImGui.IsMouseHoveringRect(new(P.X + key * Width, P.Y), new(P.X + key * Width + Width, P.Y + Height)) && ImGui.IsMouseClicked(ImGuiMouseButton.Left)
                && !Settings.KeyboardInput)
            {
                // on key mouse press
                IOHandle.OnEventReceived(null,
                    new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOnEvent((SevenBitNumber)cur_key, new SevenBitNumber(127))));
                Settings.ODevice.SendEvent(new NoteOnEvent((SevenBitNumber)cur_key, new SevenBitNumber(127)));
            }
            
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !Settings.KeyboardInput)
            {
                if (IOHandle.PressedKeys.Contains(cur_key))
                {
                    // on key mouse release
                    IOHandle.OnEventReceived(null,
                        new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOffEvent((SevenBitNumber)cur_key, new SevenBitNumber(0))));
                    Settings.ODevice.SendEvent(new NoteOffEvent((SevenBitNumber)cur_key, new SevenBitNumber(0)));
                }
            }

            if (IOHandle.PressedKeys.Contains(cur_key))
            {
                /* partial fix to keys color match
                if (Settings.KeyPressColorMatch)
                {
                    int noteIndex = MidiFileData.Notes.ToList().IndexOf(MidiFileData.Notes.ToList().Find(x => x.NoteNumber == cur_key));
                    var color = LeftRightData.S_IsRightNote[noteIndex] ? ImGui.GetColorU32(Settings.R_HandColor) : ImGui.GetColorU32(Settings.L_HandColor);
                    col = color;
                }
                else
                {
                    col = _whitePressed;
                }
                */
                var color = Settings.KeyPressColorMatch ? ImGui.GetColorU32(Settings.R_HandColor) : _whitePressed;
                col = color;
            }

            var offset = IOHandle.PressedKeys.Contains(cur_key) ? 2 : 0;

            /*
            draw_list.AddRectFilled(
                    new Vector2(P.X + key * Width, P.Y) + new Vector2(offset, 0),
                    new Vector2(P.X + key * Width + Width, P.Y + Height) + new Vector2(offset, 0),
                    col, 7, ImDrawFlags.RoundCornersBottom);
            draw_list.AddRect(
                    new(P.X + key * Width, P.Y),
                    new(P.X + key * Width + Width, P.Y + Height),
                    _black, 0); */

            draw_list.AddImageRounded(Drawings.C,
                new Vector2(P.X + key * Width, P.Y) + new Vector2(offset, 0),
                new Vector2(P.X + key * Width + Width, P.Y + Height) + new Vector2(offset, 0), Vector2.Zero, Vector2.One, col, 5, ImDrawFlags.RoundCornersBottom);

            if (WhiteNoteToKey.Count < 52)
                WhiteNoteToKey.Add((SevenBitNumber)cur_key, key);

            if (key % 7 == 1)
            {
                var text = $"C{cCount}";
                ImGui.GetForegroundDrawList().AddText(new(P.X + key * Width + Width + (Width / 2 - ImGui.CalcTextSize(text).X / 2), 
                    P.Y + Height - 25 * FontController.DSF), _black, text);
                cCount++;
            }

            cur_key++;
            if (KeysUtils.HasBlack(key))
            {
                cur_key++;
            }
        }
        
        cur_key = 22;
        for (int key = 0; key < 52; key++)
        {
            if (BlackNoteToKey.Count < 52)
                BlackNoteToKey.Add((SevenBitNumber)cur_key, key);

            if (KeysUtils.HasBlack(key))
            {
                uint col = ImGui.GetColorU32(Vector4.One);
               
                if (ImGui.IsMouseHoveringRect(new(P.X + key * Width + Width * 3 / 4, P.Y), 
                    new(P.X + key * Width + Width * 5 / 4 + 1, P.Y + Height / 1.5f)) && ImGui.IsMouseClicked(ImGuiMouseButton.Left)
                    && !Settings.KeyboardInput)
                {
                    IOHandle.OnEventReceived(null,
                        new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOnEvent((SevenBitNumber)cur_key, new SevenBitNumber(127))));
                    Settings.ODevice.SendEvent(new NoteOnEvent((SevenBitNumber)cur_key, new SevenBitNumber(127)));
                }
                
                if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && !Settings.KeyboardInput)
                {
                    if (IOHandle.PressedKeys.Contains(cur_key))
                    {
                        IOHandle.OnEventReceived(null,
                            new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOffEvent((SevenBitNumber)cur_key, new SevenBitNumber(0))));
                        Settings.ODevice.SendEvent(new NoteOffEvent((SevenBitNumber)cur_key, new SevenBitNumber(0)));
                    }
                }

                if (IOHandle.PressedKeys.Contains(cur_key))
                {
                    /* partial fix to keys color match
                    if (Settings.KeyPressColorMatch)
                    {
                        int noteIndex = MidiFileData.Notes.ToList().IndexOf(MidiFileData.Notes.ToList().Find(x => x.NoteNumber == cur_key));
                        var color = LeftRightData.S_IsRightNote[noteIndex] ? ImGui.GetColorU32(Settings.R_HandColor) : ImGui.GetColorU32(Settings.L_HandColor);
                        col = color;
                    }
                    else
                    {
                        col = _blackPressed;
                    }
                    */
                    var v3 = new Vector3(Settings.R_HandColor.X, Settings.R_HandColor.Y, Settings.R_HandColor.Z);
                    var color = Settings.KeyPressColorMatch ? ImGui.GetColorU32(new Vector4(v3, 1)) : _blackPressed;
                    col = color;
                }

                var offset = IOHandle.PressedKeys.Contains(cur_key) ? 1 : 0;
                var blackImage = IOHandle.PressedKeys.Contains(cur_key) ? Drawings.CSharpWhite : Drawings.CSharp;

                /*
                draw_list.AddRectFilledMultiColor(new Vector2(P.X + key * Width + Width * 3 / 4, P.Y),
                        new Vector2(P.X + key * Width + Width * 5 / 4 + 1, P.Y + Height / 1.5f), 
                        col, col, ImGui.GetColorU32(ImGuiTheme.HtmlToVec4("#666666")), col);
                
                draw_list.AddRect(
                        new Vector2(P.X + key * Width + Width * 3 / 4, P.Y),
                        new Vector2(P.X + key * Width + Width * 5 / 4 + 1, P.Y + Height / 1.5f),
                        _black, 0);
                */

                draw_list.AddImage(blackImage,
                    new Vector2(P.X + key * Width + Width * 3 / 4, P.Y),
                    new Vector2(P.X + key * Width + Width * 5 / 4 + 1, P.Y + Height / 1.5f) + new Vector2(offset), Vector2.Zero, Vector2.One, col);

                cur_key += 2;
            }
            else
            {
                cur_key++;
            }
        }
        ImGui.PopFont();
    }
}
