namespace Openthesia.Settings;

public static class SoundFontsPathsManager
{
    public static List<string> SoundFontsPaths { get; private set; } = new()
    {
        Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "SoundFonts"),
    };

    public static void SetSoundFontsPaths(List<string> paths)
    {
        if (paths.Count == 0)
            return;

        SoundFontsPaths = paths;
    }
}
