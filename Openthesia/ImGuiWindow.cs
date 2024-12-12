using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia;

public abstract class ImGuiWindow
{
    protected ImGuiIOPtr _io = ImGui.GetIO();

    /// <summary>
    /// ImGui window id
    /// </summary>
    protected string _id = string.Empty;

    /// <summary>
    /// ImGui window state
    /// </summary>
    protected bool _active;

    /// <summary>
    /// ImGui window flags
    /// </summary>
    protected ImGuiWindowFlags _windowFlags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar 
        | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize;

    /// <summary>
    /// True if window fills screen content
    /// </summary>
    protected bool _isMainWindow = true;

    /// <summary>
    /// Timer utility
    /// </summary>
    protected float _timer = 0f;

    public string GetId()
    {
        return _id;
    }

    public ref bool Active()
    {
        return ref _active;
    }

    /// <summary>
    /// Window rendering
    /// </summary>
    public void RenderWindow()
    {
        if (ImGui.Begin(_id, ref _active, _windowFlags))
        {
            if (_isMainWindow)
            {
                ImGui.SetWindowPos(Vector2.Zero);
                ImGui.SetWindowSize(_io.DisplaySize);
            }

            _timer += _io.DeltaTime; // update window related timer
            OnImGui();
            ImGui.End();
        }
    }

    /// <summary>
    /// Window content rendering
    /// </summary>
    protected abstract void OnImGui();
}
