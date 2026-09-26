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
LINES="${1:-200}"

if [[ ! -d "$LOG_DIR" ]]; then
  printf 'No terminal logs directory found at: %s\n' "$LOG_DIR" >&2
  exit 1
fi

LATEST_FILE="$(ls -1t "$LOG_DIR"/*.log(N) 2>/dev/null | head -n 1)"
if [[ -z "$LATEST_FILE" ]]; then
  printf 'No terminal log files found in: %s\n' "$LOG_DIR" >&2
  exit 1
fi

printf 'Tailing latest terminal log: %s\n\n' "$LATEST_FILE"
tail -n "$LINES" -f "$LATEST_FILE"
