#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${repo_root}"

kernel_project="src/NuBlox.Kernel/NuBlox.Kernel.csproj"
kernel_test_project="tests/NuBlox.Kernel.Tests/NuBlox.Kernel.Tests.csproj"
persistence_project="src/NuBlox.Persistence.PostgreSql/NuBlox.Persistence.PostgreSql.csproj"
persistence_test_project="tests/NuBlox.Persistence.PostgreSql.IntegrationTests/NuBlox.Persistence.PostgreSql.IntegrationTests.csproj"

printf 'NuBlox production verification\n'
printf 'Repository: %s\n' "${repo_root}"
printf '.NET SDK: '
dotnet --version

dotnet restore "${kernel_test_project}" --nologo
dotnet restore "${persistence_test_project}" --nologo

dotnet build "${kernel_project}" \
  --configuration Release \
  --no-restore \
  --nologo

dotnet build "${persistence_project}" \
  --configuration Release \
  --no-restore \
  --nologo

dotnet test "${kernel_test_project}" \
  --configuration Release \
  --no-restore \
  --nologo

if [[ -n "${NUBLOX_POSTGRES_CONNECTION_STRING:-}" ]]; then
  dotnet test "${persistence_test_project}" \
    --configuration Release \
    --no-restore \
    --nologo
else
  printf 'Skipping PostgreSQL integration tests: NUBLOX_POSTGRES_CONNECTION_STRING is not set.\n'
fi
