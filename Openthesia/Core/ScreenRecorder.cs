using Openthesia.Core.Midi;
using Openthesia.Settings;
using ScreenRecorderLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace Openthesia.Core;

public static class ScreenRecorder
{
    public static Recorder Recording { get; private set; }
    public static RecorderStatus Status { get; private set; }

    public static void StartRecording()
    {
        string fileName = MidiFileData.FileName.Replace(".mid", string.Empty);
        string date = DateTime.Now.ToString().Replace("/", "-").Replace(':', '.');
        string videoPath = Path.Combine(CoreSettings.VideoRecDestFolder, $"{fileName} {date}.mp4");

        var sources = new List<RecordingSourceBase>
        {
            new WindowRecordingSource(Program._window.Handle)
        };

        var options = new RecorderOptions
        {
            AudioOptions = new AudioOptions
            {
                IsAudioEnabled = true,
                IsOutputDeviceEnabled = true,
            },
            SourceOptions = new SourceOptions() {
                RecordingSources = sources
            },
            VideoEncoderOptions = new VideoEncoderOptions() {
                Framerate = CoreSettings.VideoRecFramerate
            },
        };

        Recording = Recorder.CreateRecorder(options);

        Recording.OnRecordingComplete += OnRecordingComplete;
        Recording.OnRecordingFailed += OnRecordingFailed;
        Recording.OnStatusChanged += OnStatusChanged;

        //Record to a file
        Recording.Record(videoPath);
    }

    public static void EndRecording()
    {
        Recording.Stop();
    }

    private static void OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
    {
        if (CoreSettings.VideoRecOpenDestFolder)
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(e.FilePath));
        }

        if (CoreSettings.VideoRecAutoPlay)
        {
            Process.Start("explorer.exe", e.FilePath);
        }
    }

    private static void OnRecordingFailed(object sender, RecordingFailedEventArgs e)
    {
        User32.MessageBox(IntPtr.Zero, e.Error, "Recording Failed", 
            User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
    }

    private static void OnStatusChanged(object sender, RecordingStatusEventArgs e)
    {
        Status = e.Status;
    }
}
