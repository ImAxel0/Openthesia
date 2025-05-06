using Melanchall.DryWetMidi.Core;
using Openthesia.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Core.Plugins;

public interface IPlugin
{
    /// <summary>
    /// The plugin state.
    /// <para><see langword="True"/> if the plugin is on.</para>
    /// <para><see langword="False"/> false if plugin is off.</para>
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Get or set the plugin name. (If it is a VST plugin the name is the dll name)
    /// </summary>
    string PluginName { get; set; }

    /// <summary>
    /// Unique ID of the plugin instance.
    /// </summary>
    string PluginId { get; }

    /// <summary>
    /// Type of the plugin. (Effect or Instrument)
    /// </summary>
    PluginType PluginType { get; }

    /// <summary>
    /// True if it's a VST plugin.
    /// </summary>
    bool IsVst { get; }

    /// <summary>
    /// Processes audio as defined in its implementation.
    /// </summary>
    /// <param name="input">The incoming unprocessed audio buffer</param>
    /// <param name="output">The processed audio buffer</param>
    /// <param name="samplesRead"></param>
    void Process(float[] input, float[] output, int samplesRead);

    /// <summary>
    /// Handles the incoming midi event as defined in its implementation.
    /// </summary>
    /// <param name="midiEvent"></param>
    void ReceiveMidiEvent(MidiEvent midiEvent);

    /// <summary>
    /// Toggles the plugin state.
    /// </summary>
    public void Toggle()
    {
        Enabled = !Enabled;
    }

    /// <summary>
    /// Release the resources of the plugin instance.
    /// </summary>
    void Dispose();
}
