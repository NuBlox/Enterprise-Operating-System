#!/bin/zsh
set -euo pipefail

SCRIPT_PATH="${0:A}"
SCRIPT_DIR="${SCRIPT_PATH:h}"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd -P)"
if [[ -n "${NUBLOX_TERMINAL_LOG_DIR:-}" ]]; then
  if [[ "$NUBLOX_TERMINAL_LOG_DIR" = /* ]]; then
    LOG_DIR="$NUBLOX_TERMINAL_LOG_DIR"
  else
    LOG_DIR="$REPO_ROOT/$NUBLOX_TERMINAL_LOG_DIR"
  fi
else
  LOG_DIR="$REPO_ROOT/logs/terminal"
fi
MAX_COUNT="${1:-20}"

if [[ ! -d "$LOG_DIR" ]]; then
  printf 'No terminal logs directory found at: %s\n' "$LOG_DIR"
  exit 0
fi

printf 'Latest terminal log files in %s\n\n' "$LOG_DIR"
ls -1t "$LOG_DIR" | head -n "$MAX_COUNT"
