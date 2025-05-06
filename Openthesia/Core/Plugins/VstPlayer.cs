using Melanchall.DryWetMidi.Multimedia;
using NAudio.Mixer;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Openthesia.Core.SoundFonts;
using Openthesia.Enums;
using Openthesia.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Core.Plugins;

public static class VstPlayer
{
    private static MixingSampleProvider _mixingSampleProvider;

    private static WaveOutEvent _waveOut;
    public static WaveOutEvent WaveOut => _waveOut;

    private static AsioOut _asioOut;
    public static AsioOut AsioOut => _asioOut;

    public static PluginsChain? PluginsChain { get; private set; }

    public static void Initialize()
    {
        var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(CoreSettings.SampleRate, 2))
        {
            ReadFully = true
        };

        _mixingSampleProvider = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(CoreSettings.SampleRate, 2))
        {
            ReadFully = true
        };

        PluginsChain = new PluginsChain(mixer);
        _mixingSampleProvider.AddMixerInput(PluginsChain);

        if (AudioDriverManager.AudioDriverType == AudioDriverTypes.WaveOut)
        {
            _asioOut?.Stop();
            _asioOut?.Dispose();

            _waveOut = new WaveOutEvent();
            _waveOut.DesiredLatency = CoreSettings.WaveOutLatency;
            _waveOut.Init(_mixingSampleProvider);
            _waveOut.Play();
        }
        else if (AudioDriverManager.AudioDriverType == AudioDriverTypes.ASIO)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();

            _asioOut = new AsioOut(AudioDriverManager.SelectedAsioDriverName);
            _asioOut.Init(_mixingSampleProvider);
            _asioOut.Play();
        }
    }

    public static void ChangeLatency(int newLatency)
    {
        bool isRunning = _waveOut.PlaybackState == PlaybackState.Playing || _waveOut.PlaybackState == PlaybackState.Paused;
        if (isRunning)
        {
            _waveOut.Stop();
        }

        _waveOut.DesiredLatency = newLatency;
        _waveOut.Init(_mixingSampleProvider);
        _waveOut.Play();
    }
}
