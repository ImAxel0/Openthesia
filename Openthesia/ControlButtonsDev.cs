using Melanchall.DryWetMidi.Core;

namespace Openthesia
{
    internal enum ButtonFunc
    {
        NULL = -1,
        STOP = 0,
        PLAY = 1,
        RECORD = 2,
        BACKWARD = 3,
        FOREWARD = 4,
    }

    internal static class ControlButtonsDev
    {
#pragma warning disable CS8618 //Its set in LoadSettings() at ProgramData.cs
        internal static int[] ControlNumberValues;
#pragma warning restore CS8618 

        //the same order like ButtonFunc enum:                               ■ , ► , ● , ◄◄, ►►
        //Indexes:                                                           0 , 1 , 2 , 3 , 4
        internal static void ClearAll() => ControlNumberValues = new int[] { -1, -1, -1, -1, -1 };

        private static void SetControlNumberValues(int controlNumber, int value)
        {
            for(int i = 0; i != ControlNumberValues.Length; i++)
            {
                if (ControlNumberValues[i] == value)
                    ControlNumberValues[i] = -1;
            }

            ControlNumberValues[controlNumber] = value;
        }

        //Cancel at time of btn release
        private static CancellationTokenSource Scrolling = new();
        private static async void SecondTicker(ButtonFunc direction, CancellationToken StopScrolling)
        {
            //5 times for sec
            PeriodicTimer secondTimer = new(new TimeSpan(0, 0, 0, 0, 200));

            do //dont wait for 1st clock tick
            {
                if (direction == ButtonFunc.BACKWARD)
                    ScreenCanvas.MoveBackward();
                else if (direction == ButtonFunc.FOREWARD)
                    ScreenCanvas.MoveForeward();
                else
                    return;
            }
            while (await secondTimer.WaitForNextTickAsync() && !StopScrolling.IsCancellationRequested);
        }

        private const int Press = 127;
        private const int Release = 0;

        internal static void OnControlChange(ControlChangeEvent ev)
        {
            switch (Router.Route)
            {
                case Router.Routes.MidiPlayback:
                    MidiPlaybackContr(ev.ControlNumber, ev.ControlValue);
                    break;

                case Router.Routes.PlayMode:
                    PlayModeContr(ev.ControlNumber, ev.ControlValue);
                    break;

                case Router.Routes.Settings:
                    //React only for press(, not for release)
                    if (ev.ControlValue == Press)
                        return;
                    ChangeSetti(ev.ControlNumber);
                    break;

                default: return;
            }
        }

        private static bool JustPause = false;

        private static void MidiPlaybackContr(int ControlNumber, int ControlValue)
        {
            List<int> ListOfInd = ControlNumberValues.Select((v, i) => new { i, v }).Where(t => t.v == ControlNumber).Select(t => t.i).ToList();

            //return if undef control number
            if (ListOfInd.Count == 0)
                return;

            switch ((ButtonFunc)ListOfInd[0])
            {
                case ButtonFunc.STOP:
                    //only on btn press
                    if (ControlValue == Press)
                        ScreenCanvas.Stop();
                    break;

                case ButtonFunc.PLAY:
                    //Stop plaing on button press
                    if (MidiPlayer.IsTimerRunning && ControlValue == Press)
                    {
                        ScreenCanvas.Pause();
                        JustPause = true;
                    }
                    //Play on button release if it didnt stop on this press
                    else if (!JustPause && ControlValue == Release)
                        ScreenCanvas.Play();
                    //Forget it was pressed
                    else if (ControlValue == Release)
                        JustPause = false;
                    break;

                case ButtonFunc.BACKWARD:
                    //Button press
                    if (ControlValue == Press)
                    {
                        Scrolling = new CancellationTokenSource();
                        Task.Run(() => SecondTicker(ButtonFunc.BACKWARD, Scrolling.Token));
                    }
                    //button release
                    else
                    {
                        Scrolling.Cancel();
                    }
                    break;

                case ButtonFunc.FOREWARD:
                    //Button press
                    if (ControlValue == Press)
                    {
                        Scrolling = new CancellationTokenSource();
                        Task.Run(() => SecondTicker(ButtonFunc.FOREWARD, Scrolling.Token));
                    }
                    //button release
                    else
                    {
                        Scrolling.Cancel();
                    }
                    break;

                default: return;
            }
        }

        private static void PlayModeContr(int ControlNumber, int ControlValue)
        {
            List<int> ListOfInd = ControlNumberValues.Select((v, i) => new { i, v }).Where(t => t.v == ControlNumber).Select(t => t.i).ToList();

            if (ListOfInd.Count == 0)
                return;

            switch ((ButtonFunc)ListOfInd[0])
            {
                case ButtonFunc.STOP:
                    if (MidiRecording.IsRecording() && ControlValue == Press)
                        MidiRecording.StopRecording();
                    break;

                case ButtonFunc.RECORD:
                    if (!MidiRecording.IsRecording() && ControlValue == Release)
                        MidiRecording.StartRecording();
                    break;

                default: return;
            }

            return;
        }

        private static void ChangeSetti(int ControlNumber)
        {
            int ButtonFuncInt;
            switch (Settings.PressedButt)
            {
                case ButtonFunc.NULL:
                    return;

                case ButtonFunc.STOP:
                    ButtonFuncInt = (int)(ButtonFunc.STOP);
                    break;

                case ButtonFunc.PLAY:
                    ButtonFuncInt = (int)(ButtonFunc.PLAY);
                    break;

                case ButtonFunc.RECORD:
                    ButtonFuncInt = (int)(ButtonFunc.RECORD);
                    break;

                case ButtonFunc.BACKWARD:
                    ButtonFuncInt = (int)(ButtonFunc.BACKWARD);
                    break;

                case ButtonFunc.FOREWARD:
                    ButtonFuncInt = (int)(ButtonFunc.FOREWARD);
                    break;

                default: return;
            }

            Settings.PressedButt = ButtonFunc.NULL;
            SetControlNumberValues(ButtonFuncInt, ControlNumber);
        }
    }
}