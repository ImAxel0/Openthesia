using Melanchall.DryWetMidi.Multimedia;
using Newtonsoft.Json;
using Syroot.Windows.IO;
using System.Numerics;
using Vanara.PInvoke;

namespace Openthesia;

public class ProgramData
{
    public static readonly string ProgramVersion = "1.0.0";
    public static IntPtr LogoImage;
    public static string SettingsPath = Path.Combine(KnownFolders.RoamingAppData.Path, "Openthesia", "Settings.json");

    private class SettingsJson
    {
        public string InputDevice;
        public string OutputDevice;
        public List<string> MidiPaths = new();
        public bool KeyboardInput;
        public bool AnimatedBackground;
        public bool NeonFx;
        public bool KeyPressColorMatch;
        public bool FpsCounter;
        public Settings.Themes Theme;
        public Vector4 MainBg;
        public Vector4 NotesColor;
        public bool LockTopBar;
        public bool UpDirection;
        public bool ShowTextNotes;
    }

    public static void Initialize()
    {
        Directory.CreateDirectory(Path.Combine(KnownFolders.RoamingAppData.Path, "Openthesia"));
        LoadSettings();
        if (InputDevice.GetDevicesCount() > 0 && Settings.IDevice == null)
        {
            Settings.SetInputDevice(0);
        }
        if (OutputDevice.GetDevicesCount() > 0 && Settings.ODevice == null)
        {
            Settings.SetOutputDevice(0);
        }
        ImGuiTheme.PushTheme();
    }

    public static void LoadSettings()
    {
        JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        if (File.Exists(SettingsPath))
        {
            string json = File.ReadAllText(SettingsPath);

            try
            {
                var storedSettings = JsonConvert.DeserializeObject<SettingsJson>(json, settings);

                if (!string.IsNullOrEmpty(storedSettings.InputDevice))
                {
                    Settings.SetInputDevice(storedSettings.InputDevice);
                }

                if (!string.IsNullOrEmpty(storedSettings.OutputDevice))
                {
                    Settings.SetOutputDevice(storedSettings.OutputDevice);
                }

                Settings.SetMidiPaths(storedSettings.MidiPaths);
                Settings.SetKeyboardInput(storedSettings.KeyboardInput);
                Settings.SetAnimatedBackground(storedSettings.AnimatedBackground);
                Settings.SetNeonFx(storedSettings.NeonFx);
                Settings.SetKeyPressColorMatch(storedSettings.KeyPressColorMatch);
                Settings.SetFpsCounter(storedSettings.FpsCounter);
                Settings.SetTheme(storedSettings.Theme);
                Settings.MainBg = storedSettings.MainBg;
                Settings.NotesColor = storedSettings.NotesColor;
                ScreenCanvas.SetLockTopBar(storedSettings.LockTopBar);
                ScreenCanvas.SetUpDirection(storedSettings.UpDirection);
                ScreenCanvas.SetTextNotes(storedSettings.ShowTextNotes);
            }
            catch (Exception ex)
            {
                User32.MessageBox(IntPtr.Zero, $"{ex.Message}", "Error loading program settings", User32.MB_FLAGS.MB_OK | User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
            }
        }
    }

    public static void SaveSettings()
    {
        JsonSerializerSettings settings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };

        var data = new SettingsJson()
        {
            InputDevice = Settings.IDevice?.Name,
            OutputDevice = Settings.ODevice?.Name,
            MidiPaths = Settings.MidiPaths,
            KeyboardInput = Settings.KeyboardInput,
            AnimatedBackground = Settings.AnimatedBackground,
            NeonFx = Settings.NeonFx,
            KeyPressColorMatch = Settings.KeyPressColorMatch,
            FpsCounter = Settings.FpsCounter,   
            Theme = Settings.Theme,
            MainBg = Settings.MainBg,
            NotesColor = Settings.NotesColor,
            LockTopBar = ScreenCanvas.LockTopBar,
            UpDirection = ScreenCanvas.UpDirection,
            ShowTextNotes = ScreenCanvas.ShowTextNotes,
        };

        string json = JsonConvert.SerializeObject(data, settings);

        try
        {
            File.WriteAllText(SettingsPath, json);
        }
        catch (Exception ex)
        {
            User32.MessageBox(IntPtr.Zero, $"{ex.Message}", "Error saving program settings", User32.MB_FLAGS.MB_OK | User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
        }
    }
}
