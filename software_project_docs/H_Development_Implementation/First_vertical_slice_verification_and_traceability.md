# First vertical slice verification and traceability

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-005  
**Document Type:** Implementation verification and traceability record  
**Version:** 0.2  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering / Quality  
**Reviewer:** [TBD]  
**Approver:** NuBlox programme owner  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `First_vertical_slice_definition.md`, `Product_backlog.md`, `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../F_Requirements_Analysis/Use_cases.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** Version 0.1  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/First_vertical_slice_verification_and_traceability.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Close `DEV-208` by recording auditable end-to-end traceability for the first governed NuBlox production vertical slice:

**Governed Work Product — Create, Review, Approve and Issue**.

This record links controlled requirements and use cases to accepted architecture decisions, implementation modules, automated verification and GitHub Actions evidence. It does not change the lifecycle status of the upstream Draft/Candidate requirements; it records what the current representative implementation proves.

## Verification baseline

The first slice is defined by `NBEOS-H-004` and implemented through `DEV-202` to `DEV-207`.

Production verification is executed by:

```bash
bash scripts/verify-production.sh
```

The GitHub Actions `Production foundation` workflow runs the same verifier with PostgreSQL 18.6 available for integration tests.

The implementation milestones used as evidence are:

| Backlog item | Capability | Merged evidence |
|---|---|---|
| `DEV-202` | Work Product + Revision domain/persistence | CI `36275665645` |
| `DEV-203` | review submission/routing/state | CI `36276125829` |
| `DEV-204` | authority-backed review/approval decision | CI `36277012364` |
| `DEV-205` | issue/evidence linkage | CI `36277625604` |
| `DEV-206` | attention view/governed drill-through | CI `36278067339` |
| `DEV-207` | durable issue consequence/outbox/recovery | CI `36279181900` |
| `DEV-208` | consolidated traceability/verification record | CI `36279763189` — passed before closure-status update; PR #29 reruns the exact final documentation head before merge |

## Requirement and use-case traceability

| Requirement / use case | First-slice interpretation | Architecture / design | Principal implementation | Verification evidence |
|---|---|---|---|---|
| `UC-002` Create, review and issue a governed work product | primary end-to-end workflow | `NBEOS-H-004`; ADR-001, ADR-002, ADR-007, ADR-017, ADR-018, ADR-021 | `NuBlox.WorkProducts.Domain`, `NuBlox.WorkProducts.Application`, `NuBlox.WorkProducts.Infrastructure.PostgreSql` | Work Product unit and PostgreSQL integration suites; DEV-202–207 CI evidence |
| `UC-001` Initiate and complete controlled work | submission creates controlled review work and state progression | `NBEOS-H-004`; ADR-001, ADR-017 | Work Product application submission/review contracts and persistence | `WorkProductTests.cs`; `WorkProductPersistenceTests.cs`; DEV-203 CI |
| `UC-003` Make an authorised business decision | approval/rejection is separate from technical access | `NBEOS-H-004`; ADR-009; ADR-018 | authority evaluation plus Work Product decision application/domain/persistence | `WorkProductDecisionTests.cs`; `WorkProductDecisionPersistenceTests.cs`; DEV-204 CI |
| `UC-010` Reconstruct a historical business event | submitted, decision and issue evidence remain attributable and reconstructable | ADR-018; ADR-020 | immutable decision/issue evidence and revision state/history | decision/issue unit and PostgreSQL integration suites; DEV-204/205/206 CI |
| `FR-005`–`FR-008` governed authoritative records/history | Work Product and revision records are typed, tenant-owned and persisted atomically | ADR-002, ADR-007, ADR-017, ADR-020, ADR-021 | Work Product domain model; PostgreSQL schema/migrations/repository | `WorkProductTests.cs`; `WorkProductPersistenceTests.cs`; DEV-202 CI |
| `FR-009`–`FR-014` controlled work/state/routing | Draft submission creates controlled review state/request | ADR-001, ADR-017 | submission/routing application behavior and review-request persistence | `WorkProductTests.cs`; PostgreSQL Work Product persistence suite; DEV-203 CI |
| `FR-015`–`FR-018` decision/approval/authority | review outcomes require contextual business authority and immutable evidence | ADR-009, ADR-018 | decision domain/application plus PostgreSQL decision repository | `WorkProductDecisionTests.cs`; `WorkProductDecisionPersistenceTests.cs`; DEV-204 CI |
| `FR-019`–`FR-022` work-product/evidence linkage | issue is tied to exact approved revision/decision and supersession is controlled | ADR-017, ADR-018, ADR-020 | issue application/domain plus PostgreSQL issue persistence | `WorkProductIssueTests.cs`; `WorkProductIssuePersistenceTests.cs`; DEV-205 CI |
| `FR-027`–`FR-029`, `FR-041` operational attention/read model | contributor/reviewer attention derives from authoritative source state and permits governed drill-through | ADR-015, ADR-017 | Work Product attention application/read repository | `WorkProductAttentionPersistenceTests.cs`; DEV-206 CI |
| `FR-018`, `FR-030`, `FR-032`–`FR-035` durable consequence/integration behavior | issuing a revision records durable delivery intent with recovery/idempotency controls | ADR-003, ADR-018 | Work Product issue/delivery persistence and outbox claiming/reconciliation | `WorkProductDeliveryPersistenceTests.cs`; DEV-207 CI |
| `FR-003`; tenant isolation/security requirements | route/body/header identity cannot manufacture tenant access | ADR-007, ADR-008, ADR-011, ADR-021 | `NuBlox.Identity`; verified request context; PostgreSQL tenant session/RLS | identity tests, API tests, persistence tenant-negative suites, DEV-105 onward CI |
| `FR-039`, `FR-040`; audit/operability NFRs | authoritative business evidence is separate from technical telemetry | ADR-016, ADR-018 | `NuBlox.Audit`; `NuBlox.Observability`; Work Product evidence contracts | audit/observability suites plus first-slice evidence persistence |

## Acceptance-criteria verification matrix

The numbered criteria below are the acceptance criteria from `NBEOS-H-004`.

| AC | Required proof | Implementation evidence | Automated verification | Result |
|---|---|---|---|---|
| `FV-AC-01` | authorised Tenant A contributor can create a Work Product and initial revision | Work Product creation application/domain; PostgreSQL repository/migration | `tests/NuBlox.WorkProducts.Tests/WorkProductTests.cs`; `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductPersistenceTests.cs` | **Verified** |
| `FV-AC-02` | Tenant B cannot read/mutate/submit/decide/issue Tenant A records by manipulating identifiers/context | verified Principal/Tenant request context plus PostgreSQL RLS on Work Product tables | identity/API negative tests plus Work Product PostgreSQL tenant-isolation tests across persistence, decision, issue, attention and delivery suites | **Verified** |
| `FV-AC-03` | invalid lifecycle transitions fail without partial authoritative mutation | Work Product/revision state invariants and atomic repository operations | `WorkProductTests.cs`, `WorkProductDecisionTests.cs`, `WorkProductIssueTests.cs` and corresponding PostgreSQL suites | **Verified** |
| `FV-AC-04` | submitted/decided/issued history remains reconstructable | immutable revision snapshots, review/decision evidence, issue evidence | decision/issue persistence suites and governed drill-through suite | **Verified** |
| `FV-AC-05` | decisions are attributable to verified Principal and exact revision | `DecisionEvidence` plus verified request Principal and review-request/revision linkage | `WorkProductDecisionTests.cs`; `WorkProductDecisionPersistenceTests.cs` | **Verified** |
| `FV-AC-06` | approval requires business authority beyond read/write permission | contextual authority evaluation contract separated from access permission | `WorkProductDecisionTests.cs` positive/negative authority scenarios; DEV-204 CI | **Verified** |
| `FV-AC-07` | only an approved revision may be issued | issue application invariant and persisted approval-decision linkage | `WorkProductIssueTests.cs`; `WorkProductIssuePersistenceTests.cs` | **Verified** |
| `FV-AC-08` | issuing a newer revision preserves prior issued revision/evidence | atomic issue/supersession behavior with immutable issue evidence | `WorkProductIssuePersistenceTests.cs`; DEV-205 CI | **Verified** |
| `FV-AC-09` | API failures use stable RFC 9457 semantics and do not leak stack traces/secrets | DEV-108 ASP.NET Core API host/problem-details baseline and verified request context boundary | `tests/NuBlox.Api.Tests/` plus production verifier | **Verified for platform/API contract boundary** |
| `FV-AC-10` | material operations produce authoritative audit evidence plus correlated technical telemetry | `NuBlox.Audit` and `NuBlox.Observability` are structurally separate production primitives; Work Product decision/issue evidence is authoritative | audit/observability suites plus Work Product evidence tests | **Verified for implemented first-slice material events** |
| `FV-AC-11` | clean-checkout CI verifies unit/API/PostgreSQL/tenant-negative/lifecycle-negative paths | one deterministic `scripts/verify-production.sh` entry point used by `Production foundation` | CI evidence for DEV-202–207 plus DEV-208 CI `36279763189`; exact closure head verified by PR #29 before merge | **Verified** |
| `FV-AC-12` | requirement → design/ADR → code → test traceability is recorded | this controlled document plus updated F-section RTM | document review + DEV-208 CI `36279763189`; exact closure head verified by PR #29 before merge | **Verified by DEV-208** |

## Architecture-to-code traceability

| Accepted ADR | First-slice obligation | Production boundary / implementation |
|---|---|---|
| ADR-001 cohesive modular application | business capability remains a cohesive module with explicit boundaries | `src/NuBlox.WorkProducts.Domain/`, `src/NuBlox.WorkProducts.Application/`, `src/NuBlox.WorkProducts.Infrastructure.PostgreSql/` |
| ADR-002 transactional relational persistence | authoritative state and related invariants commit transactionally | Work Product PostgreSQL repositories/migrations |
| ADR-003 durable asynchronous processing | required independently retried issue consequence commits as durable intent with recovery/idempotency | Work Product delivery/outbox persistence and claim/reconciliation implementation |
| ADR-007 layered tenant isolation | tenant identity is mandatory in application and persistence controls | verified request Tenant context, tenant-aware keys, PostgreSQL RLS |
| ADR-008 federated application identity | external identity is translated into provider-neutral authenticated Principal/context | `src/NuBlox.Identity/` and API request-context integration |
| ADR-009 business authority | decision authority remains distinct from technical permission | authority evaluation contracts used by Work Product decision behavior |
| ADR-011 HTTP API standards | stable versioned host/problem semantics and fail-closed request context | `src/NuBlox.Api/`, `/api/v1`, RFC 9457 Problem Details infrastructure |
| ADR-015 operational read/query model | management attention view is derived/read-oriented and governed | Work Product attention reader/application |
| ADR-016 observability | telemetry/correlation remains technical evidence, not business authority | `src/NuBlox.Observability/` |
| ADR-017 module-owned data | WorkProducts owns its authoritative schema/migrations/repositories | `src/NuBlox.WorkProducts.Infrastructure.PostgreSql/` and `work_products` namespace |
| ADR-018 business audit evidence | decision/issue facts are append-oriented attributable evidence | `src/NuBlox.Audit/`; Work Product decision/issue evidence models and persistence |
| ADR-020 release/schema evolution | migrations are ordered, controlled and non-destructive to historical meaning | production migration framework + Work Product module migrations |
| ADR-021 PostgreSQL 18 primary provider | provider-specific SQL/RLS remains in PostgreSQL infrastructure | Work Product PostgreSQL infrastructure and PostgreSQL 18.6 CI service |

## Code-to-test inventory

### Work Product domain/application tests

- `tests/NuBlox.WorkProducts.Tests/WorkProductTests.cs`
- `tests/NuBlox.WorkProducts.Tests/WorkProductDecisionTests.cs`
- `tests/NuBlox.WorkProducts.Tests/WorkProductIssueTests.cs`

### Work Product PostgreSQL integration tests

- `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductPersistenceTests.cs`
- `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductDecisionPersistenceTests.cs`
- `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductIssuePersistenceTests.cs`
- `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductAttentionPersistenceTests.cs`
- `tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/WorkProductDeliveryPersistenceTests.cs`

### Supporting platform verification

- `tests/NuBlox.Identity.Tests/`
- `tests/NuBlox.Api.Tests/`
- `tests/NuBlox.Audit.Tests/`
- `tests/NuBlox.Observability.Tests/`
- `tests/NuBlox.Persistence.PostgreSql.IntegrationTests/`

All are exercised through the controlled production verification path where applicable.

## Deviations and residual validation

### No unverified first-slice implementation gap recorded

The current automated implementation evidence covers all 12 `NBEOS-H-004` acceptance criteria at the representative production-slice level.

### Requirements remain Draft/Candidate

This technical verification does **not** claim that every related business/software requirement is finally validated with customers or approved as permanent scope. The first slice proves that the selected architecture and implementation can execute the representative governed-work-product workflow safely and traceably.

### Later validation still required

Later programme work must still validate and/or extend:

- discipline-specific Work Product types and metadata;
- external participant/recipient lifecycle;
- binary/object content storage under ADR-004;
- final organisation/job/position authority models;
- customer-configurable variation and workflow policy;
- search/indexing beyond direct governed retrieval;
- UX/usability and measurable business-outcome evidence with representative users.

These are controlled future scope, not failures of DEV-208.

## DEV-208 completion position

The DEV-208 evidence set now satisfies the controlled completion criteria:

1. first-slice requirements/use cases are mapped to accepted architecture and implementation;
2. all 12 `NBEOS-H-004` acceptance criteria have named verification evidence and status;
3. source/test paths and prior CI evidence are recorded;
4. the F-section RTM acknowledges live architecture/design/code/test evidence;
5. CI `36279763189` passed the full production verification path after the traceability record and RTM reconciliation were introduced;
6. PR #29 reruns the same gate on the exact closure-status head before merge.

DEV-208 is considered complete only when PR #29's exact closure-status head is green and merged to `main`.

## References

- `First_vertical_slice_definition.md`
- `Product_backlog.md`
- `Development_plan.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/Use_cases.md`
- `../F_Requirements_Analysis/Acceptance_criteria.md`
- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../../scripts/verify-production.sh`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-27 | NuBlox Product / Engineering / Quality | Established consolidated first-slice requirement → ADR/design → code → test → CI traceability for DEV-208 |
| 0.2 | 2026-09-27 | NuBlox Product / Engineering / Quality | Recorded successful DEV-208 production verification evidence from CI run 36279763189 and defined exact-head closure verification through PR #29 |
