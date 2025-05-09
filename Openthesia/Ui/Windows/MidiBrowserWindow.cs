using IconFonts;
using ImGuiNET;
using Openthesia.Core;
using Openthesia.Core.Midi;
using Openthesia.Settings;
using Openthesia.Ui.Helpers;
using System.Numerics;

namespace Openthesia.Ui.Windows;

public class MidiBrowserWindow : ImGuiWindow
{
    private string _searchBuffer = string.Empty;
    private bool _alphabeticOrder = true;

    public MidiBrowserWindow()
    {
        _id = Enums.Windows.MidiBrowser.ToString();
        _active = false;
    }

    private void RenderSearchBar()
    {
        if (ImGui.BeginChild("Searchbar container", new(_io.DisplaySize.X / 1.2f, 50)))
        {
            string orderIcon = _alphabeticOrder ? FontAwesome6.ArrowDownAZ : FontAwesome6.ArrowUpAZ;
            if (ImGui.Button(orderIcon))
            {
                _alphabeticOrder = !_alphabeticOrder;
            }
            ImGui.SameLine();
            ImGui.InputTextWithHint($"Search {FontAwesome6.MagnifyingGlass}", "Search midi file...", ref _searchBuffer, 1000);
            ImGui.EndChild();
        }
    }

    private void RenderBrowser()
    {
        Drawings.RenderMatrixBackground();

        // browser theme
        ImGui.PushStyleColor(ImGuiCol.ChildBg, ThemeManager.MainBgCol * 0.8f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, ImGuiUtils.FixedSize(new Vector2(10)));

        using (AutoFont font22 = new(FontController.GetFontOfSize(22)))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 2f);
            ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 10f);
            ImGui.SetNextWindowPos(new Vector2((_io.DisplaySize.X - _io.DisplaySize.X / 1.2f) / 2, ImGuiUtils.FixedSize(new Vector2(120)).Y));
            Vector2 containerSize = _io.DisplaySize / 1.2f;
            if (ImGui.BeginChild("Midi browser container", containerSize, ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.Border))
            {
                ImGui.PopStyleVar(2);

                ImGui.Text($"{FontAwesome6.Folder} MIDI File Browser");
                ImGui.Spacing();
                RenderSearchBar();
                ImGui.Separator();

                if (ImGui.BeginChild("Midi file list", ImGui.GetContentRegionAvail()))
                {
                    if (ImGui.BeginTable("File Table", 1, ImGuiTableFlags.PadOuterX))
                    {
                        ImGui.TableSetupColumn("Name");

                        List<string> midiFiles = new();
                        foreach (var midiPath in MidiPathsManager.MidiPaths)
                        {
                            var files = Directory.GetFiles(midiPath, "*.mid");
                            midiFiles.AddRange(files);
                        }
                        var sortedFiles = SortFiles(midiFiles);
                        foreach (var file in sortedFiles)
                        {
                            if (!Path.GetFileName(file).ToLower().Contains(_searchBuffer.ToLower()) && _searchBuffer != string.Empty)
                                continue;

                            ImGui.TableNextRow();
                            ImGui.TableSetColumnIndex(0);
                            if (ImGui.Selectable(Path.GetFileName(file)))
                            {
                                MidiFileHandler.LoadMidiFile(file);
                                // we start and stop the playback so we can change the time before playing the song,
                                // else falling notes and keypresses are mismatched
                                MidiPlayer.Playback.Start();
                                MidiPlayer.Playback.Stop();
                                WindowsManager.SetWindow(Enums.Windows.ModeSelection);
                            }
                        }

                        ImGui.EndTable();
                    }
                    ImGui.EndChild();
                }
                ImGui.EndChild();
            }

            ImGui.PopStyleColor(); // child bg
            ImGui.PopStyleVar(); // window padding
        }
    }

    private List<string> SortFiles(List<string> midiFiles)
    {
        return _alphabeticOrder ? midiFiles.OrderBy(path => Path.GetFileName(path)).ToList() : midiFiles.OrderByDescending(path => Path.GetFileName(path)).ToList();
    }

    protected override void OnImGui()
    {
        using (AutoFont font16_icon16 = new(FontController.Font16_Icon16))
        {
            // back button
            ImGui.SetCursorScreenPos(ImGuiUtils.FixedSize(new Vector2(22, 50)));
            if (ImGui.Button(FontAwesome6.ArrowLeftLong, ImGuiUtils.FixedSize(new Vector2(100, 50))))
                WindowsManager.SetWindow(Enums.Windows.Home);

            // open file button
            ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0EA5E9"), ImGuiTheme.HtmlToVec4("#096E9B"), ImGuiTheme.HtmlToVec4("#0EA5E9"));
            ImGui.SetCursorScreenPos(ImGuiUtils.FixedSize(new Vector2(132f, 50)));
            if (ImGui.Button($"Open file {FontAwesome6.FileImport}", ImGuiUtils.FixedSize(new Vector2(100, 50))))
            {
                if (MidiFileHandler.OpenMidiDialog())
                {
                    /* we start and stop the playback so we can change the time before playing the song,
                      else falling notes and keypresses are mismatched */
                    MidiPlayer.Playback.Start();
                    MidiPlayer.Playback.Stop();
                    WindowsManager.SetWindow(Enums.Windows.ModeSelection);
                }
            }
            ImGuiTheme.PopButton();

            RenderBrowser();
        }
    }
}
