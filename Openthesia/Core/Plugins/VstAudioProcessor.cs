using Jacobi.Vst.Core;
using Jacobi.Vst.Host.Interop;
using Openthesia.Enums;
using Openthesia.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Core.Plugins;

public class VstAudioProcessor
{
    private VstPlugin _vstPlugin;
    public VstPlugin VstPlugin => _vstPlugin;

    private CircularBuffer inputBuffer;
    private CircularBuffer outputBuffer;
    private VstAudioBuffer[] _inputBuffers;
    private VstAudioBuffer[] _outputBuffers;

    public bool DeleteRequested { get; set; }
    private int _blockSize;
    private bool _buffersInitialized;

    public VstAudioProcessor(VstPlugin vst)
    {
        _vstPlugin = vst;
    }

    private void InitializeBuffers(int bufferSize)
    {
        inputBuffer = new CircularBuffer(bufferSize * 2);
        outputBuffer = new CircularBuffer(bufferSize * 2);
    }

    private void AddToInputBuffer(float[] data, int offset, int count)
    {
        inputBuffer.Write(data, offset, count);
    }

    private int GetProcessedAudio(float[] output, int offset, int count)
    {
        return outputBuffer.Read(output, offset, count);
    }

    private void ProcessAudio(int blockSize)
    {
        float[] blockInput = new float[blockSize * 2];
        float[] blockOutput = new float[blockSize * 2];

        int samplesRead = inputBuffer.Read(blockInput, 0, blockInput.Length);

        // Populate input buffers for VST
        if (_vstPlugin.PluginType != PluginType.Instrument)
        {
            for (int i = 0; i < samplesRead / 2; i++)
            {
                _inputBuffers[0][i] = blockInput[i * 2];       // Left channel
                _inputBuffers[1][i] = blockInput[i * 2 + 1];   // Right channel
            }
        }

        // Process with VST
        _vstPlugin.PluginContext.PluginCommandStub.Commands.ProcessReplacing(_inputBuffers, _outputBuffers);
        
        // Store processed data in output buffer
        for (int i = 0; i < samplesRead / 2; i++)
        {
            blockOutput[i * 2] = _outputBuffers[0][i];     // Left channel
            blockOutput[i * 2 + 1] = _outputBuffers[1][i]; // Right channel
        }
        
        outputBuffer.Write(blockOutput, 0, samplesRead);
    }

    private void UpdateBlockSize(int blockSize)
    {
        if (blockSize == _blockSize)
            return;

        _blockSize = blockSize;

        int inputCount = _vstPlugin.PluginContext.PluginInfo.AudioInputCount;
        int outputCount = _vstPlugin.PluginContext.PluginInfo.AudioOutputCount;

        var inputMgr = new VstAudioBufferManager(inputCount, blockSize);
        var outputMgr = new VstAudioBufferManager(outputCount, blockSize);

        _vstPlugin.PluginContext.PluginCommandStub.Commands.SetBlockSize(blockSize);
        _vstPlugin.PluginContext.PluginCommandStub.Commands.SetSampleRate(CoreSettings.SampleRate);
        _vstPlugin.PluginContext.PluginCommandStub.Commands.SetProcessPrecision(VstProcessPrecision.Process32);

        _inputBuffers = inputMgr.Buffers.ToArray();
        _outputBuffers = outputMgr.Buffers.ToArray();
    }

    public void Process(float[] input, float[] output, int samplesRead)
    {
        if (DeleteRequested) return; // Skip processing if the plugin is flagged for deletion

        VstPlugin.MidiHandler.ProcessPendingEvents();

        if (!_buffersInitialized)
        {
            InitializeBuffers(samplesRead / 2);
            _buffersInitialized = true;
        }

        UpdateBlockSize(samplesRead / 2);

        // Add input to the input buffer
        AddToInputBuffer(input, 0, samplesRead);

        // Process audio in blocks using existing circular buffer logic
        while (inputBuffer.Count >= _blockSize * 2) // Stereo block
        {
            ProcessAudio(_blockSize); // Existing block-based processing
        }

        // Retrieve processed audio from the output buffer
        GetProcessedAudio(output, 0, samplesRead);
    }
}
