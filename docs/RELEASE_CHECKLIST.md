# Release checklist

## One-time repository setup

1. Create a public GitHub repository, suggested name: `jellyfin-plugin-guru-audio-enhancer`.
2. Upload the contents of this repository-ready package to the repository root.
3. Ensure GitHub Actions are enabled.
4. In **Settings → Actions → General**, allow workflows to have **Read and write permissions**, or keep the workflow-level `contents: write` permission enabled if your organization permits it.
5. Confirm the default branch is `main`.

## Before each release

1. Update the version in:
   - `build.yaml`
   - `Jellyfin.Plugin.GuruAudioEnhancer/Jellyfin.Plugin.GuruAudioEnhancer.csproj`
   - `CHANGELOG.md`
2. Push and confirm the `CI` workflow is green.
3. Create a four-part tag matching the version:

   ```bash
   git tag v0.1.2.0
   git push origin v0.1.2.0
   ```

## Automated release result

The `Release` workflow:

- builds with .NET 10;
- generates `meta.json`;
- creates a ZIP with the plugin DLL, `meta.json` and `logo.png` at the ZIP root;
- creates/uploads the GitHub Release asset;
- calculates the MD5 checksum;
- updates `manifest.json` on `main`.

After the workflow completes, the Jellyfin repository URL is:

```text
https://raw.githubusercontent.com/<OWNER>/<REPOSITORY>/main/manifest.json
```

## Before announcing publicly

- Install the release from the repository URL on a clean/test Jellyfin server.
- Restart Jellyfin.
- Generate one test title in Safe Test Mode.
- Confirm the external track appears in Jellyfin Web and at least one native client.
- Verify cleanup only removes Guru-generated sidecar names.
