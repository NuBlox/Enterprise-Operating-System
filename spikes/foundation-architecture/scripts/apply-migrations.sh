#!/usr/bin/env bash
set -euo pipefail

spike_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
mode="${NUBLOX_SPIKE_MIGRATION_MODE:-docker}"

for migration in "$spike_root"/migrations/*.sql; do
  echo "Applying $(basename "$migration")"
  case "$mode" in
    docker)
      docker compose -f "$spike_root/docker-compose.yml" exec -T postgres \
        psql -U nublox -d nublox_spike -v ON_ERROR_STOP=1 < "$migration"
      ;;
    host)
      psql -v ON_ERROR_STOP=1 -f "$migration"
      ;;
    *)
      echo "NUBLOX_SPIKE_MIGRATION_MODE must be docker or host" >&2
      exit 2
      ;;
  esac
done
