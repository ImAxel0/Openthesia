using Veldrid.Sdl2;
using Veldrid;
using System.Diagnostics;
using Veldrid.StartupUtilities;
using System.Numerics;
using ImGuiNET;
using Vanara.PInvoke;
using Openthesia.Core;

namespace Openthesia;

class Program
{
    public static bool IsRunning = true;
    public static Sdl2Window _window;
    private static GraphicsDevice _gd;
    private static CommandList _cl;
    private static ImGuiController _controller;
    private static Vector3 _clearColor = new(0.45f, 0.55f, 0.6f);

    [STAThread]
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

        Application app = new();

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

            app.OnUpdate();
            if (!app.IsRunning())
            {
                break;
            }

            _cl.Begin();
            _cl.SetFramebuffer(_gd.MainSwapchain.Framebuffer);
            _cl.ClearColorTarget(0, new RgbaFloat(_clearColor.X, _clearColor.Y, _clearColor.Z, 1f));
            _controller.Render(_gd, _cl);
            _cl.End();
            _gd.SubmitCommands(_cl);
            _gd.SwapBuffers(_gd.MainSwapchain);
        }

        ProgramData.SaveSettings();

        _gd.WaitForIdle();
        _controller.Dispose();
        _cl.Dispose();
        _gd.Dispose();
        Process.GetCurrentProcess().Kill(); // temporary solution since process doesn't close when using ASIO4ALL
    }
}