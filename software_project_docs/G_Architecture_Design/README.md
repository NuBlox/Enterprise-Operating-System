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
| `NBEOS-G-005` | [Architecture decision records (ADR register)](Architecture_decision_records_ADRs.md) | Draft | 0.4 |
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

The initial implementation foundation now has seven accepted decisions:

| ADR | Decision |
|---|---|
| [`ADR-001`](adr/ADR-001-cohesive-modular-application.md) | Cohesive modular application with explicit module/data ownership; independent services require later evidence |
| [`ADR-002`](adr/ADR-002-transactional-relational-primary-persistence.md) | Transactional relational persistence is the primary authoritative model; database provider remains separate |
| [`ADR-007`](adr/ADR-007-layered-tenant-isolation.md) | One tenant-aware logical model with layered enforcement; shared database/schema default plus dedicated-database profile when justified |
| [`ADR-008`](adr/ADR-008-federated-application-identity.md) | Enterprise identity remains separate from application Principal; OIDC-primary federation and governed service identities |
| [`ADR-011`](adr/ADR-011-http-api-standards.md) | HTTPS/JSON/OpenAPI remote APIs with explicit major versions, RFC 9457 errors, server-side context/security and idempotency/compatibility rules |
| [`ADR-012`](adr/ADR-012-dotnet10-server-runtime.md) | .NET 10 LTS / C# server/core runtime; ASP.NET Core default server HTTP framework |
| [`ADR-017`](adr/ADR-017-module-owned-data-boundaries.md) | Module-owned authoritative data/persistence; cross-module behaviour through explicit contracts and provider-specific details behind infrastructure boundaries |

Together they establish:

```text
cohesive modular application
+ explicit module/data ownership
+ transactional relational persistence
+ layered tenant isolation
+ federated application Principal model
+ .NET 10 LTS / C# server/core
+ governed HTTPS/JSON/OpenAPI remote contracts
```

They do **not** yet select the relational database product, identity-provider vendor, frontend framework, cloud provider or observability vendor.

## Production implementation status

The first real production code path now exists outside `spikes/`:

```text
src/NuBlox.Kernel/
tests/NuBlox.Kernel.Tests/
scripts/verify-production.sh
.github/workflows/production-foundation.yml
```

The production foundation is built and tested on the pinned .NET 10 SDK and is governed through the H-section Development & Implementation documents.

## Foundation experiment evidence

The disposable architecture experiment remains under [`../../spikes/foundation-architecture/`](../../spikes/foundation-architecture/).

`SPIKE-001` through `SPIKE-009` are complete and remain evidence rather than production source.

The [proof-of-concept report](Proof_of_concept_report.md) records the exact CI evidence, findings and limitations.

## Architecture decisions requiring resolution next

The remaining near-term production-foundation gates are:

1. `ADR-016` production observability baseline;
2. `ADR-020` release/configuration/schema evolution;
3. relational database product/provider selection under ADR-002/ADR-007/ADR-017;
4. `ADR-018` audit/evidence design as production audit capability begins.

Identity-provider vendor selection is an environment/deployment/procurement decision behind accepted ADR-008 and does not block provider-neutral Principal/context implementation.

Additional domain/process decisions such as `ADR-003`, `ADR-005`, `ADR-009`, `ADR-010`, `ADR-015` and `ADR-019` are promoted as the relevant product slices require them.

## Development handoff

The controlled transition into implementation is maintained under [`../H_Development_Implementation/`](../H_Development_Implementation/).

Accepted ADR-008 and ADR-011 now permit provider-neutral Principal/Tenant request-context abstractions and an ASP.NET Core API host to be added without yet committing to a specific hosted identity-provider product.

Accepted ADR-017 removes the schema-modularity blocker from the production persistence scaffold; provider selection and release/schema-evolution rules remain before authoritative production schema work.

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
