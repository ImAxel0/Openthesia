<p align="center">
 <img src="https://i.imgur.com/aN1rSmB.png" width="300" height="300" />
</p>

# Openthesia [![Github All Releases](https://img.shields.io/github/v/release/ImAxel0/Openthesia?&style=for-the-badge)]() [![Github All Releases](https://img.shields.io/github/downloads/ImAxel0/Openthesia/total.svg?&color=31CB15&style=for-the-badge)]()

### ...is a customizable midi visualization software with similar features to Synthesia

---

### :star2: If you really enjoy the app consider starring the repo :star2:

:orange_book: [Documentation](https://openthesia.pages.dev/documentation) :orange_book:

## What can it do :question:

Openthesia includes two main modes, **Midi playback** and **Play mode**.

In the Midi playback mode you can select a midi file to play and choose between just visualizing and listening the note blocks or play along with it.
When playing along the playback will wait for the right note input before going to the next note.
In the Play mode you can visualize midi inputs as rising note blocks and also record them to later export the recording as a midi file.

## Features :star:

- **Fully customizable**: change background and note blocks color, turn on notes glowing effect
- **Midi playback**: play and visualize midi files at different speeds, tempo and notes direction
- **Learning mode**: wait for the right key input before going to the next note
- **Play mode**: visualize midi input realtime and record it to export it as a midi file
- **Hands separation**: differentiate between left and right hand with colors
- **SoundFonts support**: built-in and external sounds support through .sf2 files
- **Video recording**: capture video of your MIDI playbacks or performances and export them directly.
- **Plugins support**: use your favorite VST2 instruments and audio effects directly inside Openthesia!

## What operating systems does it support? :desktop_computer:

Openthesia is officially supported on **Windows**.

However, it appears to be working almost flawlessly on **Linux** using [**Wine**](https://www.winehq.org/)!
If you're a Linux user and willing to experiment, you may still enjoy the full experience, just keep in mind that it's not officially supported or tested.

| Feature       | Windows | Linux (wine) |
| ---           | ---     | ---          |
| MIDI Playback | ✅      | ✅          |
| MIDI Input/Output Devices | ✅ | ✅ |
| SoundFonts    | ✅      | ✅ |
| Video Recording | ✅    | ✖️ |
| VST Plugins   | ✅      | ✅ (may need further testing) |

## Installation :arrow_down:

Download the latest setup from the [releases](https://github.com/ImAxel0/Openthesia/releases) section and install the program.

---

## Building from Source :wrench:
1. Install [NET6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) if not already installed
2. Clone the repository and build the project with your favorite IDE or tool
3. If using SoundFonts, run the program once to create the `SoundFonts` folder inside the program build directory then place your .sf2 files under it

---

# Gallery

![openthesia_home](https://github.com/ImAxel0/Openthesia/assets/124681710/bf9d0fc8-55a9-4583-9514-da29bd5159dd)

![playback](https://github.com/ImAxel0/Openthesia/assets/124681710/bfccfaac-cb8f-4ffc-87ac-23c0ced6b0e8)

![openthesia_playmode](https://github.com/ImAxel0/Openthesia/assets/124681710/915717df-796a-4697-904a-8582321f3de6)

![openthesia_settings](https://github.com/ImAxel0/Openthesia/assets/124681710/7e8afe03-764c-4ff1-af9d-2337b03edd23)

## SoundFonts credits

https://freepats.zenvoid.org/

- **Upright Piano KW** licensed under [Creative Commons CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)
- **YDP Grand Piano** licensed under [Creative Commons Attribution 3.0](https://creativecommons.org/licenses/by/3.0/)
- **Salamander Grand Piano** licensed under [Creative Commons Attribution 3.0](https://creativecommons.org/licenses/by/3.0/)
- **Old Piano FB** licensed under [Creative Commons CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/)

## Thanks to

- [DryWetMidi](https://melanchall.github.io/drywetmidi/index.html)
- [NAudio](https://github.com/naudio/NAudio)
- [MeltySynth](https://github.com/sinshu/meltysynth)
- [VST.NET](https://github.com/obiwanjacobi/vst.net)
- [Dear ImGui](https://github.com/ocornut/imgui) & [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET)
