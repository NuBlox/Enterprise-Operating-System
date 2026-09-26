# Product backlog

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-002  
**Document Type:** Product backlog  
**Version:** 0.6  
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
**Supersedes:** Version 0.5  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Product_backlog.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Product_backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled implementation backlog that links NuBlox requirements and architecture decisions to executable product increments. Product semantics remain gated by controlled requirements; architecture-foundation decisions now permit the main production platform foundations to proceed.

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

| ID | Priority | Backlog item | Traceability | Status |
|---|---|---|---|---|
| `DEV-001` | P0 | Initial application decomposition | ADR-001 | **Complete** |
| `DEV-002` | P0 | Primary persistence model | ADR-002 | **Complete** |
| `DEV-003` | P0 | Tenant/customer isolation model | ADR-007 | **Complete** |
| `DEV-004` | P0 | Production runtime/language/framework | ADR-012 | **Complete — .NET 10 LTS/C#** |
| `DEV-005` | P0 | Identity/authentication boundary | ADR-008 | **Complete** |
| `DEV-006` | P0 | API standards and compatibility | ADR-011 | **Complete** |
| `DEV-007` | P0 | Production observability baseline | ADR-016 | **Complete — OpenTelemetry/OTLP** |
| `DEV-008` | P0 | Release/configuration/schema evolution | ADR-020 | **Complete — expand/migrate/contract** |
| `DEV-009` | P0 | Schema/data modularity rules | ADR-017 | **Complete** |
| `DEV-010` | P0 | Initial relational database provider/profile | ADR-021 | **Complete — PostgreSQL 18 / Npgsql 10.0.3** |
| `DEV-011` | P0 | Authoritative business audit/evidence model | ADR-018 | **Complete — append-oriented evidence separate from telemetry** |

## Wave 1 — production engineering foundation

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-101` | P1 | Create production source/test boundaries separate from `spikes/` | ADR-001/012 | Kernel/tests compile independently; boundary test rejects spike references | **Complete — CI run 36270844112** |
| `DEV-102` | P1 | Deterministic production restore/build/test entrypoint | ADR-012 | Root SDK/build policy plus `scripts/verify-production.sh` used locally/CI | **Complete** |
| `DEV-103` | P1 | Dependency and package provenance controls | ADR-012; H-003 | Central versions plus dependency inventory | **Complete — initial baseline** |
| `DEV-104` | P1 | Production PostgreSQL persistence/migration scaffold | ADR-002/007/017/020/021; FR-005–FR-008 | Empty DB migration, journal/checksum, module schema ownership, tenant isolation and integration tests | **Ready** |
| `DEV-105` | P1 | Identity/context request boundary | ADR-007/008/011; FR-001–FR-004 | Provider-neutral Principal/Tenant context plus protected-request negative tests | **Ready** |
| `DEV-106` | P1 | Production audit/telemetry primitives | ADR-016/018; NFR-AUD/OPS | Separate authoritative audit contract plus OTel trace/metric/log/health primitives and tests | **Ready** |
| `DEV-107` | P1 | Complete CI quality gates | Development plan; NFRs | CI covers build/tests, persistence migrations/isolation, identity/security, telemetry/audit and dependency controls | **In progress — base build/test gate exists** |
| `DEV-108` | P1 | Production ASP.NET Core API host and contract primitives | ADR-008/011/012 | API host, health, RFC 9457 baseline, `/api/v1` grouping and OpenAPI infrastructure verified without invented business endpoints | **Ready** |

## Wave 2 — first governed vertical product slice

The exact workflow remains **Not Ready** until requirements work selects and validates the first priority business workflow.

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-201` | P1 | Select and baseline the first representative business workflow | SRS open questions; functional validation priorities | Approved/validated scope, actors, records, rules and acceptance criteria | Not Ready |
| `DEV-202` | P1 | Governed record creation/maintenance | FR-005–FR-008 | Typed semantics, persistence constraints/history and tests | Blocked by DEV-201 |
| `DEV-203` | P1 | Work initiation/routing/state | FR-009–FR-014 | End-to-end work state with isolation/access enforcement | Blocked by DEV-201/202 |
| `DEV-204` | P1 | Review/decision/authority path | FR-015–FR-018 | Authority, evidence and outcome verified | Blocked by DEV-201 |
| `DEV-205` | P1 | Work-product/evidence linkage | FR-019–FR-022 | Outputs/revisions/evidence traceably related | Blocked by DEV-201 |
| `DEV-206` | P1 | Operational management view/drill-through | FR-027–FR-029, FR-041 | Actionable view and governed source drill-through | Blocked by DEV-202/203 |
| `DEV-207` | P1 | Durable notification/integration consequence | FR-018, FR-030, FR-032–FR-035 | Restart/retry-safe effect and reconciliation | Blocked by DEV-201 |
| `DEV-208` | P1 | First-slice traceability/verification evidence | SRS verification | Requirement → ADR/design → code → test links | Blocked by DEV-202–207 |

## Wave 3 — cross-enterprise capability expansion

| ID | Priority | Backlog item | Traceability | Status |
|---|---|---|---|---|
| `DEV-301` | P2 | Governed customer variation/configuration | FR-036–FR-038; ADR-005 | Planned |
| `DEV-302` | P2 | Search permitted information | FR-026; ADR-014 | Planned |
| `DEV-303` | P2 | Controlled notifications/communication context | FR-030/031 | Planned |
| `DEV-304` | P2 | External API/integration catalogue/reconciliation | FR-032–FR-035 | Planned |
| `DEV-305` | P2 | Operational reporting/export expansion | FR-028/029, FR-041/042; ADR-015 | Planned |
| `DEV-306` | P2 | Commercial/financial continuity slice | FR-023–FR-025 | Planned |
| `DEV-307` | P2 | Expand by validated functional-governance, functional-delivery and built-environment outcomes | Product/requirements roadmap | Planned |

## Mastered package workstream

`packages/mastered/mysql` remains a governed NuBlox-maintained package. ADR-021 deliberately selects PostgreSQL 18 as the initial production provider based on the current NuBlox evidence. The mastered MySQL package remains available for integrations/tooling or a later explicitly approved provider implementation; it is not pulled into the .NET PostgreSQL persistence layer.

## Immediate execution order

All P0 production-foundation architecture gates are now closed. The implementation sequence is:

```text
EXECUTE IN PARALLEL WHERE SAFE:
DEV-108 ASP.NET Core API host / contract primitives
DEV-105 Principal/Tenant context boundary
DEV-104 PostgreSQL persistence / migrations / isolation
DEV-106 audit + OpenTelemetry primitives
        ↓
DEV-107 integrate all production CI quality gates
        ↓
DEV-201 select first approved business workflow
        ↓
DEV-202–DEV-208 first governed vertical product slice
```

The platform work above must stay semantic-light: it establishes production controls and infrastructure without inventing the first business workflow.

## References

- `Development_plan.md`
- `Dependency_management_document.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/API_requirements.md`
- `../F_Requirements_Analysis/Non-functional_requirements_specification.md`
- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../G_Architecture_Design/Technical_spikes.md`
- `../G_Architecture_Design/Proof_of_concept_report.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering | Established the first controlled implementation backlog and gated production-foundation sequence |
| 0.2 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-001/002/003 and added schema-modularity gate |
| 0.3 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-004 and unblocked production source/build work |
| 0.4 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-101/102/103 after production foundation CI passed |
| 0.5 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-005/006/009; added provider selection and API-host foundation |
| 0.6 | 2026-09-26 | NuBlox Product / Engineering | Closed observability, release/schema evolution, provider and audit/evidence gates through ADR-016/018/020/021; unblocked DEV-104/106 |
