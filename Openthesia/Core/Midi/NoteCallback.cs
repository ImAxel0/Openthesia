using Melanchall.DryWetMidi.Multimedia;

namespace Openthesia.Core.Midi;

public static class NoteCallback
{
    public static NotePlaybackData HandMutingNoteCallback(NotePlaybackData rawNoteData, long rawTime, long rawLength, TimeSpan playbackTime)
    {
        // Use a composite key which is the unique combination of note number and time in the midi track
        var key = rawNoteData.NoteNumber + rawTime.ToString();
        
        if (!LeftRightData.S_NoteIndexMap.ContainsKey(key))
        {
            return rawNoteData; // Play the note
        }
        
        var index = LeftRightData.S_NoteIndexMap[key];
        
        // Check if the note should be muted because the corresponding hand is muted
        if (LeftRightData.S_IsRightNote[index] && !ScreenCanvasControls.RightHandActive ||
            !LeftRightData.S_IsRightNote[index] && !ScreenCanvasControls.LeftHandActive)
        {
            return null!; // Mute the note
        }
        
        return rawNoteData; // Play the note
    }

}
