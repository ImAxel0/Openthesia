using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Openthesia.Core.Midi;
using Openthesia.Core.Plugins;
using Openthesia.Settings;
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

    private static void OnKeyPressed(SevenBitNumber noteNumber, SevenBitNumber velocity, bool isBlack)
    {
        // Check if sustain pedal is active
        if (_sustainPedalActive)
        {
            // add to sustained notes
            _sustainedNotes.Add(noteNumber);
        }

        if (WindowsManager.Window == Enums.Windows.PlayMode)
        {
            var note = new NoteRect()
            {
                KeyNum = noteNumber,
                IsBlack = isBlack,
                PY1 = PianoRenderer.P.Y,
                PY2 = PianoRenderer.P.Y,
                Time = 0f,
            };
            NoteRects.Add(note);
        }

        MidiPlayer.SoundFontEngine?.PlayNote(0, noteNumber, velocity);
        PressedKeys.Add(noteNumber);
    }

    private static void OnKeyReleased(SevenBitNumber noteNumber)
    {
        if (_sustainPedalActive)
        {
            // If sustain pedal is active, don't stop the note immediately
            _sustainedNotes.Add(noteNumber);
        }
        else
        {
            // If sustain pedal is not active, stop the note immediately
            //MidiPlayer.RealTimeSoundFontPlayer.Synthesizer.ProcessMidiMessage(0, 128, noteNumber, ev.Velocity);
            MidiPlayer.SoundFontEngine?.StopNote(0, noteNumber);
        }

        if (WindowsManager.Window == Enums.Windows.PlayMode)
        {
            int index = NoteRects.FindIndex(x => x.KeyNum == noteNumber && !x.WasReleased);
            var n = NoteRects[index];
            //var n = NoteRects.Find(x => x.KeyNum == noteNumber && !x.WasReleased);
            //var n = NoteRects[NoteRects.Count - 1];
            n.WasReleased = true;
            n.FinalTime = n.Time;
            NoteRects[index] = n;
        }

        PressedKeys.Remove(noteNumber);
    }

    private static void OnNoteOn(NoteOnEvent ev)
    {
        SevenBitNumber velocity = ev.Velocity;
        if (CoreSettings.VelocityZeroIsNoteOff && velocity == 0)
        {
            OnKeyReleased(ev.NoteNumber);
        }
        else
        {
            bool isBlack = ev.GetNoteName().ToString().EndsWith("Sharp");
            OnKeyPressed(ev.NoteNumber, velocity, isBlack);
        }
    }

    private static void OnNoteOff(NoteOffEvent ev)
    {
        OnKeyReleased(ev.NoteNumber);
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

        if (CoreSettings.SoundEngine == Enums.SoundEngine.Plugins)
        {
            VstPlayer.PluginsChain?.PluginInstrument?.ReceiveMidiEvent(e.Event);
        }

        switch (eType)
        {
            case MidiEventType.NoteOn:
                OnNoteOn((NoteOnEvent)e.Event);
                break;
            case MidiEventType.NoteOff:
                OnNoteOff((NoteOffEvent)e.Event);
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

        if (CoreSettings.SoundEngine == Enums.SoundEngine.Plugins)
        {
            VstPlayer.PluginsChain?.PluginInstrument?.ReceiveMidiEvent(e.Event);
        }

        var eType = e.Event.EventType;

        switch (eType)
        {
            case MidiEventType.NoteOn:
                OnNoteOn((NoteOnEvent)e.Event);
                break;
            case MidiEventType.NoteOff:
                OnNoteOff((NoteOffEvent)e.Event);
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
