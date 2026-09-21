# Suggested Jellyfin community submission text

## Guru Audio Enhancer — Jellyfin 12 dialogue enhancement plugin

I've developed an independent Jellyfin 12 server plugin intended to address a common playback problem where dialogue is substantially quieter than music and effects.

Rather than modifying Jellyfin clients or replacing the original soundtrack, **Guru Audio Enhancer** uses the server's Jellyfin FFmpeg installation to generate optional external AAC audio tracks alongside the media. Jellyfin discovers these as normal external audio streams, so users can select them from the existing Audio menu.

Current presets are:

- **Dialogue Boost** — brings speech forward while retaining moderate dynamics.
- **Night Mode** — adds stronger dynamic-range compression to reduce loud music/effects relative to dialogue.
- **Strong Dialogue** — a more aggressive speech-forward profile.

The original media and original audio tracks are never modified.

Version 0.1.2.0 also includes **Safe Test Mode**, which searches the Jellyfin library and locks processing to an exact Jellyfin Item ID before generation or cleanup.

Technical scope:

- Jellyfin 12.x / .NET 10
- target ABI `12.0.0.0`
- Movies and TV episodes
- selectable AAC stereo sidecar output
- Jellyfin FFmpeg / FFprobe processing
- scheduled generation and cleanup tasks
- no external service or media upload
- current limitation: first source audio stream only

The plugin has been tested with Jellyfin Web and Jellyfin Media Player, where generated Dialogue Boost and Night Mode streams appear in the normal audio selector.

Repository: `<YOUR GITHUB REPOSITORY URL>`

Plugin repository manifest: `<RAW manifest.json URL>`

Release: `<LATEST RELEASE URL>`

Feedback on the Jellyfin API usage, packaging approach, audio-processing strategy and any changes recommended before wider distribution would be appreciated.
