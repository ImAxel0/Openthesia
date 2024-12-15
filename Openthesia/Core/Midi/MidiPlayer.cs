using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Openthesia.Core.SoundFonts;

namespace Openthesia.Core.Midi;

public static class MidiPlayer
{
    public static SoundFontPlayer SoundFontEngine;
    public static Playback Playback;
    public static MetricTimeSpan Time;
    public static float Seconds;
    public static float Milliseconds;
    public static long Microseconds;

    public static bool IsTimerRunning;
    public static float Timer = 0;

    public static void OnCurrentTimeChanged(object sender, PlaybackCurrentTimeChangedEventArgs e)
    {
        foreach (var playbackTime in e.Times)
        {
            var s = (MidiTimeSpan)playbackTime.Time;
            Time = TimeConverter.ConvertTo<MetricTimeSpan>((ITimeSpan)s, MidiFileData.TempoMap);
            Seconds = (float)Time.TotalSeconds;
            Milliseconds = (float)Time.TotalMilliseconds;
            Microseconds = Time.TotalMicroseconds;
        }
    }

    public static void Playback_Finished(object? sender, EventArgs e)
    {
        StopTimer();
    }

    public static void StopTimer()
    {
        IsTimerRunning = false;
    }

    public static void StartTimer()
    {
        IsTimerRunning = true;
    }

    public static void ClearPlayback()
    {
        if (Playback == null)
            return;

        Playback.Stop();
        Playback.EventPlayed -= IOHandle.OnEventReceived;

        PlaybackCurrentTimeWatcher.Instance.Stop();
        PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= OnCurrentTimeChanged;
        PlaybackCurrentTimeWatcher.Instance.RemovePlayback(Playback);
        Playback = null;

        MidiFileData.ReleaseMidiFile();
    }
}