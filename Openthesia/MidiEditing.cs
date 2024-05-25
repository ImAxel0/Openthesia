using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Vanara.PInvoke;

namespace Openthesia;

public class MidiEditing
{
    public static void SetRightHand(int noteIndex, bool isRightHand)
    {
        LeftRightData.S_IsRightNote[noteIndex] = isRightHand;
    }

    public static void ReadData()
    {
        string filePath = Path.Combine(ProgramData.HandsDataPath, MidiFileData.FileName.Replace(".mid", string.Empty) + ".xml");
        if (!File.Exists(filePath))
            return;

        try
        {
            using (FileStream fileStream = new(filePath, FileMode.Open))
            {
                XmlSerializer xmlSerializer = new(typeof(LeftRightData));
                LeftRightData leftRightData = (LeftRightData)xmlSerializer.Deserialize(fileStream);
                LeftRightData.S_IsRightNote = leftRightData.IsRightNote;
            }
        }
        catch (Exception ex)
        {
            User32.MessageBox(IntPtr.Zero, $"{ex.Message}", "Couldn't read hands data", User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
        }
    }

    public static void SaveData()
    {
        string filePath = Path.Combine(ProgramData.HandsDataPath, MidiFileData.FileName.Replace(".mid", string.Empty) + ".xml");
        LeftRightData leftRightData = new()
        {
            IsRightNote = LeftRightData.S_IsRightNote
        };

        try
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(LeftRightData));
                xmlSerializer.Serialize(fileStream, leftRightData);
            }
        }
        catch (Exception ex)
        {
            User32.MessageBox(IntPtr.Zero, $"{ex.Message}", "Couldn't save hands data", User32.MB_FLAGS.MB_ICONERROR | User32.MB_FLAGS.MB_TOPMOST);
        }
    }
}
