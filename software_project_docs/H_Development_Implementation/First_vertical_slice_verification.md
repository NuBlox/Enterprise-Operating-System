# First vertical slice verification

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-005  
**Document Type:** Verification and traceability report  
**Version:** 0.3  
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
**Supersedes:** Version 0.2  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/First_vertical_slice_verification.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Consolidate objective evidence for the first NuBlox production vertical slice, **Governed Work Product — Create, Review, Approve and Issue**, and determine whether all acceptance criteria in NBEOS-H-004 are satisfied end to end.

This report is evidence-led. A backlog item being marked complete is not, by itself, proof that the complete vertical slice is accepted.

## Verification baseline

The bounded production slice now proves:

```text
verified HTTP Principal/Tenant context
        ↓
Work Product + Revision creation
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
minimised authoritative audit + correlated telemetry
        ↓
real production composition root
        ↓
restricted-role PostgreSQL persistence + tenant RLS
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
| `DEV-202` | WorkProduct + Revision records/persistence | `NuBlox.WorkProducts.Domain`, `.Application`, `.Infrastructure.PostgreSql`; `wp_0001_work_products.sql` | `WorkProductTests.cs`; `WorkProductPersistenceTests.cs` | `36275665645` |
| `DEV-203` | Submission/routing/state | `WorkProductApplicationService`; `ReviewRequest`; `wp_0002_review_requests.sql` | Work Product unit tests and PostgreSQL routing/isolation tests | `36276125829` |
| `DEV-204` | Decision/authority | `ReviewDecision`; contextual authority integration; `wp_0003_review_decisions.sql` | `WorkProductDecisionTests.cs`; `WorkProductDecisionPersistenceTests.cs` | `36277012364` |
| `DEV-205` | Issue/evidence linkage | `WorkProductIssueService`; `IssueEvidence`; `wp_0004_issue_evidence.sql` | `WorkProductIssueTests.cs`; `WorkProductIssuePersistenceTests.cs` | `36277625604` |
| `DEV-206` | Attention/read model + drill-through | `WorkProductAttentionApplication`; `PostgresWorkProductAttentionReader` | `WorkProductAttentionPersistenceTests.cs` including unauthorised and cross-tenant drill-through | `36278067339` / final `36278227792` |
| `DEV-207` | Durable external consequence | accepted `ADR-003`; `WorkProductDeliveryApplication`; `PostgresWorkProductDeliveryStore`; `wp_0005_delivery_intents.sql` | `WorkProductDeliveryPersistenceTests.cs` including lease recovery, retry/idempotency, concurrency and tenant isolation | `36279181900` / final `36279302579` |
| `DEV-209` | HTTP verified-context composition | Work Product create/read/submit/decide/issue routes + `IWorkProductHttpOperations` | fail-closed context, route/body/header tamper, RFC 9457 and OpenAPI tests | `36280459358` / final `36280622610` |
| `DEV-210` | Authoritative audit + correlated telemetry composition | `AuditedObservedWorkProductHttpOperations`; `NuBlox.Audit`; `NuBlox.Observability` | material-operation audit, correlation, minimisation, failure and separation tests | `36281051957` plus PR #31 final verification |
| `DEV-211` | Real production runtime composition | `NuBlox.Runtime.PostgreSql`; `RuntimeServiceCollectionExtensions`; `PostgresAuditEvidenceAppender`; combined runtime migration catalogue; production API registration | `WorkProductRuntimeCompositionTests.cs`: real create → submit → approve → issue over HTTP, Tenant B isolation, durable audit persistence and payload minimisation | `36281993924` before documentation-final rerun |

## Requirement and architecture trace

| Requirement / control | Slice behaviour | Architecture decisions / controls | Implementation / test evidence | Position |
|---|---|---|---|---|
| `FR-005`–`FR-008` | governed records, revisions, history and authoritative state | ADR-001, ADR-002, ADR-007, ADR-017, ADR-020, ADR-021 | WorkProduct domain + PostgreSQL migrations/RLS; DEV-202 and DEV-211 | Verified for bounded slice |
| `FR-009`–`FR-014` | initiate/submit/route controlled work | ADR-001, ADR-007, ADR-017 | submission service, immutable submitted fields, ReviewRequest routing; DEV-203 and real runtime flow | Verified for bounded slice |
| `FR-015`–`FR-018` | attributable decision with separate business authority | ADR-009, ADR-018 | decision domain/application/persistence + authority-negative tests + DEV-211 real reviewer decision | Verified for bounded slice |
| `FR-019`–`FR-022` | exact revision/decision/issue evidence relationships | ADR-017, ADR-018 | IssueEvidence and exact approval link; DEV-205 + DEV-211 real issue | Verified for bounded slice |
| `FR-027`–`FR-029`, `FR-041` | actionable attention and governed source drill-through | ADR-007, ADR-015 (still Proposed for broader analytics) | derived attention reader + evidence reconstruction; DEV-206 | Verified for bounded operational view only |
| `FR-030`, `FR-032`–`FR-035` | durable/reconciliable consequence | ADR-003, ADR-007, ADR-017, ADR-020, ADR-021 | transactional delivery intent, RLS, lease/retry/idempotency/reconciliation; DEV-207 | Verified for provider-neutral issue consequence; no external provider selected |
| `NFR-SEC-*` / tenant isolation | Tenant B cannot access Tenant A records | ADR-007, ADR-008, ADR-021 | PostgreSQL RLS, application negatives, DEV-209 tamper tests and DEV-211 real-host Tenant B `404` | Verified across HTTP/application/persistence boundary |
| `NFR-AUD-*` | authoritative reconstructable business evidence | ADR-018 | immutable domain evidence, DEV-210 audit composition, durable PostgreSQL `PostgresAuditEvidenceAppender` and four material audit records in DEV-211 | Verified across composed runtime boundary |
| `NFR-OPS-*` | correlation/telemetry | ADR-016 | DEV-210 activity/correlation/metric primitives with payload-minimisation tests; same operation wrapper used in DEV-211 real graph | Verified for bounded slice |
| `ADR-011` API contract | versioned HTTP + RFC 9457 failures | ADR-008, ADR-011, ADR-012 | DEV-209 routes/error/OpenAPI tests plus DEV-211 real production dependency composition | Verified across real host boundary |

## NBEOS-H-004 acceptance-criteria assessment

| # | Acceptance criterion | Evidence | Result |
|---|---|---|---|
| 1 | Authenticated authorised contributor in Tenant A can create Work Product/revision | DEV-209 verified-context endpoint plus DEV-211 real HTTP create using concrete PostgreSQL repository | **Pass** |
| 2 | Tenant B cannot read/mutate/submit/decide/issue Tenant A by changing request identifiers | RLS/application negatives, DEV-209 route/body/header tamper tests and DEV-211 real Tenant B request returning `404` | **Pass** |
| 3 | Invalid lifecycle transitions reject without partial mutation | Domain state-machine tests plus PostgreSQL transactional/concurrency tests | **Pass** |
| 4 | Submitted/decided/issued history remains reconstructable | submitted fields, ReviewRequest, immutable ReviewDecision, IssueEvidence and drill-through tests | **Pass** |
| 5 | Decisions attributable to verified Principal and exact revision | decision/issue evidence links actor, request, exact revision and approval evidence; DEV-211 reviewer path executes through real host | **Pass** |
| 6 | Approval cannot be completed from read/write access alone | separate contextual `IBusinessAuthorityEvaluator`, authority-negative tests and fail-closed configured runtime evaluator | **Pass** |
| 7 | Only approved revision may be issued | issue-domain/application negative tests and PostgreSQL state guard; DEV-211 approve-then-issue flow | **Pass** |
| 8 | New issue does not rewrite previous issued revision/evidence | atomic supersession and immutable issue-evidence verification | **Pass** |
| 9 | API failures use RFC 9457 and do not leak stacks/secrets | DEV-209 sanitised stable failure-category tests; DEV-211 proves real graph reaches same API boundary | **Pass** |
| 10 | Material operations produce authoritative audit evidence + correlated telemetry | DEV-210 operation composition plus DEV-211 durable PostgreSQL appender; create/submit/approve/issue produce four tenant-scoped events and title/rationale are absent from audit subject/reason fields | **Pass** |
| 11 | Clean-checkout CI verifies unit, API, PostgreSQL, tenant/lifecycle negative paths | production verifier executes all suites plus DEV-211 real HTTP→PostgreSQL smoke path against PostgreSQL 18.6 | **Pass — run `36281993924`; documentation-final rerun required before merge** |
| 12 | Requirement → design/ADR → code → test traceability recorded | NBEOS-H-004/H-005, NBEOS-F-015 v0.3 and controlled backlog evidence | **Pass** |

## Verification conclusion

`DEV-202` through `DEV-211` now cover the business core, verified HTTP Principal/Tenant boundary, contextual authority, PostgreSQL persistence/RLS, durable consequence handling, authoritative audit evidence, correlated telemetry and the real production service graph.

**Technical verification result: the bounded first vertical slice satisfies all twelve NBEOS-H-004 acceptance criteria.** No technical deviation is required for this bounded slice.

This conclusion is deliberately narrow. It is technical acceptance of the controlled first implementation slice, not approval of the complete NuBlox product, the full F-section Draft/Candidate requirement catalogue, customer/business acceptance, or a production release decision.

A defect was found and corrected during DEV-211 verification: the first runtime composition registered the base `PostgresWorkProductRepository`, which could not reconstruct governed review-decision context and caused the real reviewer decision endpoint to return `404`. The runtime now registers `PostgresGovernedWorkProductRepository`; the same real HTTP test then passed. This defect would not have been exposed by the earlier component tests alone and is retained as evidence for the value of composition-root verification.

## Evidence quality / limitations

- F-section source requirements remain Draft/Candidate; this report verifies the bounded implementation/validation slice and does not silently approve the full requirement catalogue.
- No external notification/integration provider is selected; DEV-207 proves the durable provider-neutral consequence boundary and recovery semantics.
- Binary work-product content remains out of scope under ADR-004.
- Broader analytics/search/external participation remain later capabilities.
- Runtime approval principals are currently supplied through controlled configuration; broader organisation/job/position authority semantics remain future product work.
- Human/business acceptance and customer evidence remain separate from automated technical verification.
- The final documentation head must pass the same production verifier before merge; the pre-documentation DEV-211 implementation head passed run `36281993924`.

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
| 0.3 | 2026-09-27 | NuBlox Product / Engineering / Quality | Closed DEV-211 and DEV-208 after the real production HTTP-to-PostgreSQL graph, restricted runtime role, tenant isolation and durable audit path passed; recorded the governed-repository DI defect found by end-to-end composition verification |
