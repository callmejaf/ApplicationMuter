# ApplicationMuter

ApplicationMuter is a tool that allows users to mute specific applications on specified playback devices. It runs in the background and ensures that the selected applications remain muted, even if they are restarted.

## Features

- Mute applications such as Discord and Parsec on selected audio devices.
- Continuous monitoring to ensure specified applications remain muted.
- Configuration through a simple JSON file.
- Low CPU usage and efficient performance.
- Runs in the background with no open console window.

## Important

- The application **must be run as an administrator** to function correctly.

## How to Use

1. **Download and Extract**: Download the release package and extract it to a desired location.

2. **Configuration**:
   - Navigate to the `config` folder.
   - Open the `settings.json` file in a text editor.
   - Modify the `settings.json` to include the names of the devices and applications you want to mute. Here is an example configuration:

     ```json
     {
       "Devices": [
         "Headphones (3- Arctis Nova Pro Wireless)", // NON SONAR PLAYBACK DEVICE
         "SteelSeries Sonar - Microphone (SteelSeries Sonar Virtual Audio Device)" // SONAR VIRTUAL MICROPHONE
       ],
       "Applications": [
         "Discord",
         "parsec"
       ]
     }
     ```

     - **Note**: Device names are case insensitive, but they must match exactly as they appear in your volume mixer (accessible by pressing `Win + R`, typing `sndvol`, and pressing `Enter`).
     - **Note**: Application names must match exactly as they appear in Task Manager.

3. **Run the Application**:
   - Run the `ApplicationMuter.exe` as an administrator. The application will start in the background with no open console window.

## Fixing Discord Screenshare When Using Sonar (or Any Virtual Audio Devices)

Namely, if participants in a voice call are hearing themselves or there is a doubling of your own mic in the screenshare audio, follow these steps:

1. **Mute Your Sonar Mic for Discord or Parsec**:
   - To fix doubling of mic audio - ensure that any virtual microphone like Sonar Mic or Voicemeeter is muted for applications like Discord or Parsec.

2. **Mute Non-Sonar Playback Device**:
   - If you are using Sonar Chat Mix, mute your **non-Sonar** playback device in the JSON configuration, not the virtual Sonar playback device.

### Example Configuration for Fixing Discord Screenshare

Here's how your `settings.json` might look if you need to mute a non-Sonar playback device and a Sonar microphone:

```json
{
  "Devices": [
    "Headphones (3- Arctis Nova Pro Wireless)", // NON SONAR PLAYBACK DEVICE
    "SteelSeries Sonar - Microphone (SteelSeries Sonar Virtual Audio Device)" // SONAR VIRTUAL MICROPHONE
  ],
  "Applications": [
    "Discord",
    "parsec"
  ]
}
