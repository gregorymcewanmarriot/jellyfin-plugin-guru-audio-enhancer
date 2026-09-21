#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$ROOT/Jellyfin.Plugin.GuruAudioEnhancer/Jellyfin.Plugin.GuruAudioEnhancer.csproj"
if [[ $# -ge 1 ]]; then
  VERSION="$1"
else
  VERSION="$(python3 - "$PROJECT" <<'PY2'
import sys
import xml.etree.ElementTree as ET
root = ET.parse(sys.argv[1]).getroot()
node = root.find('.//Version')
if node is None or not node.text:
    raise SystemExit('Could not read <Version> from project file')
print(node.text.strip())
PY2
)"
fi
PUBLISH="$ROOT/publish"
DIST="$ROOT/dist"
PACKAGE="$DIST/package"
ASSET="$DIST/GuruAudioEnhancer-${VERSION}-jf12.zip"

rm -rf "$PUBLISH" "$PACKAGE"
mkdir -p "$PUBLISH" "$PACKAGE" "$DIST"

dotnet restore "$PROJECT"
dotnet publish "$PROJECT" -c Release -o "$PUBLISH" \
  -p:Version="$VERSION" -p:AssemblyVersion="$VERSION" -p:FileVersion="$VERSION"

cp "$PUBLISH/Jellyfin.Plugin.GuruAudioEnhancer.dll" "$PACKAGE/"
cp "$ROOT/docs/images/logo.png" "$PACKAGE/logo.png"
python3 "$ROOT/.github/scripts/create_meta.py" \
  --version "$VERSION" \
  --target-abi "12.0.0.0" \
  --repository "local/GuruAudioEnhancer" \
  --output "$PACKAGE/meta.json"

if command -v zip >/dev/null 2>&1; then
  rm -f "$ASSET"
  (cd "$PACKAGE" && zip -9j "$ASSET" Jellyfin.Plugin.GuruAudioEnhancer.dll meta.json logo.png)
  echo "Built package: $ASSET"
else
  echo "Build succeeded. Install ZIP was not created because 'zip' is not installed."
fi

echo "DLL: $PUBLISH/Jellyfin.Plugin.GuruAudioEnhancer.dll"
