# First vertical slice verification

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-005  
**Document Type:** Verification and traceability report  
**Version:** 0.2  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering / Quality  
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
**Related Documents:** `First_vertical_slice_definition.md`, `Product_backlog.md`, `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** Version 0.1  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/First_vertical_slice_verification.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Consolidate objective evidence for the first NuBlox production vertical slice, **Governed Work Product — Create, Review, Approve and Issue**, and determine whether all acceptance criteria in NBEOS-H-004 are satisfied end to end.

This report is deliberately evidence-led. A backlog item being marked complete is not, by itself, proof that the complete vertical slice is accepted.

## Verification baseline

The implementation currently proves the WorkProducts domain/application/PostgreSQL workflow through:

```text
WorkProduct + Revision creation
        ↓
review submission and routed ReviewRequest
        ↓
contextual business-authority decision
        ↓
approval-linked issue evidence / supersession
        ↓
Principal-scoped attention + governed source drill-through
        ↓
transactional durable issue delivery intent
        ↓
recoverable retry / reconciliation state
        ↓
verified HTTP Tenant/Principal boundary
        ↓
minimised authoritative audit + correlated telemetry composition
```

The production verification entry point is:

```bash
bash scripts/verify-production.sh
```

GitHub Actions executes the same verifier with PostgreSQL 18.6.

## Backlog-to-evidence traceability

| Backlog | Capability | Principal implementation | Automated verification | CI evidence |
|---|---|---|---|---|
| `DEV-201` | Slice selection/baseline | `First_vertical_slice_definition.md` | controlled review of NBEOS-H-004 | NBEOS-H-004 v0.1 |
| `DEV-202` | WorkProduct + Revision records/persistence | `NuBlox.WorkProducts.Domain`, `NuBlox.WorkProducts.Application`, `NuBlox.WorkProducts.Infrastructure.PostgreSql`; `wp_0001_work_products.sql` | `WorkProductTests.cs`; `WorkProductPersistenceTests.cs` | `36275665645` |
| `DEV-203` | Submission/routing/state | `WorkProductApplicationService`; `ReviewRequest`; `wp_0002_review_requests.sql` | Work Product unit tests and PostgreSQL routing/isolation tests | `36276125829` |
| `DEV-204` | Decision/authority | `ReviewDecision`; contextual authority integration; `wp_0003_review_decisions.sql` | `WorkProductDecisionTests.cs`; `WorkProductDecisionPersistenceTests.cs` | `36277012364` |
| `DEV-205` | Issue/evidence linkage | `WorkProductIssueService`; `IssueEvidence`; `wp_0004_issue_evidence.sql` | `WorkProductIssueTests.cs`; `WorkProductIssuePersistenceTests.cs` | `36277625604` |
| `DEV-206` | Attention/read model + drill-through | `WorkProductAttentionApplication`; `PostgresWorkProductAttentionReader` | `WorkProductAttentionPersistenceTests.cs` including unauthorised and cross-tenant drill-through | `36278067339` / final `36278227792` |
| `DEV-207` | Durable external consequence | accepted `ADR-003`; `WorkProductDeliveryApplication`; `PostgresWorkProductDeliveryStore`; `wp_0005_delivery_intents.sql` | `WorkProductDeliveryPersistenceTests.cs` including lease recovery, retry/idempotency, concurrency and tenant isolation | `36279181900` / final `36279302579` |
| `DEV-209` | HTTP verified-context composition | Work Product create/read/submit/decide/issue routes + `IWorkProductHttpOperations` | fail-closed context, route/body/header tamper, RFC 9457 and OpenAPI tests | `36280459358` / final `36280622610` |
| `DEV-210` | authoritative audit + correlated telemetry composition | `AuditedObservedWorkProductHttpOperations`; `NuBlox.Audit`; `NuBlox.Observability` | material-operation audit, correlation, minimisation, failure and separation tests | `36281051957` before documentation-final rerun |

## Requirement and architecture trace

| Requirement / control | Slice behaviour | Architecture decisions / controls | Implementation / test evidence | Position |
|---|---|---|---|---|
| `FR-005`–`FR-008` | governed records, revisions, history and authoritative state | ADR-001, ADR-002, ADR-007, ADR-017, ADR-020, ADR-021 | WorkProduct domain + PostgreSQL migrations/RLS; DEV-202 CI | Verified for bounded slice |
| `FR-009`–`FR-014` | initiate/submit/route controlled work | ADR-001, ADR-007, ADR-017 | submission service, immutable submitted fields, ReviewRequest routing; DEV-203 CI | Verified for bounded slice |
| `FR-015`–`FR-018` | attributable decision with separate business authority | ADR-009, ADR-018 | decision domain/application/persistence + authority-negative tests; DEV-204 CI | Verified for bounded slice |
| `FR-019`–`FR-022` | exact revision/decision/issue evidence relationships | ADR-017, ADR-018 | IssueEvidence and exact approval link; DEV-205 CI | Verified for bounded slice |
| `FR-027`–`FR-029`, `FR-041` | actionable attention and governed source drill-through | ADR-007, ADR-015 (still Proposed for broader analytics) | derived attention reader + evidence reconstruction; DEV-206 CI | Verified for bounded operational view only |
| `FR-030`, `FR-032`–`FR-035` | durable/reconciliable consequence | ADR-003, ADR-007, ADR-017, ADR-020, ADR-021 | transactional delivery intent, RLS, lease/retry/idempotency/reconciliation; DEV-207 CI | Verified for provider-neutral issue consequence; no external provider selected |
| `NFR-SEC-*` / tenant isolation | Tenant B cannot access Tenant A records | ADR-007, ADR-008, ADR-021 | PostgreSQL RLS, application negatives and DEV-209 HTTP route/body/header tamper tests | Verified at persistence/application/HTTP security boundary; real production service graph still requires runtime composition proof |
| `NFR-AUD-*` | authoritative reconstructable business evidence | ADR-018 | immutable domain evidence plus DEV-210 governed `AuditEvidence` append composition for create/submit/decide/issue | Verified for composed operation contract; durable production audit-appender registration remains part of runtime composition proof |
| `NFR-OPS-*` | correlation/telemetry | ADR-016 | DEV-210 activity/correlation/metric primitives with payload-minimisation tests | Verified for composed operation contract |
| `ADR-011` API contract | versioned HTTP + RFC 9457 failures | ADR-008, ADR-011, ADR-012 | DEV-209 routes, verified context, tamper negatives, RFC 9457 and OpenAPI tests | Verified at HTTP adapter boundary; real production service graph still requires runtime composition proof |

## NBEOS-H-004 acceptance-criteria assessment

| # | Acceptance criterion | Evidence | Result |
|---|---|---|---|
| 1 | Authenticated authorised contributor in Tenant A can create Work Product/revision | WorkProducts creation plus DEV-209 verified-context HTTP route exists, but the real host has not yet proved resolution of the complete WorkProducts repository/access/authority/audit dependency graph | **Partial — DEV-211 runtime composition required** |
| 2 | Tenant B cannot read/mutate/submit/decide/issue Tenant A by changing request identifiers | RLS/application negatives plus DEV-209 route/body/header tamper tests pass; full live-host path still depends on DEV-211 real service composition | **Partial — security boundary verified, runtime composition outstanding** |
| 3 | Invalid lifecycle transitions reject without partial mutation | Domain state-machine tests plus PostgreSQL transactional/concurrency tests | **Pass** |
| 4 | Submitted/decided/issued history remains reconstructable | submitted fields, ReviewRequest, immutable ReviewDecision, IssueEvidence and drill-through tests | **Pass** |
| 5 | Decisions attributable to verified Principal and exact revision | decision/issue evidence links actor, request, exact revision and approval evidence | **Pass** at application/persistence boundary |
| 6 | Approval cannot be completed from read/write access alone | separate contextual `IBusinessAuthorityEvaluator` and authority-negative tests | **Pass** |
| 7 | Only approved revision may be issued | issue-domain/application negative tests and PostgreSQL state guard | **Pass** |
| 8 | New issue does not rewrite previous issued revision/evidence | atomic supersession and immutable issue-evidence verification | **Pass** |
| 9 | API failures use RFC 9457 and do not leak stacks/secrets | DEV-209 WorkProducts HTTP conflict/access/context tests prove sanitised stable categories | **Pass at HTTP adapter boundary; DEV-211 must prove real composition can reach it** |
| 10 | Material operations produce authoritative audit evidence + correlated telemetry | DEV-210 appends minimised `AuditEvidence` for create/submit/decide/issue and emits correlated telemetry; reads remain telemetry-only; failures cannot manufacture successful audit evidence | **Pass for composed operation contract** |
| 11 | Clean-checkout CI verifies unit, API, PostgreSQL, tenant/lifecycle negative paths | production verifier runs all suites including DEV-209/210 tests | **Pass**, subject to DEV-211 runtime-composition smoke path |
| 12 | Requirement → design/ADR → code → test traceability recorded | this document plus RTM/backlog evidence | **Pass for recorded evidence; overall slice remains open because real host composition is not yet proven** |

## Verification conclusion

`DEV-202` through `DEV-210` now cover the business core, HTTP verified-context boundary and audit/telemetry operation composition. **The first vertical slice is still not accepted end to end.**

The remaining gap is a real production composition-root issue, not missing Work Product semantics:

### DEV-211 — production WorkProducts runtime composition

The current `NuBlox.Api` host maps the WorkProducts routes and registers the HTTP/audit/telemetry operation adapter, but the host does not yet register the complete concrete dependency graph needed to instantiate that adapter for a real request, including:

- Work Product PostgreSQL repository/issue repository implementations;
- production access/authority evaluators;
- system clock/runtime infrastructure;
- a durable production `IAuditEvidenceAppender`;
- PostgreSQL data source/tenant-session configuration at the composition root.

DEV-211 must provide a controlled composition root without violating the existing rule that `NuBlox.Api` itself must not become the PostgreSQL/Npgsql infrastructure owner. The resulting verification must execute at least one real HTTP WorkProducts flow against PostgreSQL using the production service graph rather than replacing `IWorkProductHttpOperations` with a test double.

`DEV-208` remains **In Progress** until DEV-211 passes and this report can record all twelve acceptance criteria as satisfied or an explicitly approved deviation exists.

## Evidence quality / limitations

- F-section source requirements remain Draft/Candidate; this report verifies the bounded implementation/validation slice and does not silently approve the full requirement catalogue.
- No external notification/integration provider is selected; DEV-207 proves the durable provider-neutral consequence boundary and recovery semantics.
- Binary work-product content remains out of scope under ADR-004.
- Broader analytics/search/external participation remain later capabilities.
- Human/business acceptance and customer evidence remain separate from automated technical verification.
- DEV-209/210 component and adapter tests do not substitute for DEV-211 real-host dependency-resolution and HTTP-to-PostgreSQL proof.

## References

- `First_vertical_slice_definition.md`
- `Product_backlog.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/Use_cases.md`
- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../G_Architecture_Design/adr/ADR-003-durable-asynchronous-processing.md`
- `../G_Architecture_Design/adr/ADR-007-layered-tenant-isolation.md`
- `../G_Architecture_Design/adr/ADR-009-contextual-business-authority.md`
- `../G_Architecture_Design/adr/ADR-011-http-api-standards.md`
- `../G_Architecture_Design/adr/ADR-016-opentelemetry-observability-baseline.md`
- `../G_Architecture_Design/adr/ADR-018-business-audit-evidence.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering / Quality | Consolidated DEV-202–207 evidence and identified the HTTP request-context plus audit/telemetry integration gaps that must close before first-slice acceptance |
| 0.2 | 2026-09-27 | NuBlox Product / Engineering / Quality | Reverified after DEV-209/210; closed HTTP and audit/telemetry composition gaps, identified DEV-211 real production runtime-composition proof as the remaining end-to-end blocker |
