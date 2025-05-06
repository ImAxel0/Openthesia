using Jacobi.Vst.Core;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia.Core.Plugins;

/// <summary>
/// Handle vst midi events.
/// </summary>
public class VstMidiHandler
{
    /// <summary>
    /// The VST of this handler.
    /// </summary>
    public VstPlugin Vst { get; private set; }

    private readonly object _eventLock = new();
    private readonly List<VstEvent> _pendingEvents = new();

    public VstMidiHandler(VstPlugin plugin)
    {
        Vst = plugin;
    }

    /// <summary>
    /// Handle incoming midi events through the VST.
    /// </summary>
    /// <param name="midiEvent">The incoming midi event.</param>
    /// <exception cref="Exception"></exception>
    public void HandleMidiEvent(MidiEvent midiEvent)
    {
        if (Vst.PluginContext == null)
        {
            throw new Exception("VST context was null.");
        }

        byte[] midiData = null;

        switch (midiEvent.EventType)
        {
            // Note Events
            case MidiEventType.NoteOn:
                var noteOn = (NoteOnEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0x90 | (noteOn.Channel)),  // Note On
            (byte)noteOn.NoteNumber,
            (byte)noteOn.Velocity
        };
                break;

            case MidiEventType.NoteOff:
                var noteOff = (NoteOffEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0x80 | (noteOff.Channel)),  // Note Off
            (byte)noteOff.NoteNumber,
            (byte)noteOff.Velocity
        };
                break;

            // Control Changes
            case MidiEventType.ControlChange:
                var cc = (ControlChangeEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0xB0 | (cc.Channel)),  // Control Change
            (byte)cc.ControlNumber,
            (byte)cc.ControlValue
        };

                // Special handling for All Notes Off (CC 123)
                if (cc.ControlNumber == 123)
                {
                    SendAllNotesOff(cc.Channel);
                    return;
                }
                break;

            // Pitch Bend
            case MidiEventType.PitchBend:
                var pitchBend = (PitchBendEvent)midiEvent;
                var value = pitchBend.PitchValue;
                midiData = new byte[] {
            (byte)(0xE0 | (pitchBend.Channel)),  // Pitch Bend
            (byte)(value & 0x7F),  // LSB
            (byte)((value >> 7) & 0x7F)  // MSB
        };
                break;

            // Aftertouch (Channel Pressure)
            case MidiEventType.ChannelAftertouch:
                var aftertouch = (ChannelAftertouchEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0xD0 | (aftertouch.Channel)),  // Channel Pressure
            (byte)aftertouch.AftertouchValue,
            0x00  // Unused
        };
                break;

            // Polyphonic Aftertouch
            case MidiEventType.NoteAftertouch:
                var polyAftertouch = (NoteAftertouchEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0xA0 | (polyAftertouch.Channel)),  // Poly Aftertouch
            (byte)polyAftertouch.NoteNumber,
            (byte)polyAftertouch.AftertouchValue
        };
                break;

            // Program Change
            case MidiEventType.ProgramChange:
                var programChange = (ProgramChangeEvent)midiEvent;
                midiData = new byte[] {
            (byte)(0xC0 | (programChange.Channel)),  // Program Change
            (byte)programChange.ProgramNumber,
            0x00  // Unused
        };
                break;

            // System Exclusive (Basic handling)
            case MidiEventType.NormalSysEx:
                var sysex = (NormalSysExEvent)midiEvent;
                // VST.NET requires VstSysExEvent for sysex               
                var sysexEvent = new VstMidiSysExEvent(
                    deltaFrames: 0,
                    sysexData: sysex.Data);
                Vst.PluginContext.PluginCommandStub.Commands.ProcessEvents(new VstEvent[] { sysexEvent });
                return;

            // Timing Events (Ignored by most plugins)
            case MidiEventType.TimingClock:
            case MidiEventType.Start:
            case MidiEventType.Continue:
            case MidiEventType.Stop:
                // These are system real-time messages
                midiData = new byte[] { (byte)midiEvent.EventType };
                break;

            // Active Sensing and System Reset
            case MidiEventType.ActiveSensing:
            case MidiEventType.Reset:
                // Single-byte system messages
                midiData = new byte[] { (byte)midiEvent.EventType };
                break;

            default:
                return; // Skip unsupported events
        }

        if (midiData != null)
        {
            var vstEvent = new VstMidiEvent(
                deltaFrames: 0,
                noteLength: 0,
                noteOffset: 0,
                midiData: midiData,
                detune: 0,
                noteOffVelocity: 0);

            QueueMidiEvent(vstEvent);
        }
    }

    private void QueueMidiEvent(VstMidiEvent midiEvent)
    {
        lock (_eventLock)
        {
            _pendingEvents.Add(midiEvent);
        }
    }

    public void ProcessPendingEvents()
    {
        VstEvent[] eventsToProcess;

        lock (_eventLock)
        {
            if (_pendingEvents.Count == 0) return;

            eventsToProcess = _pendingEvents.ToArray();
            _pendingEvents.Clear();
        }

        Vst.PluginContext.PluginCommandStub.Commands.ProcessEvents(eventsToProcess);
    }

    public void SendAllNotesOff(int channel)
    {
        // Send Note Off for all 128 notes
        for (int note = 0; note < 128; note++)
        {
            var noteOff = new byte[] {
            (byte)(0x80 | (channel)),
            (byte)note,
            0x00  // Velocity 0
        };

            var vstEvent = new VstMidiEvent(
                deltaFrames: 0,
                noteLength: 0,
                noteOffset: 0,
                midiData: noteOff,
                detune: 0,
                noteOffVelocity: 0);

            QueueMidiEvent(vstEvent);
        }

        // Also send standard All Notes Off CC
        var allOff = new byte[] {
        (byte)(0xB0 | (channel)),
        0x7B,  // CC 123
        0x00   // Value 0
    };

        var ccEvent = new VstMidiEvent(
            deltaFrames: 0,
            noteLength: 0,
            noteOffset: 0,
            midiData: allOff,
            detune: 0,
            noteOffVelocity: 0);

        QueueMidiEvent(ccEvent);
    }
}
