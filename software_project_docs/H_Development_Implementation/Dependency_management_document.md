# Dependency management document

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-003  
**Document Type:** Dependency management document  
**Version:** 0.3  
**Status:** Draft  
**Author / Owner:** NuBlox Engineering  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Development_plan.md`, `Product_backlog.md`, `../G_Architecture_Design/adr/ADR-012-dotnet10-server-runtime.md`, `../G_Architecture_Design/adr/ADR-016-opentelemetry-observability-baseline.md`, `../G_Architecture_Design/adr/ADR-021-postgresql18-primary-provider.md`  
**Supersedes:** Version 0.2  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Dependency_management_document.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Dependency_management_document.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the production dependency and toolchain controls for the NuBlox server/core codebase. Dependencies are introduced deliberately, centrally versioned where practical and kept separate from the disposable architecture-spike dependency graph.

## Production dependency principles

1. Prefer the .NET shared framework and standard library before adding third-party packages.
2. Every direct production dependency requires an explicit purpose, owner, version policy, provenance and licence/security review path.
3. Package versions are controlled centrally through `Directory.Packages.props` where PackageReference is used.
4. Test-only dependencies remain test-only and must not become transitive runtime dependencies.
5. A package used by `spikes/` is not automatically approved for production use.
6. A separately governed NuBlox package/repository remains subject to product-adoption and architecture decisions.
7. Preview dependencies are prohibited in the production foundation unless explicitly approved.
8. Dependency upgrades are verified through the same production build/test path as code changes.
9. Database-provider packages remain isolated to provider-specific persistence/infrastructure projects.
10. Observability instrumentation remains based on standard .NET diagnostics/OpenTelemetry contracts rather than vendor-specific telemetry APIs.

## Production toolchain and direct dependency inventory

| Dependency / tool | Version | Scope | Source / provenance | Licence | Purpose |
|---|---:|---|---|---|---|
| .NET SDK | `10.0.401` | Build toolchain | Microsoft .NET official distribution | MIT / Microsoft component licences as applicable | Approved server/core SDK baseline under ADR-012 |
| .NET target framework | `net10.0` | Runtime contract | Microsoft .NET shared framework | MIT / Microsoft component licences as applicable | Production server/core target |
| `Npgsql` | `10.0.3` | PostgreSQL persistence infrastructure only | NuGet package published by the Npgsql project | PostgreSQL | Approved .NET PostgreSQL provider baseline under ADR-021 |
| `OpenTelemetry` | `1.19.1` | `NuBlox.Observability` only | NuGet package published by the OpenTelemetry project | Apache-2.0 | OpenTelemetry SDK registration for NuBlox trace/metric instrumentation under ADR-016 |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.12` | `NuBlox.Observability` only | Microsoft NuGet package | MIT | Standard `ILogger` correlation-scope contract without a logging vendor dependency |
| `Microsoft.NET.Test.Sdk` | `18.10.1` | Test only | NuGet package owned by Microsoft | MIT | Integrates production test projects with `dotnet test` / test platform |
| `MSTest.TestAdapter` | `4.4.1` | Test only | NuGet package owned by Microsoft / MSTest | MIT | Test discovery/execution adapter |
| `MSTest.TestFramework` | `4.4.1` | Test only | NuGet package owned by Microsoft / MSTest | MIT | Unit/integration-test framework |

`NuBlox.Kernel` retains **no third-party runtime PackageReference**. `NuBlox.Audit` also remains provider-neutral and has no OpenTelemetry or database-provider dependency. Npgsql is referenced only by `NuBlox.Persistence.PostgreSql`. OpenTelemetry and logging abstractions are confined to `NuBlox.Observability` and may flow transitively only to its verification project.

## Version controls

### SDK

`global.json` pins the production SDK feature band to `10.0.401` and permits only later patches in that band through `latestPatch`. SDK changes require the production verifier and CI to pass.

### NuGet packages

`Directory.Packages.props` is the authoritative direct-package version list for production/test projects using central package management.

Project files reference package names without local version attributes. This prevents individual projects silently drifting to different versions.

### PostgreSQL provider

ADR-021 accepts PostgreSQL 18 as the initial production primary relational provider and Npgsql `10.0.3` as the initial .NET provider baseline.

The provider dependency is intentionally isolated behind the PostgreSQL persistence project. Provider-native SQL, migrations, row-level security and Npgsql APIs must not leak into `NuBlox.Kernel` or domain/application contracts.

### Observability baseline

ADR-016 accepts OpenTelemetry as the production telemetry interoperability standard and OTLP as the preferred export boundary.

DEV-106 introduces the stable OpenTelemetry `1.19.1` SDK only where NuBlox instrumentation is registered. The observability project exposes standard `ActivitySource`, `Meter` and `ILogger` correlation primitives and does not select a monitoring backend.

An OTLP exporter/collector configuration is intentionally not embedded in the platform library. Host-level exporter configuration is introduced with the production host/deployment path and remains centrally versioned when adopted.

### Package locking

Package lock files are not yet the production baseline. Before the dependency graph becomes material, NuBlox must choose and document whether restore locking is enforced through `packages.lock.json`, repository-level dependency graph verification or another reproducible restore control.

Until that decision is implemented:

- direct versions remain exact and centrally controlled;
- CI performs a fresh restore on each verification run;
- dependency/provenance changes are reviewed as code changes.

## Governed NuBlox packages

The MySQL driver previously mastered inside this repository has been extracted to the separately governed `NuBlox/NuBloxSQL` repository. It is not a dependency of the Enterprise Operating System production foundation.

ADR-021 selects PostgreSQL 18 as the initial Enterprise Operating System primary provider. NuBloxSQL may support integrations, tooling or a later separately approved provider implementation, but its existence does not imply Enterprise Operating System product adoption.

## Adding a dependency

A pull request adding or materially changing a direct dependency must record:

- package/tool name;
- required capability and owning NuBlox component;
- selected exact version or governed version policy;
- upstream publisher/source;
- licence;
- known support lifecycle;
- security/vulnerability review method;
- whether an existing governed NuBlox package covers the need;
- exit/replacement implications for architecture-significant dependencies.

## Update and vulnerability process

The production foundation should evolve toward automated dependency update and vulnerability reporting. Until dedicated automation is established, dependency versions are reviewed through repository changes and current vendor/package information is revalidated before material upgrades.

Security fixes may be expedited but still require the production verifier to pass before merge unless an incident process explicitly authorises otherwise.

## Verification

The production dependency baseline is verified by:

```bash
bash scripts/verify-production.sh
```

The command restores/builds/tests the production Kernel, Identity, Audit and Observability boundaries. When `NUBLOX_POSTGRES_CONNECTION_STRING` is set, it also runs the PostgreSQL persistence integration suite. GitHub Actions supplies PostgreSQL 18.6 and the test connection string automatically.

## References

- `Development_plan.md`
- `Product_backlog.md`
- `../G_Architecture_Design/adr/ADR-012-dotnet10-server-runtime.md`
- `../G_Architecture_Design/adr/ADR-016-opentelemetry-observability-baseline.md`
- `../G_Architecture_Design/adr/ADR-021-postgresql18-primary-provider.md`
- https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- https://www.nuget.org/packages/Npgsql
- https://www.nuget.org/packages/OpenTelemetry
- https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions
- https://www.nuget.org/packages/Microsoft.NET.Test.Sdk
- https://www.nuget.org/packages/MSTest.TestAdapter
- https://www.nuget.org/packages/MSTest.TestFramework

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Engineering | Established the first production SDK, test dependency and central package-version controls |
| 0.2 | 2026-09-26 | NuBlox Engineering | Added the ADR-021 Npgsql production provider dependency and PostgreSQL integration-verification boundary |
| 0.3 | 2026-09-26 | NuBlox Engineering | Added OpenTelemetry 1.19.1 and logging abstractions for the vendor-neutral DEV-106 observability boundary; aligned MySQL ownership with NuBloxSQL extraction |
