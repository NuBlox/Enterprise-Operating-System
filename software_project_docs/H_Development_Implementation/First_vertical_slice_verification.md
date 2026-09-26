# First vertical slice verification

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-005  
**Document Type:** Verification and traceability report  
**Version:** 0.1  
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
**Supersedes:** None  
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

## Requirement and architecture trace

| Requirement / control | Slice behaviour | Architecture decisions / controls | Implementation / test evidence | Position |
|---|---|---|---|---|
| `FR-005`–`FR-008` | governed records, revisions, history and authoritative state | ADR-001, ADR-002, ADR-007, ADR-017, ADR-020, ADR-021 | WorkProduct domain + PostgreSQL migrations/RLS; DEV-202 CI | Verified for bounded slice |
| `FR-009`–`FR-014` | initiate/submit/route controlled work | ADR-001, ADR-007, ADR-017 | submission service, immutable submitted fields, ReviewRequest routing; DEV-203 CI | Verified for bounded slice |
| `FR-015`–`FR-018` | attributable decision with separate business authority | ADR-009, ADR-018 | decision domain/application/persistence + authority-negative tests; DEV-204 CI | Verified for bounded slice |
| `FR-019`–`FR-022` | exact revision/decision/issue evidence relationships | ADR-017, ADR-018 | IssueEvidence and exact approval link; DEV-205 CI | Verified for bounded slice |
| `FR-027`–`FR-029`, `FR-041` | actionable attention and governed source drill-through | ADR-007, ADR-015 (still Proposed for broader analytics) | derived attention reader + evidence reconstruction; DEV-206 CI | Verified for bounded operational view only |
| `FR-030`, `FR-032`–`FR-035` | durable/reconciliable consequence | ADR-003, ADR-007, ADR-017, ADR-020, ADR-021 | transactional delivery intent, RLS, lease/retry/idempotency/reconciliation; DEV-207 CI | Verified for provider-neutral issue consequence; no external provider selected |
| `NFR-SEC-*` / tenant isolation | Tenant B cannot access Tenant A records | ADR-007, ADR-008, ADR-021 | PostgreSQL RLS and negative integration tests | Verified at persistence/application boundaries |
| `NFR-AUD-*` | authoritative reconstructable business evidence | ADR-018 | submitted/decision/issue records are immutable/reconstructable | Partially verified; generic `NuBlox.Audit` appender is not yet wired into WorkProducts operations |
| `NFR-OPS-*` | correlation/telemetry | ADR-016 | platform OpenTelemetry primitives and CI exist | Platform verified; WorkProducts operation instrumentation not yet wired |
| `ADR-011` API contract | versioned HTTP + RFC 9457 failures | ADR-008, ADR-011, ADR-012 | API host foundation verified independently | Platform verified; WorkProducts endpoints/request-context composition not yet implemented |

## NBEOS-H-004 acceptance-criteria assessment

| # | Acceptance criterion | Evidence | Result |
|---|---|---|---|
| 1 | Authenticated authorised contributor in Tenant A can create Work Product/revision | Domain/application/PostgreSQL creation is verified, but the WorkProducts operation is not yet composed behind `AuthenticatedRequestContext` in the HTTP host | **Partial** |
| 2 | Tenant B cannot read/mutate/submit/decide/issue Tenant A by changing request identifiers | RLS/application cross-tenant negative tests prove the persistence boundary; no WorkProducts HTTP route/body/header tampering test exists because endpoints are not composed | **Partial** |
| 3 | Invalid lifecycle transitions reject without partial mutation | Domain state-machine tests plus PostgreSQL transactional/concurrency tests | **Pass** |
| 4 | Submitted/decided/issued history remains reconstructable | submitted fields, ReviewRequest, immutable ReviewDecision, IssueEvidence and drill-through tests | **Pass** |
| 5 | Decisions attributable to verified Principal and exact revision | decision/issue evidence links actor, request, exact revision and approval evidence | **Pass** at application/persistence boundary |
| 6 | Approval cannot be completed from read/write access alone | separate contextual `IBusinessAuthorityEvaluator` and authority-negative tests | **Pass** |
| 7 | Only approved revision may be issued | issue-domain/application negative tests and PostgreSQL state guard | **Pass** |
| 8 | New issue does not rewrite previous issued revision/evidence | atomic supersession and immutable issue-evidence verification | **Pass** |
| 9 | API failures use RFC 9457 and do not leak stacks/secrets | API host foundation proves generic RFC 9457/sanitisation, but no WorkProducts HTTP endpoints are composed | **Partial** |
| 10 | Material operations produce authoritative audit evidence + correlated telemetry | WorkProducts has domain-specific immutable evidence; `NuBlox.Audit` and `NuBlox.Observability` are not wired to WorkProducts material operations | **Not yet satisfied end to end** |
| 11 | Clean-checkout CI verifies unit, API, PostgreSQL, tenant/lifecycle negative paths | production verifier runs all these suites; WorkProducts unit/PostgreSQL paths are included | **Pass**, with API-to-WorkProducts gap noted above |
| 12 | Requirement → design/ADR → code → test traceability recorded | this document plus RTM update | **Pass for recorded evidence; overall slice remains open because criteria 1/2/9/10 are incomplete** |

## Verification conclusion

`DEV-202` through `DEV-207` have produced a substantial and independently verified production business core, but **the first vertical slice is not yet accepted end to end**.

The remaining gaps are integration boundaries rather than missing core Work Product semantics:

1. **DEV-209 — WorkProducts HTTP composition and verified request-context boundary**
   - expose the bounded Work Product operations through `/api/v1/{tenantSlug}/...`;
   - derive trusted Tenant/Principal only from `AuthenticatedRequestContext`;
   - prove route/body/header values cannot manufacture Tenant access;
   - map lifecycle/access/not-found failures to stable RFC 9457 categories without leaking exception details.

2. **DEV-210 — WorkProducts authoritative audit + correlated telemetry integration**
   - append governed `NuBlox.Audit` evidence for required material operations or explicitly map domain evidence into the authoritative audit boundary;
   - instrument WorkProducts operations with `NuBlox.Observability` correlation/traces/metrics;
   - verify audit and telemetry remain separate and payload-minimised.

`DEV-208` remains **In Progress** until DEV-209 and DEV-210 pass and this report can record all twelve acceptance criteria as satisfied or an explicitly approved deviation exists.

## Evidence quality / limitations

- F-section source requirements remain Draft/Candidate; this report verifies the bounded implementation/validation slice and does not silently approve the full requirement catalogue.
- No external notification/integration provider is selected; DEV-207 proves the durable provider-neutral consequence boundary and recovery semantics.
- Binary work-product content remains out of scope under ADR-004.
- Broader analytics/search/external participation remain later capabilities.
- Human/business acceptance and customer evidence remain separate from automated technical verification.

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
