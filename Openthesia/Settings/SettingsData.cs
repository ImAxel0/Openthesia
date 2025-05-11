using Openthesia.Enums;
using System.Numerics;

namespace Openthesia.Settings;

public class SettingsData
{
    public string InputDevice;
    public string OutputDevice;

    public List<string> MidiPaths = new();
    public List<string> SoundFontsPaths = new();
    public string InstrumentPath = string.Empty;
    public List<string> EffectsPath = new();

    public bool KeyboardInput;
    public bool VelocityZeroIsNoteOff;
    public bool AnimatedBackground;
    public bool NeonFx;
    public bool KeyPressColorMatch;
    public bool UseVelocityAsNoteOpacity;
    public bool FpsCounter;
    public int NoteRoundness;

    public Themes Theme;
    public Vector4 MainBg;
    public Vector4 R_HandColor;
    public Vector4 L_HandColor;

    public bool LockTopBar;
    public bool UpDirection;
    public bool ShowTextNotes;
    public TextTypes TextType;

    public SoundEngine SoundEngine;
    public int WaveOutLatency;
    public AudioDriverTypes AudioDriverType;
    public string SelectedAsioDriverName;
    public bool OpenPluginAtStart;

    public string VideoRecDestFolder;
    public bool VideoRecStartsPlayback;
    public bool VideoRecOpenDestFolder;
    public bool VideoRecAutoPlay;
    public int VideoRecFramerate;
}
