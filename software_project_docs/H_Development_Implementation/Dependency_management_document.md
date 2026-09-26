# Dependency management document

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-003  
**Document Type:** Dependency management document  
**Version:** 0.2  
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
**Related Documents:** `Development_plan.md`, `Product_backlog.md`, `../G_Architecture_Design/adr/ADR-012-dotnet10-server-runtime.md`, `../G_Architecture_Design/adr/ADR-021-postgresql18-primary-provider.md`  
**Supersedes:** Version 0.1  
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
6. A package under `packages/mastered/` is a governed NuBlox asset, but product adoption still requires a product need and compatible architecture decision.
7. Preview dependencies are prohibited in the production foundation unless explicitly approved.
8. Dependency upgrades are verified through the same production build/test path as code changes.
9. Database-provider packages remain isolated to provider-specific persistence/infrastructure projects.

## Production toolchain and direct dependency inventory

| Dependency / tool | Version | Scope | Source / provenance | Licence | Purpose |
|---|---:|---|---|---|---|
| .NET SDK | `10.0.401` | Build toolchain | Microsoft .NET official distribution | MIT / Microsoft component licences as applicable | Approved server/core SDK baseline under ADR-012 |
| .NET target framework | `net10.0` | Runtime contract | Microsoft .NET shared framework | MIT / Microsoft component licences as applicable | Production server/core target |
| `Npgsql` | `10.0.3` | PostgreSQL persistence infrastructure only | NuGet package published by the Npgsql project | PostgreSQL | Approved .NET PostgreSQL provider baseline under ADR-021 |
| `Microsoft.NET.Test.Sdk` | `18.10.1` | Test only | NuGet package owned by Microsoft | MIT | Integrates production test projects with `dotnet test` / test platform |
| `MSTest.TestAdapter` | `4.4.1` | Test only | NuGet package owned by Microsoft / MSTest | MIT | Test discovery/execution adapter |
| `MSTest.TestFramework` | `4.4.1` | Test only | NuGet package owned by Microsoft / MSTest | MIT | Unit/integration-test framework |

`NuBlox.Kernel` retains **no third-party runtime PackageReference**. Npgsql is referenced only by `NuBlox.Persistence.PostgreSql` and may flow transitively to persistence integration tests; domain/application/kernel projects must not reference it directly.

## Version controls

### SDK

`global.json` pins the production SDK feature band to `10.0.401` and permits only later patches in that band through `latestPatch`. SDK changes require the production verifier and CI to pass.

### NuGet packages

`Directory.Packages.props` is the authoritative direct-package version list for production/test projects using central package management.

Project files reference package names without local version attributes. This prevents individual projects silently drifting to different versions.

### PostgreSQL provider

ADR-021 accepts PostgreSQL 18 as the initial production primary relational provider and Npgsql `10.0.3` as the initial .NET provider baseline.

The provider dependency is intentionally isolated behind the PostgreSQL persistence project. Provider-native SQL, migrations, row-level security and Npgsql APIs must not leak into `NuBlox.Kernel` or future domain/application contracts.

### Package locking

Package lock files are not yet the production baseline. Before the dependency graph becomes material, NuBlox must choose and document whether restore locking is enforced through `packages.lock.json`, repository-level dependency graph verification or another reproducible restore control.

Until that decision is implemented:

- direct versions remain exact and centrally controlled;
- CI performs a fresh restore on each verification run;
- dependency/provenance changes are reviewed as code changes.

## Mastered packages

`packages/mastered/mysql` is maintained separately as a NuBlox-mastered package with its own upstream provenance and synchronisation controls.

It is not a dependency of the .NET PostgreSQL persistence foundation. ADR-021 explicitly selects PostgreSQL 18 as the initial primary provider while retaining the mastered MySQL package for integrations, tooling or a later separately approved provider implementation.

## Adding a dependency

A pull request adding or materially changing a direct dependency must record:

- package/tool name;
- required capability and owning NuBlox component;
- selected exact version or governed version policy;
- upstream publisher/source;
- licence;
- known support lifecycle;
- security/vulnerability review method;
- whether an existing NuBlox-mastered package covers the need;
- exit/replacement implications for architecture-significant dependencies.

## Update and vulnerability process

The production foundation should evolve toward automated dependency update and vulnerability reporting. Until dedicated automation is established, dependency versions are reviewed through repository changes and current vendor/package information is revalidated before material upgrades.

Security fixes may be expedited but still require the production verifier to pass before merge unless an incident process explicitly authorises otherwise.

## Verification

The production dependency baseline is verified by:

```bash
bash scripts/verify-production.sh
```

When `NUBLOX_POSTGRES_CONNECTION_STRING` is set, the same command also restores/builds and runs the PostgreSQL persistence integration suite. GitHub Actions supplies PostgreSQL 18.6 and the test connection string automatically.

## References

- `Development_plan.md`
- `Product_backlog.md`
- `../G_Architecture_Design/adr/ADR-012-dotnet10-server-runtime.md`
- `../G_Architecture_Design/adr/ADR-021-postgresql18-primary-provider.md`
- `../../packages/mastered/mysql/` where applicable
- https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- https://www.nuget.org/packages/Npgsql
- https://www.nuget.org/packages/Microsoft.NET.Test.Sdk
- https://www.nuget.org/packages/MSTest.TestAdapter
- https://www.nuget.org/packages/MSTest.TestFramework

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Engineering | Established the first production SDK, test dependency and central package-version controls |
| 0.2 | 2026-09-26 | NuBlox Engineering | Added the ADR-021 Npgsql production provider dependency and PostgreSQL integration-verification boundary |
