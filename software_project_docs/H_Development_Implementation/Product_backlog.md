# Product backlog

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-002  
**Document Type:** Product backlog  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Development_plan.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Product_backlog.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Product_backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled implementation backlog that links NuBlox requirements and architecture decisions to executable product increments. This backlog deliberately starts with architecture-decision and production-foundation work because the current functional requirements remain Candidate and the initial priority business workflow has not yet been approved.

## Backlog rules

1. A backlog item must state the requirement, architecture or control it advances.
2. Candidate requirements may be explored, but product semantics are not treated as approved until their lifecycle status supports implementation.
3. Spike code is evidence, not production source.
4. A mastered NuBlox package may be reused when its capability is required and its provenance/licence/support boundary is understood.
5. Every implementation item must define objective completion evidence.
6. Security, tenant isolation, authority and audit controls require negative-path verification, not only successful-path tests.

## Priority model

- **P0** — blocks safe production implementation.
- **P1** — required for the first governed vertical product slice.
- **P2** — follows once the first vertical slice proves the production foundation.
- **P3** — later product expansion or optimisation.

## Wave 0 — implementation decision gates

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-001` | P0 | Resolve initial application decomposition and production module-boundary rules | ADR-001, ADR-017; SPIKE-001 | Individual ADR(s) record alternatives, evidence, ownership rules and explicit decision status | Ready |
| `DEV-002` | P0 | Resolve primary persistence and schema-modularity approach | ADR-002, ADR-017; FR-005–FR-008; SPIKE-001/002 | Database/persistence ADR states authoritative model, transaction boundary and schema ownership rules | Ready |
| `DEV-003` | P0 | Resolve tenant/customer isolation model | ADR-007; FR-002/003; SPIKE-003 | Isolation ADR records shared/schema/database options, operational implications and chosen/deferred boundary | Ready |
| `DEV-004` | P0 | Resolve production runtime/language/framework baseline | ADR-012; SPIKE-001–009; technology evaluation matrix | Runtime ADR compares credible alternatives against spike evidence, NFRs, skills, support and TCO | Ready |
| `DEV-005` | P0 | Resolve identity/authentication boundary | ADR-008; FR-001–FR-004 | Identity ADR defines IdP/federation, account/service identity and tenant-resolution responsibilities | Ready |
| `DEV-006` | P0 | Define API standards and compatibility rules | ADR-011; FR-032–FR-035 | API ADR defines contract style, errors, versioning, idempotency, auth context and observability requirements | Ready |
| `DEV-007` | P0 | Define production observability baseline | ADR-016; FR-034/039/040; SPIKE-009 | Observability ADR separates audit evidence from logs and defines correlation, metrics/tracing and export boundary | Ready |
| `DEV-008` | P0 | Define release/configuration/schema evolution strategy | ADR-020; FR-036/037 | ADR defines backward-safe deployment, migration, rollback/forward-fix and configuration compatibility rules | Ready |

## Wave 1 — production engineering foundation

These items start only when their dependent P0 decisions are explicit enough to avoid embedding unresolved architecture choices.

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-101` | P1 | Create production source/test boundaries separate from `spikes/` | Development plan; ADR-001/012 | Product `src/` and `tests/` structure builds without referencing synthetic spike projects | Blocked by DEV-001/004 |
| `DEV-102` | P1 | Establish deterministic production restore/build/test entrypoint | ADR-012; SPIKE-009 | Clean checkout executes one documented build/test path locally and in CI | Blocked by DEV-004 |
| `DEV-103` | P1 | Establish dependency and package provenance controls | ADR-012; mastered-package policy | Dependency inventory identifies source, version, licence, mastered status and update/security process | Blocked by DEV-004 |
| `DEV-104` | P1 | Establish production persistence/migration scaffold | ADR-002/007/017/020; FR-005–FR-008 | Empty production schema can be created, upgraded and verified deterministically with no synthetic spike taxonomy | Blocked by DEV-002/003/008 |
| `DEV-105` | P1 | Establish identity/context request boundary | ADR-007/008/011; FR-001–FR-004 | Requests resolve authenticated identity and approved working/tenant context server-side with negative tests | Blocked by DEV-003/005/006 |
| `DEV-106` | P1 | Establish production audit/telemetry primitives | ADR-016/018; FR-039/040 | Business audit evidence and technical telemetry are separate, correlated and testable | Blocked by DEV-007 |
| `DEV-107` | P1 | Establish CI quality gates for production paths | Development plan; NFRs | CI verifies deterministic build, automated tests, migration checks and dependency/provenance controls | Blocked by DEV-101–106 |

## Wave 2 — first governed vertical product slice

The exact business workflow remains **Not Ready** until the requirements process selects and validates the first priority workflow. The first slice must nevertheless cover the following capability chain so that the production architecture is exercised end to end.

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-201` | P1 | Select and baseline the first representative business workflow | SRS open questions; Functional validation priorities | Named workflow has approved/validated scope, actors, authoritative records, rules and acceptance criteria | Not Ready |
| `DEV-202` | P1 | Implement governed record creation/maintenance for the selected workflow | FR-005–FR-008 | Typed product semantics, persistence constraints, history rules and automated tests | Blocked by DEV-201 |
| `DEV-203` | P1 | Implement work initiation, routing and state management | FR-009–FR-014 | Work can be initiated, assigned/routed, progressed and exceptions surfaced with isolation/access enforcement | Blocked by DEV-201/202 |
| `DEV-204` | P1 | Implement review/decision/authority path where the workflow requires it | FR-015–FR-018 | Approval outcome, authority enforcement, evidence and downstream consequence are verified | Blocked by DEV-201 |
| `DEV-205` | P1 | Implement work-product/evidence linkage required by the workflow | FR-019–FR-022 | Outputs/revisions/evidence are traceably related to work and decisions | Blocked by DEV-201 |
| `DEV-206` | P1 | Implement operational attention/management view and drill-through | FR-027–FR-029, FR-041 | Authorised users can see actionable work/measure state and navigate to governed source context | Blocked by DEV-202/203 |
| `DEV-207` | P1 | Implement durable notification/integration consequence where required | FR-018, FR-030, FR-032–FR-035 | Intent survives restart/failure, outcome is observable and duplicate effects are controlled | Blocked by DEV-201 |
| `DEV-208` | P1 | Produce first-slice verification and traceability evidence | SRS verification requirement | Requirement → design/ADR → code → automated verification links are recorded | Blocked by DEV-202–207 |

## Wave 3 — cross-enterprise capability expansion

| ID | Priority | Backlog item | Traceability | Status |
|---|---|---|---|---|
| `DEV-301` | P2 | Governed customer variation/configuration | FR-036–FR-038; ADR-005 | Planned |
| `DEV-302` | P2 | Search permitted information | FR-026; ADR-014 | Planned |
| `DEV-303` | P2 | Controlled notifications and communication context | FR-030/031 | Planned |
| `DEV-304` | P2 | External API/integration catalogue and reconciliation tooling | FR-032–FR-035 | Planned |
| `DEV-305` | P2 | Governed operational reporting/export expansion | FR-028/029, FR-041/042; ADR-015 | Planned |
| `DEV-306` | P2 | Commercial/financial continuity slice | FR-023–FR-025 | Planned |
| `DEV-307` | P2 | Expand work/product/decision capabilities by validated functional-governance and functional-delivery outcomes | Product/requirements roadmap | Planned |

## Mastered package workstream

`packages/mastered/mysql` is a governed NuBlox-maintained package and can support product work that requires MySQL protocol/client capability. Its existence does not pre-judge `ADR-002` or make MySQL the primary NuBlox database automatically.

Backlog additions for any mastered package must distinguish:

- package capability development;
- product adoption of the package;
- database/provider architecture decisions;
- upstream synchronisation and vulnerability maintenance.

## Immediate execution order

The next executable sequence is:

```text
DEV-001 / DEV-002 / DEV-003
        +
DEV-004 runtime decision
        +
DEV-005 / DEV-006 / DEV-007 / DEV-008
        ↓
DEV-101–DEV-107 production foundation
        ↓
DEV-201 select first approved workflow
        ↓
DEV-202–DEV-208 first governed vertical slice
```

The P0 ADR work can proceed in parallel where dependencies allow, but no production scaffold should encode an unresolved runtime, persistence or isolation decision.

## References

- `Development_plan.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/Non-functional_requirements_specification.md`
- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../G_Architecture_Design/Technical_spikes.md`
- `../G_Architecture_Design/Proof_of_concept_report.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering | Established the first controlled implementation backlog and gated production-foundation sequence |
