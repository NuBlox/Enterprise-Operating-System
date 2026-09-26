# Business rules

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-006  
**Document Type:** Business rules  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Software_requirements_specification_SRS.md`, `Functional_requirements_specification.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Business_rules.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Business_rules.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define candidate business rules that constrain how NuBlox must behave independently of any particular user interface, database design or implementation technology.

## Rule status

All rules in version 0.1 are **Candidate** and require validation against customer workflows, regulation and approved requirements.

## Candidate rules

| ID | Rule | Source / rationale |
|---|---|---|
| `RULE-001` | A user action shall only be permitted when the acting participant has the required access and, where relevant, business authority for the action and context. | BR-006, BR-015 |
| `RULE-002` | Permission to access a function shall not by itself establish authority to approve, commit, accept or decide. | BR-006, BR-007 |
| `RULE-003` | An approval or decision requiring evidence shall record the subject, outcome, actor, timestamp, applicable authority/context and retained evidence sufficient for reconstruction. | BR-007, BR-014 |
| `RULE-004` | Controlled business information shall have an identifiable authoritative representation or explicitly documented external system of record. | BR-004, BR-012 |
| `RULE-005` | Material changes to controlled business information shall preserve the history required by the applicable business, audit, contractual or regulatory need. | BR-014, BR-015 |
| `RULE-006` | Work that creates or changes a controlled output shall retain a traceable relationship to that output where the business outcome requires it. | BR-005 |
| `RULE-007` | Cross-organisational or cross-team handoffs shall preserve the information required to understand ownership, state, dependencies and next action. | BR-001, BR-009 |
| `RULE-008` | External participants shall receive only the information and actions explicitly authorised for their business context. | BR-010, BR-015 |
| `RULE-009` | Customer configuration shall not silently alter product-wide invariants or bypass mandatory security, audit or data-integrity controls. | BR-011, BR-015, BR-017 |
| `RULE-010` | Integrations shall have an explicit ownership and authority boundary for data created, updated, mastered or merely referenced. | BR-004, BR-012 |
| `RULE-011` | Migration shall not mark information as successfully migrated until agreed reconciliation and integrity checks pass. | BR-013 |
| `RULE-012` | Operational reporting shall identify the governed source and definition of material measures. | BR-008, BR-016 |
| `RULE-013` | Deletion, retention and archival behaviour shall respect the applicable legal, contractual, security and business-record obligations. | BR-014, BR-015 |
| `RULE-014` | A business state change shall not be treated as complete when required validations, approvals or prerequisite evidence remain unsatisfied. | BR-002, BR-007 |
| `RULE-015` | Where NuBlox coordinates an external system, successful completion shall be based on an agreed acknowledgement/result rather than merely dispatching a request. | BR-002, BR-012 |
| `RULE-016` | Automated decisions or actions with material business effect shall be attributable to the rule/service/version that produced them and shall retain required human oversight. | BR-007, BR-015 |
| `RULE-017` | Material business identifiers shall not be reused in a way that makes historical records ambiguous. | BR-004, BR-014 |
| `RULE-018` | A superseded controlled record shall remain distinguishable from the currently effective record where historical reconstruction is required. | BR-014 |

## Rule categories still requiring discovery

Further rules are expected for:

- commercial commitments and financial controls;
- segregation of duties;
- project/professional review and issue processes;
- document/model revision and status;
- resource assignment and competence;
- contractual notices and deadlines;
- data retention and legal hold;
- jurisdictional/regulatory controls;
- customer/client/supplier collaboration;
- AI-assisted or automated work.

## Acceptance for baseline

This document can move to In Review when every rule has:

- a traceable source;
- an accountable business owner;
- defined applicability/conditions;
- identified conflicts or exceptions;
- a planned verification method.

## References

- `Business_requirements_document_BRD.md`
- `Software_requirements_specification_SRS.md`
- `Functional_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial candidate business-rule baseline derived from controlled requirements |