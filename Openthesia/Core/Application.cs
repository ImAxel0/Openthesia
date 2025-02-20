using Openthesia.Core.Midi;
using Openthesia.Ui.Windows;

namespace Openthesia.Core;

public class Application
{
    public static Application AppInstance;
    protected bool _isRunning = true;
    protected List<ImGuiWindow> _imguiWindows = new();

    public Application()
    {
        AppInstance = this;
        Init();
    }

    private void Init()
    {
        CreateWindows();
    }

    private void CreateWindows()
    {
        HomeWindow homeWindow = new();
        MidiBrowserWindow midiBrowserWindow = new();
        ModeSelectionWindow modeSelectionWindow = new();
        MidiPlaybackWindow midiPlaybackWindow = new();
        PlayModeWindow playModeWindow = new();
        SettingsWindow settingsWindow = new();
        _imguiWindows.Add(homeWindow);
        _imguiWindows.Add(midiBrowserWindow);
        _imguiWindows.Add(modeSelectionWindow);
        _imguiWindows.Add(midiPlaybackWindow);
        _imguiWindows.Add(playModeWindow);
        _imguiWindows.Add(settingsWindow);
    }

    public List<ImGuiWindow> GetWindows()
    {
        return _imguiWindows;
    }

    public void OnUpdate()
    {
        foreach (ImGuiWindow window in GetWindows())
        {
            if (window.Active())
                window.RenderWindow();
        }
        ImGuiController.UpdateMouseCursor();
    }

    public bool IsRunning()
    {
        return _isRunning;
    }

    public void Quit()
    {
        MidiPlayer.SoundFontEngine?.Dispose();
        _isRunning = false;
    }
}
