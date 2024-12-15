using ImGuiNET;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Openthesia.Settings;

namespace Openthesia.Core;

public static class VirtualKeyboard
{
    private static int _octaveShift = 0;
    private static int _velocity = 127;
    private static bool _isKeyDown;
    private static readonly Dictionary<ImGuiKey, int> _keyNoteMap = new()
    {
        { ImGuiKey.A, 60 }, // C4
        { ImGuiKey.W, 61 }, // C#4
        { ImGuiKey.S, 62 }, // D4
        { ImGuiKey.E, 63 }, // D#4
        { ImGuiKey.D, 64 }, // E4
        { ImGuiKey.F, 65 }, // F4
        { ImGuiKey.T, 66 }, // F#4
        { ImGuiKey.G, 67 }, // G4
        { ImGuiKey.Y, 68 }, // G#4
        { ImGuiKey.H, 69 }, // A4
        { ImGuiKey.U, 70 }, // A#4
        { ImGuiKey.J, 71 }, // B4
        { ImGuiKey.K, 72 }, // C5
    };

    public static void ListenForKeyPresses()
    {
        foreach (var key in _keyNoteMap.Keys)
        {
            if (ImGui.IsKeyPressed(key, false))
            {
                IOHandle.OnEventReceived(null,
                    new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOnEvent(new SevenBitNumber((byte)(_keyNoteMap[key] + _octaveShift)),
                    new SevenBitNumber((byte)_velocity))));
                DevicesManager.ODevice.SendEvent(new NoteOnEvent(new SevenBitNumber((byte)(_keyNoteMap[key] + _octaveShift)), new SevenBitNumber((byte)_velocity)));
                _isKeyDown = true;
            }

            if (ImGui.IsKeyReleased(key))
            {
                IOHandle.OnEventReceived(null,
                    new Melanchall.DryWetMidi.Multimedia.MidiEventReceivedEventArgs(new NoteOffEvent(new SevenBitNumber((byte)(_keyNoteMap[key] + _octaveShift)),
                    new SevenBitNumber(0))));
                DevicesManager.ODevice.SendEvent(new NoteOffEvent(new SevenBitNumber((byte)(_keyNoteMap[key] + _octaveShift)), new SevenBitNumber(0)));
                _isKeyDown = false;
            }
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Z, false) && !_isKeyDown)
        {
            _octaveShift -= 12;
            _octaveShift = Math.Clamp(_octaveShift, -36, 36);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.X, false) && !_isKeyDown)
        {
            _octaveShift += 12;
            _octaveShift = Math.Clamp(_octaveShift, -36, 36);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.C, false))
        {
            _velocity -= 10;
            _velocity = Math.Clamp(_velocity, 7, 127);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.V, false))
        {
            _velocity += 10;
            _velocity = Math.Clamp(_velocity, 7, 127);
        }
    }
}
