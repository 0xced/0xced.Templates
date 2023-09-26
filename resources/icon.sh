#!/bin/bash -xeo pipefail

SCRIPT_DIR=$(dirname "$BASH_SOURCE")

rsvg-convert --version > /dev/null 2>/dev/null || (echo "Please install librsvg with \`brew install librsvg\` and try again." && exit 1)

curl https://raw.githubusercontent.com/dmhendricks/file-icon-vectors/9b4b95928f7ff8d73bf45edf34862386e3c48ea5/dist/icons/classic/csproj.svg -o icon.svg

rsvg-convert --page-width 256 --page-height 256 --left 36 --height 256 --keep-aspect-ratio --output "${SCRIPT_DIR}/icon.png" "${SCRIPT_DIR}/icon.svg"
