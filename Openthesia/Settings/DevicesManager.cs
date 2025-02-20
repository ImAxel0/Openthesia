using Melanchall.DryWetMidi.Multimedia;
using Openthesia.Core;

namespace Openthesia.Settings;

public static class DevicesManager
{
    public static InputDevice IDevice { get; private set; }
    public static OutputDevice ODevice { get; private set; }

    public static void SetInputDevice(int deviceIndex)
    {
        if (IDevice != null)
        {
            ReleaseInputDevice();
        }

        IDevice = InputDevice.GetByIndex(deviceIndex);
        IDevice.EventReceived += IOHandle.OnEventReceived;
        IDevice.StartEventsListening();
    }

    public static void SetInputDevice(string deviceName)
    {
        if (IDevice != null)
        {
            ReleaseInputDevice();
        }

        List<string> deviceNames = new();
        foreach (var iDevice in InputDevice.GetAll())
        {
            deviceNames.Add(iDevice.Name);
        }

        if (!deviceNames.Contains(deviceName))
            return;

        IDevice = InputDevice.GetByName(deviceName);
        if (IDevice != null)
        {
            IDevice.EventReceived += IOHandle.OnEventReceived;
            IDevice.StartEventsListening();
        }
    }

    public static void ReleaseInputDevice()
    {
        IDevice?.Dispose();
    }

    public static void SetOutputDevice(int deviceIndex)
    {
        if (ODevice != null)
        {
            ReleaseOutputDevice();
        }

        ODevice = OutputDevice.GetByIndex(deviceIndex);
        ODevice.EventSent += IOHandle.OnEventSent;
        ODevice.PrepareForEventsSending();
    }

    public static void SetOutputDevice(string deviceName)
    {
        if (ODevice != null)
        {
            ReleaseOutputDevice();
        }

        List<string> deviceNames = new();
        foreach (var oDevice in OutputDevice.GetAll())
        {
            deviceNames.Add(oDevice.Name);
        }

        if (!deviceNames.Contains(deviceName))
            return;

        ODevice = OutputDevice.GetByName(deviceName);
        if (ODevice != null)
        {
            ODevice.EventSent += IOHandle.OnEventSent;
            ODevice.PrepareForEventsSending();
        }
    }

    public static void ReleaseOutputDevice()
    {
        ODevice?.Dispose();
    }
}
