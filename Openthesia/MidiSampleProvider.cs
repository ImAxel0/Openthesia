using MeltySynth;
using NAudio.Wave;

namespace Openthesia;

public class MidiSampleProvider : ISampleProvider
{
    private static WaveFormat format = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);
    private Synthesizer _synthesizer;

    public MidiSampleProvider(Synthesizer synthesizer)
    {
        _synthesizer = synthesizer;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        _synthesizer.RenderInterleaved(buffer.AsSpan(offset, count));
        return count;
    }
    public WaveFormat WaveFormat => format;
}
