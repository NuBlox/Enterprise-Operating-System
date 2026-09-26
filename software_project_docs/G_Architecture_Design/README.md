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
| `NBEOS-G-005` | [Architecture decision records (ADR register)](Architecture_decision_records_ADRs.md) | Draft | 0.1 |
| `NBEOS-G-006` | [High-level design (HLD)](High-level_design_HLD.md) | Draft | 0.1 |
| `NBEOS-G-007` | [Container diagram](Container_diagram.md) | Draft | 0.1 |
| `NBEOS-G-008` | [Data architecture](Data_architecture.md) | Draft | 0.1 |
| `NBEOS-G-009` | [Integration design](Integration_design.md) | Draft | 0.1 |
| `NBEOS-G-010` | [Security architecture](Security_architecture.md) | Draft | 0.1 |
| `NBEOS-G-011` | [Data migration design](Data_migration_design.md) | Draft | 0.1 |
| `NBEOS-G-012` | [Evaluation matrix for technology selection](Evaluation_matrix_for_technology_selection.md) | Draft | 0.1 |
| `NBEOS-G-013` | [Technical spikes](Technical_spikes.md) | Draft | 0.1 |
| `NBEOS-G-014` | [Proof of concept report](Proof_of_concept_report.md) | Draft | 0.5 |

## Current architecture hypothesis

The leading first-implementation hypothesis is:

```text
cohesive modular application
+ explicit internal responsibility boundaries
+ transactional relational persistence
+ durable asynchronous processing for external/long-running work
+ explicit integration adapters
+ separate binary/object storage when required
+ governed configuration rather than customer forks
```

This remains provisional. Spike success provides evidence; it does not silently approve the production architecture.

## Foundation experiment evidence

The active disposable experiment is:

[`../../spikes/foundation-architecture/`](../../spikes/foundation-architecture/)

Its independent .NET 10/PostgreSQL 18 checks cover modular transactions,
effective history, customer row isolation, business authority, a simulated
durable outbox, typed configuration and synthetic migration. SPIKE-008 adds
two operational measures, source drill-through, isolation checks and query
plans at synthetic volume on [PR #1](https://github.com/NuBlox/Enterprise-Operating-System/pull/1).
Its [workflow run 36265786034](https://github.com/NuBlox/Enterprise-Operating-System/actions/runs/36265786034)
passed on the tested commit.

The [proof-of-concept report](Proof_of_concept_report.md) records the exact
workflow runs, findings and limits. Passing experiments remain evidence for
proposed ADRs, not approved production decisions.

## Architecture decisions requiring evidence next

Highest-priority remaining decisions/spikes include:

1. `ADR-007` isolation options and privileged/support access beyond the tested shared-schema candidate;
2. `ADR-008` identity/authentication boundary;
3. `ADR-010` long-running workflow/orchestration approach;
4. `ADR-015` historical work-state reporting, published snapshots and realistic volume;
5. `ADR-019` representative source-data migration and scale;
6. `SPIKE-009` reproducible build, testing and operability review.

## Next architecture documents/evidence

Use the supplied templates as evidence requires, including:

- Data model / ERD;
- Database design;
- Component diagram;
- Threat model;
- Identity and access management design;
- Privacy-by-design assessment;
- API specification/versioning strategy;
- Deployment/infrastructure design;
- observability/operations architecture;
- individual spike result updates and ADR decisions.

## Architecture gate

Architecture may move toward approval only when:

- the selected initial workflow/customer scope is sufficiently validated;
- material NFR/isolation/security assumptions are explicit;
- high-risk decisions have spike/benchmark evidence where necessary;
- architecture traces to the requirements baseline;
- migration, security, operations and test implications are credible;
- technology choice follows the requirements/option analysis rather than becoming the product model.

The successful foundation spike satisfies part of this gate only. It does **not** approve the full production architecture.
