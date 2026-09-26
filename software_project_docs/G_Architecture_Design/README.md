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
| `NBEOS-G-005` | [Architecture decision records (ADR register)](Architecture_decision_records_ADRs.md) | Draft | 0.3 |
| `NBEOS-G-006` | [High-level design (HLD)](High-level_design_HLD.md) | Draft | 0.1 |
| `NBEOS-G-007` | [Container diagram](Container_diagram.md) | Draft | 0.1 |
| `NBEOS-G-008` | [Data architecture](Data_architecture.md) | Draft | 0.1 |
| `NBEOS-G-009` | [Integration design](Integration_design.md) | Draft | 0.1 |
| `NBEOS-G-010` | [Security architecture](Security_architecture.md) | Draft | 0.1 |
| `NBEOS-G-011` | [Data migration design](Data_migration_design.md) | Draft | 0.1 |
| `NBEOS-G-012` | [Evaluation matrix for technology selection](Evaluation_matrix_for_technology_selection.md) | Draft | 0.1 |
| `NBEOS-G-013` | [Technical spikes](Technical_spikes.md) | Draft | 0.1 |
| `NBEOS-G-014` | [Proof of concept report](Proof_of_concept_report.md) | Draft | 0.6 |

## Accepted architecture foundation

Four architecture decisions are now accepted as the initial implementation foundation:

| ADR | Decision |
|---|---|
| [`ADR-001`](adr/ADR-001-cohesive-modular-application.md) | NuBlox begins as a cohesive modular application with explicit module/data ownership; independent services require later evidence |
| [`ADR-002`](adr/ADR-002-transactional-relational-primary-persistence.md) | Transactional relational persistence is the primary authoritative model; database vendor/provider remains a separate decision |
| [`ADR-007`](adr/ADR-007-layered-tenant-isolation.md) | One tenant-aware logical data model with layered enforcement; shared database/schema is the default profile and dedicated database is available when justified |
| [`ADR-012`](adr/ADR-012-dotnet10-server-runtime.md) | .NET 10 LTS / C# is the production server/core runtime; ASP.NET Core is the default server HTTP framework; frontend remains a separate decision |

Together they establish:

```text
cohesive modular application
+ explicit module responsibility boundaries
+ transactional relational persistence
+ layered tenant-aware data isolation
+ controlled physical isolation profiles
+ .NET 10 LTS / C# server and core runtime
```

They do **not** yet select the relational database product, identity provider, frontend framework, cloud provider or observability vendor.

## Foundation experiment evidence

The disposable architecture experiment is maintained under:

[`../../spikes/foundation-architecture/`](../../spikes/foundation-architecture/)

`SPIKE-001` through `SPIKE-009` are complete. The experiment demonstrated modular transactions, effective history, shared-schema row isolation, business authority, durable asynchronous work, typed configuration, migration staging/reconciliation, operational reporting/drill-through, reproducible build/migration execution, structured correlation logging and stranded-work recovery.

The [proof-of-concept report](Proof_of_concept_report.md) records the exact CI evidence, findings and limitations.

Passing experiments remain bounded evidence. Production code must be implemented separately under the Development & Implementation controls.

## Architecture decisions requiring resolution next

With the runtime decision accepted, the next production-foundation gates are:

1. `ADR-008` identity/authentication and service identity;
2. `ADR-011` internal/external API standards;
3. `ADR-017` schema/data modularity and cross-module persistence controls;
4. `ADR-016` observability baseline;
5. `ADR-020` release/configuration/schema evolution;
6. relational database product/provider selection under the accepted ADR-002 model.

Additional domain/process decisions such as `ADR-003`, `ADR-005`, `ADR-009`, `ADR-010`, `ADR-015`, `ADR-018` and `ADR-019` are promoted as the relevant product slices require them.

## Development handoff

The controlled transition from architecture evidence into implementation is maintained under:

[`../H_Development_Implementation/`](../H_Development_Implementation/)

The accepted runtime decision is sufficient to begin the production source/test/build scaffold while persistence-provider, identity, API and other affected capabilities remain blocked behind their own decisions.

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
