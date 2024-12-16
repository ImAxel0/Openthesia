using Melanchall.DryWetMidi.Multimedia;
using Newtonsoft.Json;
using Openthesia.Core.SoundFonts;
using Openthesia.Settings;
using Openthesia.Ui;
using Syroot.Windows.IO;
using Vanara.PInvoke;

namespace Openthesia.Core;

public static class ProgramData
{
    public static readonly string ProgramVersion = "1.4.0";
    public static IntPtr LogoImage;
    public static string SettingsPath = Path.Combine(KnownFolders.RoamingAppData.Path, "Openthesia", "Settings.json");
    public static string HandsDataPath = Path.Combine(KnownFolders.RoamingAppData.Path, "Openthesia\\HandsData");

    public static void Initialize()
    {
        Directory.CreateDirectory(Path.Combine(KnownFolders.RoamingAppData.Path, "Openthesia"));
        Directory.CreateDirectory(HandsDataPath);
        LoadSettings();
        if (InputDevice.GetDevicesCount() > 0 && DevicesManager.IDevice == null)
        {
            DevicesManager.SetInputDevice(0);
        }
        if (OutputDevice.GetDevicesCount() > 0 && DevicesManager.ODevice == null)
        {
            DevicesManager.SetOutputDevice(0);
        }
        ImGuiTheme.PushTheme();

        if (CoreSettings.SoundFontEngine)
        {
            SoundFontPlayer.Initialize();
        }
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
                var storedSettings = JsonConvert.DeserializeObject<SettingsData>(json, settings);

                if (!string.IsNullOrEmpty(storedSettings.InputDevice))
                {
                    DevicesManager.SetInputDevice(storedSettings.InputDevice);
                }

                if (!string.IsNullOrEmpty(storedSettings.OutputDevice))
                {
                    DevicesManager.SetOutputDevice(storedSettings.OutputDevice);
                }

                MidiPathsManager.SetMidiPaths(storedSettings.MidiPaths);
                SoundFontsPathsManager.SetSoundFontsPaths(storedSettings.SoundFontsPaths);
                CoreSettings.SetKeyboardInput(storedSettings.KeyboardInput);
                CoreSettings.SetAnimatedBackground(storedSettings.AnimatedBackground);
                CoreSettings.SetNeonFx(storedSettings.NeonFx);
                CoreSettings.SetKeyPressColorMatch(storedSettings.KeyPressColorMatch);
                CoreSettings.SetUseVelocityAsNoteOpacity(storedSettings.UseVelocityAsNoteOpacity);
                CoreSettings.SetFpsCounter(storedSettings.FpsCounter);
                CoreSettings.SetNoteRoundness(storedSettings.NoteRoundness);
                ThemeManager.SetTheme(storedSettings.Theme);
                ThemeManager.MainBgCol = storedSettings.MainBg;
                ThemeManager.RightHandCol = storedSettings.R_HandColor;
                ThemeManager.LeftHandCol = storedSettings.L_HandColor;
                ScreenCanvasControls.SetLockTopBar(storedSettings.LockTopBar);
                ScreenCanvasControls.SetUpDirection(storedSettings.UpDirection);
                ScreenCanvasControls.SetTextNotes(storedSettings.ShowTextNotes);
                ScreenCanvasControls.SetTextType(storedSettings.TextType);
                CoreSettings.SetUseSoundFontEngine(storedSettings.SoundFontEngine);
                CoreSettings.SetSoundFontLatency(storedSettings.SoundFontLatency < 15 ? CoreSettings.SoundFontLatency : storedSettings.SoundFontLatency);
                AudioDriverManager.SetAudioDriverType(storedSettings.AudioDriverType);
                AudioDriverManager.SetAsioDriverDevice(storedSettings.SelectedAsioDriverName);
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

        var data = new SettingsData()
        {
            InputDevice = DevicesManager.IDevice?.Name,
            OutputDevice = DevicesManager.ODevice?.Name,
            MidiPaths = MidiPathsManager.MidiPaths,
            SoundFontsPaths = SoundFontsPathsManager.SoundFontsPaths,
            KeyboardInput = CoreSettings.KeyboardInput,
            AnimatedBackground = CoreSettings.AnimatedBackground,
            NeonFx = CoreSettings.NeonFx,
            KeyPressColorMatch = CoreSettings.KeyPressColorMatch,
            UseVelocityAsNoteOpacity = CoreSettings.UseVelocityAsNoteOpacity,
            NoteRoundness = CoreSettings.NoteRoundness,
            FpsCounter = CoreSettings.FpsCounter,
            Theme = ThemeManager.Theme,
            MainBg = ThemeManager.MainBgCol,
            R_HandColor = ThemeManager.RightHandCol,
            L_HandColor = ThemeManager.LeftHandCol,
            LockTopBar = ScreenCanvasControls.LockTopBar,
            UpDirection = ScreenCanvasControls.UpDirection,
            ShowTextNotes = ScreenCanvasControls.ShowTextNotes,
            TextType = ScreenCanvasControls.TextType,
            SoundFontEngine = CoreSettings.SoundFontEngine,
            SoundFontLatency = CoreSettings.SoundFontLatency,
            AudioDriverType = AudioDriverManager.AudioDriverType,
            SelectedAsioDriverName = AudioDriverManager.SelectedAsioDriverName,
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
