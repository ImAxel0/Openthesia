using IconFonts;
using ImGuiNET;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Openthesia.Core;
using Openthesia.Core.Midi;
using Openthesia.Core.SoundFonts;
using Openthesia.Enums;
using Openthesia.Settings;
using Openthesia.Ui.Helpers;
using System.Numerics;
using Veldrid;
using ScreenRecorderLib;
using Note = Melanchall.DryWetMidi.Interaction.Note;
using static Openthesia.Core.ScreenCanvasControls;
using Openthesia.Core.Plugins;
using Openthesia.Core.FileDialogs;
using Vanara.PInvoke;

namespace Openthesia.Ui;

public class ScreenCanvas
{
    public static Vector2 CanvasPos { get; private set; }

    // controls state to handle top bar hiding
    private static bool _leftHandColorPicker;
    private static bool _rightHandColorPicker;
    private static bool _comboFallSpeed;
    private static bool _comboPlaybackSpeed;
    private static bool _comboSoundFont;
    private static bool _comboPlugins;

    private static Vector2 _rectStart;
    private static Vector2 _rectEnd;
    private static bool _isRectMode;
    private static bool _isRightRect;
    private static bool _isHoveringTextBtn;
    private static bool _isProgressBarHovered;
    private static float _panVelocity;
    private static bool _isProgressBarActive;

    private static void RenderGrid()
    {
        var drawList = ImGui.GetWindowDrawList();
        for (int key = 0; key < 52; key++)
        {
            if (key % 7 == 2)
            {
                drawList.AddLine(CanvasPos + new Vector2(key * PianoRenderer.Width, 0),
                    new(PianoRenderer.P.X + key * PianoRenderer.Width, PianoRenderer.P.Y), ImGui.GetColorU32(new Vector4(Vector3.One, 0.08f)), 2);
            }
            else if (key % 7 == 5)
            {
                drawList.AddLine(CanvasPos + new Vector2(key * PianoRenderer.Width, 0),
                    new(PianoRenderer.P.X + key * PianoRenderer.Width, PianoRenderer.P.Y), ImGui.GetColorU32(new Vector4(Vector3.One, 0.06f)));
            }
        }
    }

    private static bool IsRectInside(Vector2 aMin, Vector2 aMax, Vector2 bMin, Vector2 bMax)
    {
        return aMin.X >= bMin.X && aMax.X <= bMax.X && aMin.Y >= bMin.Y && aMax.Y <= bMax.Y;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    private static void DrawInputNotes()
    {
        var speed = 100f * ImGui.GetIO().DeltaTime * FallSpeedVal;
        var drawList = ImGui.GetWindowDrawList();

        int index = 0;
        List<IOHandle.NoteRect> toRemove = new();
        foreach (var note in IOHandle.NoteRects.ToArray())
        {
            float py1;
            float py2;

            //int idx = IOHandle.NoteRects.IndexOf(note);

            var n = IOHandle.NoteRects[index];
            n.Time += speed;
            IOHandle.NoteRects[index] = n;

            var length = note.WasReleased ? note.FinalTime : note.Time;

            py1 = note.PY1 - note.Time;
            py2 = note.PY2 + length - note.Time;

            if (py2 < 0)
            {
                toRemove.Add(note);
                //IOHandle.NoteRects.Remove(note);
                index++;
                continue;
            }

            if (note.IsBlack)
            {
                if (CoreSettings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(ThemeManager.RightHandCol.X, ThemeManager.RightHandCol.Y, ThemeManager.RightHandCol.Z, alpha) * 0.5f * 0.7f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4 + 1, py2 + 1),
                            color,
                            CoreSettings.NoteRoundness,
                            0,
                            thickness
                        );
                    }
                }
                else
                {
                    uint color = ImGui.GetColorU32(new Vector4(Vector3.Zero, 1f) * 0.5f);
                    drawList.AddRect(
                        new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 - 1, py1 - 1),
                        new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4 + 1, py2 + 1),
                        color,
                        CoreSettings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4, py1),
                  new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4, py2),
                  ImGui.GetColorU32(ThemeManager.RightHandCol * 0.7f), CoreSettings.NoteRoundness, ImDrawFlags.RoundCornersAll);
            }
            else
            {
                if (CoreSettings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(ThemeManager.RightHandCol.X, ThemeManager.RightHandCol.Y, ThemeManager.RightHandCol.Z, alpha) * 0.5f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width + 1, py2 + 1),
                            color,
                            CoreSettings.NoteRoundness,
                            0,
                            thickness
                        );
                    }
                }
                else
                {
                    uint color = ImGui.GetColorU32(new Vector4(Vector3.Zero, 1f) * 0.5f);
                    drawList.AddRect(
                        new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width - 1, py1 - 1),
                        new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width + 1, py2 + 1),
                        color,
                        CoreSettings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width, py1),
                    new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width, py2),
                    ImGui.GetColorU32(ThemeManager.RightHandCol), CoreSettings.NoteRoundness, ImDrawFlags.RoundCornersAll);
            }
            index++;
        }

        if (toRemove.Count > 0)
        {
            IOHandle.NoteRects.RemoveRange(0, toRemove.Count - 1);
            IOHandle.NoteRects.RemoveAt(0);
        }
    }

    private static void DrawPlaybackNotes()
    {
        var drawList = ImGui.GetWindowDrawList();

        if (MidiPlayer.IsTimerRunning)
        {
            MidiPlayer.Timer += ImGui.GetIO().DeltaTime * 100f * (float)MidiPlayer.Playback.Speed * FallSpeedVal;
        }

        int index = 0;
        var notes = MidiFileData.Notes;
        bool missingNote = false;
        foreach (Note note in notes)
        {
            var time = (float)note.TimeAs<MetricTimeSpan>(MidiFileData.TempoMap).TotalSeconds * FallSpeedVal;
            var length = (float)note.LengthAs<MetricTimeSpan>(MidiFileData.TempoMap).TotalSeconds * FallSpeedVal;
            var col = LeftRightData.S_IsRightNote[index] ? ThemeManager.RightHandCol : ThemeManager.LeftHandCol;

            // color opacity based on note velocity
            if (CoreSettings.UseVelocityAsNoteOpacity)
            {
                col.W = note.Velocity * 1.27f / 161.29f;
                col.W = Math.Clamp(col.W, 0.3f, 1f); // we clamp it so they don't disappear with lower velocities
            }

            float py1;
            float py2;
            if (UpDirection && !IsLearningMode && !IsEditMode)
            {
                py1 = PianoRenderer.P.Y + time * 100 - MidiPlayer.Timer;
                py2 = PianoRenderer.P.Y + time * 100 + length * 100 - MidiPlayer.Timer;

                // skip notes outside of screen to save performance
                if (py1 > PianoRenderer.P.Y || py2 < 0)
                {
                    index++;
                    continue;
                }
            }
            else
            {
                py1 = PianoRenderer.P.Y - time * 100 + MidiPlayer.Timer;
                py2 = PianoRenderer.P.Y - time * 100 + length * 100 + MidiPlayer.Timer;

                py1 -= length * 100;
                py2 -= length * 100;

                if (IsLearningMode)
                {
                    if (py2 > PianoRenderer.P.Y - 1.5f && py2 < PianoRenderer.P.Y)
                    {
                        if (!IOHandle.PressedKeys.Contains(note.NoteNumber))
                        {
                            missingNote = true;
                            MidiPlayer.StopTimer();
                            MidiPlayer.Playback.Stop();

                            if (note.NoteName.ToString().EndsWith("Sharp"))
                            {
                                var v3 = new Vector3(col.X, col.Y, col.Z);
                                ImGui.GetForegroundDrawList().AddCircleFilled(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 + 10,
                                    py2 + PianoRenderer.Height / 1.7f), 7, ImGui.GetColorU32(new Vector4(v3, 1)));
                            }
                            else
                            {
                                ImGui.GetForegroundDrawList().AddCircleFilled(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width / 2,
                                    py2 + PianoRenderer.Height / 1.2f), 7, ImGui.GetColorU32(col));
                            }
                        }
                    }
                }

                if (IsEditMode && !_isProgressBarHovered && !_isProgressBarActive)
                {
                    if (ImGui.GetIO().KeyCtrl && ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_isRectMode)
                    {
                        _rectStart = ImGui.GetMousePos();
                        _isRightRect = false;
                        _isRectMode = true;
                    }

                    if (ImGui.GetIO().KeyCtrl && ImGui.IsMouseDown(ImGuiMouseButton.Right) && !_isRectMode)
                    {
                        _rectStart = ImGui.GetMousePos();
                        _isRightRect = true;
                        _isRectMode = true;
                    }

                    if (_isRectMode)
                    {
                        // only allow rect going top-left
                        if (ImGui.GetMousePos().Y > _rectStart.Y || ImGui.GetMousePos().X > _rectStart.X)
                        {
                            _isRectMode = false;
                        }

                        Vector4 rectCol = _isRightRect ? ThemeManager.RightHandCol : ThemeManager.LeftHandCol;
                        var v3 = new Vector3(rectCol.X, rectCol.Y, rectCol.Z);
                        ImGui.GetWindowDrawList().AddRectFilled(_rectStart, ImGui.GetMousePos(), ImGui.GetColorU32(new Vector4(v3, .005f)));

                        float rpx1;
                        float rpx2;
                        if (note.NoteName.ToString().EndsWith("Sharp"))
                        {
                            rpx1 = PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4;
                            rpx2 = PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4;
                        }
                        else
                        {
                            rpx1 = PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width;
                            rpx2 = PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width;
                        }

                        bool isInside = IsRectInside(_rectStart, ImGui.GetMousePos(), new(rpx1, py1), new(rpx2, py2));
                        if (isInside)
                        {
                            MidiEditing.SetRightHand(index, _isRightRect);
                        }
                    }

                    if ((ImGui.IsMouseReleased(ImGuiMouseButton.Left) || ImGui.IsMouseReleased(ImGuiMouseButton.Right)) && _isRectMode)
                    {
                        MidiEditing.SaveData();
                        _rectEnd = ImGui.GetMousePos();
                        _isRectMode = false;
                    }

                    if (note.NoteName.ToString().EndsWith("Sharp"))
                    {
                        if (ImGui.IsMouseHoveringRect(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4, py1),
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4, py2)))
                        {
                            if (ShowTextNotes)
                            {
                                Drawings.NoteTooltip($"Note: {note.NoteName}\nOctave: {note.Octave}\nVelocity: {note.Velocity}" +
                                    $"\nNumber: {note.NoteNumber}\nRight Hand: {LeftRightData.S_IsRightNote[index]}");
                            }

                            if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_isRectMode)
                            {
                                // set left
                                MidiEditing.SetRightHand(index, false);
                                MidiEditing.SaveData();
                            }
                            else if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && !_isRectMode)
                            {
                                // set right
                                MidiEditing.SetRightHand(index, true);
                                MidiEditing.SaveData();
                            }
                        }
                    }
                    else
                    {
                        if (ImGui.IsMouseHoveringRect(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width, py1),
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width, py2)))
                        {
                            if (ShowTextNotes)
                            {
                                Drawings.NoteTooltip($"Note: {note.NoteName}\nOctave: {note.Octave}\nVelocity: {note.Velocity}" +
                                    $"\nNumber: {note.NoteNumber}\nRight Hand: {LeftRightData.S_IsRightNote[index]}");
                            }

                            if (ImGui.IsMouseDown(ImGuiMouseButton.Left) && !_isRectMode)
                            {
                                // set left
                                MidiEditing.SetRightHand(index, false);
                                MidiEditing.SaveData();
                            }
                            else if (ImGui.IsMouseDown(ImGuiMouseButton.Right) && !_isRectMode)
                            {
                                // set right
                                MidiEditing.SetRightHand(index, true);
                                MidiEditing.SaveData();
                            }
                        }
                    }
                }
                else
                {
                    // Disable rect mode when the progress bar is hovered or active
                    _isRectMode = false;
                }

                // skip notes outside of screen to save performance
                if (py2 < 0 || py1 > PianoRenderer.P.Y)
                {
                    index++;
                    continue;
                }
            }

            if (note.NoteName.ToString().EndsWith("Sharp"))
            {
                if (CoreSettings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(col.X, col.Y, col.Z, alpha) * 0.5f * 0.7f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4 + 1, py2 + 1),
                            color,
                            CoreSettings.NoteRoundness,
                            0,
                            thickness
                        );
                    }
                }
                else
                {
                    uint color = ImGui.GetColorU32(new Vector4(Vector3.Zero, 1f) * 0.5f);
                    drawList.AddRect(
                        new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 - 1, py1 - 1),
                        new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4 + 1, py2 + 1),
                        color,
                        CoreSettings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4, py1),
                      new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4, py2),
                      ImGui.GetColorU32(col * 0.7f), CoreSettings.NoteRoundness, ImDrawFlags.RoundCornersAll);

                if (ShowTextNotes)
                {
                    ImGui.PushFont(FontController.Font16_Icon12);
                    string noteInfo = Drawings.GetNoteTextAs(TextType, note);
                    
                    if (TextType == TextTypes.NoteName)
                        noteInfo = noteInfo.Replace("Sharp", "#");

                    var pos = new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4,
                        py2 - length * 100 / 2 - ImGui.CalcTextSize(noteInfo).Y / 2);

                    drawList.AddText(pos + new Vector2(1), ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), noteInfo);
                    drawList.AddText(pos, ImGui.GetColorU32(Vector4.One), noteInfo);
                    ImGui.PopFont();
                }
            }
            else
            {
                if (CoreSettings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(col.X, col.Y, col.Z, alpha) * 0.5f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width + 1, py2 + 1),
                            color,
                            CoreSettings.NoteRoundness,
                            0,
                            thickness
                        );
                    }
                }
                else
                {
                    uint color = ImGui.GetColorU32(new Vector4(Vector3.Zero, 1f) * 0.5f);
                    drawList.AddRect(
                        new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width - 1, py1 - 1),
                        new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width + 1, py2 + 1),
                        color,
                        CoreSettings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width, py1),
                    new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width, py2),
                    ImGui.GetColorU32(col), CoreSettings.NoteRoundness, ImDrawFlags.RoundCornersAll);

                if (ShowTextNotes)
                {
                    ImGui.PushFont(FontController.Font16_Icon12);
                    string noteInfo = Drawings.GetNoteTextAs(TextType, note);
                    var pos = new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width / 2 - ImGui.CalcTextSize(noteInfo).X / 2,
                        py2 - length * 100 / 2 - ImGui.CalcTextSize(noteInfo).Y / 2);
                    drawList.AddText(pos + new Vector2(1), ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), noteInfo);
                    drawList.AddText(pos, ImGui.GetColorU32(Vector4.One), noteInfo);
                    ImGui.PopFont();
                }
            }
            index++;
        }
        if (IsLearningMode && !MidiPlayer.IsTimerRunning && !missingNote)
        {
            MidiPlayer.StartTimer();
            MidiPlayer.Playback.Start();
        }
    }

    private static void GetPlaybackInputs()
    {
        if (!IsLearningMode && !_isHoveringTextBtn)
        {
            if (ImGui.GetIO().MouseWheel < 0)
            {
                float speed = (float)(MidiPlayer.Playback.Speed - 0.25f);
                float cValue = Math.Clamp(speed, 0.25f, 4);
                MidiPlayer.Playback.Speed = cValue;
            }
            else if (ImGui.GetIO().MouseWheel > 0)
            {
                float speed = (float)(MidiPlayer.Playback.Speed + 0.25f);
                float cValue = Math.Clamp(speed, 0.25f, 4);
                MidiPlayer.Playback.Speed = cValue;
            }
        }

        var panButton = IsEditMode ? ImGuiMouseButton.Middle : ImGuiMouseButton.Right;
        if (ImGui.IsMouseHoveringRect(Vector2.Zero, new(ImGui.GetIO().DisplaySize.X, PianoRenderer.P.Y)) && ImGui.IsMouseDown(panButton))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeNS);
            const float interpolationFactor = 0.05f;
            const float decelerationFactor = 0.75f;
            float mouseDeltaY = ImGui.GetIO().MouseDelta.Y;
            if (UpDirection) mouseDeltaY = -mouseDeltaY;
            _panVelocity = Lerp(_panVelocity, mouseDeltaY, interpolationFactor);
            _panVelocity *= decelerationFactor;
            float targetTime = Math.Clamp(MidiPlayer.Seconds + _panVelocity, 0, (float)MidiPlayer.Playback.GetDuration<MetricTimeSpan>().TotalSeconds);
            var newTime = Lerp(MidiPlayer.Seconds, targetTime, interpolationFactor);
            long ms = (long)(newTime * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Seconds = newTime;
            MidiPlayer.Timer = MidiPlayer.Seconds * 100 * FallSpeedVal;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Space, false))
        {
            MidiPlayer.IsTimerRunning = !MidiPlayer.IsTimerRunning;
            if (MidiPlayer.IsTimerRunning)
            {
                MidiPlayer.Playback.Start();
            }
            else
            {
                MidiPlayer.Playback.Stop();
            }
        }

        if (ImGui.IsKeyPressed(ImGuiKey.R, false) && !CoreSettings.KeyboardInput && !IsLearningMode && !IsEditMode)
        {
            SetUpDirection(!UpDirection);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.T, false) && !CoreSettings.KeyboardInput)
        {
            SetTextNotes(!ShowTextNotes);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
        {
            float n = ImGui.GetIO().KeyCtrl ? 0.1f : 1f;
            var newTime = Math.Clamp(MidiPlayer.Seconds + n, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds);
            long ms = (long)(newTime * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Timer = newTime * 100 * FallSpeedVal;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
        {
            float n = ImGui.GetIO().KeyCtrl ? 0.1f : 1f;
            var newTime = Math.Clamp(MidiPlayer.Seconds - n, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds);
            long ms = (long)(newTime * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Timer = newTime * 100 * FallSpeedVal;
        }
    }

    private static void GetInputs()
    {
        if (CoreSettings.KeyboardInput)
        {
            VirtualKeyboard.ListenForKeyPresses();
        }

        if (ImGui.IsKeyPressed(ImGuiKey.G, false) && !CoreSettings.KeyboardInput)
        {
            CoreSettings.SetNeonFx(!CoreSettings.NeonFx);
        }

        if (!IsLearningMode)
        {
            if (ImGui.IsKeyPressed(ImGuiKey.UpArrow, false))
            {
                switch (FallSpeed)
                {
                    case FallSpeeds.Slow:
                        SetFallSpeed(FallSpeeds.Default);
                        break;
                    case FallSpeeds.Default:
                        SetFallSpeed(FallSpeeds.Fast);
                        break;
                    case FallSpeeds.Fast:
                        SetFallSpeed(FallSpeeds.Faster);
                        break;
                }
            }

            if (ImGui.IsKeyPressed(ImGuiKey.DownArrow, false))
            {
                switch (FallSpeed)
                {
                    case FallSpeeds.Faster:
                        SetFallSpeed(FallSpeeds.Fast);
                        break;
                    case FallSpeeds.Fast:
                        SetFallSpeed(FallSpeeds.Default);
                        break;
                    case FallSpeeds.Default:
                        SetFallSpeed(FallSpeeds.Slow);
                        break;
                }
            }
        }
    }

    public static void RenderCanvas(bool playMode = false)
    {
        using (AutoFont font22 = new(FontController.GetFontOfSize(22)))
        {
            CanvasPos = ImGui.GetWindowPos();
            RenderGrid();

            if (CoreSettings.FpsCounter)
            {
                var fps = $"{ImGui.GetIO().Framerate:0 FPS}";
                ImGui.GetWindowDrawList().AddText(new(ImGui.GetIO().DisplaySize.X - ImGui.CalcTextSize(fps).X - 5, ImGui.GetContentRegionAvail().Y - 25),
                    ImGui.GetColorU32(Vector4.One), fps);
            }

            if (playMode)
                DrawInputNotes();
            else
                DrawPlaybackNotes();

            GetInputs();

            var showTopBar = ImGui.IsMouseHoveringRect(Vector2.Zero, new(ImGui.GetIO().DisplaySize.X, 300));
            if (_comboFallSpeed || _comboPlaybackSpeed || _leftHandColorPicker || _rightHandColorPicker || _comboSoundFont || _comboPlugins)
                showTopBar = true;

            if (playMode)
            {
                if (showTopBar || LockTopBar)
                {
                    DrawPlayModeControls();
                    DrawPlayModeRightControls();
                }
            }

            if (!playMode)
            {
                GetPlaybackInputs();

                if (showTopBar || LockTopBar)
                {
                    DrawProgressBar();
                    DrawPlaybackControls();
                    DrawPlaybackRightControls();
                }
            }

            DrawSharedControls(showTopBar, playMode);
        }
    }

    private static void DrawProgressBar()
    {
        ImGui.SetNextItemWidth(ImGui.GetIO().DisplaySize.X);

        var pBarBg = new Vector3(ThemeManager.MainBgCol.X, ThemeManager.MainBgCol.Y, ThemeManager.MainBgCol.Z);
        var oldFrameBg = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg];
        var oldFrameBgHovered = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered];
        var oldFrameBgActive = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive];
        var oldSliderGrab = ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab];
        var oldSliderGrabActive = ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive];

        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(pBarBg, 0.8f);
        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(pBarBg, 0.8f);
        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(pBarBg, 0.8f);
        ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab] = ThemeManager.RightHandCol;
        ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive] = ThemeManager.RightHandCol;

        if (ImGui.SliderFloat("##Progress slider", ref MidiPlayer.Seconds, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds, "%.1f",
            ImGuiSliderFlags.NoRoundToFormat | ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.NoInput))
        {
            long ms = (long)(MidiPlayer.Seconds * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Timer = MidiPlayer.Seconds * 100 * FallSpeedVal;
        }
        _isProgressBarActive = ImGui.IsItemActive();
        _isProgressBarHovered = ImGui.IsItemHovered();
        if (_isProgressBarActive && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
        }

        var pBarHeight = ImGui.GetItemRectSize().Y;
        var playbackPercentage = MidiPlayer.Seconds * 100 / (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds;
        var pBarWidth = ImGui.GetIO().DisplaySize.X * playbackPercentage / 100;
        var v3 = new Vector3(ThemeManager.RightHandCol.X, ThemeManager.RightHandCol.Y, ThemeManager.RightHandCol.Z);
        ImGui.GetWindowDrawList().AddRectFilled(Vector2.Zero, new Vector2(pBarWidth, pBarHeight), ImGui.GetColorU32(new Vector4(v3, 0.2f)));

        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg] = oldFrameBg;
        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered] = oldFrameBgHovered;
        ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive] = oldFrameBgActive;
        ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab] = oldSliderGrab;
        ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive] = oldSliderGrabActive;
    }

    private static void DrawPlaybackControls()
    {
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X / 2 - ImGuiUtils.FixedSize(new Vector2(110)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.BeginChild("Player controls", ImGuiUtils.FixedSize(new Vector2(220, 50)), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var playColor = !MidiPlayer.IsTimerRunning ? Vector4.One : ThemeManager.RightHandCol;

            // PLAY BUTTON
            ImGui.PushFont(FontController.Font16_Icon16);
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = playColor;
            if (ImGui.Button($"{FontAwesome6.Play}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)))
            {
                MidiPlayer.Playback.Start();
                MidiPlayer.StartTimer();
            }
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
            var pauseColor = MidiPlayer.IsTimerRunning ? Vector4.One : new(0.70f, 0.22f, 0.22f, 1);
            ImGui.SameLine();
            // PAUSE BUTTON
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = pauseColor;
            if (ImGui.Button($"{FontAwesome6.Pause}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)))
            {
                MidiPlayer.Playback.Stop();
                MidiPlayer.IsTimerRunning = false;
            }
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
            ImGui.SameLine();
            // STOP BUTTON
            if (ImGui.Button($"{FontAwesome6.Stop}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)) || ImGui.IsKeyPressed(ImGuiKey.Backspace, false))
            {
                MidiPlayer.SoundFontEngine?.StopAllNote(0);
                MidiPlayer.Playback.Stop();
                MidiPlayer.Playback.MoveToStart();
                MidiPlayer.IsTimerRunning = false;
                MidiPlayer.Timer = 0;
            }
            ImGui.SameLine();
            // RECORD SCREEN BUTTON
            ImGui.PushStyleColor(ImGuiCol.Text, ScreenRecorder.Status == RecorderStatus.Recording ? new Vector4(0.08f, 0.80f, 0.27f, 1) : Vector4.One);
            if (ImGui.Button($"{FontAwesome6.Video}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y))
                || (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.R)))
            {
                switch (ScreenRecorder.Status)
                {
                    case RecorderStatus.Idle:
                        ScreenRecorder.StartRecording();
                        if (CoreSettings.VideoRecStartsPlayback)
                        {
                            MidiPlayer.Playback.Start();
                            MidiPlayer.StartTimer();
                        }
                        break;
                    case RecorderStatus.Recording:
                        ScreenRecorder.EndRecording();
                        MidiPlayer.SoundFontEngine?.StopAllNote(0);
                        MidiPlayer.Playback.Stop();
                        MidiPlayer.Playback.MoveToStart();
                        MidiPlayer.IsTimerRunning = false;
                        MidiPlayer.Timer = 0;
                        break;
                }
            }
            ImGui.PopStyleColor();

            ImGui.PopFont();
            ImGui.EndChild();
        }
    }

    private static void DrawPlaybackRightControls()
    {
        var directionIcon = UpDirection ? FontAwesome6.ArrowUp : FontAwesome6.ArrowDown;
        var icon = LockTopBar ? FontAwesome6.Lock : FontAwesome6.LockOpen;
        var showTextIcon = ShowTextNotes ? FontAwesome6.TextHeight : FontAwesome6.TextSlash;

        if (!IsLearningMode && !IsEditMode)
        {
            // NOTES DIRECTION BUTTON
            ImGui.PushFont(FontController.Font16_Icon16);
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(220)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
            if (ImGui.Button(directionIcon, ImGuiUtils.FixedSize(new Vector2(50))))
            {
                SetUpDirection(!UpDirection);
            }
            ImGui.PopFont();
        }

        // NOTES NOTATION BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(160)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.Button(showTextIcon, ImGuiUtils.FixedSize(new Vector2(50))))
        {
            SetTextNotes(!ShowTextNotes);
        }
        ImGui.PopFont();
        _isHoveringTextBtn = ImGui.IsItemHovered();
        if (_isHoveringTextBtn)
        {
            if (ImGui.GetIO().MouseWheel > 0)
            {
                switch (TextType)
                {
                    case TextTypes.Octave:
                        SetTextType(TextTypes.Velocity);
                        break;
                    case TextTypes.Velocity:
                        SetTextType(TextTypes.NoteName);
                        break;
                }
            }
            else if (ImGui.GetIO().MouseWheel < 0)
            {
                switch (TextType)
                {
                    case TextTypes.NoteName:
                        SetTextType(TextTypes.Velocity);
                        break;
                    case TextTypes.Velocity:
                        SetTextType(TextTypes.Octave);
                        break;
                }
            }

            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(160)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(250)).Y));
            ImGui.BeginGroup();
            foreach (var textType in Enum.GetValues<TextTypes>())
            {
                var selected = textType == TextType;
                ImGui.Selectable(textType.ToString(), selected);
            }
            ImGui.EndGroup();
        }

        // LOCK BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(100)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.Button(icon, ImGuiUtils.FixedSize(new Vector2(50))))
        {
            SetLockTopBar(!LockTopBar);
        }
        ImGui.PopFont();

        var fullScreenIcon = Program._window.WindowState == WindowState.BorderlessFullScreen ? FontAwesome6.Minimize : FontAwesome6.Expand;

        // FULLSCREEN BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(40)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.Button(fullScreenIcon, ImGuiUtils.FixedSize(new Vector2(25))))
        {
            var windowsState = Program._window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
            Program._window.WindowState = windowsState;
        }
        ImGui.PopFont();

        if (!IsLearningMode)
        {
            // FALLSPEED DROPDOWN LIST
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(220)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(110)).Y));
            if (ImGui.BeginCombo("##Fall speed", $"{FallSpeed}",
                ImGuiComboFlags.WidthFitPreview | ImGuiComboFlags.HeightLarge))
            {
                _comboFallSpeed = true;
                foreach (var speed in Enum.GetValues(typeof(FallSpeeds)))
                {
                    if (ImGui.Selectable(speed.ToString()))
                    {
                        SetFallSpeed((FallSpeeds)speed);
                    }
                }
                ImGui.EndCombo();
            }
            else
                _comboFallSpeed = false;

            // PLAYBACK SPEED DROPDOWN LIST
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(220)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(155)).Y));
            if (ImGui.BeginCombo("##Playback speed", $"{MidiPlayer.Playback.Speed}x",
                ImGuiComboFlags.WidthFitPreview | ImGuiComboFlags.HeightLarge))
            {
                _comboPlaybackSpeed = true;
                for (float i = 0.25f; i <= 4; i += 0.25f)
                {
                    if (ImGui.Selectable($"{i}x"))
                    {
                        MidiPlayer.Playback.Speed = i;
                    }
                }
                ImGui.EndCombo();
            }
            else
                _comboPlaybackSpeed = false;
        }
    }

    private static void DrawSharedControls(bool showTopBar, bool playMode)
    {
        if (!showTopBar && !LockTopBar)
            return;

        // BACK BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.BeginDisabled(ScreenRecorder.Status == RecorderStatus.Recording);
        ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(25)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, ImGuiUtils.FixedSize(new Vector2(100, 50))) || ImGui.IsKeyPressed(ImGuiKey.Escape, false))
        {
            MidiPlayer.Playback?.Stop();
            MidiPlayer.Playback?.MoveToStart();
            MidiPlayer.IsTimerRunning = false;
            MidiPlayer.Timer = 0;
            SetLearningMode(false);
            var route = playMode ? Enums.Windows.Home : Enums.Windows.MidiBrowser;
            WindowsManager.SetWindow(route);
        }
        ImGui.EndDisabled();
        ImGui.PopFont();

        var neonIcon = CoreSettings.NeonFx ? FontAwesome6.Lightbulb : FontAwesome6.PowerOff;

        // GLOW BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(25)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(110)).Y));
        if (ImGui.Button(neonIcon, ImGuiUtils.FixedSize(new Vector2(35))))
        {
            CoreSettings.SetNeonFx(!CoreSettings.NeonFx);
        }
        ImGui.PopFont();

        // LEFT HAND COLOR PICKER
        ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(70)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(110)).Y));
        ImGui.ColorEdit4("Left Hand Color", ref ThemeManager.LeftHandCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel
            | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoAlpha);

        _leftHandColorPicker = ImGui.IsPopupOpen("Left Hand Colorpicker");

        // RIGHT HAND COLOR PICKER
        ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(115)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(110)).Y));
        ImGui.ColorEdit4("Right Hand Color", ref ThemeManager.RightHandCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel
            | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoAlpha);

        _rightHandColorPicker = ImGui.IsPopupOpen("Right Hand Colorpicker");

        if (CoreSettings.SoundEngine == SoundEngine.SoundFonts)
        {
            // SOUNDFONTS DROPDOWN LIST
            ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(140)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
            if (ImGui.BeginCombo("##SoundFont", SoundFontPlayer.ActiveSoundFont, ImGuiComboFlags.HeightLargest | ImGuiComboFlags.WidthFitPreview))
            {
                _comboSoundFont = true;
                foreach (var folderPath in SoundFontsPathsManager.SoundFontsPaths)
                {
                    foreach (var soundFontPath in Directory.GetFiles(folderPath).Where(f => Path.GetExtension(f) == ".sf2"))
                    {
                        if (ImGui.Selectable(Path.GetFileNameWithoutExtension(soundFontPath)))
                        {
                            MidiPlayer.SoundFontEngine?.StopAllNote(0);
                            SoundFontPlayer.LoadSoundFont(soundFontPath);
                        }
                    }
                }
                ImGui.EndCombo();
            }
            else
                _comboSoundFont = false;
        }
        else if (CoreSettings.SoundEngine == SoundEngine.Plugins)
        {
            var instrument = VstPlayer.PluginsChain?.PluginInstrument;
            var name = instrument == null ? "No Plugin Instrument" : instrument.PluginName;

            // PLUGINS DROPDOWN LIST
            ImGui.SetCursorScreenPos(new(ImGuiUtils.FixedSize(new Vector2(140)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
            if (ImGui.BeginCombo("##Plugins", name, ImGuiComboFlags.HeightLargest | ImGuiComboFlags.WidthFitPreview))
            {
                _comboPlugins = true;

                ImGui.SeparatorText("Instrument");

                ImGui.Text(name);
                ImGui.SameLine();
                if (ImGui.SmallButton($"{FontAwesome6.ScrewdriverWrench}##tweak_instrument") && instrument is VstPlugin vstInstrument)
                {
                    vstInstrument.OpenPluginWindow();
                }
                ImGui.SameLine();
                if (ImGui.SmallButton($"{FontAwesome6.FolderOpen}##change_instrument"))
                {
                    var dialog = new OpenFileDialog()
                    {
                        Title = "Select a VST2 plugin instrument",
                        Filter = "vst plugin (*.dll)|*.dll"
                    };
                    dialog.ShowOpenFileDialog();

                    if (dialog.Success)
                    {
                        var file = new FileInfo(dialog.Files.First());
                        var plugin = new VstPlugin(file.FullName);
                        if (plugin.PluginType != PluginType.Instrument)
                        {
                            plugin.Dispose();
                            User32.MessageBox(IntPtr.Zero, "Plugin is not an instrument.", "Error Loading Plugin",
                                User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
                        }
                        else
                        {
                            VstPlayer.PluginsChain.AddPlugin(plugin);
                            PluginsPathManager.LoadValidInstrumentPath(file.FullName);
                        }
                    }
                }

                ImGui.Spacing();
                ImGui.SeparatorText("Effects");

                foreach (var effect in VstPlayer.PluginsChain.FxPlugins.ToList())
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(effect.PluginName);
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"{FontAwesome6.ScrewdriverWrench}##tweak_effect{effect.PluginId}") && effect is VstPlugin vstEffect)
                    {
                        vstEffect.OpenPluginWindow();
                    }
                    bool enabled = effect.Enabled;
                    string state = enabled ? "ON" : "OFF";
                    ImGui.SameLine();
                    if (ImGui.SmallButton($"{state}##{effect.PluginId}"))
                    {
                        effect.Enabled = !effect.Enabled;
                    }
                }

                ImGui.EndCombo();
            }
            else
                _comboPlugins = false;
        }

        // SUSTAIN PEDAL BUTTON
        ImGui.SetCursorPos(new Vector2(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(75)).X, ImGui.GetWindowSize().Y - ImGuiUtils.FixedSize(new Vector2(60)).Y));
        if (ImGui.ImageButton("SustainBtn", IOHandle.SustainPedalActive ? Drawings.SustainPedalOn : Drawings.SustainPedalOff,
            new Vector2(50)))
        {
            IOHandle.OnEventReceived(null, new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(
                new ControlChangeEvent(ControlUtilities.AsSevenBitNumber(ControlName.DamperPedal),
                new SevenBitNumber((byte)(IOHandle.SustainPedalActive ? 0 : 100)))));
            DevicesManager.ODevice?.SendEvent(new ControlChangeEvent(new SevenBitNumber(64), new SevenBitNumber((byte)(IOHandle.SustainPedalActive ? 0 : 100))));
        }
    }

    private static void DrawPlayModeControls()
    {
        ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X / 2 - ImGuiUtils.FixedSize(new Vector2(110)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.BeginChild("Player controls", ImGuiUtils.FixedSize(new Vector2(220, 50)), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        {
            var recordColor = MidiRecording.IsRecording() ? new Vector4(1, 0, 0, 1) : Vector4.One;

            // RECORD BUTTON
            ImGui.PushFont(FontController.Font16_Icon16);
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = recordColor;
            if (ImGui.Button($"{FontAwesome6.CircleDot}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)))
            {
                MidiRecording.StartRecording();
            }
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
            ImGui.SameLine();
            // STOP BUTTON
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new(0.70f, 0.22f, 0.22f, 1);
            if (ImGui.Button($"{FontAwesome6.Stop}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)))
            {
                MidiRecording.StopRecording();
            }
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
            ImGui.SameLine();
            // SAVE RECORDING BUTTON
            if (ImGui.Button($"{FontAwesome6.SdCard}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y)))
            {
                MidiRecording.SaveRecordingToFile();
            }
            ImGui.SameLine();
            // RECORD SCREEN BUTTON
            ImGui.PushStyleColor(ImGuiCol.Text, ScreenRecorder.Status == RecorderStatus.Recording ? new Vector4(0.08f, 0.80f, 0.27f, 1) : Vector4.One);
            if (ImGui.Button($"{FontAwesome6.Video}", new(ImGuiUtils.FixedSize(new Vector2(50)).X, ImGui.GetWindowSize().Y))
                || (ImGui.IsKeyDown(ImGuiKey.ModCtrl) && ImGui.IsKeyPressed(ImGuiKey.R)))
            {
                switch (ScreenRecorder.Status)
                {
                    case RecorderStatus.Idle:
                        MidiPlayer.ClearPlayback();
                        ScreenRecorder.StartRecording();
                        break;
                    case RecorderStatus.Recording:
                        ScreenRecorder.EndRecording();
                        break;
                }
            }
            ImGui.PopStyleColor();
            ImGui.PopFont();
            ImGui.EndChild();
        }      
    }

    private static void DrawPlayModeRightControls()
    {
        var icon = LockTopBar ? FontAwesome6.Lock : FontAwesome6.LockOpen;

        // LOCK BUTTON
        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(280)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
        if (ImGui.Button(icon, ImGuiUtils.FixedSize(new Vector2(50))))
        {
            SetLockTopBar(!LockTopBar);
        }
        ImGui.PopFont();

        if (!MidiRecording.IsRecording())
        {
            // VIEW LAST RECORDING BUTTON
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(220)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
            if (ImGui.Button("View last recording", ImGuiUtils.FixedSize(new Vector2(180, 50))))
            {
                var recordedMidi = MidiRecording.GetRecordedMidi();
                if (recordedMidi != null)
                {
                    LeftRightData.S_IsRightNote.Clear();
                    foreach (var n in recordedMidi.GetNotes())
                    {
                        LeftRightData.S_IsRightNote.Add(true);
                    }
                    MidiFileHandler.LoadMidiFile(recordedMidi);
                    WindowsManager.SetWindow(Enums.Windows.MidiPlayback);
                }
            }

            // FALLSPEED DROPDOWN LIST
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(220)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(110)).Y));
            if (ImGui.BeginCombo("##Fall speed", $"{FallSpeed}",
                ImGuiComboFlags.WidthFitPreview | ImGuiComboFlags.HeightLarge))
            {
                foreach (var speed in Enum.GetValues(typeof(FallSpeeds)))
                {
                    if (ImGui.Selectable(speed.ToString()))
                    {
                        SetFallSpeed((FallSpeeds)speed);
                    }
                }
                ImGui.EndCombo();
            }

            var fullScreenIcon = Program._window.WindowState == WindowState.BorderlessFullScreen ? FontAwesome6.Minimize : FontAwesome6.Expand;

            // FULLSCREEN BUTTON
            ImGui.PushFont(FontController.Font16_Icon16);
            ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - ImGuiUtils.FixedSize(new Vector2(30)).X, CanvasPos.Y + ImGuiUtils.FixedSize(new Vector2(50)).Y));
            if (ImGui.Button(fullScreenIcon, ImGuiUtils.FixedSize(new Vector2(25))))
            {
                var windowsState = Program._window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
                Program._window.WindowState = windowsState;
            }
            ImGui.PopFont();
        }
    }
}
