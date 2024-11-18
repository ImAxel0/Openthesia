using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace Openthesia;

public class IOHandle
{
    public static List<int> PressedKeys { get; private set; } = new();

    public static List<NoteRect> NoteRects = new();
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
        if (Router.Route == Router.Routes.PlayMode)
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

        PressedKeys.Add(ev.NoteNumber);
    }

    private static void OnKeyRelease(NoteOffEvent ev)
    {
        if (Router.Route == Router.Routes.PlayMode)
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
                ControlButtonsDev.OnControlChange((ControlChangeEvent)e.Event);
                break;
        }
    }

    public static void OnEventReceived(object sender, MidiEventPlayedEventArgs e)
    {
        // return in learning mode to prevent key presses
        if (ScreenCanvas.IsLearningMode)
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
        }
    }

    public static void OnEventSent(object sender, MidiEventSentEventArgs e)
    {
        var midiDevice = (MidiDevice)sender;
        //Console.WriteLine($"Event sent to '{midiDevice.Name}' at {DateTime.Now}: {e.Event}");
    }
}
