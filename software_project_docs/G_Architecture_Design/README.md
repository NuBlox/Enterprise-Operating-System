# NuBlox Architecture & Design — Controlled Index

This directory contains the live controlled **Architecture & Design** documents for the NuBlox Enterprise Operating System programme.

Templates remain under [`../../software_project_docs_templates/G_Architecture_Design/`](../../software_project_docs_templates/G_Architecture_Design/).

## Current Draft architecture baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-G-001` | [Architecture vision](Architecture_vision.md) | Draft | 0.1 |
| `NBEOS-G-002` | [Architecture definition](Architecture_definition.md) | Draft | 0.1 |
| `NBEOS-G-003` | [System context diagram](System_context_diagram.md) | Draft | 0.1 |
| `NBEOS-G-004` | [Solution architecture document](Solution_architecture_document.md) | Draft | 0.1 |
| `NBEOS-G-005` | [Architecture decision records (ADR register)](Architecture_decision_records_ADRs.md) | Draft | 0.5 |
| `NBEOS-G-006` | [High-level design (HLD)](High-level_design_HLD.md) | Draft | 0.1 |
| `NBEOS-G-007` | [Container diagram](Container_diagram.md) | Draft | 0.1 |
| `NBEOS-G-008` | [Data architecture](Data_architecture.md) | Draft | 0.1 |
| `NBEOS-G-009` | [Integration design](Integration_design.md) | Draft | 0.1 |
| `NBEOS-G-010` | [Security architecture](Security_architecture.md) | Draft | 0.1 |
| `NBEOS-G-011` | [Data migration design](Data_migration_design.md) | Draft | 0.1 |
| `NBEOS-G-012` | [Evaluation matrix for technology selection](Evaluation_matrix_for_technology_selection.md) | Draft | 0.1 |
| `NBEOS-G-013` | [Technical spikes](Technical_spikes.md) | Draft | 0.1 |
| `NBEOS-G-014` | [Proof of concept report](Proof_of_concept_report.md) | Draft | 0.6 |

## Accepted production-foundation decisions

The implementation foundation is now explicit across application shape, runtime, identity, API, data, operations and release evolution:

| ADR | Decision |
|---|---|
| [`ADR-001`](adr/ADR-001-cohesive-modular-application.md) | Cohesive modular application with explicit module/data ownership |
| [`ADR-002`](adr/ADR-002-transactional-relational-primary-persistence.md) | Transactional relational persistence as the authoritative persistence model |
| [`ADR-007`](adr/ADR-007-layered-tenant-isolation.md) | Tenant-aware logical model with layered isolation and shared/dedicated profiles |
| [`ADR-008`](adr/ADR-008-federated-application-identity.md) | Federated application Principal separate from Person; OIDC-primary human federation and service identities |
| [`ADR-011`](adr/ADR-011-http-api-standards.md) | HTTPS/JSON/OpenAPI remote API standard with governed errors/versioning/idempotency |
| [`ADR-012`](adr/ADR-012-dotnet10-server-runtime.md) | .NET 10 LTS / C# server/core runtime; ASP.NET Core default server HTTP framework |
| [`ADR-016`](adr/ADR-016-opentelemetry-observability-baseline.md) | OpenTelemetry traces/metrics/logs and OTLP preferred export boundary |
| [`ADR-017`](adr/ADR-017-module-owned-data-boundaries.md) | Module-owned authoritative data/persistence and explicit cross-module contracts |
| [`ADR-018`](adr/ADR-018-business-audit-evidence.md) | Append-oriented authoritative business audit/evidence separate from technical telemetry |
| [`ADR-020`](adr/ADR-020-release-schema-configuration-evolution.md) | Expand/migrate/contract release evolution with controlled migration journal and recovery strategy |
| [`ADR-021`](adr/ADR-021-postgresql18-primary-provider.md) | PostgreSQL 18 initial primary provider; Npgsql 10.0.3 .NET provider baseline |

Together these establish:

```text
cohesive modular application
+ .NET 10 LTS / C# / ASP.NET Core
+ federated Principal + verified Tenant context
+ governed HTTPS/JSON/OpenAPI contracts
+ module-owned relational data
+ PostgreSQL 18 / Npgsql
+ layered tenant isolation
+ append-oriented business audit evidence
+ OpenTelemetry / OTLP technical observability
+ expand/migrate/contract schema/configuration evolution
```

No identity-provider vendor, frontend framework, cloud provider or monitoring backend/vendor is selected by these decisions.

## Production implementation status

The first real production code path exists outside `spikes/`:

```text
src/NuBlox.Kernel/
tests/NuBlox.Kernel.Tests/
scripts/verify-production.sh
.github/workflows/production-foundation.yml
```

The current production foundation is built/tested on the pinned .NET 10 SDK. With the foundation ADRs accepted, implementation can now expand in controlled parallel slices:

```text
ASP.NET Core API host / contract infrastructure
Principal + Tenant request context
PostgreSQL persistence + migrations + tenant isolation
business audit + OpenTelemetry primitives
CI integration tests / quality gates
```

These platform slices must remain semantic-light until the first approved business workflow is selected from controlled requirements.

## Provider position

PostgreSQL 18 is the initial production database provider because the complete NuBlox architecture-spike programme already exercised its transactions, tenant row security, effective-history constraints, migrations and reporting patterns.

`packages/mastered/mysql` remains a governed NuBlox asset. It may serve integrations/tooling or future explicitly approved MySQL product support; it does not change the initial PostgreSQL production provider decision.

## Foundation experiment evidence

The disposable architecture experiment remains under [`../../spikes/foundation-architecture/`](../../spikes/foundation-architecture/).

`SPIKE-001` through `SPIKE-009` are complete and remain evidence rather than production source. The [proof-of-concept report](Proof_of_concept_report.md) records exact CI evidence, findings and limitations.

## Decisions still intentionally open/deferred

The production platform foundation no longer has an unresolved P0 architecture decision in the current backlog, but important product/domain decisions remain evidence-driven:

- `ADR-003` durable async processing — promoted when the first production async/integration slice is built;
- `ADR-004` binary content storage — promoted when work-product content is implemented;
- `ADR-005` configuration/extensibility — promoted with governed tenant variation;
- `ADR-006` information authority — promoted with concrete integration boundaries;
- `ADR-009` business authority — promoted with the first decision/approval workflow;
- `ADR-010` workflow/orchestration — promoted when validated processes justify it;
- `ADR-013` cloud/deployment provider — remains deferred pending residency/resilience/economics;
- `ADR-014` search — remains deferred pending validated search workload;
- `ADR-015` analytics — promoted when operational reporting no longer satisfies workload/retention needs;
- `ADR-019` migration architecture — promoted with full product/legacy migration capability.

Identity-provider vendor, hosting topology, frontend framework and monitoring backend remain deployment/product decisions behind stable architectural boundaries.

## Development handoff

The controlled implementation sequence is maintained under [`../H_Development_Implementation/`](../H_Development_Implementation/).

All current P0 architecture gates for the production platform foundation are closed. `DEV-104`, `DEV-105`, `DEV-106` and `DEV-108` can now proceed, followed by consolidation through `DEV-107` CI quality gates.

## Architecture gate

Architecture decisions may be accepted when:

- decision scope is explicit;
- requirements/NFRs/constraints are traced;
- credible alternatives are considered;
- material security/data/operational implications are understood;
- migration/reversibility consequences are recorded;
- spikes/benchmarks support high-risk assumptions where appropriate;
- the accountable NuBlox architecture governance role accepts the decision.

Accepted ADRs remain reviewable when their stated review triggers occur.
