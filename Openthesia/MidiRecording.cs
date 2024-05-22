using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Openthesia.FileDialogs;
using Vanara.PInvoke;

namespace Openthesia;

public class MidiRecording
{
    private static Recording _recInstance;

    public static void StartRecording()
    {
        if (Settings.IDevice == null)
        {
            User32.MessageBox(IntPtr.Zero, "To start recording select an input device from the settings window", "No input device enabled",
                User32.MB_FLAGS.MB_ICONWARNING | User32.MB_FLAGS.MB_TOPMOST);
            return;
        }

        if (IsRecording())
        {
            return;
        }

        _recInstance = new Recording(TempoMap.Default, Settings.IDevice);
        _recInstance.Start();
    }

    public static void StopRecording()
    {
        _recInstance?.Stop();
        _recInstance?.Dispose();
    }

    public static bool IsRecording()
    {
        if (_recInstance == null)
        {
            return false;
        }
        return _recInstance.IsRunning;
    }

    public static MidiFile GetRecordedMidi()
    {
        return _recInstance?.ToFile();
    }

    public static void SaveRecordingToFile()
    {
        if (_recInstance == null)
        {
            User32.MessageBox(IntPtr.Zero, "No midi recording to save", "Warning",
                User32.MB_FLAGS.MB_ICONWARNING| User32.MB_FLAGS.MB_TOPMOST);
            return;
        }

        StopRecording();

        var saveFileDialog = new SaveFileDialog();
        bool result = saveFileDialog.ShowDialog(filter: "Midi file (*.mid)\0*.mid", title: "Save recorded midi file");

        if (result)
        {
            var recordedMidi = GetRecordedMidi();
            recordedMidi?.Write(saveFileDialog.FileName, true);
        }
        else
        {
            User32.MessageBox(IntPtr.Zero, "Couldn't save recorded midi file at location", "Error saving midi file", 
                User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
        }
    }
}
