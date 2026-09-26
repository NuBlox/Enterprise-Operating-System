# Requirements traceability matrix (RTM)

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-015  
**Document Type:** Requirements traceability matrix (RTM)  
**Version:** 0.1  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Functional_requirements_specification.md`, `Non-functional_requirements_specification.md`, `Acceptance_criteria.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain end-to-end traceability from business intent through stakeholder/software requirements to verification evidence. Version 0.1 establishes the first traceability backbone; design, code and test references will be added as those artifacts exist.

## Traceability model

```text
Business requirement
→ Stakeholder need
→ Software requirement / supporting specification
→ Architecture / design
→ Implementation
→ Verification / acceptance evidence
```

## Initial traceability matrix

| Business requirement | Principal stakeholder links | Principal software / supporting links | Planned verification | Status |
|---|---|---|---|---|
| `BR-001` End-to-end operational continuity | `SR-001`, `SR-002`, `SR-005`, `SR-015` | `FR-002`, `FR-009`–`FR-014`, `IF-001`–`IF-010` | End-to-end workflow acceptance; handoff/error scenarios | Candidate |
| `BR-002` Work must be executable | `SR-001`, `SR-004`, `SR-005` | `FR-009`–`FR-014`, `FR-018` | Workflow execution tests; `AC-002` | Candidate |
| `BR-003` Organisational and delivery context connected | `SR-003`, `SR-010`, `SR-015` | `FR-002`, `FR-005`–`FR-008`, `FR-012`, `FR-023`; `DATA-001`–`DATA-006` | Data/relationship integrity and workflow traceability | Candidate |
| `BR-004` Authoritative business information | `SR-013`, `SR-018`, `SR-020` | `FR-005`–`FR-008`, `FR-035`; `DATA-001`–`DATA-011`; `RULE-004` | Source-of-truth tests; history/provenance checks | Candidate |
| `BR-005` Work and outputs connected | `SR-003`, `SR-016`, `SR-017` | `FR-012`, `FR-019`–`FR-022`; `RULE-006` | Work-product traceability; `AC-006` | Candidate |
| `BR-006` Responsibility, authority and access understandable | `SR-001`, `SR-009`, `SR-016`, `SR-023` | `FR-003`, `FR-010`, `FR-017`; `RULE-001`, `RULE-002`; `NFR-SEC-001` | Positive/negative authorisation tests; authority scenarios | Candidate |
| `BR-007` Decisions and approvals evidenced | `SR-016`, `SR-017`, `SR-018` | `FR-015`–`FR-018`, `FR-022`; `RULE-003`; `NFR-AUD-001`–`004` | Decision/approval reconstruction; `AC-004` | Candidate |
| `BR-008` Management information from operational work | `SR-006`, `SR-013`, `SR-014`, `SR-015` | `FR-028`, `FR-029`, `FR-041`; `REP-001`–`REP-012` | Report reconciliation/drill-through/access tests | Candidate |
| `BR-009` Cross-functional work | `SR-005`, `SR-006`, `SR-008`, `SR-015` | `FR-010`–`FR-014`; `RULE-007` | Cross-team workflow/handoff scenarios | Candidate |
| `BR-010` Controlled external participation | `SR-031`, `SR-032`, `SR-023` | `FR-004`, `FR-003`; `RULE-008`; `NFR-SEC-001` | External-user access/isolation/usability tests | Candidate |
| `BR-011` Variation without customer forks | `SR-022`, `SR-029` | `FR-036`–`FR-038`; `RULE-009`; `NFR-MNT-004` | Configuration/upgrade regression tests; `AC-010` | Candidate |
| `BR-012` Specialist-system interoperability | `SR-020`, `SR-028`, `SR-030` | `FR-032`–`FR-035`; `INT-001`–`INT-015`; `API-001`–`API-015` | Contract/integration/failure/reconciliation tests | Candidate |
| `BR-013` Feasible migration | `SR-028`, `SR-030` | `DATA-009`, `DATA-010`; migration requirements in `Data_requirements.md` | Migration reconciliation; `AC-008` | Candidate |
| `BR-014` Preserve historical meaning | `SR-018`, `SR-016` | `FR-008`, `FR-020`, `FR-039`, `FR-040`; `NFR-DATA-004`; `NFR-AUD-001`–`004` | Historical/as-at reconstruction; `AC-005` | Candidate |
| `BR-015` Security, privacy and auditability | `SR-023`–`SR-026`, `SR-028` | `FR-003`, `FR-039`, `FR-040`; `NFR-SEC-*`, `NFR-PRV-*`, `NFR-AUD-*`; `DATA-007`, `DATA-013`, `DATA-015` | Security/privacy/audit tests; `AC-003`, `AC-011` | Candidate |
| `BR-016` Measurable priority workflows | `SR-013`, `SR-027` | `FR-041`; `REP-001`–`REP-012`; `ANA-001`–`ANA-012` | Benefit/outcome measurement; `AC-015` | Candidate |
| `BR-017` Commercially implementable product | `SR-027`–`SR-030` | `NFR-MNT-*`, `NFR-OPS-*`, `NFR-PORT-*`; configuration/integration/migration controls | Pilot implementation metrics; TCO/support evidence | Candidate |
| `BR-018` Outcome-led scope | All stakeholder groups as applicable | Requirements prioritisation, RTM and controlled change process | Scope-review evidence; no orphan requirement accepted | Candidate |

## Downstream traceability placeholders

The matrix will add columns/references for the following when they exist:

- user story / use case / process flow;
- architecture decision / component;
- data model/interface design;
- implementation package/module;
- automated/manual test ID;
- release/version;
- verification result/evidence;
- defect/deviation/change request.

## Traceability quality rules

1. No Approved software requirement may be orphaned from a business/stakeholder need unless its source is an explicit legal, security, architecture or operational obligation.
2. Every Approved requirement must have a verification path.
3. A design or implementation item that cannot be traced to an approved need should be challenged as potential uncontrolled scope.
4. Requirement changes shall trigger review of downstream design, implementation and verification references.
5. Rejected/deferred requirements remain traceable with rationale rather than disappearing from history.

## Current gaps

- All listed requirements remain Candidate.
- Primary customer evidence is incomplete.
- Architecture/design/implementation/test artifacts do not yet exist.
- Quantitative NFR targets remain unresolved.
- Prioritisation is not yet baselined.

## References

- `Business_requirements_document_BRD.md`
- `Stakeholder_requirements_specification.md`
- `Functional_requirements_specification.md`
- `Non-functional_requirements_specification.md`
- `Acceptance_criteria.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis / Quality | Initial traceability backbone linking all BRs to stakeholder/software requirements and verification concepts |