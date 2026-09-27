# Product backlog

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-002  
**Document Type:** Product backlog  
**Version:** 0.22  
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
**Related Documents:** `Development_plan.md`, `Dependency_management_document.md`, `First_vertical_slice_definition.md`, `First_vertical_slice_verification.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** Version 0.21  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Product_backlog.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Product_backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled implementation backlog linking NuBlox requirements and architecture decisions to executable product increments. The production foundation is complete and the first governed vertical slice, **Governed Work Product — Create, Review, Approve and Issue**, has now passed bounded end-to-end technical verification through the real HTTP/application/PostgreSQL runtime graph.

The backlog remains Draft because wider product requirements, customer evidence and subsequent capability priorities remain controlled but not fully approved.

## Backlog rules

1. Every item states the requirement, architecture or control it advances.
2. Candidate requirements are not treated as approved permanent product semantics merely because implementation is technically possible.
3. Spike code is evidence, not production source.
4. A NuBlox-mastered or separately governed package may be adopted only when a product need and compatible architecture decision exists.
5. Every implementation item requires objective completion evidence.
6. Security, tenant isolation, authority and audit controls require negative-path verification.
7. Adapter/component tests do not substitute for real production composition-root verification where end-to-end acceptance requires actual dependency resolution and infrastructure.

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

| ID | Priority | Backlog item | Status |
|---|---|---|---|
| `DEV-101` | P1 | Production source/test boundaries separate from `spikes/` | **Complete — CI 36270844112** |
| `DEV-102` | P1 | Deterministic restore/build/test entrypoint | **Complete** |
| `DEV-103` | P1 | Dependency and provenance controls | **Complete — initial baseline** |
| `DEV-104` | P1 | PostgreSQL persistence/migration scaffold | **Complete — CI 36272246291** |
| `DEV-105` | P1 | Identity/context request boundary | **Complete — CI 36273403103** |
| `DEV-106` | P1 | Audit/telemetry primitives | **Complete — CI 36274166788** |
| `DEV-107` | P1 | Complete CI quality gates | **Complete — CI 36274812831** |
| `DEV-108` | P1 | ASP.NET Core API host/contract primitives | **Complete — CI 36274812831** |

## Wave 2 — first governed vertical product slice

Selected workflow: **Governed Work Product — Create, Review, Approve and Issue**. Controlled definition: `First_vertical_slice_definition.md`.

| ID | Priority | Backlog item | Completion evidence | Status |
|---|---|---|---|---|
| `DEV-201` | P1 | Select/baseline first workflow | NBEOS-H-004 | **Complete** |
| `DEV-202` | P1 | Governed record creation/maintenance | WorkProduct/Revision + PostgreSQL/RLS | **Complete — CI 36275665645** |
| `DEV-203` | P1 | Work initiation/routing/state | submission + ReviewRequest + tenant isolation | **Complete — CI 36276125829** |
| `DEV-204` | P1 | Review/decision/authority | contextual authority + immutable decision evidence | **Complete — CI 36277012364** |
| `DEV-205` | P1 | Work-product/evidence linkage | approved-only issue + exact approval evidence + supersession | **Complete — CI 36277625604** |
| `DEV-206` | P1 | Operational attention/drill-through | Principal-scoped attention + governed source evidence | **Complete — CI 36278067339** |
| `DEV-207` | P1 | Durable notification/integration consequence | atomic issue intent + RLS + lease/retry/idempotency/reconciliation | **Complete — CI 36279181900** |
| `DEV-208` | P1 | First-slice traceability/verification | NBEOS-H-005 v0.3 + NBEOS-F-015 v0.3 + all 12 acceptance criteria | **Complete — bounded technical acceptance; final documentation-head CI required before merge** |
| `DEV-209` | P1 | WorkProducts HTTP composition + verified request context | Work Product create/read/submit/decide/issue endpoints derive Principal/Tenant only from verified context; fail-closed, route/body/header tamper, RFC 9457 and OpenAPI tests | **Complete — final-head CI 36280622610** |
| `DEV-210` | P1 | WorkProducts audit + correlated telemetry composition | successful material operations append minimised authoritative audit evidence; audit/telemetry share correlation but remain separate; read/failure/minimisation tests | **Complete — merged PR #31 after green production verification** |
| `DEV-211` | P1 | Production WorkProducts runtime composition | `NuBlox.Runtime.PostgreSql` composition root, concrete governed repository graph, restricted runtime role, durable PostgreSQL audit appender, combined migrations and real HTTP→application→PostgreSQL smoke flow | **Complete — implementation-head CI 36281993924; final documentation-head CI required before merge** |

## DEV-211 verification finding retained

The first real composition run exposed a defect that component tests had not detected: the runtime initially registered `PostgresWorkProductRepository`, whose base interface behaviour could not reconstruct governed review-decision context. The real reviewer decision returned `404` after create and submit had already succeeded.

The production composition now registers `PostgresGovernedWorkProductRepository`. The same real HTTP create → submit → approve → issue flow then passed against PostgreSQL 18.6, including Tenant B isolation and durable audit verification.

This finding reinforces the backlog rule that component success does not replace composition-root verification.

## Wave 3 — cross-enterprise capability expansion

The first bounded slice is technically verified. Wave 3 should now be ordered from validated product/customer priorities rather than selected by implementation convenience.

| ID | Priority | Backlog item | Traceability | Status |
|---|---|---|---|---|
| `DEV-301` | P2 | Governed customer variation/configuration | FR-036–FR-038; ADR-005 | Planned — priority not yet baselined |
| `DEV-302` | P2 | Search permitted information | FR-026; ADR-014 | Planned — priority not yet baselined |
| `DEV-303` | P2 | Controlled notifications/communication context | FR-030/031 | Planned — priority not yet baselined |
| `DEV-304` | P2 | External API/integration catalogue/reconciliation | FR-032–FR-035 | Planned — priority not yet baselined |
| `DEV-305` | P2 | Operational reporting/export expansion | FR-028/029, FR-041/042; ADR-015 | Planned — priority not yet baselined |
| `DEV-306` | P2 | Commercial/financial continuity slice | FR-023–FR-025 | Planned — priority not yet baselined |
| `DEV-307` | P2 | Expand by validated functional-governance, functional-delivery and built-environment outcomes | Product/requirements roadmap | Planned — priority not yet baselined |

## Governed SQL package workstream

The separately governed `NuBlox/NuBloxSQL` repository is the authoritative NuBlox location for the extracted MySQL driver capability previously maintained here. ADR-021 selects PostgreSQL 18 as the initial Enterprise Operating System production provider. NuBloxSQL remains available for integrations/tooling or later explicitly approved provider support and is not pulled into the .NET PostgreSQL persistence layer.

## Immediate execution order

The first production vertical slice has reached bounded technical acceptance. The next controlled sequence is:

```text
final documentation-head production verifier
        ↓
merge DEV-211 + DEV-208 acceptance evidence
        ↓
baseline next product/customer priority from Wave 3 or a newly validated workflow
        ↓
trace requirement → ADR/design → implementation → verification
        ↓
implement next end-to-end product increment
```

NuBlox should not select the next business capability solely because its infrastructure is easiest to build. The next slice must be justified by controlled product/customer evidence and the requirements roadmap.

## References

- `Development_plan.md`
- `Dependency_management_document.md`
- `First_vertical_slice_definition.md`
- `First_vertical_slice_verification.md`
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
| 0.19 | 2026-09-26 | NuBlox Product / Engineering | Started DEV-208 verification; recorded HTTP/request-context and audit/telemetry composition gaps as DEV-209/210 rather than falsely accepting the first slice |
| 0.20 | 2026-09-27 | NuBlox Product / Engineering | Closed DEV-209 after WorkProducts HTTP verified-context composition, tenant/actor tamper protection, RFC 9457 failure mapping and OpenAPI verification passed in final-head CI run 36280622610 |
| 0.21 | 2026-09-27 | NuBlox Product / Engineering | Closed DEV-210 after minimised authoritative audit and correlated telemetry composition; re-verification recorded DEV-211 real production runtime composition as the remaining first-slice blocker |
| 0.22 | 2026-09-27 | NuBlox Product / Engineering | Closed DEV-211 and DEV-208 after real production composition verified the WorkProducts HTTP/application/PostgreSQL graph, corrected the governed-repository registration defect, proved tenant isolation and durable audit persistence, and completed all twelve first-slice acceptance criteria |
