using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Openthesia.Core.Midi;

public static class MidiFileData
{
    public static MidiFile MidiFile;
    public static string FileName = "No midi file opened";
    public static TempoMap TempoMap;
    public static IEnumerable<Note> Notes;

    public static void ReleaseMidiFile()
    {
        MidiFile = null;
        FileName = "No midi file opened";
        TempoMap = null;
    }
}
