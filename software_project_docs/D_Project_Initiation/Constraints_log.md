# Constraints log

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-010  
**Document Type:** Constraints log  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Programme  
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
**Related Documents:** `Assumptions_log.md`, `Initial_risk_register.md`, `Initial_dependency_log.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Constraints_log.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Constraints_log.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Record constraints that limit or shape the NuBlox project so delivery decisions are made with explicit awareness of boundaries rather than discovering them accidentally during implementation.

## Constraints register

| ID | Constraint | Current effect | Owner | Status |
|---|---|---|---|---|
| CON-001 | No approved final architecture or technology stack | Technology choices remain provisional until requirements/design | Architecture | Active |
| CON-002 | Primary customer evidence is incomplete | Product scope cannot be considered fully validated | Product | Active |
| CON-003 | Budget and funding envelope are not yet baselined | Delivery commitments must remain bounded and evidence-gated | Programme | Active |
| CON-004 | Named governance roles are not fully appointed | Formal approvals remain pending | Programme | Active |
| CON-005 | Product must remain commercially reusable | Customer-specific code forks are not an acceptable default | Product/Architecture | Active |
| CON-006 | Enterprise-grade security/privacy expectations apply to production use | Security/privacy must be designed in, not deferred | Security | Active |
| CON-007 | Migration and integration are unavoidable for enterprise adoption | Architecture and delivery planning must include them | Data/Architecture | Active |
| CON-008 | Existing specialist systems may remain necessary | NuBlox must support deliberate interoperability | Product/Architecture | Active |
| CON-009 | Repository `main` is the authoritative completed-work branch | Completed delivery must be merged/cleaned with no residual PR/branches unless explicitly retained | Engineering | Active |
| CON-010 | Controlled project documents must use the template estate and metadata discipline | New formal documents cannot use ad-hoc uncontrolled formats | Programme | Active |
| CON-011 | Benefits and financial returns cannot be asserted without evidence | ROI/business-case claims remain provisional | Product/Commercial | Active |
| CON-012 | Production launch requires operational, security and quality evidence | Demo completeness is insufficient for release | Quality/Security | Active |
| CON-013 | Initial market scope should remain focused enough to be deliverable | Broad multi-industry coverage is deferred unless justified | Product | Active |
| CON-014 | Legal/regulatory obligations vary by customer/jurisdiction | Regulatory scope must be explicitly baselined before affected releases | Compliance | Active |

## Constraint categories

Constraints should be classified where useful as:

- commercial/funding;
- schedule/resource;
- regulatory/legal;
- security/privacy;
- architectural/technical;
- product/market;
- operational;
- governance;
- contractual/customer.

## Management rules

- A constraint is not automatically a risk; a risk arises where uncertainty exists about consequences or ability to comply.
- Constraints that change materially require impact assessment.
- Constraints that can be removed should have an owner and action.
- Where a constraint becomes a requirement, it should be traceable into the requirements baseline.

## References

- `software_project_docs/D_Project_Initiation/Assumptions_log.md`
- `software_project_docs/D_Project_Initiation/Initial_risk_register.md`
- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial explicit project constraints baseline |
