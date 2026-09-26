# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.12 |
| `NBEOS-H-003` | [Dependency management document](Dependency_management_document.md) | Draft | 0.4 |
| `NBEOS-H-004` | [First vertical slice selection and validation plan](First_vertical_slice_selection.md) | Draft — Candidate for validation | 0.1 |

## Production foundation

Wave 1 of the controlled production foundation is complete. Production code is physically separated from disposable architecture spikes and organised into explicit boundaries:

```text
src/NuBlox.Kernel/
src/NuBlox.Identity/
src/NuBlox.Audit/
src/NuBlox.Observability/
src/NuBlox.Persistence.PostgreSql/
src/NuBlox.Api/
        ↓
corresponding automated test projects
        ↓
scripts/verify-production.sh
        ↓
.github/workflows/production-foundation.yml
```

The repository root pins the approved .NET SDK through `global.json`, defines shared production build/analyser rules through `Directory.Build.props`, and centrally controls direct NuGet package versions through `Directory.Packages.props`.

The integrated production gate verifies the production boundaries together against PostgreSQL 18.6 in CI. The DEV-108 final implementation evidence passed 33 tests with zero failed tests and zero production build warnings/errors.

## Completed Wave 0 architecture gates

The implementation-critical architecture decisions required for the production foundation are explicit, including:

- `ADR-001` cohesive modular application;
- `ADR-002` transactional relational primary persistence;
- `ADR-007` layered tenant isolation;
- `ADR-008` federated/provider-neutral application identity boundary;
- `ADR-011` HTTP API standards;
- `ADR-012` .NET 10 LTS / C# server runtime;
- `ADR-016` OpenTelemetry observability baseline;
- `ADR-017` module-owned data boundaries;
- `ADR-018` authoritative business audit/evidence model;
- `ADR-020` release/schema/configuration evolution;
- `ADR-021` PostgreSQL 18 / Npgsql initial production provider.

## Completed Wave 1 implementation controls

The production engineering foundation has completed:

- `DEV-101` source/test separation;
- `DEV-102` deterministic restore/build/test entrypoint;
- `DEV-103` dependency/provenance controls;
- `DEV-104` PostgreSQL migration, checksum, RLS and restricted-runtime verification;
- `DEV-105` provider-neutral Principal/Tenant verified context boundary;
- `DEV-106` append-oriented audit evidence and OpenTelemetry primitives;
- `DEV-107` integrated production CI quality gate;
- `DEV-108` ASP.NET Core API host, OpenAPI, RFC 9457 and operational health infrastructure.

Production verification runs with:

```bash
bash scripts/verify-production.sh
```

GitHub Actions supplies PostgreSQL 18.6 and executes the same verifier with the persistence integration leg enabled.

## Current Wave 2 gate

`DEV-201` is now **In Progress**.

The controlled candidate for the first representative vertical workflow is **governed work-product review and issue**, documented in [`First_vertical_slice_selection.md`](First_vertical_slice_selection.md).

This is a **candidate for primary validation, not an approved production workflow**. The BRD, stakeholder requirements, SRS and functional requirements remain Draft/Candidate, and `NBEOS-A-005` states that primary customer discovery is not yet sufficient for product approval.

Accordingly:

```text
candidate workflow selected
        ↓
primary customer/process validation
        ↓
approved initial scope, actors, information subjects, rules and acceptance criteria
        ↓
DEV-201 complete
        ↓
DEV-202–DEV-208 production business vertical slice
```

Production business semantics must not be implemented merely because the platform foundation is technically ready.

## Development rule

Architecture-spike code remains disposable experimental evidence. It must not be copied into production by momentum. Product business semantics must trace to sufficiently validated requirements and acceptance evidence; platform capabilities remain behind their explicit ADR and project boundaries.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../G_Architecture_Design/Technical_spikes.md`](../G_Architecture_Design/Technical_spikes.md)
- [`../G_Architecture_Design/Proof_of_concept_report.md`](../G_Architecture_Design/Proof_of_concept_report.md)
- [`../F_Requirements_Analysis/`](../F_Requirements_Analysis/)
- [`../A_Enterprise_Pre_Project/Customer_research_summary.md`](../A_Enterprise_Pre_Project/Customer_research_summary.md)
- [`Product_backlog.md`](Product_backlog.md)
- [`First_vertical_slice_selection.md`](First_vertical_slice_selection.md)
