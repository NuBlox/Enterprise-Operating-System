# Product backlog

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-002  
**Document Type:** Product backlog  
**Version:** 0.4  
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
**Related Documents:** `Development_plan.md`, `Dependency_management_document.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** Version 0.3  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Product_backlog.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Product_backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled implementation backlog that links NuBlox requirements and architecture decisions to executable product increments. Product semantics remain gated by controlled requirements and architecture decisions; spike code remains evidence rather than production source.

## Backlog rules

1. Every item states the requirement, architecture or control it advances.
2. Candidate requirements are not treated as approved product semantics merely because implementation is technically possible.
3. Spike code is evidence, not production source.
4. A mastered NuBlox package may be adopted only when a product need and compatible architecture decision exist.
5. Every implementation item requires objective completion evidence.
6. Security, tenant isolation, authority and audit controls require negative-path verification.

## Priority model

- **P0** — blocks safe production implementation.
- **P1** — required for the first governed vertical product slice.
- **P2** — follows once the first vertical slice proves the production foundation.
- **P3** — later product expansion or optimisation.

## Wave 0 — implementation decision gates

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-001` | P0 | Resolve initial application decomposition | ADR-001; SPIKE-001 | Accepted ADR records alternatives, evidence and boundary rules | **Complete — ADR-001 ACCEPTED** |
| `DEV-002` | P0 | Resolve primary persistence model | ADR-002; FR-005–FR-008; SPIKE-001/002 | Accepted ADR states authoritative model and transaction principles without silently selecting a provider | **Complete — ADR-002 ACCEPTED** |
| `DEV-003` | P0 | Resolve tenant/customer isolation model | ADR-007; FR-002/003; SPIKE-003 | Accepted ADR records logical tenant invariants, physical profiles and provider-independent enforcement | **Complete — ADR-007 ACCEPTED** |
| `DEV-004` | P0 | Resolve production runtime/language/framework baseline | ADR-012; SPIKE-001–009; technology evaluation matrix | Accepted runtime ADR backed by current vendor/support evidence and NuBlox spike evidence | **Complete — ADR-012 ACCEPTED (.NET 10 LTS/C# server/core)** |
| `DEV-005` | P0 | Resolve identity/authentication boundary | ADR-008; FR-001–FR-004 | Identity ADR defines IdP/federation, account/service identity and tenant-resolution responsibilities | Ready |
| `DEV-006` | P0 | Define API standards and compatibility rules | ADR-011; FR-032–FR-035 | API ADR defines contract style, errors, versioning, idempotency, auth context and observability requirements | Ready |
| `DEV-007` | P0 | Define production observability baseline | ADR-016; FR-034/039/040; SPIKE-009 | Observability ADR separates audit evidence from logs and defines correlation, metrics/tracing and export boundary | Ready |
| `DEV-008` | P0 | Define release/configuration/schema evolution strategy | ADR-020; FR-036/037 | ADR defines backward-safe deployment, migration, rollback/forward-fix and configuration compatibility rules | Ready |
| `DEV-009` | P0 | Resolve schema/data modularity rules | ADR-017; ADR-001/002 | ADR defines module-owned persistence boundaries and prevents unrestricted cross-module persistence access | Ready |

## Wave 1 — production engineering foundation

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-101` | P1 | Create production source/test boundaries separate from `spikes/` | Development plan; ADR-001/012 | `src/NuBlox.Kernel` and `tests/NuBlox.Kernel.Tests` compile independently of spike projects; boundary test rejects `FoundationSpike` references | **Complete — CI run 36270844112** |
| `DEV-102` | P1 | Establish deterministic production restore/build/test entrypoint | ADR-012; SPIKE-009 | Root SDK/build policy plus `bash scripts/verify-production.sh` executes restore/build/test locally and in CI | **Complete — SDK 10.0.401; 0 warnings/errors; 2/2 tests passed in CI run 36270844112** |
| `DEV-103` | P1 | Establish dependency and package provenance controls | ADR-012; mastered-package policy | Central package versions and `NBEOS-H-003` inventory record direct dependency purpose/version/provenance/licence boundary | **Complete — initial baseline** |
| `DEV-104` | P1 | Establish production persistence/migration scaffold | ADR-002/007/017/020; FR-005–FR-008 | Empty production schema can be created, upgraded and verified deterministically with no synthetic spike taxonomy | Blocked by DEV-008/009 and database-provider decision |
| `DEV-105` | P1 | Establish identity/context request boundary | ADR-007/008/011; FR-001–FR-004 | Requests resolve authenticated identity and approved working/tenant context server-side with negative tests | Blocked by DEV-005/006 |
| `DEV-106` | P1 | Establish production audit/telemetry primitives | ADR-016/018; FR-039/040 | Business audit evidence and technical telemetry are separate, correlated and testable | Blocked by DEV-007 |
| `DEV-107` | P1 | Establish complete CI quality gates for production paths | Development plan; NFRs | CI verifies build/tests plus migration, identity/security, telemetry and dependency/provenance controls as those paths exist | **In progress — base build/test gate established; blocked by DEV-104–106 for full scope** |

## Wave 2 — first governed vertical product slice

The exact workflow remains **Not Ready** until requirements work selects and validates the first priority business workflow.

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-201` | P1 | Select and baseline the first representative business workflow | SRS open questions; Functional validation priorities | Named workflow has approved/validated scope, actors, authoritative records, rules and acceptance criteria | Not Ready |
| `DEV-202` | P1 | Implement governed record creation/maintenance for the selected workflow | FR-005–FR-008 | Typed product semantics, persistence constraints, history rules and automated tests | Blocked by DEV-201 |
| `DEV-203` | P1 | Implement work initiation, routing and state management | FR-009–FR-014 | Work can be initiated, assigned/routed, progressed and exceptions surfaced with isolation/access enforcement | Blocked by DEV-201/202 |
| `DEV-204` | P1 | Implement review/decision/authority path where required | FR-015–FR-018 | Approval outcome, authority enforcement, evidence and downstream consequence are verified | Blocked by DEV-201 |
| `DEV-205` | P1 | Implement work-product/evidence linkage | FR-019–FR-022 | Outputs/revisions/evidence are traceably related to work and decisions | Blocked by DEV-201 |
| `DEV-206` | P1 | Implement operational attention/management view and drill-through | FR-027–FR-029, FR-041 | Authorised users can see actionable work/measure state and navigate to governed source context | Blocked by DEV-202/203 |
| `DEV-207` | P1 | Implement durable notification/integration consequence where required | FR-018, FR-030, FR-032–FR-035 | Intent survives restart/failure, outcome is observable and duplicate effects are controlled | Blocked by DEV-201 |
| `DEV-208` | P1 | Produce first-slice verification and traceability evidence | SRS verification requirement | Requirement → ADR/design → code → automated verification links are recorded | Blocked by DEV-202–207 |

## Wave 3 — cross-enterprise capability expansion

| ID | Priority | Backlog item | Traceability | Status |
|---|---|---|---|---|
| `DEV-301` | P2 | Governed customer variation/configuration | FR-036–FR-038; ADR-005 | Planned |
| `DEV-302` | P2 | Search permitted information | FR-026; ADR-014 | Planned |
| `DEV-303` | P2 | Controlled notifications and communication context | FR-030/031 | Planned |
| `DEV-304` | P2 | External API/integration catalogue and reconciliation tooling | FR-032–FR-035 | Planned |
| `DEV-305` | P2 | Governed operational reporting/export expansion | FR-028/029, FR-041/042; ADR-015 | Planned |
| `DEV-306` | P2 | Commercial/financial continuity slice | FR-023–FR-025 | Planned |
| `DEV-307` | P2 | Expand capabilities by validated functional-governance, functional-delivery and built-environment outcomes | Product/requirements roadmap | Planned |

## Mastered package workstream

`packages/mastered/mysql` remains a governed NuBlox-maintained package. It is not a dependency of the current .NET production kernel and does not pre-judge the relational database/provider decision under ADR-002.

Package capability development, product adoption, database/provider decisions, and upstream synchronisation/vulnerability maintenance remain distinct workstreams.

## Immediate execution order

```text
COMPLETE:
DEV-001 decomposition
DEV-002 persistence model
DEV-003 tenant isolation
DEV-004 .NET 10 server/core runtime
DEV-101 production source/test boundary
DEV-102 deterministic restore/build/test
DEV-103 dependency/provenance baseline
        ↓
NEXT P0 DECISIONS:
DEV-005 identity
DEV-006 API standards
DEV-009 schema/data modularity
DEV-007 observability
DEV-008 release/schema evolution
+ relational database/provider selection
        ↓
DEV-104 persistence/migrations
DEV-105 identity/context
DEV-106 audit/telemetry
DEV-107 complete CI gate
        ↓
DEV-201 first approved workflow
        ↓
DEV-202–DEV-208 first governed vertical slice
```

## References

- `Development_plan.md`
- `Dependency_management_document.md`
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
| 0.2 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-001/002/003 following accepted ADR-001/002/007; separated ADR-017 schema modularity into DEV-009 |
| 0.3 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-004 after ADR-012 accepted .NET 10 LTS/C# for the server/core |
| 0.4 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-101/102/103 after production foundation CI passed; recorded partial DEV-107 build/test gate |
