# Requirements traceability matrix (RTM)

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-015  
**Document Type:** Requirements traceability matrix (RTM)  
**Version:** 0.4  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Business Analysis / Quality  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Functional_requirements_specification.md`, `Non-functional_requirements_specification.md`, `Acceptance_criteria.md`, `../H_Development_Implementation/First_vertical_slice_definition.md`, `../H_Development_Implementation/First_vertical_slice_verification.md`, `../H_Development_Implementation/Second_vertical_slice_definition.md`, `../H_Development_Implementation/Second_vertical_slice_verification.md`  
**Supersedes:** Version 0.3  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain end-to-end traceability from business intent through stakeholder/software requirements to architecture, implementation and verification evidence.

The matrix remains a Draft programme baseline because the underlying requirements remain Draft/Candidate. Version 0.4 adds the bounded governed Organisation Party foundation without implying that all linked requirements, a wider commercial/workforce model or the next market priority are generally approved product requirements.

## Traceability model

```text
Business requirement
→ Stakeholder need
→ Software requirement / supporting specification
→ Architecture / design
→ Implementation
→ Verification / acceptance evidence
```

## Business-requirement traceability matrix

| Business requirement | Principal stakeholder links | Principal software / supporting links | Planned / current verification | Status |
|---|---|---|---|---|
| `BR-001` End-to-end operational continuity | `SR-001`, `SR-002`, `SR-005`, `SR-015` | `FR-002`, `FR-009`–`FR-014`, `IF-001`–`IF-010` | first-slice controlled work routing/state evidence plus future cross-domain workflows | Candidate / partly exercised |
| `BR-002` Work must be executable | `SR-001`, `SR-004`, `SR-005` | `FR-009`–`FR-014`, `FR-018` | Work Product submission/review/decision flow; `AC-002` | Candidate / exercised in first slice |
| `BR-003` Organisational and delivery context connected | `SR-003`, `SR-010`, `SR-015` | `FR-002`, `FR-005`–`FR-008`, `FR-012`, `FR-023`; `DATA-001`–`DATA-006` | verified Tenant/Principal boundary plus canonical Organisation Party foundation; wider org/project/relationship context remains future | Candidate / partly exercised |
| `BR-004` Authoritative business information | `SR-013`, `SR-018`, `SR-020` | `FR-005`–`FR-008`, `FR-035`; `DATA-001`–`DATA-011`; `RULE-004` | Work Product authoritative state/history plus stable Organisation Party identity and ownership | Candidate / exercised in bounded slices |
| `BR-005` Work and outputs connected | `SR-003`, `SR-016`, `SR-017` | `FR-012`, `FR-019`–`FR-022`; `RULE-006` | Work Product revision/decision/issue evidence linkage; `AC-006` | Candidate / exercised in first slice |
| `BR-006` Responsibility, authority and access understandable | `SR-001`, `SR-009`, `SR-016`, `SR-023` | `FR-003`, `FR-010`, `FR-017`; `RULE-001`, `RULE-002`; `NFR-SEC-001` | authority/permission separation and negative tests; bounded Organisation administrator policy | Candidate / exercised in bounded slices |
| `BR-007` Decisions and approvals evidenced | `SR-016`, `SR-017`, `SR-018` | `FR-015`–`FR-018`, `FR-022`; `RULE-003`; `NFR-AUD-001`–`004` | immutable decision/issue reconstruction plus durable audit evidence | Candidate / exercised in first slice |
| `BR-008` Management information from operational work | `SR-006`, `SR-013`, `SR-014`, `SR-015` | `FR-028`, `FR-029`, `FR-041`; `REP-001`–`REP-012` | Principal-scoped attention view and governed source drill-through | Candidate / bounded first-slice evidence |
| `BR-009` Cross-functional work | `SR-005`, `SR-006`, `SR-008`, `SR-015` | `FR-010`–`FR-014`; `RULE-007` | later cross-team/domain scenarios | Candidate |
| `BR-010` Controlled external participation | `SR-031`, `SR-032`, `SR-023` | `FR-004`, `FR-003`; `RULE-008`; `NFR-SEC-001` | later external-user access/isolation/usability tests | Candidate |
| `BR-011` Variation without customer forks | `SR-022`, `SR-029` | `FR-036`–`FR-038`; `RULE-009`; `NFR-MNT-004` | later configuration/upgrade regression tests; `AC-010` | Candidate |
| `BR-012` Specialist-system interoperability | `SR-020`, `SR-028`, `SR-030` | `FR-032`–`FR-035`; `INT-001`–`INT-015`; `API-001`–`API-015` | durable provider-neutral issue consequence/reconciliation; external provider contracts later | Candidate / partly exercised |
| `BR-013` Feasible migration | `SR-028`, `SR-030` | `DATA-009`, `DATA-010`; migration requirements in `Data_requirements.md` | migration reconciliation; `AC-008` | Candidate |
| `BR-014` Preserve historical meaning | `SR-018`, `SR-016` | `FR-008`, `FR-020`, `FR-039`, `FR-040`; `NFR-DATA-004`; `NFR-AUD-001`–`004` | Work Product submitted/decision/issue/audit evidence reconstruction; `AC-005` | Candidate / exercised in first slice |
| `BR-015` Security, privacy and auditability | `SR-023`–`SR-026`, `SR-028` | `FR-003`, `FR-039`, `FR-040`; `NFR-SEC-*`, `NFR-PRV-*`, `NFR-AUD-*`; `DATA-007`, `DATA-013`, `DATA-015` | RLS/tenant/authority negatives across WorkProducts and Enterprise Organisation; real runtime audit proof | Candidate / exercised in bounded slices |
| `BR-016` Measurable priority workflows | `SR-013`, `SR-027` | `FR-041`; `REP-001`–`REP-012`; `ANA-001`–`ANA-012` | first-slice attention view; wider benefit/outcome measurement later | Candidate / partly exercised |
| `BR-017` Commercially implementable product | `SR-027`–`SR-030` | `NFR-MNT-*`, `NFR-OPS-*`, `NFR-PORT-*`; configuration/integration/migration controls | production foundation CI/dependency/operability evidence plus two bounded real-runtime slices | Candidate / partly exercised |
| `BR-018` Outcome-led scope | All stakeholder groups as applicable | requirements prioritisation, RTM and controlled change process | bounded WorkProduct acceptance plus deliberately scoped Organisation prerequisite without wider priority claim | Candidate / actively controlled |

## First governed vertical slice — downstream traceability

Controlled slice: `NBEOS-H-004 — Governed Work Product — Create, Review, Approve and Issue`.

| Requirements / use cases | Architecture / design | Implementation | Verification evidence | Current result |
|---|---|---|---|---|
| `UC-002`; `FR-005`–`FR-008` | ADR-001, 002, 007, 017, 020, 021; NBEOS-H-004 | `NuBlox.WorkProducts.Domain`, `.Application`, `.Infrastructure.PostgreSql`; `wp_0001_work_products.sql` | `WorkProductTests.cs`, `WorkProductPersistenceTests.cs`; CI `36275665645` | Verified bounded record/revision core |
| `UC-001/002`; `FR-009`–`FR-014` | ADR-001, 007, 017; NBEOS-H-004 | submission/state/routing in `WorkProductApplicationService`; `ReviewRequest`; `wp_0002_review_requests.sql` | unit + PostgreSQL routing/concurrency/isolation tests; CI `36276125829` | Verified bounded work state |
| `UC-003`; `FR-015`–`FR-018` | ADR-009, 018; NBEOS-H-004 | `ReviewDecision`, contextual authority contract, `wp_0003_review_decisions.sql` | `WorkProductDecisionTests.cs`, `WorkProductDecisionPersistenceTests.cs`; CI `36277012364` | Verified authority-backed decision |
| `UC-002/010`; `FR-019`–`FR-022` | ADR-017, 018; NBEOS-H-004 | `IssueEvidence`, approved-only issue/supersession, `wp_0004_issue_evidence.sql` | `WorkProductIssueTests.cs`, `WorkProductIssuePersistenceTests.cs`; CI `36277625604` | Verified issue/evidence linkage |
| `FR-027`–`FR-029`, `FR-041` | ADR-007 plus bounded operational-read approach; NBEOS-H-004 | `WorkProductAttentionApplication`, `PostgresWorkProductAttentionReader` | `WorkProductAttentionPersistenceTests.cs`; CI `36278067339` and final `36278227792` | Verified derived attention/drill-through |
| `FR-018`, `FR-030`, `FR-032`–`FR-035` | ADR-003, 007, 017, 020, 021 | `WorkProductDeliveryApplication`, `PostgresWorkProductDeliveryStore`, `wp_0005_delivery_intents.sql` | `WorkProductDeliveryPersistenceTests.cs`; CI `36279181900` and final `36279302579` | Verified durable/recoverable provider-neutral consequence |
| `FR-001`–`FR-004` + first-slice HTTP operations | ADR-008, 011, 012 | verified request context + WorkProducts HTTP endpoints + production runtime composition boundary | DEV-209 adapter/tamper/RFC 9457/OpenAPI tests plus DEV-211 real HTTP→application→PostgreSQL smoke test | **Verified end to end for bounded slice** |
| `NFR-AUD-*`, `NFR-OPS-*` for material WorkProduct operations | ADR-016, 018 | `AuditedObservedWorkProductHttpOperations`, `NuBlox.Audit`, `NuBlox.Observability`, `PostgresAuditEvidenceAppender`, `audit_0001_evidence.sql` | DEV-210 minimisation/correlation tests plus DEV-211 durable audit persistence proof | **Verified end to end for bounded slice** |
| `NFR-SEC-*` tenant isolation | ADR-007, 008, 021 | verified Principal/Tenant context, PostgreSQL RLS and restricted runtime role | DEV-209 route/body/header tamper tests plus DEV-211 Tenant B real-host negative | **Verified end to end for bounded slice** |

## Governed Organisation Party increment — downstream traceability

Controlled slice: `NBEOS-H-006 — Governed Organisation Party — Register and Retrieve`.

| Requirements / evidence | Architecture / design | Implementation | Verification evidence | Current result |
|---|---|---|---|---|
| `EP-004`; `DATA-001` stable identifiers | accepted clean-slate enterprise model; ADR-022 | `PartyId`, `PartyKind`, `Organisation` in `NuBlox.Enterprise.Domain` | Enterprise unit tests; CI `36310775633` | **Verified bounded Organisation identity** |
| `DATA-002` explicit material relationships / identity distinctions | ADR-022; `Tenant != Party`; business roles not Party kinds | `enterprise.parties` + `enterprise.organisations`; no employee/client/vendor Party-kind values | database constraint + persistence integration query | **Verified for Organisation identity boundary** |
| `DATA-003` authoritative ownership/source | ADR-017, ADR-022 | Enterprise module owns Organisation contracts/schema/migrations | module boundary + migration catalog + common verifier | **Verified bounded ownership** |
| `NFR-SEC-*`; tenant isolation | ADR-007, 008, 021, 022 | verified request context, transaction-local tenant propagation, forced RLS, restricted runtime role | Enterprise PostgreSQL RLS negative + Tenant B real-host read negative; CI `36310775633` | **Verified for bounded slice** |
| `NFR-AUD-*` creation accountability | ADR-018, 022 | `enterprise.organisation.create` through durable PostgreSQL audit appender | real API test verifies exact actor/tenant/action and zero display-name leakage | **Verified for bounded slice** |
| API/runtime composition | ADR-011, 012, 017, 021, 022 | Organisation POST/GET + `NuBlox.Runtime.PostgreSql` Enterprise registrations | real HTTP→application→PostgreSQL API test; API suite 17/17 | **Verified on implementation head** |

Detailed acceptance evidence is recorded in `NBEOS-H-007 — Second vertical slice verification`.

## Traceability quality rules

1. No Approved software requirement may be orphaned from a business/stakeholder need unless its source is an explicit legal, security, architecture or operational obligation.
2. Every Approved requirement must have a verification path.
3. A design or implementation item that cannot be traced to an approved/controlled need should be challenged as potential uncontrolled scope.
4. Requirement changes trigger review of downstream design, implementation and verification references.
5. Rejected/deferred requirements remain traceable with rationale rather than disappearing from history.
6. A green component/backlog test does not equal end-to-end slice acceptance when a required composition boundary remains absent.
7. Bounded technical acceptance does not convert Draft/Candidate source requirements into an Approved product baseline.
8. Canonical Party identity must not be expanded with contextual role labels unless a later controlled architecture decision supersedes ADR-022.

## Current gaps

- All F-section requirements remain Draft/Candidate and primary customer evidence remains incomplete.
- Quantitative NFR targets remain unresolved.
- Broader Wave-3 requirement prioritisation remains evidence-gated.
- The first Governed Work Product vertical slice is technically verified end to end through the real HTTP/application/PostgreSQL runtime graph.
- The governed Organisation Party foundation is technically verified on its implementation head; Person, Work Relationships, organisational structure and customer/supplier relationship semantics remain future controlled work.
- External notification/integration provider selection, binary work-product content, broader analytics/search and external-participant scenarios remain later controlled work.

## References

- `Business_requirements_document_BRD.md`
- `Stakeholder_requirements_specification.md`
- `Functional_requirements_specification.md`
- `Non-functional_requirements_specification.md`
- `Acceptance_criteria.md`
- `../H_Development_Implementation/First_vertical_slice_definition.md`
- `../H_Development_Implementation/First_vertical_slice_verification.md`
- `../H_Development_Implementation/Second_vertical_slice_definition.md`
- `../H_Development_Implementation/Second_vertical_slice_verification.md`
- `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis / Quality | Initial traceability backbone linking all BRs to stakeholder/software requirements and verification concepts |
| 0.2 | 2026-09-26 | NuBlox Product / Business Analysis / Quality | Added actual first-slice architecture/implementation/test/CI traceability and recorded DEV-209/210 composition gaps discovered by DEV-208 verification |
| 0.3 | 2026-09-27 | NuBlox Product / Business Analysis / Quality | Closed the first-slice downstream trace after DEV-209/210/211 verified HTTP context, durable audit/telemetry composition and the real production HTTP-to-PostgreSQL service graph |
| 0.4 | 2026-09-27 | NuBlox Product / Business Analysis / Quality | Added DEV-308 governed Organisation Party traceability from EP-004/DATA-001–003 through ADR-022, Enterprise implementation and CI 36310775633 |
