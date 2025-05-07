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

    public static void LoadValidPaths(List<string> paths)
    {
        foreach (var folderPath in paths)
        {
            if (Directory.Exists(folderPath) && !MidiPaths.Contains(folderPath))
                MidiPaths.Add(folderPath);
        }
    }
}
