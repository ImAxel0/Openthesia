using IconFonts;
using ImGuiNET;
using System.Numerics;

namespace Openthesia;

public class MidiList
{
    private static string _searchBuffer = string.Empty;
    private static bool _alphabeticOrder = true;

    public static void Render()
    {
        ImGui.BeginChild("Midi list", ImGui.GetContentRegionAvail(), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollWithMouse );

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new Vector2(22, 50) * FontController.DSF);
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, new Vector2(100, 50) * FontController.DSF))
        {
            Router.SetRoute(Router.Routes.Home);
        }
        ImGui.PopFont();

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0EA5E9"), ImGuiTheme.HtmlToVec4("#096E9B"), ImGuiTheme.HtmlToVec4("#0EA5E9"));
        ImGui.SetCursorScreenPos(new Vector2(132f, 50) * FontController.DSF);
        if (ImGui.Button($"Open file {FontAwesome6.FileImport}", new Vector2(100, 50) * FontController.DSF))
        {
            if (MidiFileHandler.OpenMidiDialog())
            {
                // we start and stop the playback so we can change the time before playing the song,
                // else falling notes and keypresses are mismatched
                MidiPlayer.Playback.Start();
                MidiPlayer.Playback.Stop();
                Router.SetRoute(Router.Routes.MidiFileView);
            }
        }
        ImGuiTheme.PopButton();
        ImGui.PopFont();

        ImGuiTheme.Style.Colors[(int)ImGuiCol.ChildBg] = Settings.MainBg * 0.8f;// ImGuiTheme.HtmlToVec4("#1F2329");
        ImGuiTheme.Style.ChildRounding = 5;
        ImGuiTheme.Style.WindowPadding = new(10 * FontController.DSF);
        ImGui.PushFont(FontController.GetFontOfSize(22));

        ImGui.SetNextWindowPos(new Vector2((ImGui.GetIO().DisplaySize.X - ImGui.GetIO().DisplaySize.X / 1.2f) / 2, 120 * FontController.DSF));
        ImGui.BeginChild("Midis container", new Vector2(ImGui.GetIO().DisplaySize.X / 1.2f, ImGui.GetIO().DisplaySize.Y / 1.2f), 
            ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.Border);
        ImGui.Text($"{FontAwesome6.Folder} MIDI File Browser");
        ImGui.Spacing();

        ImGui.BeginChild("Searchbar", new(ImGui.GetIO().DisplaySize.X / 1.2f, 50));
        string orderIcon = _alphabeticOrder ? FontAwesome6.ArrowDownAZ : FontAwesome6.ArrowUpAZ;
        if (ImGui.Button(orderIcon))
        {
            _alphabeticOrder = !_alphabeticOrder;
        }
        ImGui.SameLine();
        ImGui.InputTextWithHint($"Search {FontAwesome6.MagnifyingGlass}", "Search midi file...", ref _searchBuffer, 1000);
        ImGui.EndChild();

        ImGui.Separator();

        ImGui.BeginChild("Midi file list", ImGui.GetContentRegionAvail());
        //var width = ImGui.GetContentRegionAvail().X;

        if (ImGui.BeginTable("File Table", 1, ImGuiTableFlags.PadOuterX))
        {
            ImGui.TableSetupColumn("Name");

            foreach (var midiPath in Settings.MidiPaths)
            {
                var files = Directory.GetFiles(midiPath, "*.mid");
                foreach (var file in _alphabeticOrder ? files : files.Reverse())
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
                        Router.SetRoute(Router.Routes.MidiFileView);
                    }
                }
            }
            ImGui.EndTable();
        }

        ImGui.PopFont();

        ImGui.EndChild();
        ImGui.EndChild();
        ImGuiTheme.Style.Colors[(int)ImGuiCol.ChildBg] = Vector4.Zero;
        ImGuiTheme.Style.WindowPadding = new(0);
        ImGui.EndChild();
    }
}
