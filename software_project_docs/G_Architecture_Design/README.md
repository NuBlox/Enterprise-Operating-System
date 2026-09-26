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
| `NBEOS-G-014` | [Proof of concept report](Proof_of_concept_report.md) | Draft | 0.1 |

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

## Verified foundation evidence

The active disposable experiment is:

[`../../spikes/foundation-architecture/`](../../spikes/foundation-architecture/)

It currently demonstrates a generic governed-work transaction across explicit Subjects, Work, Decisions and Audit module boundaries using .NET 10 and PostgreSQL 18.

GitHub Actions workflow `.github/workflows/foundation-spike.yml` verifies the spike against PostgreSQL 18.

### CI run 36256385149

Commit tested: `6bb6d0595dd8f50853416de439e1fed96d611594`

Result: **PASS**

Verified in CI:

- PostgreSQL 18 service startup;
- versioned migration application;
- .NET 10 restore/build for the API harness;
- .NET 10 restore/build for the verification harness;
- atomic commit of subject/work/decision/audit records;
- database rejection of a cross-customer relationship;
- explicit rollback leaving no partial subject/work state.

The evidence and its limitations are recorded in [`Proof_of_concept_report.md`](Proof_of_concept_report.md).

## Architecture decisions requiring evidence next

Highest-priority remaining decisions/spikes include:

1. `ADR-002` historical/effective-dated persistence behaviour;
2. `ADR-003` durable async/outbox mechanism;
3. `ADR-007` customer/tenant isolation options beyond the current composite-key precursor;
4. `ADR-008` identity/authentication boundary;
5. `ADR-009` permission vs business authority enforcement;
6. `ADR-010` workflow/orchestration approach;
7. `ADR-015` operational reporting/read-model approach;
8. `ADR-019` repeatable migration capability.

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
