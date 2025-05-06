using Jacobi.Vst.Core;
using Jacobi.Vst.Host.Interop;
using Melanchall.DryWetMidi.Core;
using Openthesia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Veldrid.Sdl2;

namespace Openthesia.Core.Plugins;

public class VstPlugin : IPlugin
{
    #region Interface Properties

    /// <inheritdoc/>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Name of the VST plugin. (coming from dll name)
    /// </summary>
    public string PluginName { get; set; }

    /// <inheritdoc/>
    public string PluginId { get; private set; } = Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public PluginType PluginType { get; private set; }

    /// <inheritdoc/>
    public bool IsVst { get; } = true;

    #endregion

    #region Public Properties

    /// <summary>
    /// Process audio buffer through the VST plugin.
    /// </summary>
    public VstAudioProcessor VstProcessor { get; private set; }

    /// <summary>
    /// Handle VST midi events.
    /// </summary>
    public VstMidiHandler MidiHandler { get; private set; }

    /// <summary>
    /// Plugin context.
    /// </summary>
    public VstPluginContext PluginContext { get; private set; }

    /// <summary>
    /// Plugin window.
    /// </summary>
    public Sdl2Window PluginWindow { get; private set; }

    #endregion

    public VstPlugin(string pluginPath)
    {
        PluginContext = LoadPlugin(pluginPath);
        PluginName = Path.GetFileNameWithoutExtension(pluginPath);
        PluginType = PluginContext.PluginInfo.Flags.HasFlag(VstPluginFlags.IsSynth)
            ? PluginType.Instrument : PluginType.Effect;
        VstProcessor = new VstAudioProcessor(this);
        MidiHandler = new VstMidiHandler(this);
    }

    private void HostCmdStub_PluginCalled(object sender, PluginCalledEventArgs e)
    {
        var hostCmdStub = (HostCommandStub)sender;

        // can be null when called from inside the plugin main entry point.
        if (hostCmdStub.PluginContext.PluginInfo != null)
        {
            Console.WriteLine("Plugin " + hostCmdStub.PluginContext.PluginInfo.PluginID + " called:" + e.Message);
        }
        else
        {
            Console.WriteLine("The loading Plugin called:" + e.Message);
        }
    }

    private void HostCmdStub_SizeWindow(object sender, SizeWindowEventArgs e)
    {
        PluginWindow.Width = e.Width;
        PluginWindow.Height = e.Height;
    }

    private VstPluginContext LoadPlugin(string pluginPath)
    {
        try
        {
            var hostCmdStub = new HostCommandStub();
            hostCmdStub.PluginCalled += new EventHandler<PluginCalledEventArgs>(HostCmdStub_PluginCalled);
            hostCmdStub.SizeWindow += new EventHandler<SizeWindowEventArgs>(HostCmdStub_SizeWindow);
            var ctx = VstPluginContext.Create(pluginPath, hostCmdStub);

            // add custom data to the context
            ctx.Set("PluginPath", pluginPath);
            ctx.Set("HostCmdStub", hostCmdStub);

            // actually open the plugin itself
            ctx.PluginCommandStub.Commands.Open();

            // We check if plugin returns rect data; if it doesn't we try to populate it with by opening the editor with dummy handle
            var rect = ctx.PluginCommandStub.Commands.EditorGetRect(out var rectangle);
            System.Drawing.Rectangle pluginRect = new();
            bool rectWasFound = rect;
            if (!rectWasFound)
            {
                ctx.PluginCommandStub.Commands.EditorOpen(IntPtr.Zero); // Open dummy editor, may works for some plugins to populate the rectangle data
                rect = ctx.PluginCommandStub.Commands.EditorGetRect(out var dummyRect);
                ctx.PluginCommandStub.Commands.EditorClose(); // Destroy the dummy editor
                pluginRect = dummyRect;
            }
            else
                pluginRect = rectangle;

            // Check if the plugin has an editor
            if (rect)
            {
                // Create a host window for the editor
                string windowTitle = Path.GetFileNameWithoutExtension(pluginPath);
                IntPtr hwnd = CreateWindow(windowTitle, pluginRect.Width, pluginRect.Height);

                // Attach the editor to the window
                ctx.PluginCommandStub.Commands.EditorOpen(hwnd);

                StartEditorIdle();
                Console.WriteLine("Plugin editor opened successfully.");
            }
            else
            {
                Console.WriteLine("The plugin does not have an editor.");
            }

            return ctx;
        }
        catch (Exception e)
        {
            User32.MessageBox(IntPtr.Zero, e.ToString(), e.Message);
        }

        return null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const int GWL_STYLE = -16;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_MAXIMIZEBOX = 0x00010000;
    private const int WS_THICKFRAME = 0x00040000;
    private void RemoveMinimizeAndMaximizeButtons(IntPtr windowHandle)
    {
        int style = GetWindowLong(windowHandle, GWL_STYLE);
        //style &= ~WS_MINIMIZEBOX; // Remove minimize button
        style &= ~WS_MAXIMIZEBOX; // Remove maximize button
        style &= ~WS_THICKFRAME; // Make the window non-resizable (except from plugin controls)
        SetWindowLong(windowHandle, GWL_STYLE, style);
    }

    private IntPtr CreateWindow(string title, int width, int height)
    {
        PluginWindow = new Sdl2Window(title, 400, 400, width, height, SDL_WindowFlags.AlwaysOnTop | SDL_WindowFlags.Resizable, false);
        PluginWindow.Closing += () =>
        {
            if (PluginWindow.Exists)
            {
                PluginContext.PluginCommandStub?.Commands.EditorClose();
            }
        };

        // Make window always stay on top
        User32.SetWindowPos(PluginWindow.Handle, HWND.HWND_TOPMOST, 0, 0, 0, 0, User32.SetWindowPosFlags.SWP_NOSIZE | User32.SetWindowPosFlags.SWP_NOMOVE
            | User32.SetWindowPosFlags.SWP_NOACTIVATE | User32.SetWindowPosFlags.SWP_SHOWWINDOW);

        RemoveMinimizeAndMaximizeButtons(PluginWindow.Handle);

        return PluginWindow.Handle;
    }

    /// <summary>
    /// Makes the plugin ui updated accordingly to control changes
    /// </summary>
    private void StartEditorIdle()
    {
        Task.Run(async () =>
        {
            while (PluginWindow.Exists)
            {
                PluginContext?.PluginCommandStub.Commands.EditorIdle();
                await Task.Delay(16);
            }
        });
    }

    private void RecreateWindow()
    {
        // Check if the plugin has an editor
        var rect = PluginContext.PluginCommandStub.Commands.EditorGetRect(out var rectange);
        if (rect)
        {
            // Create a host window for the editor
            string windowTitle = Path.GetFileNameWithoutExtension(PluginContext.Find<string>("PluginPath"));
            IntPtr hwnd = CreateWindow(windowTitle, rectange.Width, rectange.Height);

            // Attach the editor to the window
            PluginContext.PluginCommandStub.Commands.EditorOpen(hwnd);

            StartEditorIdle();
            Console.WriteLine("Plugin editor opened successfully.");
        }
        else
        {
            Console.WriteLine("The plugin does not have an editor.");
        }
    }

    public void OpenPluginWindow()
    {
        if (!PluginWindow.Exists)
        {
            int x = PluginWindow.X;
            int y = PluginWindow.Y;
            RecreateWindow();
            PluginWindow.X = x;
            PluginWindow.Y = y;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        DisposeVST();
    }

    public void DisposeVST(bool closeWindow = true)
    {
        VstProcessor.DeleteRequested = true;
        if (closeWindow)
        {
            PluginWindow?.Close();
        }
        PluginContext?.Dispose();
    }

    /// <inheritdoc/>
    public void Process(float[] input, float[] output, int samplesRead)
    {
        VstProcessor.Process(input, output, samplesRead);
    }

    /// <inheritdoc/>
    public void ReceiveMidiEvent(MidiEvent midiEvent)
    {
        MidiHandler.HandleMidiEvent(midiEvent);
    }
}
