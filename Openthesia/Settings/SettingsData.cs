using Openthesia.Enums;
using System.Numerics;

namespace Openthesia.Settings;

public class SettingsData
{
    public string InputDevice;
    public string OutputDevice;
    public List<string> MidiPaths = new();
    public List<string> SoundFontsPaths = new();
    public bool KeyboardInput;
    public bool AnimatedBackground;
    public bool NeonFx;
    public bool KeyPressColorMatch;
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
    public bool SoundFontEngine;
    public int SoundFontLatency;
    public AudioDriverTypes AudioDriverType;
    public string SelectedAsioDriverName;
}
