# Acceptance criteria

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-014  
**Document Type:** Acceptance criteria  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Business Analysis  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Software_requirements_specification_SRS.md`, `Functional_requirements_specification.md`, `Non-functional_requirements_specification.md`, `Requirements_traceability_matrix_RTM.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Acceptance_criteria.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Acceptance_criteria.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the acceptance framework used to determine whether approved NuBlox requirements and end-to-end outcomes have been implemented sufficiently for the relevant stage.

## Acceptance principles

1. Acceptance shall trace to approved requirements and business outcomes.
2. Feature existence is not sufficient evidence of outcome completion.
3. Acceptance shall include functional, control, data, security, quality and operational evidence where applicable.
4. Negative/error/recovery paths are part of acceptance for material workflows.
5. Customer/pilot acceptance shall be distinct from internal technical completion.
6. A requirement may be deferred only through controlled scope/change decisions.

## Acceptance levels

| Level | Purpose |
|---|---|
| Requirement acceptance | Verify one approved requirement/criterion. |
| Workflow acceptance | Verify a coherent end-to-end business scenario across requirements. |
| Non-functional acceptance | Verify security, performance, resilience, accessibility, auditability and other quality attributes. |
| Migration/data acceptance | Verify migrated/created information is complete, reconciled and usable to agreed tolerances. |
| Operational readiness | Verify monitoring, support, recovery, runbooks and release controls. |
| Pilot/customer acceptance | Verify the solution supports agreed real-world outcomes and success measures. |

## Core candidate acceptance criteria

| ID | Criterion |
|---|---|
| `AC-001` | Every Approved requirement has an identified verification method and evidence owner. |
| `AC-002` | Priority end-to-end workflows complete without uncontrolled manual reconstruction of context or unsupported data correction. |
| `AC-003` | Required access and authority controls prevent unauthorised actions and permit authorised work. |
| `AC-004` | Required decisions/approvals retain the agreed actor, authority/context, outcome, timing and evidence. |
| `AC-005` | Required business history can be reconstructed to the agreed level from retained data/audit evidence. |
| `AC-006` | Required work products/transactions/records remain traceably related to the work/business context that created or changed them. |
| `AC-007` | Integration failures/retries/reconciliation do not create uncontrolled duplicate or inconsistent business state. |
| `AC-008` | Approved migration scope reconciles to agreed source/target counts, values and exception rules. |
| `AC-009` | Operational/reporting views use governed data definitions and enforce applicable access constraints. |
| `AC-010` | Configured customer variation remains within supported product controls and does not require an unmanaged source-code fork. |
| `AC-011` | Security/privacy controls pass the agreed testing and review applicable to the release. |
| `AC-012` | Availability/performance/recovery criteria meet the quantitative targets approved for the release; numeric targets remain TBD until baselined. |
| `AC-013` | Accessibility/usability testing demonstrates that priority workflows are operable by intended users to agreed criteria. |
| `AC-014` | Monitoring/support evidence can identify and diagnose material failure modes for production operation. |
| `AC-015` | Pilot acceptance includes measurable evidence against agreed customer/business outcome metrics, not only stakeholder opinion. |

## Per-requirement acceptance record

Each approved requirement should include or link to:

- requirement ID and version;
- acceptance criterion/expected outcome;
- preconditions/test data;
- positive scenario;
- material negative/error scenario;
- verification method;
- expected evidence;
- accountable accepter;
- result/date/version tested;
- defects/exceptions/deviations;
- final status.

## Exit conditions for requirements baseline

Before detailed design is approved:

- priority requirements must have acceptance concepts;
- material NFRs must have measurable targets or controlled TBDs with owners/dates;
- traceability must link business need to software requirement and planned verification;
- unresolved conflicts must be recorded;
- no high-risk requirement may rely solely on informal/manual assurance without explicit acceptance.

## References

- `Business_requirements_document_BRD.md`
- `Software_requirements_specification_SRS.md`
- `Functional_requirements_specification.md`
- `Non-functional_requirements_specification.md`
- `Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial acceptance framework and cross-cutting candidate criteria |