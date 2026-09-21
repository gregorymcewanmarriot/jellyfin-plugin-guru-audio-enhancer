# Changelog

All notable changes to Guru Audio Enhancer are documented here.

## 0.1.2.0 — 2026-09-21

- Added Jellyfin library search to Safe Test Mode.
- Added exact Jellyfin Item ID selection to prevent fuzzy-title mismatches.
- Added selected-title status showing source audio and discovered enhanced tracks.
- Kept the proven Dialogue Boost and Night Mode filter profiles from v0.1.1 unchanged.
- Retained backwards compatibility with the v0.1.1 text-based test title setting.
- Confirmed generated tracks appear in Jellyfin Web and Jellyfin Media Player's in-player audio selector.

## 0.1.1.0 — 2026-09-21

- Added Safe Test Mode to restrict generation and cleanup to one matching title.
- Added clearer safeguards around existing generated tracks.
- Improved local build packaging behaviour when the `zip` utility is not installed.

## 0.1.0.0 — 2026-09-21

- Initial Jellyfin 12 / .NET 10 build.
- Added Dialogue Boost, Night Mode and Strong Dialogue presets.
- Added movie/episode processing options.
- Added FFmpeg-based sidecar AAC generation.
- Added generation and cleanup Scheduled Tasks.
- Added overwrite protection and non-local-media skipping.
