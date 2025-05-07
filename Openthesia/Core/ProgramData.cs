using Melanchall.DryWetMidi.Multimedia;
using Newtonsoft.Json;
using Openthesia.Core.Plugins;
using Openthesia.Core.SoundFonts;
using Openthesia.Settings;
using Syroot.Windows.IO;
using Vanara.PInvoke;

namespace Openthesia.Core;

public static class ProgramData
{
    public static readonly string ProgramVersion = "1.5.0";
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
        ImGuiTheme.PushTheme();

        if (CoreSettings.SoundEngine == Enums.SoundEngine.SoundFonts)
        {
            SoundFontPlayer.Initialize();
        }
        else if (CoreSettings.SoundEngine == Enums.SoundEngine.Plugins)
        {
            VstPlayer.Initialize();
            if (!string.IsNullOrEmpty(PluginsPathManager.InstrumentPath))
            {
                var instrument = new VstPlugin(PluginsPathManager.InstrumentPath);
                if (!CoreSettings.OpenPluginAtStart)
                {
                    instrument.PluginWindow.Close();
                }
                VstPlayer.PluginsChain?.AddPlugin(instrument);
            }

            foreach (var effect in PluginsPathManager.EffectsPath)
            {
                if (!string.IsNullOrEmpty(effect))
                {
                    var fx = new VstPlugin(effect);
                    if (!CoreSettings.OpenPluginAtStart)
                    {
                        fx.PluginWindow.Close();
                    }
                    VstPlayer.PluginsChain?.AddPlugin(fx);
                }
            }
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

                MidiPathsManager.LoadValidPaths(storedSettings.MidiPaths);             
                SoundFontsPathsManager.LoadValidPaths(storedSettings.SoundFontsPaths);
                PluginsPathManager.LoadValidInstrumentPath(storedSettings.InstrumentPath);
                PluginsPathManager.LoadValidEffectsPath(storedSettings.EffectsPath);
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
                CoreSettings.SetSoundEngine(storedSettings.SoundEngine);
                CoreSettings.SetSoundFontLatency(storedSettings.WaveOutLatency < 15 ? CoreSettings.WaveOutLatency : storedSettings.WaveOutLatency);
                AudioDriverManager.SetAudioDriverType(storedSettings.AudioDriverType);
                AudioDriverManager.SetAsioDriverDevice(storedSettings.SelectedAsioDriverName);
                CoreSettings.SetVideoRecDestFolder(string.IsNullOrEmpty(storedSettings.VideoRecDestFolder) 
                    ? KnownFolders.Videos.Path 
                    : storedSettings.VideoRecDestFolder);
                CoreSettings.SetVideoRecOpenDestFolder(storedSettings.VideoRecOpenDestFolder);
                CoreSettings.SetVideoRecStartsPlayback(storedSettings.VideoRecStartsPlayback);
                CoreSettings.SetVideoRecAutoPlay(storedSettings.VideoRecAutoPlay);
                CoreSettings.SetVideoRecFramerate(storedSettings.VideoRecFramerate == 0 ? 60 : storedSettings.VideoRecFramerate);
                CoreSettings.SetOpenPluginAtStartup(storedSettings.OpenPluginAtStart);
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

        // Update effect plugins order for next run
        if (CoreSettings.SoundEngine == Enums.SoundEngine.Plugins && VstPlayer.PluginsChain != null)
        {
            PluginsPathManager.EffectsPath.Clear();
            foreach (var effect in VstPlayer.PluginsChain.FxPlugins)
            {
                if (effect is VstPlugin vst)
                {
                    var path = vst.PluginContext.Find<string>("PluginPath");
                    if (!string.IsNullOrEmpty(path))
                    {
                        PluginsPathManager.EffectsPath.Add(path);
                    }
                }
            }
        }

        var data = new SettingsData()
        {
            InputDevice = DevicesManager.IDevice?.Name,
            OutputDevice = DevicesManager.ODevice?.Name,
            MidiPaths = MidiPathsManager.MidiPaths,
            SoundFontsPaths = SoundFontsPathsManager.SoundFontsPaths,
            InstrumentPath = PluginsPathManager.InstrumentPath,
            EffectsPath = PluginsPathManager.EffectsPath,
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
            SoundEngine = CoreSettings.SoundEngine,
            WaveOutLatency = CoreSettings.WaveOutLatency,
            AudioDriverType = AudioDriverManager.AudioDriverType,
            SelectedAsioDriverName = AudioDriverManager.SelectedAsioDriverName,
            VideoRecDestFolder = CoreSettings.VideoRecDestFolder,
            VideoRecOpenDestFolder = CoreSettings.VideoRecOpenDestFolder,
            VideoRecStartsPlayback = CoreSettings.VideoRecStartsPlayback,
            VideoRecAutoPlay = CoreSettings.VideoRecAutoPlay,
            VideoRecFramerate = CoreSettings.VideoRecFramerate,
            OpenPluginAtStart = CoreSettings.OpenPluginAtStart,
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
