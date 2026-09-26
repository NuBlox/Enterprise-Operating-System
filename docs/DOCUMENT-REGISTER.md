# NuBlox — Master Document Register

## Status

Published

## Purpose

This is the single governing index of documents in the clean-slate NuBlox Enterprise Operating System repository.

A document is added here only when it exists or when an evidenced dependency justifies registering it as Planned.

The register is intentionally small. It will grow with the architecture and product, not ahead of them.

## Governing register

| ID | Document | Role | Status | Purpose | Depends on |
|---|---|---|---|---|---|
| `ARCH-001` | `00-product-definition.md` | ARCH | PUBLISHED | Defines what NuBlox is and the clean-slate product boundary | — |
| `ARCH-002` | `01-enterprise-model.md` | ARCH | PUBLISHED | Defines enterprise identity, organisational structure and participation semantics | ARCH-001 |
| `ARCH-003` | `02-work-model.md` | ARCH | PUBLISHED | Defines work definition, work instances, assignment and execution concepts | ARCH-001; ARCH-002 |
| `ARCH-004` | `03-capability-model.md` | ARCH | PUBLISHED | Defines the capability questions to resolve before Functions/tools are promoted | ARCH-001; ARCH-003 |
| `ARCH-005` | `04-object-model.md` | ARCH | PUBLISHED | Defines business-object identity, lifecycle, relationship and extensibility principles | ARCH-002; ARCH-003 |
| `ARCH-006` | `05-control-model.md` | ARCH | PUBLISHED | Separates identity, Permission, Responsibility, Authority, Delegation, Decision, Evidence and Audit | ARCH-002; ARCH-005 |
| `EVID-001` | `06-evidence-reconciliation.md` | EVID | PUBLISHED | Governs how V3/external evidence is reconciled without becoming automatic canon | ARCH-001–006 |
| `EVID-002` | `07-wave-1-enterprise-identity-reconciliation.md` | EVID | PUBLISHED | Reconciles Party, Person, Organisation, Position, Work Relationship, Occupancy, reporting and control concepts | ARCH-002; ARCH-006; EVID-001 |
| `GOV-001` | `08-document-governance.md` | GOV | PUBLISHED | Governs document identity, lifecycle, authority, dependencies and anti-sprawl rules | EVID-001 |
| `EVID-003` | Wave 2 — Work Execution Reconciliation | EVID | PLANNED | Reconcile Activity, Method, Work Item, assignment, workflow, work product, Decision, Evidence and handoff semantics | ARCH-003; ARCH-005; ARCH-006; EVID-001; EVID-002 |

## Current phase gate

The active architecture phase is:

```text
Wave 2 — Work Execution Reconciliation
```

No application framework, persistence schema, migration history, route model, Function catalogue, Tool Registry or UI specification is authorised yet.

## Candidate future documents

The following topics are known to require eventual attention but are **not registered as Planned documents yet** because their precise document boundaries depend on later reconciliation:

- enterprise capability structure / Function model;
- Method model;
- enterprise Activity model;
- shared platform capability/kernel boundaries;
- authority resolution details;
- evidence and audit integrity specification;
- tenancy/isolation architecture;
- persistence architecture;
- extensibility/metadata architecture;
- integration architecture;
- industry solution model;
- CBE reconciliation;
- provisioning and tenant configuration;
- product implementation programme;
- operations, commercial and brand documentation.

Known need is not the same thing as an authorised document boundary.

## Register rules

1. Every governed document has one unique immutable ID.
2. IDs are never reused.
3. Role and lifecycle status are separate.
4. `PUBLISHED` documents are authoritative within their defined scope.
5. `EVID` material informs/challenges architecture but does not silently override it.
6. A new `PLANNED` entry requires a current downstream dependency or governance need.
7. Superseded and Retired documents remain traceable.
8. The register is updated before or in the same commit that introduces a new governed document.
9. A document not in this register may exist as working material, but is not a governed NuBlox artifact.

## Source suite reconciliation

The earlier 139-document NuBlox suite is evidence for possible future coverage, not a backlog to recreate.

Its useful governance mechanics have been promoted through `GOV-001`. Its architectural assumptions will be considered individually during the relevant evidence waves.
