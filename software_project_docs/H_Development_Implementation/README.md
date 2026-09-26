# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.1 |

## Entry position

Development & Implementation begins from the controlled architecture evidence produced by `SPIKE-001` through `SPIKE-009`.

The spike code remains disposable experimental code. It is not the production application and must not be copied into a production codebase by momentum. Promotion of a proven pattern requires the relevant ADR decision, traceability to requirements, a controlled implementation plan, review and normal build/test/security controls.

## Immediate focus

1. convert completed spike evidence into explicit architecture decisions;
2. resolve the deferred runtime/framework, persistence/isolation, identity, API, observability and release-evolution decisions needed for implementation;
3. establish the production repository/toolchain baseline only after those gates are explicit;
4. implement the first end-to-end production slice from approved requirements and design rather than from spike taxonomy;
5. preserve traceability from requirement → ADR/design → backlog item → code → test → release evidence.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../G_Architecture_Design/Technical_spikes.md`](../G_Architecture_Design/Technical_spikes.md)
- [`../G_Architecture_Design/Proof_of_concept_report.md`](../G_Architecture_Design/Proof_of_concept_report.md)
- [`../F_Requirements_Analysis/`](../F_Requirements_Analysis/)
