#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${repo_root}"

kernel_project="src/NuBlox.Kernel/NuBlox.Kernel.csproj"
test_project="tests/NuBlox.Kernel.Tests/NuBlox.Kernel.Tests.csproj"

printf 'NuBlox production verification\n'
printf 'Repository: %s\n' "${repo_root}"
printf '.NET SDK: '
dotnet --version

dotnet restore "${test_project}" --nologo

dotnet build "${kernel_project}" \
  --configuration Release \
  --no-restore \
  --nologo

dotnet test "${test_project}" \
  --configuration Release \
  --no-restore \
  --nologo
