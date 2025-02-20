# KLC-Lanner 
Lanner is a fork of KLC-Hawk which was intended for use with KLC-Canary (which itself is a fork of KLC-Finch which is an alternative to Kaseya Live Connect) to simulate what Canary/Finch was like to use without having Kaseya VSA 9.5 access to launch a real connection.

It has some limited functionality to replay parts of Hawk capture files but unfortunately not remote control.

## Usage
- Run Lanner (Hawk must not be running).
- Run Canary, hold down Left Shift key and press "RC Test".
  - This will trigger Canary to connect to Lanner.
- Some modules are filled in with simulated replacements (Files, Registry, Events, Services, Processes) from the local machine instead of a remote machine.
- Other modules are not yet simulated (Remote Control, CMD, PowerShell, Toolbox).
- Some modules might accept Hawk capture file replays.

## Required other repos to build (all the same as KLC-Hawk)
- LibKaseya
- LibKaseyaLiveConnect
- VP8.NET (modified)

## Required packages to build (all the same as KLC-Hawk)
- Fleck
- Newtonsoft.Json
- nucs.JsonSettings
- RestSharp
- WatsonWebsocket