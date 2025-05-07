namespace Openthesia.Settings;

public static class SoundFontsPathsManager
{
    public static List<string> SoundFontsPaths { get; private set; } = new()
    {
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "SoundFonts"),
    };

    public static void LoadValidPaths(List<string> paths)
    {
        foreach (var folderPath in paths)
        {
            if (Directory.Exists(folderPath) && !SoundFontsPaths.Contains(folderPath))
                SoundFontsPaths.Add(folderPath);
        }
    }
}
