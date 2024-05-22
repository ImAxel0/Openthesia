namespace Openthesia;

public class Router
{
    public static Routes Route { get; private set; }

    public enum Routes
    {
        Home,
        MidiList,
        MidiFileView,
        MidiPlayback,
        PlayMode,
        Settings
    }

    public static void SetRoute(Routes route)
    {
        if (route != Routes.MidiPlayback && route != Routes.PlayMode)
        {
            Program._window.Title = $"Openthesia {ProgramData.ProgramVersion}";
        }
        Route = route;
    }
}
