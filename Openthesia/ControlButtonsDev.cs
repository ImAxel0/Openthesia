using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Openthesia
{
    internal enum ButtonFunc
    {
        STOP = 0,
        PLAY = 1,
        RECORD = 2,
        NULL,
    }

    internal static class ControlButtonsDev
    {
        internal static int?[] ControlNumberValues = new int?[3];

        private static void SetControlNumberValues(int controlNumber, int value)
        {
            for(int i = 0; i != ControlNumberValues.Length; i++)
            {
                if (!ControlNumberValues[i].HasValue)
                    continue;

                if (ControlNumberValues[i].Value == value)
                    ControlNumberValues[i] = null;
            }

            ControlNumberValues[controlNumber] = value;
        }

        internal static void SetControlButtonsDev(List<int> list)
        {

        }

        internal static void OnControlChange(ControlChangeEvent ev)
        {
            if (Router.Route == Router.Routes.MidiPlayback)
            {
                List<int> ListOfInd = ControlNumberValues.Select((v, i) => new { i, v }).Where(t => t.v == ev.ControlNumber).Select(t => t.i).ToList();

                if (ListOfInd.Count == 0)
                    return;

                if (ev.ControlValue == 0)
                    return;

                switch ((ButtonFunc)ListOfInd[0])
                {
                    case ButtonFunc.STOP:
                            ScreenCanvas.Stop();
                        break;

                    case ButtonFunc.PLAY:
                        if (MidiPlayer.IsTimerRunning)
                            ScreenCanvas.Pause();
                        else
                            ScreenCanvas.Play();
                        break;

                    default: return;
                }
            }
            else if (Router.Route == Router.Routes.PlayMode)
            {
                List<int> ListOfInd = ControlNumberValues.Select((v, i) => new { i, v }).Where(t => t.v == ev.ControlNumber).Select(t => t.i).ToList();

                if (ListOfInd.Count == 0)
                    return;

                switch ((ButtonFunc)ListOfInd[0])
                {
                    case ButtonFunc.STOP:
                        if (MidiRecording.IsRecording())
                            MidiRecording.StopRecording();
                        else if (MidiPlayer.IsTimerRunning)
                            ScreenCanvas.Stop();
                        break;

                    case ButtonFunc.RECORD:
                        if (!MidiRecording.IsRecording())
                            MidiRecording.StartRecording();
                        break;

                    default: return;
                }

                return;
            }
            else if (Router.Route == Router.Routes.Settings)
            {
                if (ev.ControlValue == 0)
                    return;

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

                    default: return;
                }

                Settings.PressedButt = ButtonFunc.NULL;
                SetControlNumberValues(ButtonFuncInt, ev.ControlNumber);

                return;
            }
        }

        internal static void SetControlButtonsDev(int?[]? controlButtonDev)
        {
            throw new NotImplementedException();
        }
    }
}
