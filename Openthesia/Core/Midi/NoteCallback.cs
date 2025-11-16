using Melanchall.DryWetMidi.Multimedia;

namespace Openthesia.Core.Midi;

public static class NoteCallback
{
    public static NotePlaybackData HandMutingNoteCallback(NotePlaybackData rawNoteData, long rawTime, long rawLength, TimeSpan playbackTime)
    {
        // Rebuild the same composite key as before
        var key = $"{rawNoteData.NoteNumber}_{rawTime}";

        // If we have any matching notes, check them all
        if (LeftRightData.S_NoteIndexMap.TryGetValue(key, out var indices))
        {
            foreach (var index in indices)
            {
                // Mute if the corresponding hand is inactive
                if (LeftRightData.S_IsRightNote[index] && !ScreenCanvasControls.RightHandActive ||
                    !LeftRightData.S_IsRightNote[index] && !ScreenCanvasControls.LeftHandActive)
                {
                    return null!; // Mute the note
                }
            }
        }
        return rawNoteData; // Play the note
    }

}
