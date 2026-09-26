#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "${repo_root}"

kernel_project="src/NuBlox.Kernel/NuBlox.Kernel.csproj"
kernel_test_project="tests/NuBlox.Kernel.Tests/NuBlox.Kernel.Tests.csproj"
identity_project="src/NuBlox.Identity/NuBlox.Identity.csproj"
identity_test_project="tests/NuBlox.Identity.Tests/NuBlox.Identity.Tests.csproj"
authority_project="src/NuBlox.Authority/NuBlox.Authority.csproj"
audit_project="src/NuBlox.Audit/NuBlox.Audit.csproj"
audit_test_project="tests/NuBlox.Audit.Tests/NuBlox.Audit.Tests.csproj"
observability_project="src/NuBlox.Observability/NuBlox.Observability.csproj"
observability_test_project="tests/NuBlox.Observability.Tests/NuBlox.Observability.Tests.csproj"
api_project="src/NuBlox.Api/NuBlox.Api.csproj"
api_test_project="tests/NuBlox.Api.Tests/NuBlox.Api.Tests.csproj"
persistence_project="src/NuBlox.Persistence.PostgreSql/NuBlox.Persistence.PostgreSql.csproj"
persistence_test_project="tests/NuBlox.Persistence.PostgreSql.IntegrationTests/NuBlox.Persistence.PostgreSql.IntegrationTests.csproj"
work_products_domain_project="src/NuBlox.WorkProducts.Domain/NuBlox.WorkProducts.Domain.csproj"
work_products_application_project="src/NuBlox.WorkProducts.Application/NuBlox.WorkProducts.Application.csproj"
work_products_postgres_project="src/NuBlox.WorkProducts.Infrastructure.PostgreSql/NuBlox.WorkProducts.Infrastructure.PostgreSql.csproj"
work_products_test_project="tests/NuBlox.WorkProducts.Tests/NuBlox.WorkProducts.Tests.csproj"
work_products_postgres_test_project="tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/NuBlox.WorkProducts.PostgreSql.IntegrationTests.csproj"

printf 'NuBlox production verification\n'
printf 'Repository: %s\n' "${repo_root}"
printf '.NET SDK: '
dotnet --version

dotnet restore "${kernel_test_project}" --nologo
dotnet restore "${identity_test_project}" --nologo
dotnet restore "${audit_test_project}" --nologo
dotnet restore "${observability_test_project}" --nologo
dotnet restore "${api_test_project}" --nologo
dotnet restore "${persistence_test_project}" --nologo
dotnet restore "${work_products_test_project}" --nologo
dotnet restore "${work_products_postgres_test_project}" --nologo

dotnet build "${kernel_project}" --configuration Release --no-restore --nologo
dotnet build "${identity_project}" --configuration Release --no-restore --nologo
dotnet build "${authority_project}" --configuration Release --no-restore --nologo
dotnet build "${audit_project}" --configuration Release --no-restore --nologo
dotnet build "${observability_project}" --configuration Release --no-restore --nologo
dotnet build "${api_project}" --configuration Release --no-restore --nologo
dotnet build "${persistence_project}" --configuration Release --no-restore --nologo
dotnet build "${work_products_domain_project}" --configuration Release --no-restore --nologo
dotnet build "${work_products_application_project}" --configuration Release --no-restore --nologo
dotnet build "${work_products_postgres_project}" --configuration Release --no-restore --nologo

dotnet test "${kernel_test_project}" --configuration Release --no-restore --nologo
dotnet test "${identity_test_project}" --configuration Release --no-restore --nologo
dotnet test "${audit_test_project}" --configuration Release --no-restore --nologo
dotnet test "${observability_test_project}" --configuration Release --no-restore --nologo
dotnet test "${api_test_project}" --configuration Release --no-restore --nologo
dotnet test "${work_products_test_project}" --configuration Release --no-restore --nologo

if [[ -n "${NUBLOX_POSTGRES_CONNECTION_STRING:-}" ]]; then
  dotnet test "${persistence_test_project}" --configuration Release --no-restore --nologo
  dotnet test "${work_products_postgres_test_project}" --configuration Release --no-restore --nologo
else
  printf 'Skipping PostgreSQL integration tests: NUBLOX_POSTGRES_CONNECTION_STRING is not set.\n'
fi
