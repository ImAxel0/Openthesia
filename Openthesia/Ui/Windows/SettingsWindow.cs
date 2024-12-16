using IconFonts;
using ImGuiNET;
using Melanchall.DryWetMidi.Multimedia;
using NAudio.Wave;
using System.Numerics;
using Vanara.PInvoke;
using static Openthesia.Settings.CoreSettings;
using static Openthesia.Settings.AudioDriverManager;
using static Openthesia.Settings.DevicesManager;
using static Openthesia.Settings.MidiPathsManager;
using static Openthesia.Settings.SoundFontsPathsManager;
using static Openthesia.Settings.ThemeManager;
using Openthesia.Ui.Helpers;
using Openthesia.Core;
using Openthesia.Core.FileDialogs;
using Openthesia.Core.Midi;
using Openthesia.Core.SoundFonts;
using Openthesia.Enums;

namespace Openthesia.Ui.Windows;

public class SettingsWindow : ImGuiWindow
{
    public SettingsWindow()
    {
        _id = Enums.Windows.Settings.ToString();
        _active = false;
    }

    protected override void OnImGui()
    {
        ImGui.BeginChild("Settings", ImGui.GetContentRegionAvail(), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar);
        ImGui.PushFont(FontController.GetFontOfSize(22));

        if (AnimatedBackground)
        {
            Drawings.RenderMatrixBackground();
        }

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(22, 50));
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, ImGuiUtils.FixedSize(new Vector2(100, 50))))
        {
            WindowsManager.SetWindow(Enums.Windows.Home);
        }
        ImGui.PopFont();

        ImGui.PushFont(FontController.Title);
        var textPos = new Vector2(ImGui.GetIO().DisplaySize.X / 2 - ImGui.CalcTextSize("SETTINGS").X / 2, ImGui.GetIO().DisplaySize.Y / 20);
        ImGui.SetCursorPos(textPos);
        ImGui.Text("SETTINGS");
        ImGui.PopFont();

        ImGuiTheme.Style.FramePadding = ImGuiUtils.FixedSize(new Vector2(15));
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
        ImGui.TableSetupColumn("##delete midi path", ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableHeadersRow();

        int index = 0;
        foreach (var path in MidiPaths.ToList())
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
                MidiPaths.Remove(path);
            }
            ImGui.PopID();
            ImGui.PopFont();
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new Vector4(1);
            index++;
        }

        ImGui.EndTable();

        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - ImGuiUtils.FixedSize(new Vector2(100)).X);
        if (ImGui.Button($"{FontAwesome6.FolderPlus}##addMidiPath", ImGuiUtils.FixedSize(new Vector2(100, 50))))
        {
            var dlg = new FolderPicker();
            dlg.InputPath = "C:\\";
            if (dlg.ShowDialog(Program._window.Handle) == true)
            {
                if (MidiPaths.Contains(dlg.ResultPath))
                {
                    User32.MessageBox(IntPtr.Zero, "Specified folder is already present", "Error", User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
                }
                else
                {
                    MidiPaths.Add(dlg.ResultPath);
                }
            }
        }

        // SOUND FONTS
        ImGui.Text($"SOUND FONTS {FontAwesome6.Music}");

        if (ImGui.Checkbox("SoundFont engine", ref SoundFontEngine))
        {
            // if sound font wasn't loaded at startup, load it on enable
            if (SoundFontEngine && MidiPlayer.SoundFontEngine == null)
            {
                SoundFontPlayer.Initialize();
            }
        }
        Drawings.Tooltip("If enabled, built in or external soundfonts will be used for audio playback");

        if (SoundFontEngine)
        {
            ImGui.Dummy(new(10));

            if (ImGui.BeginCombo("Audio driver", AudioDriverType.ToString()))
            {
                foreach (var driver in Enum.GetValues<AudioDriverTypes>())
                {
                    var flag = driver == AudioDriverTypes.ASIO && !AsioOut.GetDriverNames().Any()
                        ? ImGuiSelectableFlags.Disabled : ImGuiSelectableFlags.None;

                    if (ImGui.Selectable(driver.ToString(), false, flag))
                    {
                        if (driver == AudioDriverTypes.ASIO)
                        {
                            // if switching to ASIO, select the first availible driver
                            SetAsioDriverDevice(AsioOut.GetDriverNames()[0]);
                        }
                        // change driver type then re-initialize the sound font engine
                        SetAudioDriverType(driver);
                        SoundFontPlayer.Initialize();
                    }
                }
                ImGui.EndCombo();
            }
            Drawings.Tooltip("Driver used by SoundFonts for sound playback\n" +
                "- WaveOut: higher latency, good enough for listening only\n" +
                "- ASIO: lower latency, ideal if playing a midi instrument");

            if (AudioDriverType == AudioDriverTypes.ASIO)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.20f, 0.32f, 0.94f, 1));
                ImGui.SameLine();
                if (ImGui.Button("ASIO settings"))
                {
                    MidiPlayer.SoundFontEngine?.AsioOut.ShowControlPanel();
                }
                ImGui.PopStyleColor();

                ImGui.Dummy(new(10));

                if (ImGui.BeginCombo("ASIO driver", SelectedAsioDriverName))
                {
                    foreach (var driver in AsioOut.GetDriverNames())
                    {
                        if (ImGui.Selectable(driver))
                        {
                            SetAsioDriverDevice(driver);
                            SoundFontPlayer.Initialize();
                        }
                    }
                    ImGui.EndCombo();
                }
            }

            if (AudioDriverType == AudioDriverTypes.WaveOut)
            {
                ImGui.Dummy(new(10));

                if (ImGui.SliderInt("SoundFont latency (WaveOut driver only)", ref SoundFontLatency, 15, 300, "%i", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.NoInput))
                {
                    MidiPlayer.SoundFontEngine?.ChangeLatency(SoundFontLatency);
                }
                Drawings.Tooltip("Lower values reduce sound lag but can introduce audio artifacts, " +
                    "values under 100 are recommended for an optimal playback (default = 75)");
            }
        }

        ImGui.Dummy(new(10));

        ImGui.BeginTable("Sound fonts paths scan", 3, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg);
        ImGui.TableSetupColumn("Path");
        ImGui.TableSetupColumn("N° of sound fonts", ImGuiTableColumnFlags.WidthFixed, 220);
        ImGui.TableSetupColumn("##delete sound font path", ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableHeadersRow();

        int index2 = 0;
        foreach (var path in SoundFontsPaths.ToList())
        {
            // disable built in path
            if (path == SoundFontsPaths[0])
                ImGui.BeginDisabled(true);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text(path);

            int nSoundFont = 0;
            foreach (var soundFont in Directory.GetFiles(path))
            {
                if (Path.GetExtension(soundFont) == ".sf2")
                {
                    nSoundFont++;
                }
            }
            ImGui.TableSetColumnIndex(1);
            ImGui.Text(nSoundFont.ToString());
            ImGui.TableSetColumnIndex(2);
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new Vector4(1, 0, 0.2f, 1);
            ImGui.PushFont(FontController.Font16_Icon12);
            ImGui.PushID(index2.ToString());
            if (ImGui.SmallButton(FontAwesome6.CircleXmark))
            {
                SoundFontsPaths.Remove(path);
            }
            ImGui.PopID();
            ImGui.PopFont();
            ImGuiTheme.Style.Colors[(int)ImGuiCol.Text] = new Vector4(1);
            index2++;

            if (path == SoundFontsPaths[0])
                ImGui.EndDisabled();
        }

        ImGui.EndTable();

        ImGui.SetCursorPosX(ImGui.GetContentRegionAvail().X - ImGuiUtils.FixedSize(new Vector2(100)).X);
        if (ImGui.Button($"{FontAwesome6.FolderPlus}##addSoundPath", ImGuiUtils.FixedSize(new Vector2(100, 50))))
        {
            var dlg = new FolderPicker();
            dlg.InputPath = "C:\\";
            if (dlg.ShowDialog(Program._window.Handle) == true)
            {
                if (SoundFontsPaths.Contains(dlg.ResultPath))
                {
                    User32.MessageBox(IntPtr.Zero, "Specified folder is already present", "Error", User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
                }
                else
                {
                    SoundFontsPaths.Add(dlg.ResultPath);
                }
            }
        }

        // INPUT
        ImGui.Text($"INPUT {FontAwesome6.Keyboard}");
        ImGui.Checkbox("Keyboard input", ref KeyboardInput);
        Drawings.Tooltip("When keyboard input is enabled, mouse input and shortcuts using letters are disabled");

        ImGui.Dummy(new(50));

        ImGui.Text($"LOOK AND FEEL {FontAwesome6.Paintbrush}");
        ImGui.SliderInt("Note block roundness", ref NoteRoundness, 0, 15);

        ImGui.Dummy(new(10));

        ImGui.ColorEdit4("Background color", ref MainBgCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.ColorEdit4("Right Hand color", ref RightHandCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.ColorEdit4("Left Hand color", ref LeftHandCol, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoAlpha | ImGuiColorEditFlags.NoDragDrop);
        ImGui.SameLine();
        ImGui.Checkbox("Notes glow FX", ref NeonFx);
        Drawings.Tooltip("Adds a subtle glowing effect around each note");

        ImGui.Dummy(new(10));

        ImGui.Checkbox("Colored keypresses", ref KeyPressColorMatch);
        Drawings.Tooltip("Pressed keys color matches notes color");
        ImGui.SameLine();
        ImGui.Checkbox("Velocity as note opacity", ref UseVelocityAsNoteOpacity);
        Drawings.Tooltip("If enabled, note blocks opacity will correspond to their velocity (Midi playback only)");
        ImGui.SameLine();
        ImGui.Checkbox("Animated background", ref AnimatedBackground);
        ImGui.SameLine();
        ImGui.Checkbox("Fps counter", ref FpsCounter);

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
