# Architecture

Guru Audio Enhancer is intentionally server-side so compatible Jellyfin clients do not need plugin-specific code.

## Flow

```text
Jellyfin library item
       |
       v
Guru Audio Enhancer Scheduled Task
       |
       v
Jellyfin FFmpeg / FFprobe
       |
       +-- channel-layout-aware downmix
       +-- dialogue-frequency EQ
       +-- dynamic compression
       +-- peak limiting
       |
       v
External AAC sidecar
       |
       v
Jellyfin library refresh
       |
       v
Normal client Audio selector
```

## Why sidecar audio

A Jellyfin server plugin cannot reliably insert a live equalizer control into every official client. Sidecar audio uses Jellyfin's existing media model: generated tracks are ordinary external audio streams, so clients can select them without custom UI integration.

## Processing

The current release probes channel count and uses different downmix coefficients for surround layouts. The centre channel is deliberately weighted more heavily on 5.1/7.1 sources before EQ, compression and limiting.

The first source audio stream is currently selected with `-map 0:a:0`.

## File naming

Generated tracks follow Jellyfin external-audio conventions:

```text
<Movie base name>.Dialogue Boost.eng.aac
<Movie base name>.Night Mode.eng.aac
<Movie base name>.Strong Dialogue.eng.aac
```

## Non-goals for v0.1.x

- Real-time DSP during Direct Play.
- Per-user client-side EQ controls.
- Replacing or rewriting the original media container.
- Automatic source-language selection.
