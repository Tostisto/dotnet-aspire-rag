#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

if [ "${1:-}" = "update" ]; then
  git submodule update --init --remote --depth 1
else
  git submodule update --init --depth 1
fi

git submodule status
