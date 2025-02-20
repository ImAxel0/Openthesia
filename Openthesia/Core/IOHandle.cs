using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Openthesia.Core.Midi;
using Openthesia.Ui;

namespace Openthesia.Core;

public static class IOHandle
{
    public static List<int> PressedKeys { get; private set; } = new();

    public static List<NoteRect> NoteRects = new();

    private static HashSet<int> _sustainedNotes = new(); // Keeps track of sustained notes

    private static bool _sustainPedalActive = false;
    public static bool SustainPedalActive => _sustainPedalActive;

    public struct NoteRect
    {
        public int KeyNum;
        public bool IsBlack;
        public float PY1;
        public float PY2;
        public float Time;
        public bool WasReleased;
        public float FinalTime;
    }

    private static void OnKeyPress(NoteOnEvent ev)
    {
        // Check if sustain pedal is active
        if (_sustainPedalActive)
        {
            // add to sustained notes
            _sustainedNotes.Add(ev.NoteNumber);
        }

        if (WindowsManager.Window == Enums.Windows.PlayMode)
        {
            bool isBlack = ev.GetNoteName().ToString().EndsWith("Sharp");

            var note = new NoteRect()
            {
                KeyNum = ev.NoteNumber,
                IsBlack = isBlack,
                PY1 = PianoRenderer.P.Y,
                PY2 = PianoRenderer.P.Y,
                Time = 0f,
            };
            NoteRects.Add(note);
        }
        //MidiPlayer.RealTimeSoundFontPlayer.Synthesizer.ProcessMidiMessage(0, 144 /*NOTE ON*/, ev.NoteNumber, ev.Velocity);
        MidiPlayer.SoundFontEngine?.PlayNote(0, ev.NoteNumber, ev.Velocity);
        PressedKeys.Add(ev.NoteNumber);
    }

    private static void OnKeyRelease(NoteOffEvent ev)
    {
        if (_sustainPedalActive)
        {
            // If sustain pedal is active, don't stop the note immediately
            _sustainedNotes.Add(ev.NoteNumber);
        }
        else
        {
            // If sustain pedal is not active, stop the note immediately
            //MidiPlayer.RealTimeSoundFontPlayer.Synthesizer.ProcessMidiMessage(0, 128, ev.NoteNumber, ev.Velocity);
            MidiPlayer.SoundFontEngine?.StopNote(0, ev.NoteNumber);
        }

        if (WindowsManager.Window == Enums.Windows.PlayMode)
        {
            int index = NoteRects.FindIndex(x => x.KeyNum == ev.NoteNumber && !x.WasReleased);
            var n = NoteRects[index];
            //var n = NoteRects.Find(x => x.KeyNum == ev.NoteNumber && !x.WasReleased);
            //var n = NoteRects[NoteRects.Count - 1];
            n.WasReleased = true;
            n.FinalTime = n.Time;
            NoteRects[index] = n;
        }

        PressedKeys.Remove(ev.NoteNumber);
    }

    private static void OnSustainPedalOn()
    {
        _sustainPedalActive = true;
    }

    private static void OnSustainPedalOff()
    {
        _sustainPedalActive = false;
        // Stop all sustained notes when the sustain pedal is released
        foreach (var note in _sustainedNotes)
        {
            MidiPlayer.SoundFontEngine?.StopNote(0, note);
        }
        _sustainedNotes.Clear();
    }

    public static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
    {
        var eType = e.Event.EventType;

        switch (eType)
        {
            case MidiEventType.NoteOn:
                OnKeyPress((NoteOnEvent)e.Event);
                break;
            case MidiEventType.NoteOff:
                OnKeyRelease((NoteOffEvent)e.Event);
                break;
            case MidiEventType.ControlChange:
                var controlChangeEvent = (ControlChangeEvent)e.Event;
                if (controlChangeEvent.ControlNumber == 64) // 64 is the sustain pedal
                {
                    if (controlChangeEvent.ControlValue > 63)  // Sustain pedal ON (value greater than 63)
                    {
                        OnSustainPedalOn();
                    }
                    else  // Sustain pedal OFF (value <= 63)
                    {
                        OnSustainPedalOff();
                    }
                }
                break;
        }
    }

    public static void OnEventReceived(object sender, MidiEventPlayedEventArgs e)
    {
        // return in learning mode to prevent key presses
        if (ScreenCanvasControls.IsLearningMode)
            return;

        var eType = e.Event.EventType;

        switch (eType)
        {
            case MidiEventType.NoteOn:
                OnKeyPress((NoteOnEvent)e.Event);
                break;
            case MidiEventType.NoteOff:
                OnKeyRelease((NoteOffEvent)e.Event);
                break;
            case MidiEventType.ControlChange:
                var controlChangeEvent = (ControlChangeEvent)e.Event;
                if (controlChangeEvent.ControlNumber == 64) // 64 is the sustain pedal
                {
                    if (controlChangeEvent.ControlValue > 63)  // Sustain pedal ON (value greater than 63)
                    {
                        OnSustainPedalOn();
                    }
                    else  // Sustain pedal OFF (value <= 63)
                    {
                        OnSustainPedalOff();
                    }
                }
                break;
        }
    }

    public static void OnEventSent(object sender, MidiEventSentEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        //Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
    }
}
