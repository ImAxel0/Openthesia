namespace Openthesia.Settings;

public static class CoreSettings
{
    private static bool _keyboardInput;
    public static ref bool KeyboardInput => ref _keyboardInput;

    private static bool _animatedBackground = true;
    public static ref bool AnimatedBackground => ref _animatedBackground;

    private static bool _neonFx = true;
    public static ref bool NeonFx => ref _neonFx;

    private static bool _keypressColorMatch;
    public static ref bool KeyPressColorMatch => ref _keypressColorMatch;

    private static bool _fpsCounter;
    public static ref bool FpsCounter => ref _fpsCounter;

    private static int _noteRoundness = 7;
    public static ref int NoteRoundness => ref _noteRoundness;

    private static bool _soundFontEngine;
    public static ref bool SoundFontEngine => ref _soundFontEngine;

    private static int _soundFontLatency = 75;
    public static ref int SoundFontLatency => ref _soundFontLatency;

    public static void SetKeyboardInput(bool onoff)
    {
        _keyboardInput = onoff;
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

    public static void SetFpsCounter(bool onoff)
    {
        _fpsCounter = onoff;
    }

    public static void SetNoteRoundness(int value)
    {
        _noteRoundness = value;
    }

    public static void SetUseSoundFontEngine(bool onoff)
    {
        _soundFontEngine = onoff;
    }

    public static void SetSoundFontLatency(int value)
    {
        _soundFontLatency = value;
    }

}
