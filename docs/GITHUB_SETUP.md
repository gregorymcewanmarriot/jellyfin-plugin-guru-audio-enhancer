# GitHub setup

## Suggested repository

**Name**

```text
jellyfin-plugin-guru-audio-enhancer
```

**Description**

```text
Jellyfin 12 server plugin that creates selectable Dialogue Boost and Night Mode audio tracks without modifying original media.
```

**Suggested topics**

```text
jellyfin jellyfin-plugin audio dialogue accessibility ffmpeg dotnet
```

## Create the repository

1. Create a **public** GitHub repository with the suggested name.
2. Do not initialise it with a separate README or licence if you plan to upload this package as-is; they are already included.
3. Upload/commit **the contents of the repository-ready folder**, not the outer ZIP as a single file.
4. Confirm these files are at the repository root:

   ```text
   README.md
   LICENSE
   CHANGELOG.md
   build.yaml
   manifest.json
   Jellyfin.Plugin.GuruAudioEnhancer/
   .github/
   docs/
   ```

5. Confirm the default branch is `main`.
6. Open the repository's **Actions** tab and confirm the CI workflow completes successfully.

## First public release

Once CI is green, create the release by pushing the existing version tag:

```bash
git tag v0.1.2.0
git push origin v0.1.2.0
```

The Release workflow will build and publish the install ZIP and then populate `manifest.json` with the actual GitHub owner/repository URL and MD5 checksum.

After it finishes, the Jellyfin repository URL will be:

```text
https://raw.githubusercontent.com/<YOUR_GITHUB_USERNAME>/jellyfin-plugin-guru-audio-enhancer/main/manifest.json
```

Test installation from that URL before announcing it publicly.

## Suggested GitHub release title

The workflow creates:

```text
Guru Audio Enhancer 0.1.2.0
```

## Jellyfin community submission

After repository installation has been tested, use the draft in [SUBMISSION_TEXT.md](SUBMISSION_TEXT.md) when asking the Jellyfin community for review/feedback.
