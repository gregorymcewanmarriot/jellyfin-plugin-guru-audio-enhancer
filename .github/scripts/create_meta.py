#!/usr/bin/env python3
import argparse
import json
from datetime import datetime, timezone

PLUGIN_GUID = "61a21ab4-b0c7-4e18-92d1-42f8495ed131"
PLUGIN_NAME = "Guru Audio Enhancer"
DESCRIPTION = (
    "Creates optional dialogue-enhanced and night-mode external AAC tracks for "
    "movies and TV episodes without modifying the original media."
)
OVERVIEW = "Optional dialogue-enhanced and night-mode audio tracks for Jellyfin"

parser = argparse.ArgumentParser()
parser.add_argument("--version", required=True)
parser.add_argument("--target-abi", default="12.0.0.0")
parser.add_argument("--repository", default="")
parser.add_argument("--owner", default="Guru Audio Enhancer Contributors")
parser.add_argument("--output", required=True)
args = parser.parse_args()

image_url = ""
if args.repository and "/" in args.repository and not args.repository.startswith("local/"):
    image_url = f"https://raw.githubusercontent.com/{args.repository}/main/docs/images/logo.png"

meta = {
    "category": "General",
    "changelog": f"Guru Audio Enhancer {args.version}. See CHANGELOG.md / GitHub release notes.",
    "description": DESCRIPTION,
    "guid": PLUGIN_GUID,
    "name": PLUGIN_NAME,
    "overview": OVERVIEW,
    "owner": args.owner,
    "targetAbi": args.target_abi,
    "timestamp": datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z"),
    "version": args.version,
}
if image_url:
    meta["imageUrl"] = image_url

with open(args.output, "w", encoding="utf-8") as f:
    json.dump(meta, f, indent=2, ensure_ascii=False)
    f.write("\n")
