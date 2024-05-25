using ImGuiNET;
using Melanchall.DryWetMidi.Multimedia;
using IconFonts;
using System.Numerics;
using Syroot.Windows.IO;
using Vanara.PInvoke;
using Openthesia.FileDialogs;

namespace Openthesia;

public class Settings
{
    public static InputDevice IDevice { get; private set; }
    public static OutputDevice ODevice { get; private set; }

    private static List<string> _midiPaths = new()
    {
        KnownFolders.Documents.Path,
        KnownFolders.Downloads.Path,
        KnownFolders.Music.Path,
    };

    public static List<string> MidiPaths { get { return _midiPaths; } }

    private static bool _keyboardInput;
    public static bool KeyboardInput { get { return _keyboardInput; } }

    private static bool _animatedBackground = true;
    public static bool AnimatedBackground { get { return _animatedBackground; } }

    private static bool _neonFx = true;
    public static bool NeonFx { get { return _neonFx; } }

    private static bool _keypressColorMatch;
    public static bool KeyPressColorMatch { get { return _keypressColorMatch; } }

    private static bool _fpsCounter;
    public static bool FpsCounter { get { return _fpsCounter; } }

    public static Themes Theme { get; private set; } = Themes.Sky;
    public enum Themes
    {
        Sky,
        Volcano,
        Synthesia,
    }

    public static Vector4 MainBg = ImGuiTheme.HtmlToVec4("#1F2937");
    //public static Vector4 NotesColor = ImGuiTheme.HtmlToVec4("#31CB15");
    public static Vector4 R_HandColor = ImGuiTheme.HtmlToVec4("#15CB44");
    public static Vector4 L_HandColor = ImGuiTheme.HtmlToVec4("#D4084A");

    public static void SetMidiPaths(List<string> paths)
    {
        _midiPaths = paths;
    }

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

    public static void SetTheme(Themes theme)
    {
        switch (theme)
        {
            case Themes.Sky:
                MainBg = ImGuiTheme.HtmlToVec4("#1F2937");
                R_HandColor = ImGuiTheme.HtmlToVec4("#15CB44");
                L_HandColor = ImGuiTheme.HtmlToVec4("#D4084A");
                break;

            case Themes.Volcano:
                MainBg = ImGuiTheme.HtmlToVec4("#151617");
                R_HandColor = ImGuiTheme.HtmlToVec4("#E51C1C");
                break;
            case Themes.Synthesia:
                MainBg = ImGuiTheme.HtmlToVec4("#313131");
                R_HandColor = ImGuiTheme.HtmlToVec4("#87C853");
                break;
        }
        Theme = theme;
        ImGuiTheme.PushTheme();
    }
    
    public static void SetInputDevice(int deviceIndex)
    {
        if (IDevice != null)
        {
            ReleaseInputDevice();
        }

        IDevice = InputDevice.GetByIndex(deviceIndex);
        IDevice.EventReceived += IOHandle.OnEventReceived;
        IDevice.StartEventsListening();
    }

    public static void SetInputDevice(string deviceName)
    {
        if (IDevice != null)
        {
            ReleaseInputDevice();
        }

        List<string> deviceNames = new();
        foreach (var iDevice in InputDevice.GetAll())
        {
            deviceNames.Add(iDevice.Name);
        }

        if (!deviceNames.Contains(deviceName))
            return;

        IDevice = InputDevice.GetByName(deviceName);
        if (IDevice != null)
        {
            IDevice.EventReceived += IOHandle.OnEventReceived;
            IDevice.StartEventsListening();
        }
    }

    public static void ReleaseInputDevice()
    {
        if (IDevice != null)
        {
            IDevice.Dispose();
        }
    }

    public static void SetOutputDevice(int deviceIndex)
    {
        if (ODevice != null)
        {
            ReleaseOutputDevice();
        }

        ODevice = OutputDevice.GetByIndex(deviceIndex);
        ODevice.EventSent += IOHandle.OnEventSent;
        ODevice.PrepareForEventsSending();
    }

    public static void SetOutputDevice(string deviceName)
    {
        if (ODevice != null)
        {
            ReleaseOutputDevice();
        }

        List<string> deviceNames = new();
        foreach (var oDevice in OutputDevice.GetAll())
        {
            deviceNames.Add(oDevice.Name);
        }

        if (!deviceNames.Contains(deviceName))
            return;

        ODevice = OutputDevice.GetByName(deviceName);
        if (ODevice != null)
        {
            ODevice.EventSent += IOHandle.OnEventSent;
            ODevice.PrepareForEventsSending();
        }
    }

    public static void ReleaseOutputDevice()
    {
        if (ODevice != null)
        {
            ODevice.Dispose();
        }
    }

    public static void Render()
    {
        ImGui.BeginChild("Settings", ImGui.GetContentRegionAvail(), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
        ImGui.PushFont(FontController.GetFontOfSize(22));

        if (AnimatedBackground)
        {
            Drawings.RenderMatrixBackground();
        }

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(22, 50));
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, new(100, 50)))
        {
            Router.SetRoute(Router.Routes.Home);
        }
        ImGui.PopFont();

        ImGuiTheme.Style.FramePadding = new(15);
        ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0284C7"), ImGuiTheme.HtmlToVec4("#0284C7"), ImGuiTheme.HtmlToVec4("#0284C7"));
        ImGuiTheme.Style.WindowPadding = new(10);

        ImGui.SetNextWindowPos(ImGui.GetIO().DisplaySize / 2 - new Vector2(ImGui.GetIO().DisplaySize.X / 1.5f, ImGui.GetIO().DisplaySize.Y / 1.4f) / 2);
        ImGui.BeginChild("Settings controls", new(ImGui.GetIO().DisplaySize.X / 1.5f, ImGui.GetIO().DisplaySize.Y / 1.2f), ImGuiChildFlags.AlwaysUseWindowPadding);

        ImGui.Text($"MIDI DEVICES {FontAwesome6.Keyboard}");

        if (InputDevice.GetDevicesCount() <= 0)
            ImGui.BeginDisabled();

        var inputName = IDevice != null ? IDevice.Name : "None";
        if (ImGui.BeginCombo($"Input device {FontAwesome6.CircleArrowRight}", inputName))
        {
            for (int i = 0; i < InputDevice.GetAll().Count; i++)
            {
                if (ImGui.Selectable(InputDevice.GetByIndex(i).Name))
                {
                    SetInputDevice(i);
                }
            }
            ImGui.EndCombo();
        }

        if (InputDevice.GetDevicesCount() <= 0)
            ImGui.EndDisabled();

        ImGui.Dummy(new(10));

        if (OutputDevice.GetDevicesCount() <= 0)
            ImGui.BeginDisabled();

        var outputName = ODevice != null ? ODevice.Name : "None";
        if (ImGui.BeginCombo($"Output device {FontAwesome6.CircleArrowLeft}", outputName))
        {
            for (int i = 0; i < OutputDevice.GetAll().Count; i++)
            {
                if (ImGui.Selectable(OutputDevice.GetByIndex(i).Name))
                {
                    SetOutputDevice(i);
                }
            }
            ImGui.EndCombo();
        }

        if (OutputDevice.GetDevicesCount() <= 0)
            ImGui.EndDisabled();

        ImGuiTheme.PopButton();

        ImGui.Dummy(new(50));

        ImGui.Text($"MIDI PATHS {FontAwesome6.FolderOpen}");

        ImGui.BeginTable("Midi paths scan", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("Path");
        ImGui.TableSetupColumn("N° of midi", ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn("##delete path", ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableHeadersRow();

        int index = 0;
        foreach (var path in _midiPaths.ToList())
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text(path);

            int nMidis = 0;
            foreach (var midiFile in Directory.GetFiles(path))
            {
                if (Path.GetExtension(midiFile) == ".mid")
                {
                    nMidis++;
                }               
            }
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(nMidis.ToString());
            ImGui.TableSetColumnIndex(2);
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new Vector4(1, 0, 0.2f, 1);
            ImGui.PushFont(FontController.Font16_Icon12);
            ImGui.PushID(index.ToString());
            if (ImGui.SmallButton(FontAwesome6.CircleXmark))
            {
                _midiPaths.Remove(path);
            }
            ImGui.PopID();
            ImGui.PopFont();
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new Vector4(1);
            index++;
        }

        ImGui.EndTable();

        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - 100);
        if (ImGui.Button(FontAwesome6.FolderPlus, new(100, 50)))
        {
            var dlg = new FolderPicker();
            dlg.InputPath = "C:\\";
            if (dlg.ShowDialog(Program._window.SdlWindowHandle) == true)
            {
                if (_midiPaths.Contains(dlg.ResultPath))
                {
                    User32.MessageBox(IntPtr.Zero, "Specified folder is already present", "Error", User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
                }
                else
                {
                    _midiPaths.Add(dlg.ResultPath);
                }
            }
        }

        ImGui.Text($"INPUT {FontAwesome6.Keyboard}");
        ImGui.Checkbox("Keyboard input", ref _keyboardInput);
        Drawings.Tooltip("When keyboard input is enabled, mouse input and shortcuts using letters are disabled");

        ImGui.Dummy(new(50));

        ImGui.Text($"LOOK AND FEEL {FontAwesome6.Paintbrush}");
        ImGui.ColorEdit4("Background color", ref MainBg, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.ColorEdit4("Right Hand color", ref R_HandColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.ColorEdit4("Left Hand color", ref L_HandColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.Checkbox("Notes glow FX", ref _neonFx);
        Drawings.Tooltip("Adds a subtle glowing effect around each note");

        ImGui.Dummy(new(10));

        ImGui.Checkbox("Colored keypresses", ref _keypressColorMatch);
        Drawings.Tooltip("Pressed keys color matches notes color");
        ImGui.SameLine();
        ImGui.Checkbox("Animated background", ref _animatedBackground);
        ImGui.SameLine();
        ImGui.Checkbox("Fps counter", ref _fpsCounter);
        
        ImGui.Dummy(new(10));

        ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0284C7"), ImGuiTheme.HtmlToVec4("#0284C7"), ImGuiTheme.HtmlToVec4("#0284C7"));
        if (ImGui.BeginCombo($"Theme {FontAwesome6.PaintRoller}", Theme.ToString()))
        {
            foreach (var theme in Enum.GetValues(typeof(Themes)))
            {
                if (ImGui.Selectable(theme.ToString()))
                {
                    SetTheme((Themes)theme);
                }
            }
            ImGui.EndCombo();
        }
        ImGuiTheme.PopButton();

        ImGui.EndChild();
        ImGui.EndChild();

        ImGui.PopFont();
        ImGuiTheme.PushTheme();
    }
}
