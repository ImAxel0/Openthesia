using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Openthesia.Core.FileDialogs;
using Openthesia.Core.Plugins;
using Openthesia.Settings;

namespace Openthesia.Core.Midi;

public static class MidiFileHandler
{
    public static void LoadMidiFile(string filePath)
    {
        var midiFile = MidiFile.Read(filePath);
        MidiFileData.FileName = Path.GetFileName(filePath);
        LoadMidiFile(midiFile);
        Program._window.Title = $"Openthesia ({MidiFileData.FileName})";
    }

    public static void LoadMidiFile(MidiFile midi)
    {
        var midiFile = midi;

        MidiFileData.MidiFile = midiFile;
        MidiFileData.TempoMap = midiFile.GetTempoMap();
        MidiFileData.Notes = midiFile.GetNotes();

        if (MidiPlayer.Playback != null)
        {
            MidiPlayer.Playback.Stop();
            MidiPlayer.Playback.EventPlayed -= IOHandle.OnEventReceived;

            PlaybackCurrentTimeWatcher.Instance.Stop();
            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= MidiPlayer.OnCurrentTimeChanged;
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(MidiPlayer.Playback);
        }

        MidiPlayer.Playback = DevicesManager.ODevice != null
            ? midiFile.GetPlayback(DevicesManager.ODevice) : midiFile.GetPlayback();

        MidiPlayer.Playback.TrackNotes = true;
        MidiPlayer.Playback.TrackProgram = true;
        MidiPlayer.Playback.EventPlayed += IOHandle.OnEventReceived;
        MidiPlayer.Playback.Finished += MidiPlayer.Playback_Finished;
        MidiPlayer.Playback.NoteCallback = NoteCallback.HandMutingNoteCallback;

        PlaybackCurrentTimeWatcher.Instance.AddPlayback(MidiPlayer.Playback, TimeSpanType.Midi);
        PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += MidiPlayer.OnCurrentTimeChanged;
        PlaybackCurrentTimeWatcher.Instance.Start();
    }

    public static bool OpenMidiDialog()
    {
        var dialog = new OpenFileDialog()
        {
            Title = "Select a midi file",
            Filter = "midi files (*.mid)|*.mid"
        };
        dialog.ShowOpenFileDialog();

        if (dialog.Success)
        {
            var file = new FileInfo(dialog.Files.First());
            //MidiFileData.FileName = file.Name;
            LoadMidiFile(file.FullName);
            return true;
        }
        return false;
    }
}
