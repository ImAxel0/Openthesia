using MeltySynth;
using NAudio.Wave;
using Openthesia.Core.Midi;
using Openthesia.Enums;
using Openthesia.Settings;

namespace Openthesia.Core.SoundFonts;

public class SoundFontPlayer
{
    private Synthesizer _synthesizer;
    public Synthesizer Synthesizer => _synthesizer;

    private MidiSampleProvider _midiSampleProvider;
    public MidiSampleProvider MidiSampleProvider => _midiSampleProvider;

    private WaveOutEvent _waveOut;
    public WaveOutEvent WaveOut => _waveOut;

    private AsioOut _asioOut;
    public AsioOut AsioOut => _asioOut;

    private static string _activeSoundFont = string.Empty;
    public static string ActiveSoundFont => _activeSoundFont;

    // stores loaded soundfonts and their path
    private static Dictionary<string, SoundFont> _soundFontsPool = new();

    public SoundFontPlayer(string soundFontPath, int sampleRate = 44100)
    {
        // if not loaded in memory load and store the soundfont
        if (!_soundFontsPool.ContainsKey(soundFontPath))
        {
            var soundFont = new SoundFont(soundFontPath);
            LoadSynthesizer(soundFont, sampleRate);
            _soundFontsPool.TryAdd(soundFontPath, soundFont);
        }
        else
        {
            // load the already in memory soundfont
            if (_soundFontsPool.TryGetValue(soundFontPath, out SoundFont? soundFont))
            {
                LoadSynthesizer(soundFont, sampleRate);
            }
        }
    }

    private void LoadSynthesizer(SoundFont soundFont, int sampleRate)
    {
        var settings = new SynthesizerSettings(sampleRate);
        settings.MaximumPolyphony = 256;
        _synthesizer = new Synthesizer(soundFont, settings);

        _midiSampleProvider = new MidiSampleProvider(_synthesizer);

        if (AudioDriverManager.AudioDriverType == AudioDriverTypes.WaveOut)
        {
            _asioOut?.Stop();
            _asioOut?.Dispose();

            _waveOut = new WaveOutEvent();
            _waveOut.DesiredLatency = CoreSettings.WaveOutLatency;
            _waveOut.Init(_midiSampleProvider);
            _waveOut.Play();
        }
        else if (AudioDriverManager.AudioDriverType == AudioDriverTypes.ASIO)
        {
            _waveOut?.Stop();
            _waveOut?.Dispose();

            _asioOut = new AsioOut(AudioDriverManager.SelectedAsioDriverName);
            _asioOut.Init(_midiSampleProvider);
            _asioOut.Play();
        }
    }

    public static void Initialize()
    {
        string defaultSoundFontPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "SoundFonts\\SalamanderGrandPiano.sf2");
        if (File.Exists(defaultSoundFontPath))
        {
            // load default sound font
            LoadSoundFont(defaultSoundFontPath);
        }
        else
        {
            // load first available if default is missing or nothing
            var soundFonts = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "SoundFonts")).Where(f => Path.GetExtension(f) == ".sf2");
            if (soundFonts.Any())
            {
                if (File.Exists(soundFonts.ElementAt(0)))
                {
                    LoadSoundFont(soundFonts.ElementAt(0));
                }
            }
        }
    }

    public void ChangeLatency(int newLatency)
    {
        bool isRunning = _waveOut.PlaybackState == PlaybackState.Playing || _waveOut.PlaybackState == PlaybackState.Paused;
        if (isRunning)
        {
            _waveOut.Stop();
        }

        _waveOut.DesiredLatency = newLatency;
        _waveOut.Init(_midiSampleProvider);
        _waveOut.Play();
    }

    public static void LoadSoundFont(string soundFontPath, int sampleRate = 44100)
    {
        MidiPlayer.SoundFontEngine = new SoundFontPlayer(soundFontPath, sampleRate);
        _activeSoundFont = Path.GetFileNameWithoutExtension(soundFontPath);
    }

    public void PlayNote(int channel, int noteNumber, int velocity)
    {
        if (CoreSettings.SoundEngine != SoundEngine.SoundFonts)
            return;

        _synthesizer.NoteOn(channel, noteNumber, velocity);
    }

    public void StopNote(int channel, int noteNumber)
    {
        if (CoreSettings.SoundEngine != SoundEngine.SoundFonts)
            return;

        _synthesizer.NoteOff(channel, noteNumber);
    }

    public void StopAllNote(int channel)
    {
        if (CoreSettings.SoundEngine != SoundEngine.SoundFonts)
            return;

        _synthesizer.NoteOffAll(channel, false);
    }

    public void Dispose()
    {
        MidiPlayer.SoundFontEngine?.WaveOut?.Dispose();
        MidiPlayer.SoundFontEngine?.AsioOut?.Dispose();
    }
}
