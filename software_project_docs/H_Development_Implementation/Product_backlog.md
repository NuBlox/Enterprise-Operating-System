# Product backlog

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-002  
**Document Type:** Product backlog  
**Version:** 0.20  
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
**Related Documents:** `Development_plan.md`, `Dependency_management_document.md`, `First_vertical_slice_definition.md`, `First_vertical_slice_verification_and_traceability.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** Version 0.19  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Product_backlog.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Product_backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled implementation backlog that links NuBlox requirements and architecture decisions to executable product increments. Product semantics remain gated by controlled requirements; the production foundation and first governed vertical slice are now implemented with consolidated traceability/verification evidence.

## Backlog rules

1. Every item states the requirement, architecture or control it advances.
2. Candidate requirements are not treated as approved permanent product semantics merely because implementation is technically possible.
3. Spike code is evidence, not production source.
4. A NuBlox-mastered or separately governed NuBlox package may be adopted only when a product need and compatible architecture decision exist.
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
| `DEV-104` | P1 | Production PostgreSQL persistence/migration scaffold | ADR-002/007/017/020/021; FR-005–FR-008 | Empty DB migration, journal/checksum, module schema ownership, tenant isolation and integration tests | **Complete — CI run 36272246291** |
| `DEV-105` | P1 | Identity/context request boundary | ADR-007/008/011; FR-001–FR-004 | Provider-neutral Principal/Tenant context plus protected-request negative tests | **Complete — CI run 36273403103** |
| `DEV-106` | P1 | Production audit/telemetry primitives | ADR-016/018; NFR-AUD/OPS | Separate authoritative audit contract plus OTel trace/metric/log/health primitives and tests | **Complete — CI run 36274166788** |
| `DEV-107` | P1 | Complete CI quality gates | Development plan; NFRs | CI covers build/tests, persistence migrations/isolation, identity/security, telemetry/audit and dependency controls | **Complete — CI run 36274812831** |
| `DEV-108` | P1 | Production ASP.NET Core API host and contract primitives | ADR-008/011/012 | API host, health, RFC 9457 baseline, `/api/v1` grouping and OpenAPI infrastructure verified without invented business endpoints | **Complete — CI run 36274812831** |

## Wave 2 — first governed vertical product slice

Selected workflow: **Governed Work Product — Create, Review, Approve and Issue**. Controlled definition: `First_vertical_slice_definition.md`.

| ID | Priority | Backlog item | Traceability | Completion evidence | Status |
|---|---|---|---|---|---|
| `DEV-201` | P1 | Select and baseline the first representative business workflow | UC-001/002/003/010; functional validation priorities | Scope, actors, records, lifecycle, authority boundary and acceptance criteria baselined | **Complete — NBEOS-H-004 v0.1** |
| `DEV-202` | P1 | Governed record creation/maintenance | FR-005–FR-008; NBEOS-H-004 | WorkProduct + Revision typed semantics, atomic persistence, module-owned migration, constraints, RLS isolation and automated tests | **Complete — CI run 36275665645** |
| `DEV-203` | P1 | Work initiation/routing/state | FR-009–FR-014; NBEOS-H-004 | Access-evaluated Draft → InReview submission, immutable submission evidence, routed open ReviewRequest, atomic persistence/concurrency guard and tenant isolation | **Complete — CI run 36276125829** |
| `DEV-204` | P1 | Review/decision/authority path | FR-015–FR-018; NBEOS-H-004; ADR-009 | Permission-separated contextual authority evaluation, immutable decision evidence, atomic request completion/revision outcome and tenant-isolated PostgreSQL verification | **Complete — CI run 36277012364** |
| `DEV-205` | P1 | Work-product/evidence linkage | FR-019–FR-022; NBEOS-H-004 | Approved-only issue path, attributable revision issue fields, exact approval-decision linkage, atomic issue/supersession, immutable issue evidence and tenant-isolated PostgreSQL verification | **Complete — CI run 36277625604** |
| `DEV-206` | P1 | Operational management view/drill-through | FR-027–FR-029, FR-041; NBEOS-H-004 | Derived Principal-scoped contributor/reviewer attention view, tenant/RLS isolation, governed source drill-through with review/decision/issue evidence and unauthorised/cross-tenant negative verification | **Complete — CI run 36278067339** |
| `DEV-207` | P1 | Durable notification/integration consequence | FR-018, FR-030, FR-032–FR-035; NBEOS-H-004; ADR-003 | Atomic issue+delivery-intent outbox, tenant RLS, recoverable `SKIP LOCKED` lease claiming, stable idempotency, retry scheduling, concurrent single-claim behaviour and completion reconciliation | **Complete — CI run 36279181900** |
| `DEV-208` | P1 | First-slice traceability/verification evidence | SRS verification; NBEOS-H-004; NBEOS-H-005 | Requirement → ADR/design → code → automated test → CI links, all 12 first-slice acceptance criteria mapped, F-section RTM reconciled | **Complete — traceability/verification CI run 36279763189; PR #29 exact closure head must pass before merge** |

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

## Governed SQL package workstream

The separately governed `NuBlox/NuBloxSQL` repository is the authoritative NuBlox location for the extracted MySQL driver capability previously maintained in this repository. ADR-021 selects PostgreSQL 18 as the initial Enterprise Operating System production provider.

NuBloxSQL remains available for integrations/tooling or a later explicitly approved provider implementation. It is not pulled into the .NET PostgreSQL persistence layer.

## Immediate execution order

The first governed vertical slice is complete through implementation and consolidated verification. The next controlled development transition is Wave 3 capability expansion:

```text
first slice complete (DEV-201–DEV-208)
        ↓
select one validated P2 capability with clear requirement/ADR readiness
        ↓
implement as another end-to-end governed increment
        ↓
extend traceability + production CI evidence
```

Current planned Wave 3 candidates are customer variation/configuration, search, controlled communication, external integration/reconciliation, reporting expansion, commercial/financial continuity and broader functional-governance / functional-delivery / built-environment outcomes.

The first slice remains work-product-type neutral. Discipline/customer-specific semantics are introduced only through later validated requirements.

## References

- `Development_plan.md`
- `Dependency_management_document.md`
- `First_vertical_slice_definition.md`
- `First_vertical_slice_verification_and_traceability.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/Use_cases.md`
- `../F_Requirements_Analysis/API_requirements.md`
- `../F_Requirements_Analysis/Non-functional_requirements_specification.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`
- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../G_Architecture_Design/adr/ADR-003-durable-asynchronous-processing.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering | Established the first controlled implementation backlog and gated production-foundation sequence |
| 0.2 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-001/002/003 and added schema-modularity gate |
| 0.3 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-004 and unblocked production source/build work |
| 0.4 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-101/102/103 after production foundation CI passed |
| 0.5 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-005/006/009; added provider selection and API-host foundation |
| 0.6 | 2026-09-26 | NuBlox Product / Engineering | Closed observability, release/schema evolution, provider and audit/evidence gates through ADR-016/018/020/021; unblocked DEV-104/106 |
| 0.7 | 2026-09-26 | NuBlox Product / Engineering | Started DEV-104 production PostgreSQL migration, isolation and integration-verification implementation |
| 0.8 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-104 after production PostgreSQL migration, RLS/runtime-role and checksum integration verification passed in CI run 36272246291 |
| 0.9 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-105 after provider-neutral Principal/Tenant context resolution and protected-request negative tests passed in CI run 36273403103 |
| 0.10 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-106 after audit/observability verification passed in CI run 36274166788 |
| 0.11 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-107/108 after API-host and full production foundation verification passed in CI run 36274812831 |
| 0.12 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-201 by baselining Governed Work Product — Create, Review, Approve and Issue as the first representative production workflow |
| 0.13 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-202 after WorkProduct/Revision domain, module-owned PostgreSQL persistence, tenant isolation and negative-path verification passed in CI run 36275665645 |
| 0.14 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-203 after access-evaluated review submission, ReviewRequest routing, concurrency and tenant-isolation verification passed in CI run 36276125829 |
| 0.15 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-204 after contextual business-authority separation, immutable decision evidence and atomic tenant-isolated decision persistence passed in CI run 36277012364 |
| 0.16 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-205 after approved-only issue, exact approval-decision evidence linkage, atomic issue/supersession and tenant-isolation verification passed in CI run 36277625604 |
| 0.17 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-206 after Principal-scoped attention/read-model and governed review/decision/issue evidence drill-through verification passed in CI run 36278067339 |
| 0.18 | 2026-09-26 | NuBlox Product / Engineering | Closed DEV-207 after accepted ADR-003 was implemented with atomic durable issue intent, tenant-scoped recoverable claiming, retry/idempotency/concurrency and reconciliation verification in CI run 36279181900 |
| 0.19 | 2026-09-27 | NuBlox Product / Engineering | Added NBEOS-H-005 consolidated first-slice traceability/verification evidence and reconciled the F-section RTM; DEV-208 awaiting final-head production CI |
| 0.20 | 2026-09-27 | NuBlox Product / Engineering | Closed DEV-208 after consolidated first-slice traceability/acceptance evidence passed the production verification path in CI run 36279763189; PR #29 retains exact-head CI as the final merge gate |
