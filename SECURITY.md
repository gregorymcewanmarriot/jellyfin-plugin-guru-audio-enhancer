# Security Policy

## Supported versions

Only the latest published release is actively supported while the plugin is in the 0.x development series.

## Reporting a vulnerability

Please use GitHub's private security advisory feature for vulnerabilities that could expose files, execute unintended commands, bypass Jellyfin permissions, or delete/modify media unexpectedly.

Do not publish exploit details in a public issue before a fix is available.

## Security model

Guru Audio Enhancer runs inside the Jellyfin Server process and launches the administrator-configured FFmpeg executable. It reads media paths supplied by Jellyfin and writes generated AAC sidecar files beside local media. Server filesystem permissions therefore form part of the plugin's security boundary.
