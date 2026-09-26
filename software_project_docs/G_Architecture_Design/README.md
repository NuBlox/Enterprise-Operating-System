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

This remains provisional. The ADR register contains 20 material decisions, none silently accepted by this index.

## Architecture decisions requiring evidence next

Highest-priority decisions/spikes:

1. `ADR-001` initial deployment decomposition;
2. `ADR-002` primary relational persistence;
3. `ADR-003` durable async/outbox mechanism;
4. `ADR-007` customer/tenant isolation;
5. `ADR-008` identity/authentication boundary;
6. `ADR-009` permission vs business authority enforcement;
7. `ADR-010` workflow/orchestration approach;
8. `ADR-012` runtime/language/framework;
9. `ADR-017` schema/module ownership;
10. `ADR-019` repeatable migration capability.

## Next architecture documents

Use the supplied templates to create, as evidence requires:

- Technical spikes;
- Evaluation matrix for technology selection;
- Proof of concept report;
- Data model / ERD;
- Database design;
- Component diagram;
- Threat model;
- Identity and access management design;
- Privacy-by-design assessment;
- API specification/versioning strategy;
- Deployment/infrastructure design;
- observability/operations architecture.

## Architecture gate

Architecture may move toward approval only when:

- the selected initial workflow/customer scope is sufficiently validated;
- material NFR/isolation/security assumptions are explicit;
- high-risk decisions have spike/benchmark evidence where necessary;
- architecture traces to the requirements baseline;
- migration, security, operations and test implications are credible;
- technology choice follows the requirements/option analysis rather than becoming the product model.
