using Openthesia.Core.Midi;
using Openthesia.Enums;

namespace Openthesia.Core;

public static class ScreenCanvasControls
{
    private static bool _lockTopBar = true;
    private static bool _upDirection;
    private static bool _showTextNotes;
    private static bool _isLearningMode;
    private static bool _isEditMode;

    public static bool LockTopBar => _lockTopBar;
    public static bool UpDirection => _upDirection;
    public static bool ShowTextNotes => _showTextNotes;
    public static bool IsLearningMode => _isLearningMode;
    public static bool IsEditMode => _isEditMode;
    public static bool LeftHandActive { get; set; } = true;

    public static bool RightHandActive { get; set; } = true;


    private static float _fallSpeedVal = 2f;
    public static float FallSpeedVal => _fallSpeedVal;
    public static FallSpeeds FallSpeed { get; private set; } = FallSpeeds.Default;
    public static TextTypes TextType { get; private set; } = TextTypes.Velocity;

    public static void SetLearningMode(bool onoff)
    {
        _isLearningMode = onoff;
    }

    public static void SetEditMode(bool onoff)
    {
        _isEditMode = onoff;
    }

    public static void SetLockTopBar(bool onoff)
    {
        _lockTopBar = onoff;
    }

    public static void SetUpDirection(bool onoff)
    {
        _upDirection = onoff;
    }

    public static void SetTextNotes(bool onoff)
    {
        _showTextNotes = onoff;
    }

    public static void SetFallSpeed(FallSpeeds speed)
    {
        switch (speed)
        {
            case FallSpeeds.Slow:
                _fallSpeedVal = 1;
                break;
            case FallSpeeds.Default:
                _fallSpeedVal = 2;
                break;
            case FallSpeeds.Fast:
                _fallSpeedVal = 3;
                break;
            case FallSpeeds.Faster:
                _fallSpeedVal = 4;
                break;
        }
        FallSpeed = speed;
        MidiPlayer.Timer = MidiPlayer.Seconds * 100 * _fallSpeedVal; // keeps in sync when changing speed
    }

    public static void SetTextType(TextTypes type)
    {
        TextType = type;
    }
}
