using NAudio.Wave;
using Openthesia.Enums;

namespace Openthesia.Settings;

public static class AudioDriverManager
{
    public static string SelectedAsioDriverName { get; private set; } = string.Empty;
    public static AudioDriverTypes AudioDriverType { get; private set; } = AudioDriverTypes.WaveOut;

    public static void SetAudioDriverType(AudioDriverTypes driverType)
    {
        AudioDriverType = driverType;
    }

    public static void SetAsioDriverDevice(string deviceName)
    {
        var drivers = AsioOut.GetDriverNames();
        if (drivers.Length > 0)
        {
            // on startup: if last device is still present select it
            if (drivers.Contains(deviceName))
            {
                SelectedAsioDriverName = deviceName;
            }
            // else select the first available
            else
                SelectedAsioDriverName = drivers[0];
        }
    }
}
