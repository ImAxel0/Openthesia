using IconFonts;
using ImGuiNET;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Numerics;
using Veldrid;
using Note = Melanchall.DryWetMidi.Interaction.Note;

namespace Openthesia;

public class ScreenCanvas
{
    public static Vector2 CanvasPos { get; private set; }

    static bool _lockTopBar = true;
    static bool _upDirection;
    static bool _showTextNotes;
    static bool _isLearningMode;
    static bool _isEditMode;

    // controls state to handle top bar hiding
    static bool _leftHandColorPicker;
    static bool _rightHandColorPicker;
    static bool _comboFallSpeed;
    static bool _comboPlaybackSpeed;
    static bool _comboSoundFont;

    public static bool LockTopBar { get { return _lockTopBar; } }
    public static bool UpDirection { get { return _upDirection; } }
    public static bool ShowTextNotes { get { return _showTextNotes; } }
    public static bool IsLearningMode { get { return _isLearningMode; } }
    public static bool IsEditMode { get { return _isEditMode; } }

    private static Vector2 _rectStart;
    private static Vector2 _rectEnd;
    private static bool _isRectMode;
    private static bool _isRightRect;
    private static bool _isHoveringTextBtn;
    private static bool _isProgressBarHovered;
    private static float _panVelocity;

    private static float _fallSpeed = 2f;
    public static FallSpeeds FallSpeed = FallSpeeds.Default;
    public enum FallSpeeds
    {
        Slow,
        Default,
        Fast,
        Faster
    }

    public static TextTypes TextType = TextTypes.Velocity;
    public enum TextTypes
    {
        NoteName,
        Velocity,
        Octave,
    }

    public static void SetFallSpeed(FallSpeeds speed)
    {
        switch (speed)
        {
            case FallSpeeds.Slow:
                _fallSpeed = 1;
                break;
            case FallSpeeds.Default:
                _fallSpeed = 2;
                break;
            case FallSpeeds.Fast:
                _fallSpeed = 3;
                break;
            case FallSpeeds.Faster:
                _fallSpeed = 4;
                break;
        }
        FallSpeed = speed;
        MidiPlayer.Timer = MidiPlayer.Seconds * 100 * _fallSpeed; // keeps in sync when changing speed
    }

    public static void SetLearningMode(bool onoff)
    {
        _isLearningMode = onoff;
    }

    public static void SetEditMode(bool onoff)
    {
        _isEditMode = onoff;
    }

    public static void SetLockTopBar(bool onoff)
    {
        _lockTopBar = onoff;
    }

    public static void SetUpDirection(bool onoff)
    {
        _upDirection = onoff;
    }

    public static void SetTextNotes(bool onoff)
    {
        _showTextNotes = onoff;
    }

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
        var speed = 100f * ImGui.GetIO().DeltaTime * _fallSpeed;
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
                if (Settings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(Settings.R_HandColor.X, Settings.R_HandColor.Y, Settings.R_HandColor.Z, alpha) * 0.5f * 0.7f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4 - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4 + 1, py2 + 1),
                            color,
                            Settings.NoteRoundness,
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
                        Settings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4, py1),
                  new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4, py2),
                  ImGui.GetColorU32(Settings.R_HandColor * 0.7f), Settings.NoteRoundness, ImDrawFlags.RoundCornersAll);
            }
            else
            {
                if (Settings.NeonFx)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float thickness = i * 2;
                        float alpha = 0.2f + (3 - i) * 0.2f;
                        uint color = ImGui.GetColorU32(new Vector4(Settings.R_HandColor.X, Settings.R_HandColor.Y, Settings.R_HandColor.Z, alpha) * 0.5f);
                        drawList.AddRect(
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width - 1, py1 - 1),
                            new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width + 1, py2 + 1),
                            color,
                            Settings.NoteRoundness,
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
                        Settings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width, py1),
                    new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault((SevenBitNumber)note.KeyNum, 0) * PianoRenderer.Width + PianoRenderer.Width, py2),
                    ImGui.GetColorU32(Settings.R_HandColor), Settings.NoteRoundness, ImDrawFlags.RoundCornersAll);
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
            MidiPlayer.Timer += ImGui.GetIO().DeltaTime * 100f * (float)MidiPlayer.Playback.Speed * _fallSpeed;
        }

        int index = 0;
        var notes = MidiFileData.Notes;
        foreach (Note note in notes)
        {
            var time = (float)note.TimeAs<MetricTimeSpan>(MidiFileData.TempoMap).TotalSeconds * _fallSpeed;
            var length = (float)note.LengthAs<MetricTimeSpan>(MidiFileData.TempoMap).TotalSeconds * _fallSpeed;
            var col = LeftRightData.S_IsRightNote[index] ? Settings.R_HandColor : Settings.L_HandColor;

            // color opacity based on note velocity
            col.W = note.Velocity * 1.27f / 161.29f;
            col.W = Math.Clamp(col.W, 0.3f, 1f); // we clamp it so they don't disappear with lower velocities

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
                        if (IOHandle.PressedKeys.Contains(note.NoteNumber))
                        {
                            if (!MidiPlayer.IsTimerRunning)
                            {
                                MidiPlayer.StartTimer();
                                MidiPlayer.Playback.Start();
                            }
                        }
                        else
                        {
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

                if (IsEditMode && !_isProgressBarHovered)
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

                        Vector4 rectCol = _isRightRect ? Settings.R_HandColor : Settings.L_HandColor;
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
                
                // skip notes outside of screen to save performance
                if (py2 < 0 || py1 > PianoRenderer.P.Y)
                {
                    index++;
                    continue;
                }
            }

            if (note.NoteName.ToString().EndsWith("Sharp"))
            {
                if (Settings.NeonFx)
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
                            Settings.NoteRoundness,
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
                        Settings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4, py1),
                      new(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 5 / 4, py2),
                      ImGui.GetColorU32(col * 0.7f), Settings.NoteRoundness, ImDrawFlags.RoundCornersAll);
                
                if (ShowTextNotes)
                {
                    ImGui.PushFont(FontController.Font16_Icon12);
                    string noteInfo = Drawings.GetNoteTextAs(TextType, note);
                    var pos = new Vector2(PianoRenderer.P.X + PianoRenderer.BlackNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width * 3 / 4,
                        py2 - length * 100 / 2 - ImGui.CalcTextSize(noteInfo).Y / 2);

                    if (TextType == TextTypes.NoteName)
                        noteInfo = noteInfo.Replace("Sharp", "#");

                    drawList.AddText(pos + new Vector2(1), ImGui.GetColorU32(new Vector4(0, 0, 0, 1)), noteInfo);
                    drawList.AddText(pos, ImGui.GetColorU32(Vector4.One), noteInfo);
                    ImGui.PopFont();
                }
            }
            else
            {
                if (Settings.NeonFx)
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
                            Settings.NoteRoundness,
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
                        Settings.NoteRoundness,
                        0,
                        1f
                    );
                }

                drawList.AddRectFilled(new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width, py1),
                    new(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width, py2),
                    ImGui.GetColorU32(col), Settings.NoteRoundness, ImDrawFlags.RoundCornersAll);
                
                if (ShowTextNotes)
                {
                    ImGui.PushFont(FontController.Font16_Icon12);
                    string noteInfo = Drawings.GetNoteTextAs(TextType, note);
                    var pos = new Vector2(PianoRenderer.P.X + PianoRenderer.WhiteNoteToKey.GetValueOrDefault(note.NoteNumber, 0) * PianoRenderer.Width + PianoRenderer.Width / 2 - ImGui.CalcTextSize(noteInfo).X / 2,
                        py2 - length * 100 / 2 - ImGui.CalcTextSize(noteInfo).Y / 2);
                    drawList.AddText(pos + new Vector2(1), ImGui.GetColorU32(new Vector4(0,0,0,1)), noteInfo);
                    drawList.AddText(pos, ImGui.GetColorU32(Vector4.One), noteInfo);
                    ImGui.PopFont();
                }
            }
            index++;
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
            MidiPlayer.Timer = MidiPlayer.Seconds * 100 * _fallSpeed;
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

        if (ImGui.IsKeyPressed(ImGuiKey.R, false) && !Settings.KeyboardInput && !IsLearningMode && !IsEditMode)
        {
            _upDirection = !_upDirection;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.T, false) && !Settings.KeyboardInput)
        {
            _showTextNotes = !_showTextNotes;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.RightArrow))
        {
            float n = ImGui.GetIO().KeyCtrl ? 0.1f : 1f;
            var newTime = Math.Clamp(MidiPlayer.Seconds + n, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds);
            long ms = (long)(newTime * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Timer = newTime * 100 * _fallSpeed;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
        {
            float n = ImGui.GetIO().KeyCtrl ? 0.1f : 1f;
            var newTime = Math.Clamp(MidiPlayer.Seconds - n, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds);
            long ms = (long)(newTime * 1000000);
            MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
            MidiPlayer.Timer = newTime * 100 * _fallSpeed;
        }
    }

    private static void GetInputs()
    {
        if (ImGui.IsKeyPressed(ImGuiKey.G, false) && !Settings.KeyboardInput)
        {
            Settings.SetNeonFx(!Settings.NeonFx);
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

    public static void RenderScreen(bool playMode = false)
    {
        ImGui.PushFont(FontController.GetFontOfSize(22));

        CanvasPos = ImGui.GetWindowPos();
        RenderGrid();

        if (Settings.FpsCounter)
        {
            var fps = $"{ImGui.GetIO().Framerate:0 FPS}";
            ImGui.GetWindowDrawList().AddText(new(ImGui.GetIO().DisplaySize.X - ImGui.CalcTextSize(fps).X - 5, ImGui.GetContentRegionAvail().Y - 25), 
                ImGui.GetColorU32(Vector4.One), fps);
        }

        if (Settings.KeyboardInput)
        {
            VirtualKeyboard.ListenForKeyPresses();
        }

        if (playMode)
        {
            DrawInputNotes();
        }
        else
        {
            DrawPlaybackNotes();
        }   
        
        GetInputs();

        var showTopBar = ImGui.IsMouseHoveringRect(Vector2.Zero, new(ImGui.GetIO().DisplaySize.X, 300));
        if (_comboFallSpeed || _comboPlaybackSpeed || _leftHandColorPicker || _rightHandColorPicker || _comboSoundFont)
            showTopBar = true;

        if (playMode)
        {
            if (showTopBar || LockTopBar)
            {
                ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X / 2 - 85 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                ImGui.BeginChild("Player controls", new Vector2(170, 50) * FontController.DSF, ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                var recordColor = MidiRecording.IsRecording() ? new Vector4(1, 0, 0, 1) : Vector4.One;

                ImGui.PushFont(FontController.Font16_Icon16);
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = recordColor;
                if (ImGui.Button($"{FontAwesome6.CircleDot}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)))
                {
                    MidiRecording.StartRecording();
                }
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
                ImGui.SameLine();
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new(0.70f, 0.22f, 0.22f, 1);
                if (ImGui.Button($"{FontAwesome6.Stop}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)))
                {
                    MidiRecording.StopRecording();
                }
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
                ImGui.SameLine();
                ImGui.PushFont(FontController.Font16_Icon16);
                if (ImGui.Button($"{FontAwesome6.SdCard}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)))
                {
                    MidiRecording.SaveRecordingToFile();
                }
                ImGui.PopFont();

                ImGui.PopFont();

                ImGui.EndChild();

                var icon = LockTopBar ? FontAwesome6.Lock : FontAwesome6.LockOpen;

                ImGui.PushFont(FontController.Font16_Icon16);
                ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 280 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                if (ImGui.Button(icon, new Vector2(50, 50) * FontController.DSF))
                {
                    _lockTopBar = !_lockTopBar;
                }
                ImGui.PopFont();

                if (!MidiRecording.IsRecording())
                {
                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 220 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                    if (ImGui.Button("View last recording", new Vector2(180, 50) * FontController.DSF))
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
                            Router.SetRoute(Router.Routes.MidiPlayback);
                        }
                    }

                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 220 * FontController.DSF, CanvasPos.Y + 110 * FontController.DSF));
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

                    var fullScreenIcon = Program._window.WindowState == Veldrid.WindowState.BorderlessFullScreen ? FontAwesome6.Minimize : FontAwesome6.Expand;

                    ImGui.PushFont(FontController.Font16_Icon16);
                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 30 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                    if (ImGui.Button(fullScreenIcon, new Vector2(25, 25) * FontController.DSF))
                    {
                        var windowsState = Program._window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
                        Program._window.WindowState = windowsState;
                    }
                    ImGui.PopFont();
                }
            }
        }

        if (!playMode)
        {        
            GetPlaybackInputs();

            if (showTopBar || LockTopBar)
            {
                ImGui.SetNextItemWidth(ImGui.GetIO().DisplaySize.X);

                var pBarBg = new Vector3(Settings.MainBg.X, Settings.MainBg.Y, Settings.MainBg.Z);
                var oldFrameBg = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg];
                var oldFrameBgHovered = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered];
                var oldFrameBgActive = ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive];
                var oldSliderGrab = ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab];
                var oldSliderGrabActive = ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive];

                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(pBarBg, 0.8f);
                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(pBarBg, 0.8f);
                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(pBarBg, 0.8f);
                ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab] = Settings.R_HandColor;
                ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive] = Settings.R_HandColor;

                if (ImGui.SliderFloat("##Progress slider", ref MidiPlayer.Seconds, 0, (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds, "%.1f",
                    ImGuiSliderFlags.NoRoundToFormat | ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.NoInput))
                {
                    long ms = (long)(MidiPlayer.Seconds * 1000000);
                    MidiPlayer.Playback.MoveToTime(new MetricTimeSpan(ms));
                    MidiPlayer.Timer = MidiPlayer.Seconds * 100 * _fallSpeed;
                }
                _isProgressBarHovered = ImGui.IsItemHovered();
                if (_isProgressBarHovered && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                {
                    ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                }
                var pBarHeight = ImGui.GetItemRectSize().Y;
                var playbackPercentage = MidiPlayer.Seconds * 100 / (float)MidiFileData.MidiFile.GetDuration<MetricTimeSpan>().TotalSeconds;
                var pBarWidth = ImGui.GetIO().DisplaySize.X * playbackPercentage / 100;
                var v3 = new Vector3(Settings.R_HandColor.X, Settings.R_HandColor.Y, Settings.R_HandColor.Z);
                ImGui.GetWindowDrawList().AddRectFilled(Vector2.Zero, new Vector2(pBarWidth, pBarHeight), ImGui.GetColorU32(new Vector4(v3, 0.2f)));

                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBg] = oldFrameBg;
                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgHovered] = oldFrameBgHovered;
                ImGuiTheme.Style.Colors[(int)ImGuiCol.FrameBgActive] = oldFrameBgActive;
                ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrab] = oldSliderGrab;
                ImGuiTheme.Style.Colors[(int)ImGuiCol.SliderGrabActive] = oldSliderGrabActive;

                ImGui.SetNextWindowPos(new Vector2(ImGui.GetIO().DisplaySize.X / 2 - 85 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                ImGui.BeginChild("Player controls", new Vector2(170, 50) * FontController.DSF, ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                var playColor = !MidiPlayer.IsTimerRunning ? Vector4.One : Settings.R_HandColor;

                ImGui.PushFont(FontController.Font16_Icon16);
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = playColor;
                if (ImGui.Button($"{FontAwesome6.Play}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)))
                {
                    MidiPlayer.Playback.Start();
                    MidiPlayer.StartTimer();
                }
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
                var pauseColor = MidiPlayer.IsTimerRunning ? Vector4.One : new(0.70f, 0.22f, 0.22f, 1);
                ImGui.SameLine();
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = pauseColor;
                if (ImGui.Button($"{FontAwesome6.Pause}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)))
                {
                    MidiPlayer.Playback.Stop();
                    MidiPlayer.IsTimerRunning = false;
                }
                ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = Vector4.One;
                ImGui.SameLine();
                if (ImGui.Button($"{FontAwesome6.Stop}", new(50 * FontController.DSF, ImGui.GetWindowSize().Y)) || ImGui.IsKeyPressed(ImGuiKey.Backspace, false))
                {
                    MidiPlayer.SoundFontEngine?.StopAllNote(0);
                    MidiPlayer.Playback.Stop();
                    MidiPlayer.Playback.MoveToStart();
                    MidiPlayer.IsTimerRunning = false;
                    MidiPlayer.Timer = 0;
                }

                ImGui.PopFont();
                ImGui.EndChild();

                var directionIcon = UpDirection ? FontAwesome6.ArrowUp : FontAwesome6.ArrowDown;
                var icon = LockTopBar ? FontAwesome6.Lock : FontAwesome6.LockOpen;
                var showTextIcon = ShowTextNotes ? FontAwesome6.TextHeight : FontAwesome6.TextSlash;

                if (!IsLearningMode && !IsEditMode)
                {
                    ImGui.PushFont(FontController.Font16_Icon16);
                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 220 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                    if (ImGui.Button(directionIcon, new Vector2(50, 50) * FontController.DSF))
                    {
                        _upDirection = !_upDirection;
                    }
                    ImGui.PopFont();
                }
                
                ImGui.PushFont(FontController.Font16_Icon16);
                ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 160 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                if (ImGui.Button(showTextIcon, new Vector2(50, 50) * FontController.DSF))
                {
                    _showTextNotes = !_showTextNotes;
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
                                TextType = TextTypes.Velocity;
                                break;
                            case TextTypes.Velocity:
                                TextType = TextTypes.NoteName;
                                break;
                        }
                    }
                    else if (ImGui.GetIO().MouseWheel < 0)
                    {
                        switch (TextType)
                        {
                            case TextTypes.NoteName:
                                TextType = TextTypes.Velocity;
                                break;
                            case TextTypes.Velocity:
                                TextType = TextTypes.Octave;
                                break;
                        }
                    }

                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 160 * FontController.DSF, CanvasPos.Y + 250 * FontController.DSF));
                    ImGui.BeginGroup();
                    foreach (var textType in Enum.GetValues<TextTypes>())
                    {
                        var selected = textType == TextType;
                        ImGui.Selectable(textType.ToString(), selected);
                    }
                    ImGui.EndGroup();
                }               

                ImGui.PushFont(FontController.Font16_Icon16);
                ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 100 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                if (ImGui.Button(icon, new Vector2(50, 50) * FontController.DSF))
                {
                    _lockTopBar = !_lockTopBar;
                }
                ImGui.PopFont();

                var fullScreenIcon = Program._window.WindowState == Veldrid.WindowState.BorderlessFullScreen ? FontAwesome6.Minimize : FontAwesome6.Expand;

                ImGui.PushFont(FontController.Font16_Icon16);
                ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 40 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                if (ImGui.Button(fullScreenIcon, new Vector2(25, 25) * FontController.DSF))
                {
                    var windowsState = Program._window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
                    Program._window.WindowState = windowsState;
                }
                ImGui.PopFont();

                if (!IsLearningMode)
                {
                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 220 * FontController.DSF, CanvasPos.Y + 110 * FontController.DSF));
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

                    ImGui.SetCursorScreenPos(new(ImGui.GetIO().DisplaySize.X - 220 * FontController.DSF, CanvasPos.Y + 155 * FontController.DSF));
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
        }

        if (showTopBar || LockTopBar)
        {
            ImGui.PushFont(FontController.Font16_Icon16);
            ImGui.SetCursorScreenPos(new(25 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
            if (ImGui.Button(FontAwesome6.ArrowLeftLong, new Vector2(100, 50) * FontController.DSF) || ImGui.IsKeyPressed(ImGuiKey.Escape, false))
            {
                MidiPlayer.Playback?.Stop();
                MidiPlayer.Playback?.MoveToStart();
                MidiPlayer.IsTimerRunning = false;
                MidiPlayer.Timer = 0;
                SetLearningMode(false);
                var route = playMode ? Router.Routes.Home : Router.Routes.MidiList;
                Router.SetRoute(route);
            }
            ImGui.PopFont();

            var neonIcon = Settings.NeonFx ? FontAwesome6.Lightbulb : FontAwesome6.PowerOff;

            ImGui.PushFont(FontController.Font16_Icon16);
            ImGui.SetCursorScreenPos(new(25 * FontController.DSF, CanvasPos.Y + 110 * FontController.DSF));
            if (ImGui.Button(neonIcon, new Vector2(35, 35) * FontController.DSF))
            {
                Settings.SetNeonFx(!Settings.NeonFx);
            }
            ImGui.PopFont();

            ImGui.SetCursorScreenPos(new(70 * FontController.DSF, CanvasPos.Y + 110 * FontController.DSF));
            ImGui.ColorEdit4("Left Hand Color", ref Settings.L_HandColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel
                | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoAlpha);

            _leftHandColorPicker = ImGui.IsPopupOpen("Left Hand Colorpicker");

            ImGui.SetCursorScreenPos(new(115 * FontController.DSF, CanvasPos.Y + 110 * FontController.DSF));
            ImGui.ColorEdit4("Right Hand Color", ref Settings.R_HandColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel
                | ImGuiColorEditFlags.NoDragDrop | ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoAlpha);

            _rightHandColorPicker = ImGui.IsPopupOpen("Right Hand Colorpicker");

            if (Settings.SoundFontEngine)
            {
                ImGui.SetCursorScreenPos(new(140 * FontController.DSF, CanvasPos.Y + 50 * FontController.DSF));
                if (ImGui.BeginCombo("##SoundFont", SoundFontPlayer.ActiveSoundFont, ImGuiComboFlags.HeightLargest | ImGuiComboFlags.WidthFitPreview))
                {
                    _comboSoundFont = true;
                    foreach (var folderPath in Settings.SoundFontsPaths)
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

            ImGui.SetCursorPos(new Vector2(ImGui.GetIO().DisplaySize.X - 75 * FontController.DSF, ImGui.GetWindowSize().Y - 60 * FontController.DSF));
            if (ImGui.ImageButton("SustainBtn", IOHandle.SustainPedalActive ? Drawings.SustainPedalOn : Drawings.SustainPedalOff,
                new Vector2(50)))
            {
                Settings.ODevice.SendEvent(new ControlChangeEvent(new SevenBitNumber(64), new SevenBitNumber((byte)(IOHandle.SustainPedalActive ? 0 : 100))));
            }
        }

        ImGui.PopFont();
    }
}
