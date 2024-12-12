using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia;

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
        _imguiWindows.Add(homeWindow);
    }

    public void OnUpdate()
    {
        foreach (ImGuiWindow window in _imguiWindows)
        {
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
        _isRunning = false;
    }
}
