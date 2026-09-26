# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.4 |
| `NBEOS-H-003` | [Dependency management document](Dependency_management_document.md) | Draft | 0.1 |

## Current production foundation

The first production code path is now physically separate from the disposable architecture spikes:

```text
src/NuBlox.Kernel/
        ↓
tests/NuBlox.Kernel.Tests/
        ↓
scripts/verify-production.sh
        ↓
.github/workflows/production-foundation.yml
```

The repository root pins the approved .NET SDK through `global.json`, defines shared production build rules through `Directory.Build.props`, and centrally controls direct NuGet package versions through `Directory.Packages.props`.

The first production kernel intentionally contains no synthetic spike business model, persistence-provider dependency, identity model or API contract. Those capabilities remain behind their own controlled decisions.

## Completed entry controls

The current implementation baseline has completed:

- `ADR-001` cohesive modular application decision;
- `ADR-002` transactional relational primary persistence-model decision;
- `ADR-007` layered tenant-isolation decision;
- `ADR-012` .NET 10 LTS / C# server/core runtime decision;
- `DEV-101` production source/test separation;
- `DEV-102` deterministic restore/build/test entrypoint;
- `DEV-103` initial dependency/provenance controls.

Production verification runs with:

```bash
bash scripts/verify-production.sh
```

and the identical command is enforced in the `Production foundation` GitHub Actions workflow.

## Remaining production-foundation decisions

The next P0 decisions are:

1. `ADR-008` identity/authentication and service identity;
2. `ADR-011` API standards;
3. `ADR-017` schema/data modularity;
4. `ADR-016` observability baseline;
5. `ADR-020` release/configuration/schema evolution;
6. relational database product/provider under accepted `ADR-002`.

These decisions unblock persistence, identity/context, audit/telemetry and the complete production CI quality gate.

## Development rule

The spike code remains disposable experimental code. It is not the production application and must not be copied into the production codebase by momentum. Promotion of a proven pattern requires the relevant ADR decision, traceability to requirements, a controlled implementation plan, review and normal build/test/security controls.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../G_Architecture_Design/Technical_spikes.md`](../G_Architecture_Design/Technical_spikes.md)
- [`../G_Architecture_Design/Proof_of_concept_report.md`](../G_Architecture_Design/Proof_of_concept_report.md)
- [`../F_Requirements_Analysis/`](../F_Requirements_Analysis/)
