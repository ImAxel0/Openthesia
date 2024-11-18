using Veldrid.Sdl2;
using Veldrid;
using System.Diagnostics;
using Veldrid.StartupUtilities;
using System.Numerics;
using ImGuiNET;
using Vanara.PInvoke;
using Newtonsoft.Json;

namespace Openthesia;

class Program
{
    public static bool IsRunning = true;
    public static Sdl2Window _window;
    private static GraphicsDevice _gd;
    private static CommandList _cl;
    private static ImGuiController _controller;
    private static Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    static void Main(string[] args)
    {
        User32.SetProcessDPIAware();

        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(50, 50, 1280, 720, WindowState.Maximized, $"Openthesia {ProgramData.ProgramVersion}"),
            new GraphicsDeviceOptions(false, null, true, ResourceBindingModel.Improved, true, true),
            out _window,
            out _gd);

        _cl = _gd.ResourceFactory.CreateCommandList();
        _controller = new ImGuiController(_gd, _gd.MainSwapchain.Framebuffer.OutputDescription, _window.Width, _window.Height);

        _window.Resized += () =>
        {
            int minWidth = (int)(1280 * FontController.DSF);
            int minHeigth = (int)(720 * FontController.DSF);

            if (_window.Width < minWidth)
                _window.Width = minWidth;

            if (_window.Height < minHeigth)
                _window.Height = minHeigth;

            _gd.MainSwapchain.Resize((uint)_window.Width, (uint)_window.Height);
            _controller.WindowResized(_window.Width, _window.Height);
        };

        var stopwatch = Stopwatch.StartNew();
        float deltaTime = 0f;

        ImGuiController.LoadImages(_gd, _controller);
        ProgramData.Initialize();

        while (_window.Exists)
        {
            deltaTime = stopwatch.ElapsedTicks / (float)Stopwatch.Frequency;
            stopwatch.Restart();
            InputSnapshot snapshot = _window.PumpEvents();
            if (!_window.Exists) { break; }
            _controller.Update(deltaTime, snapshot);

            if (ImGui.IsKeyPressed(ImGuiKey.F11, false))
            {
                var windowsState = _window.WindowState == WindowState.BorderlessFullScreen ? WindowState.Normal : WindowState.BorderlessFullScreen;
                _window.WindowState = windowsState;
            }
            
            RenderUI();

            ImGuiController.UpdateMouseCursor();

            if (!IsRunning)
                break;

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

        //Hidden winsow in time of finishing of work it
        _window.WindowState = WindowState.Hidden;

        //Finish work
        ProgramData.SaveSettings();
        _gd.WaitForIdle();
        _controller.Dispose();
        _cl.Dispose();
        _gd.Dispose();

        //Close colourfull win
        _window.Close();

        //TODO at this room all thread should be aborted to close black window
    }

    static void RenderUI()
    {
        ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Once);
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize);
        ImGui.Begin("Main", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar
            | ImGuiWindowFlags.NoScrollWithMouse);

        switch (Router.Route)
        {
            case Router.Routes.Home:
                Home.Render();
                break;
            case Router.Routes.MidiList:
                MidiList.Render();
                break;
            case Router.Routes.MidiFileView:
                MidiFileView.Render();
                break;
            case Router.Routes.MidiPlayback:
                ImGui.BeginChild("Screen", new(ImGui.GetContentRegionAvail().X, ImGui.GetIO().DisplaySize.Y - (ImGui.GetIO().DisplaySize.Y * 25f / 100)));
                ScreenCanvas.RenderScreen();
                ImGui.EndChild();

                ImGui.GetForegroundDrawList().AddLine(new(0, ImGui.GetCursorPos().Y), new(ImGui.GetIO().DisplaySize.X, ImGui.GetCursorPos().Y), ImGui.GetColorU32(Settings.R_HandColor), 2);

                ImGui.BeginChild("Keyboard", ImGui.GetContentRegionAvail());
                PianoRenderer.RenderKeyboard();
                ImGui.EndChild();
                break;
            case Router.Routes.PlayMode:
                ImGui.BeginChild("Screen", new(ImGui.GetContentRegionAvail().X, ImGui.GetIO().DisplaySize.Y - (ImGui.GetIO().DisplaySize.Y * 25f / 100)));
                ScreenCanvas.RenderScreen(true);
                ImGui.EndChild();

                ImGui.GetForegroundDrawList().AddLine(new(0, ImGui.GetCursorPos().Y), new(ImGui.GetIO().DisplaySize.X, ImGui.GetCursorPos().Y), ImGui.GetColorU32(Settings.R_HandColor), 2);

                ImGui.BeginChild("Keyboard", ImGui.GetContentRegionAvail());
                PianoRenderer.RenderKeyboard();
                ImGui.EndChild();
                break;
            case Router.Routes.Settings:
                Settings.Render();
                break;
        }
        ImGui.End();
    }
}