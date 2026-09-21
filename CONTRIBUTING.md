# Contributing

Contributions are welcome.

## Before opening a pull request

1. Test against a current Jellyfin 12.x server.
2. Keep original media immutable. New features should not overwrite source video/audio files.
3. Use Safe Test Mode while developing processing changes.
4. Confirm the plugin builds with .NET 10.
5. Avoid introducing client-specific dependencies unless there is a clear server-side fallback.

## Audio-processing changes

Audio filters can have very different results across stereo, 5.1 and 7.1 material. A proposed filter change should ideally be tested against:

- a stereo source;
- a 5.1 source with centre-channel dialogue;
- a 7.1 source;
- quiet dialogue followed by loud music/effects;
- speech with background music.

Please describe the source layout and subjective result in the pull request. Do not upload copyrighted movie clips to the repository.

## Code style

Run:

```bash
dotnet build Jellyfin.Plugin.GuruAudioEnhancer/Jellyfin.Plugin.GuruAudioEnhancer.csproj -c Release
```

New public-facing settings should include clear help text in the configuration page.

## Versioning

Releases use four-part Jellyfin-style versions, for example `0.1.2.0`, and tags use the `v` prefix: `v0.1.2.0`.
