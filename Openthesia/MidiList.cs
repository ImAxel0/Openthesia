using IconFonts;
using ImGuiNET;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Numerics;

namespace Openthesia;

public class MidiList
{
    private static string _searchBuffer = string.Empty;

    public static void Render()
    {
        ImGui.BeginChild("Midi list", ImGui.GetContentRegionAvail(), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollWithMouse );

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGui.SetCursorScreenPos(new(22, 50));
        if (ImGui.Button(FontAwesome6.ArrowLeftLong, new(100, 50)))
        {
            Router.SetRoute(Router.Routes.Home);
        }
        ImGui.PopFont();

        ImGui.PushFont(FontController.Font16_Icon16);
        ImGuiTheme.PushButton(ImGuiTheme.HtmlToVec4("#0EA5E9"), ImGuiTheme.HtmlToVec4("#096E9B"), ImGuiTheme.HtmlToVec4("#0EA5E9"));
        ImGui.SetCursorScreenPos(new(132, 50));
        if (ImGui.Button($"Open file {FontAwesome6.FileImport}", new(100, 50)))
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
        ImGuiTheme.Style.WindowPadding = new(10);
        ImGui.PushFont(FontController.GetFontOfSize(22));

        ImGui.SetNextWindowPos(new((ImGui.GetIO().DisplaySize.X - ImGui.GetIO().DisplaySize.X / 1.2f) / 2, 120));
        ImGui.BeginChild("Midis container", new(ImGui.GetIO().DisplaySize.X / 1.2f, ImGui.GetIO().DisplaySize.Y / 1.2f), 
            ImGuiChildFlags.AlwaysUseWindowPadding | ImGuiChildFlags.Border);

        ImGui.BeginChild("Searchbar", new(ImGui.GetIO().DisplaySize.X / 1.2f, 50));
        ImGui.InputTextWithHint("Search", "Search midi file...", ref _searchBuffer, 1000);
        ImGui.EndChild();

        ImGui.BeginChild("Midi file list", ImGui.GetContentRegionAvail());
        var width = ImGui.GetContentRegionAvail().X;

        foreach (var midiPath in Settings.MidiPaths)
        {
            int index = 0;
            var files = Directory.GetFiles(midiPath);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".mid")
                {
                    if (!Path.GetFileName(file).ToLower().Contains(_searchBuffer.ToLower()) && _searchBuffer != string.Empty)
                        continue;

                    ImGui.Columns(2, file, true);
                    ImGui.SetColumnWidth(0, width - 150);

                    if (ImGui.Selectable(Path.GetFileName(file)))
                    {
                        MidiFileHandler.LoadMidiFile(file);
                        // we start and stop the playback so we can change the time before playing the song,
                        // else falling notes and keypresses are mismatched
                        MidiPlayer.Playback.Start();
                        MidiPlayer.Playback.Stop();
                        Router.SetRoute(Router.Routes.MidiFileView);
                    }

                    ImGui.NextColumn();
                    ImGui.Text(FontAwesome6.Star);
                    ImGui.Columns(1);
                    ImGui.Dummy(new(5));
                    /*
                    if (ImGui.Button($"View and listen ##{index}"))
                    {
                        MidiFileHandler.LoadMidiFile(file);
                        // we start and stop the playback so we can change the time before playing the song,
                        // else falling notes and keypresses are mismatched
                        MidiPlayer.Playback.Start();
                        MidiPlayer.Playback.Stop();
                        ScreenCanvas.SetLearningMode(false);
                        Router.SetRoute(Router.Routes.MidiPlayback);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Play along ##{index}"))
                    {
                        MidiFileHandler.LoadMidiFile(file);
                        // we start and stop the playback so we can change the time before playing the song,
                        // else falling notes and keypresses are mismatched
                        MidiPlayer.Playback.Start();
                        MidiPlayer.Playback.Stop();
                        MidiPlayer.Playback.Speed = 1;
                        ScreenCanvas.SetFallSpeed(ScreenCanvas.FallSpeeds.Default);
                        MidiPlayer.Playback.OutputDevice = null; // mute the device
                        ScreenCanvas.SetLearningMode(true);
                        Router.SetRoute(Router.Routes.MidiPlayback);
                    }
                    */
                }
                index++;
            }
        }
        /*
        foreach (var midiPath in Settings.MidiPaths)
        {
            ImGui.Dummy(new(10));

            ImGui.TextDisabled(midiPath);

            ImGui.BeginTable("Midi files", 2, ImGuiTableFlags.PadOuterX | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInner);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Length", ImGuiTableColumnFlags.WidthFixed, 150);
            //ImGui.TableSetupColumn("Stars", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            int index = 0;
            var files = Directory.GetFiles(midiPath);
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".mid")
                {
                    if (!Path.GetFileName(file).ToLower().Contains(_searchBuffer.ToLower()) && _searchBuffer != string.Empty)
                        continue;

                    ImGui.Text(Path.GetFileName(file));
                    //ImGui.SameLine();
                    if (ImGui.Button($"View and listen ##{index}"))
                    {
                        MidiFileHandler.LoadMidiFile(file);
                        // we start and stop the playback so we can change the time before playing the song,
                        // else falling notes and keypresses are mismatched
                        MidiPlayer.Playback.Start();
                        MidiPlayer.Playback.Stop();
                        ScreenCanvas.SetLearningMode(false);
                        Router.SetRoute(Router.Routes.MidiPlayback);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button($"Play along ##{index}"))
                    {
                        MidiFileHandler.LoadMidiFile(file);
                        // we start and stop the playback so we can change the time before playing the song,
                        // else falling notes and keypresses are mismatched
                        MidiPlayer.Playback.Start();
                        MidiPlayer.Playback.Stop();
                        MidiPlayer.Playback.Speed = 1;
                        ScreenCanvas.SetFallSpeed(ScreenCanvas.FallSpeeds.Default);
                        MidiPlayer.Playback.OutputDevice = null; // mute the device
                        ScreenCanvas.SetLearningMode(true);
                        Router.SetRoute(Router.Routes.MidiPlayback);
                    }
                    ImGui.TableNextColumn();
                    try
                    {
                        ImGui.Text($"{MidiFile.Read(file).GetDuration<MetricTimeSpan>().TotalSeconds:0} sec");
                    }
                    catch (Exception ex)
                    {
                        ImGui.Text("Unknown");
                    }
                    //ImGui.TableNextColumn();
                    //ImGui.Text(FontAwesome6.Star);
                    ImGui.TableNextColumn();
                }
                index++;
            }
            ImGui.EndTable();
        }
        */
        ImGui.PopFont();

        ImGui.EndChild();
        ImGui.EndChild();
        ImGuiTheme.Style.Colors[(int)ImGuiCol.ChildBg] = Vector4.Zero;
        ImGuiTheme.Style.WindowPadding = new(0);
        ImGui.EndChild();
    }
}
