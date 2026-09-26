# Requirements traceability matrix (RTM)

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-015  
**Document Type:** Requirements traceability matrix (RTM)  
**Version:** 0.2  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Functional_requirements_specification.md`, `Non-functional_requirements_specification.md`, `Acceptance_criteria.md`, `../H_Development_Implementation/First_vertical_slice_definition.md`, `../H_Development_Implementation/First_vertical_slice_verification_and_traceability.md`  
**Supersedes:** Version 0.1  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain end-to-end traceability from business intent through stakeholder/software requirements to architecture, implementation and verification evidence. Version 0.2 retains the programme-level traceability backbone and records the first live downstream implementation evidence from the Governed Work Product representative vertical slice.

## Traceability model

```text
Business requirement
→ Stakeholder need
→ Software requirement / supporting specification
→ Architecture / design
→ Implementation
→ Verification / acceptance evidence
```

## Programme traceability matrix

| Business requirement | Principal stakeholder links | Principal software / supporting links | Planned / current verification | Status |
|---|---|---|---|---|
| `BR-001` End-to-end operational continuity | `SR-001`, `SR-002`, `SR-005`, `SR-015` | `FR-002`, `FR-009`–`FR-014`, `IF-001`–`IF-010` | Governed Work Product review/decision/issue flow now provides first representative implementation evidence; broader cross-functional validation remains | Candidate |
| `BR-002` Work must be executable | `SR-001`, `SR-004`, `SR-005` | `FR-009`–`FR-014`, `FR-018` | Work Product submission/routing/decision/issue automated verification; `AC-002` broader validation remains | Candidate |
| `BR-003` Organisational and delivery context connected | `SR-003`, `SR-010`, `SR-015` | `FR-002`, `FR-005`–`FR-008`, `FR-012`, `FR-023`; `DATA-001`–`DATA-006` | Tenant-aware Work Product relationship/integrity evidence exists; wider organisation/project model remains | Candidate |
| `BR-004` Authoritative business information | `SR-013`, `SR-018`, `SR-020` | `FR-005`–`FR-008`, `FR-035`; `DATA-001`–`DATA-011`; `RULE-004` | Work Product authoritative persistence/history/evidence tested; broader source-of-truth coverage remains | Candidate |
| `BR-005` Work and outputs connected | `SR-003`, `SR-016`, `SR-017` | `FR-012`, `FR-019`–`FR-022`; `RULE-006` | Work Product revision/decision/issue evidence linkage implemented and tested; `AC-006` broader validation remains | Candidate |
| `BR-006` Responsibility, authority and access understandable | `SR-001`, `SR-009`, `SR-016`, `SR-023` | `FR-003`, `FR-010`, `FR-017`; `RULE-001`, `RULE-002`; `NFR-SEC-001` | Principal/Tenant negative tests plus contextual authority tests now exist; enterprise authority model remains | Candidate |
| `BR-007` Decisions and approvals evidenced | `SR-016`, `SR-017`, `SR-018` | `FR-015`–`FR-018`, `FR-022`; `RULE-003`; `NFR-AUD-001`–`004` | Work Product decision/issue evidence reconstruction implemented and tested; `AC-004` first-slice evidence recorded in H-005 | Candidate |
| `BR-008` Management information from operational work | `SR-006`, `SR-013`, `SR-014`, `SR-015` | `FR-028`, `FR-029`, `FR-041`; `REP-001`–`REP-012` | Work Product Principal-scoped attention/read model and governed drill-through tested; broader reporting remains | Candidate |
| `BR-009` Cross-functional work | `SR-005`, `SR-006`, `SR-008`, `SR-015` | `FR-010`–`FR-014`; `RULE-007` | First-slice controlled workflow exists; cross-function/team validation remains | Candidate |
| `BR-010` Controlled external participation | `SR-031`, `SR-032`, `SR-023` | `FR-004`, `FR-003`; `RULE-008`; `NFR-SEC-001` | External participant lifecycle remains future scope; current first slice verifies internal authenticated Principal boundaries only | Candidate |
| `BR-011` Variation without customer forks | `SR-022`, `SR-029` | `FR-036`–`FR-038`; `RULE-009`; `NFR-MNT-004` | Configuration/upgrade regression tests planned; first slice deliberately avoids customer-specific forks | Candidate |
| `BR-012` Specialist-system interoperability | `SR-020`, `SR-028`, `SR-030` | `FR-032`–`FR-035`; `INT-001`–`INT-015`; `API-001`–`API-015` | Durable Work Product issue intent/recovery now provides first integration-consequence evidence; external connector contracts remain | Candidate |
| `BR-013` Feasible migration | `SR-028`, `SR-030` | `DATA-009`, `DATA-010`; migration requirements in `Data_requirements.md` | Production ordered/checksummed migration framework exists; business-data migration remains | Candidate |
| `BR-014` Preserve historical meaning | `SR-018`, `SR-016` | `FR-008`, `FR-020`, `FR-039`, `FR-040`; `NFR-DATA-004`; `NFR-AUD-001`–`004` | Immutable Work Product revision/decision/issue evidence and drill-through provide first reconstruction proof | Candidate |
| `BR-015` Security, privacy and auditability | `SR-023`–`SR-026`, `SR-028` | `FR-003`, `FR-039`, `FR-040`; `NFR-SEC-*`, `NFR-PRV-*`, `NFR-AUD-*`; `DATA-007`, `DATA-013`, `DATA-015` | Principal/Tenant fail-closed resolution, PostgreSQL RLS, authority-negative and audit evidence tests now exist | Candidate |
| `BR-016` Measurable priority workflows | `SR-013`, `SR-027` | `FR-041`; `REP-001`–`REP-012`; `ANA-001`–`ANA-012` | Work Product attention/read model provides first operational workflow evidence; business benefit measurement remains | Candidate |
| `BR-017` Commercially implementable product | `SR-027`–`SR-030` | `NFR-MNT-*`, `NFR-OPS-*`, `NFR-PORT-*`; configuration/integration/migration controls | Production foundation and first vertical slice now have deterministic CI evidence; TCO/support/pilot evidence remains | Candidate |
| `BR-018` Outcome-led scope | All stakeholder groups as applicable | Requirements prioritisation, RTM and controlled change process | First vertical slice is explicitly bounded and traceable through H-004/H-005; wider scope remains governed | Candidate |

## First implemented vertical-slice traceability

The first implemented representative workflow is **Governed Work Product — Create, Review, Approve and Issue**.

Primary controlled evidence:

- `../H_Development_Implementation/First_vertical_slice_definition.md` (`NBEOS-H-004`);
- `../H_Development_Implementation/First_vertical_slice_verification_and_traceability.md` (`NBEOS-H-005`);
- accepted ADR records under `../G_Architecture_Design/adr/`;
- production code under `../../src/NuBlox.WorkProducts.*` plus platform foundations;
- automated tests under `../../tests/NuBlox.WorkProducts.Tests/` and `../../tests/NuBlox.WorkProducts.PostgreSql.IntegrationTests/`;
- supporting identity/API/audit/observability/persistence tests;
- the controlled `../../scripts/verify-production.sh` CI entrypoint.

### First-slice downstream map

| Requirement group | Use-case/design link | Architecture | Implementation | Verification |
|---|---|---|---|---|
| `FR-005`–`FR-008` authoritative records/history | `UC-002`; H-004 | ADR-002, ADR-007, ADR-017, ADR-020, ADR-021 | Work Product domain/application/PostgreSQL infrastructure | Work Product unit + PostgreSQL persistence tests; DEV-202 CI |
| `FR-009`–`FR-014` work/state/routing | `UC-001`, `UC-002`; H-004 | ADR-001, ADR-017 | submission/review application behavior and persistence | Work Product unit/integration tests; DEV-203 CI |
| `FR-015`–`FR-018` decisions/authority | `UC-003`; H-004 | ADR-009, ADR-018 | authority evaluation + decision evidence/persistence | decision unit/integration tests; DEV-204 CI |
| `FR-019`–`FR-022` output/evidence linkage | `UC-002`, `UC-010`; H-004 | ADR-017, ADR-018, ADR-020 | issue evidence and supersession persistence | issue unit/integration tests; DEV-205 CI |
| `FR-027`–`FR-029`, `FR-041` attention/reporting | H-004 | ADR-015, ADR-017 | Work Product attention reader/drill-through | attention PostgreSQL tests; DEV-206 CI |
| `FR-018`, `FR-030`, `FR-032`–`FR-035` durable consequence | H-004 | ADR-003, ADR-018 | issue delivery intent/outbox claim/retry/reconcile | delivery PostgreSQL tests; DEV-207 CI |
| `FR-003`, security/isolation obligations | H-004 | ADR-007, ADR-008, ADR-011, ADR-021 | Identity/API/Persistence request and tenant boundaries | identity/API/RLS/tenant-negative tests |
| `FR-039`, `FR-040`, audit/operability obligations | H-004 | ADR-016, ADR-018 | Audit + Observability + business evidence | audit/observability and Work Product evidence tests |

## Traceability quality rules

1. No Approved software requirement may be orphaned from a business/stakeholder need unless its source is an explicit legal, security, architecture or operational obligation.
2. Every Approved requirement must have a verification path.
3. A design or implementation item that cannot be traced to an approved need should be challenged as potential uncontrolled scope.
4. Requirement changes shall trigger review of downstream design, implementation and verification references.
5. Rejected/deferred requirements remain traceable with rationale rather than disappearing from history.
6. Implementation evidence does not automatically elevate a Draft/Candidate requirement to Approved; requirement lifecycle and implementation verification remain separate controls.

## Current gaps

- All listed programme requirements remain Candidate unless separately advanced through requirements governance.
- Primary customer/user evidence and measurable business-outcome validation remain incomplete.
- Quantitative NFR targets remain unresolved in several areas.
- Final enterprise-wide prioritisation remains incomplete.
- The first Work Product slice now has architecture/design/code/test/CI evidence, but many other requirement groups remain unimplemented.
- External participation, binary content, discipline-specific Work Product semantics, final authority models, customer variation/configuration and broader search/reporting remain future controlled scope.

## References

- `Business_requirements_document_BRD.md`
- `Stakeholder_requirements_specification.md`
- `Functional_requirements_specification.md`
- `Non-functional_requirements_specification.md`
- `Acceptance_criteria.md`
- `../H_Development_Implementation/First_vertical_slice_definition.md`
- `../H_Development_Implementation/First_vertical_slice_verification_and_traceability.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis / Quality | Initial traceability backbone linking all BRs to stakeholder/software requirements and verification concepts |
| 0.2 | 2026-09-27 | NuBlox Product / Business Analysis / Quality | Reconciled RTM with the live production foundation and first Governed Work Product architecture, implementation, automated test and CI evidence |
