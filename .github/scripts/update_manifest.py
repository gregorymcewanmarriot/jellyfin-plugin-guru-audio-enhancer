#!/usr/bin/env python3
import argparse
import json
import re
from datetime import datetime, timezone
from pathlib import Path

PLUGIN_GUID = "61a21ab4-b0c7-4e18-92d1-42f8495ed131"
PLUGIN_NAME = "Guru Audio Enhancer"
DESCRIPTION = (
    "Creates optional dialogue-enhanced and night-mode external AAC tracks for "
    "movies and TV episodes without modifying the original media."
)
OVERVIEW = "Optional dialogue-enhanced and night-mode audio tracks for Jellyfin"


def changelog_for(version: str) -> str:
    path = Path("CHANGELOG.md")
    if not path.exists():
        return f"Release {version}."
    text = path.read_text(encoding="utf-8")
    pattern = re.compile(
        rf"^##\s+{re.escape(version)}(?:\s+—[^\n]*)?\n(.*?)(?=^##\s+|\Z)",
        re.MULTILINE | re.DOTALL,
    )
    match = pattern.search(text)
    return match.group(1).strip() if match else f"Release {version}."


parser = argparse.ArgumentParser()
parser.add_argument("--manifest", default="manifest.json")
parser.add_argument("--version", required=True)
parser.add_argument("--tag", required=True)
parser.add_argument("--checksum", required=True)
parser.add_argument("--repository", required=True, help="owner/repository")
parser.add_argument("--asset", required=True)
parser.add_argument("--target-abi", default="12.0.0.0")
args = parser.parse_args()

owner = args.repository.split("/", 1)[0]
manifest_path = Path(args.manifest)
try:
    data = json.loads(manifest_path.read_text(encoding="utf-8")) if manifest_path.exists() else []
except json.JSONDecodeError:
    data = []

if not isinstance(data, list):
    data = []

plugin = next((p for p in data if p.get("guid") == PLUGIN_GUID), None)
if plugin is None:
    plugin = {
        "guid": PLUGIN_GUID,
        "name": PLUGIN_NAME,
        "description": DESCRIPTION,
        "overview": OVERVIEW,
        "owner": owner,
        "category": "General",
        "imageUrl": f"https://raw.githubusercontent.com/{args.repository}/main/docs/images/logo.png",
        "versions": [],
    }
    data.insert(0, plugin)
else:
    plugin.update(
        {
            "name": PLUGIN_NAME,
            "description": DESCRIPTION,
            "overview": OVERVIEW,
            "owner": owner,
            "category": "General",
            "imageUrl": f"https://raw.githubusercontent.com/{args.repository}/main/docs/images/logo.png",
        }
    )

versions = [v for v in plugin.get("versions", []) if v.get("version") != args.version]
versions.insert(
    0,
    {
        "version": args.version,
        "changelog": changelog_for(args.version),
        "targetAbi": args.target_abi,
        "sourceUrl": f"https://github.com/{args.repository}/releases/download/{args.tag}/{args.asset}",
        "checksum": args.checksum.lower(),
        "timestamp": datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z"),
    },
)
plugin["versions"] = versions[:20]

manifest_path.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
