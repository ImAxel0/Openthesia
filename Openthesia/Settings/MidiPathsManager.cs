using Syroot.Windows.IO;

namespace Openthesia.Settings;

public static class MidiPathsManager
{
    public static List<string> MidiPaths { get; private set; } = new()
    {
        KnownFolders.Documents.Path,
        KnownFolders.Downloads.Path,
        KnownFolders.Music.Path,
    };

    public static void SetMidiPaths(List<string> paths)
    {
        if (paths.Count == 0)
            return;

        MidiPaths = paths;
    }
}
