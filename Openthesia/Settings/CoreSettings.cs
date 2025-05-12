using Openthesia.Enums;
using Syroot.Windows.IO;

namespace Openthesia.Settings;

public static class CoreSettings
{
    private static bool _keyboardInput;
    public static ref bool KeyboardInput => ref _keyboardInput;

    private static bool _velocityZeroIsNoteOff = true;
    public static ref bool VelocityZeroIsNoteOff => ref _velocityZeroIsNoteOff;

    private static bool _animatedBackground = true;
    public static ref bool AnimatedBackground => ref _animatedBackground;

    private static bool _neonFx = true;
    public static ref bool NeonFx => ref _neonFx;

    private static bool _keypressColorMatch;
    public static ref bool KeyPressColorMatch => ref _keypressColorMatch;

    private static bool _useVelocityAsNoteOpacity;
    public static ref bool UseVelocityAsNoteOpacity => ref _useVelocityAsNoteOpacity;

    private static bool _fpsCounter;
    public static ref bool FpsCounter => ref _fpsCounter;

    private static int _noteRoundness = 7;
    public static ref int NoteRoundness => ref _noteRoundness;

    private static int _waveOutLatency = 75;
    public static ref int WaveOutLatency => ref _waveOutLatency;

    private static SoundEngine _soundEngine = SoundEngine.None;
    public static ref SoundEngine SoundEngine => ref _soundEngine;

    private static bool _openPluginAtStart;
    public static ref bool OpenPluginAtStart => ref _openPluginAtStart;

    private static int _sampleRate = 44100;
    public static int SampleRate => _sampleRate;

    #region Video Recording

    private static string _videoRecDestFolder = KnownFolders.Videos.Path;
    public static ref string VideoRecDestFolder => ref _videoRecDestFolder;

    private static bool _videoRecStartsPlayback = true;
    public static ref bool VideoRecStartsPlayback => ref _videoRecStartsPlayback;

    private static bool _videoRecOpenDestFolder = true;
    public static ref bool VideoRecOpenDestFolder => ref _videoRecOpenDestFolder;

    private static bool _videoRecAutoPlay;
    public static ref bool VideoRecAutoPlay => ref _videoRecAutoPlay;

    private static int _videoRecFramerate = 60;
    public static ref int VideoRecFramerate => ref _videoRecFramerate;

    #endregion

    public static void SetKeyboardInput(bool onoff)
    {
        _keyboardInput = onoff;
    }

    public static void SetVelocityZeroIsNoteOff(bool onoff)
    {
        _velocityZeroIsNoteOff = onoff;
    }

    public static void SetAnimatedBackground(bool onoff)
    {
        _animatedBackground = onoff;
    }

    public static void SetNeonFx(bool onoff)
    {
        _neonFx = onoff;
    }

    public static void SetKeyPressColorMatch(bool onoff)
    {
        _keypressColorMatch = onoff;
    }

    public static void SetUseVelocityAsNoteOpacity(bool onoff)
    {
        _useVelocityAsNoteOpacity = onoff;
    }

    public static void SetFpsCounter(bool onoff)
    {
        _fpsCounter = onoff;
    }

    public static void SetNoteRoundness(int value)
    {
        _noteRoundness = value;
    }

    public static void SetSoundFontLatency(int value)
    {
        _waveOutLatency = value;
    }

    public static void SetSoundEngine(SoundEngine soundEngine)
    {
        _soundEngine = soundEngine;
    }

    public static void SetVideoRecDestFolder(string path)
    {
        if (Directory.Exists(path))
        {
            _videoRecDestFolder = path;
        }
    }

    public static void SetVideoRecStartsPlayback(bool onoff)
    {
        _videoRecStartsPlayback = onoff;
    }

    public static void SetVideoRecOpenDestFolder(bool onoff)
    {
        _videoRecOpenDestFolder = onoff;
    }

    public static void SetVideoRecAutoPlay(bool onoff)
    {
        _videoRecAutoPlay = onoff;
    }

    public static void SetVideoRecFramerate(int framerate)
    {
        _videoRecFramerate = framerate;
    }

    public static void SetOpenPluginAtStartup(bool onoff)
    {
        _openPluginAtStart = onoff;
    }
}
