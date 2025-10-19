namespace Openthesia.Core;
using Openthesia.Enums;

public static class WindowsManager
{
    public static Windows Window { get; private set; }

    public static void SetWindow(Windows window)
    {
        if (window != Windows.MidiPlayback && window != Windows.PlayMode)
        {
            Program._window.Title = $"Openthesia {ProgramData.ProgramVersion}";
        }

        foreach (var win in Application.AppInstance.GetWindows())
        {
            win.SetActive(win.GetId() == window.ToString());
        }

        Window = window;
    }
}
