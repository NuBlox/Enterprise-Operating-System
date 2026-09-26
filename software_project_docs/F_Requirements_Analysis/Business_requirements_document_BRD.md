# Business requirements document (BRD)

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-001  
**Document Type:** Business requirements document (BRD)  
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
**Related Documents:** `../D_Project_Initiation/High-level_scope_statement.md`, `../D_Project_Initiation/Objectives_and_success_criteria.md`, `Stakeholder_requirements_specification.md`, `Software_requirements_specification_SRS.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Business_requirements_document_BRD.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Business_requirements_document_BRD.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the initial business-level outcomes and capabilities the NuBlox Enterprise Operating System must support before detailed software requirements or architecture are baselined.

## Business context

NuBlox is being developed to test and address the operational divide that can exist between:

```text
operating the organisation
+
delivering the work through which the organisation creates value
```

The product must therefore be evaluated against complete business outcomes, not only against isolated software functions.

## Requirement status model

- **Candidate** — derived from current product/programme strategy and requires further validation.
- **Validated** — supported by sufficient customer/business evidence for the current stage.
- **Approved** — formally baselined by the relevant authority.
- **Rejected** — considered and deliberately excluded.

All requirements in version 0.1 are **Candidate** unless explicitly stated otherwise.

## Core business requirements

### BR-001 — End-to-end operational continuity

NuBlox shall enable priority business outcomes to progress across organisational, professional, project, commercial and control boundaries without requiring the user to reconstruct context manually at every handoff.

**Status:** Candidate  
**Evidence needed:** representative end-to-end customer workflows.

### BR-002 — Work must be executable, not merely visible

NuBlox shall enable users to perform the work required for agreed business outcomes, whether natively or through a deliberately governed interaction with another system.

**Status:** Candidate

### BR-003 — Organisational and delivery context must remain connected

The system shall preserve the relationship between the organisation, people, customers/clients, suppliers/partners, projects/services/assets or other delivery contexts, commitments, work performed, resulting information and commercial consequences where relevant.

**Status:** Candidate

### BR-004 — Authoritative business information

For information managed by NuBlox, the organisation shall be able to determine the authoritative representation, ownership, current state and relevant history of important business information.

**Status:** Candidate

### BR-005 — Work and its outputs must remain connected

The system shall maintain traceable relationships between work performed and the transactions, records, documents, models, decisions, deliverables or other outputs that the work creates or changes.

**Status:** Candidate

### BR-006 — Responsibility, authority and access must be understandable

The organisation shall be able to determine who is responsible for work, who may perform or access it, who has authority to approve/commit/decide, and under what scope or conditions.

**Status:** Candidate

### BR-007 — Decisions and approvals must be evidenced

Where a business outcome requires review, approval, acceptance or decision, NuBlox shall retain sufficient context and evidence to understand what was decided, by whom, when, under what authority and with what result.

**Status:** Candidate

### BR-008 — Management information should arise from operational work

The system shall support current management visibility from controlled operational information rather than requiring the business to reconstruct its primary operational truth through disconnected offline reporting processes.

**Status:** Candidate

### BR-009 — Cross-functional work must be supported

Priority workflows shall be able to cross organisational/team boundaries while preserving ownership, status, dependencies, information and accountability.

**Status:** Candidate

### BR-010 — External participation must be possible where the work requires it

The product shall support controlled interaction with external organisations or individuals where required by the selected business outcome, subject to security, privacy and commercial requirements.

**Status:** Candidate

### BR-011 — The product must support organisational variation without customer-specific forks

NuBlox shall provide controlled configuration/extensibility sufficient to support legitimate customer differences while maintaining one supportable product codebase as the default operating model.

**Status:** Candidate

### BR-012 — Specialist systems may remain part of the operating environment

NuBlox shall support deliberate interoperability with specialist or incumbent systems where integration provides better customer value than native replacement.

**Status:** Candidate

### BR-013 — Migration from existing estates must be feasible

The product and implementation approach shall support controlled migration of relevant master, transactional, work, document and historical information from existing customer systems where required.

**Status:** Candidate

### BR-014 — Historical meaning must be preserved

For business information requiring historical reconstruction, the system shall preserve sufficient history to understand what was true, effective, approved or recorded at the relevant time.

**Status:** Candidate

### BR-015 — Security, privacy and auditability are business requirements

NuBlox shall protect business information and actions according to their sensitivity and obligations, and provide sufficient auditability for agreed assurance needs.

**Status:** Candidate

### BR-016 — Priority workflows must be measurable

The product shall support measurement of agreed operational outcomes so customers and NuBlox can determine whether the software produces the intended benefit.

**Status:** Candidate

### BR-017 — The initial solution must be commercially implementable

The selected initial product scope shall be capable of repeatable deployment, configuration, migration, operation and support at a cost consistent with a sustainable product business.

**Status:** Candidate

### BR-018 — Product scope shall be outcome-led

A capability should enter the initial scope only when it is necessary to complete a priority outcome, meet a cross-cutting control/operability need, preserve required authoritative information, satisfy an obligation, or avoid a material architectural dead end.

**Status:** Candidate

## Initial business outcome areas to investigate

The following are discovery areas, not approved modules:

1. market/client need to opportunity;
2. proposal, appointment and commercial commitment;
3. project/service mobilisation;
4. people/resource planning;
5. professional/project work execution;
6. governed production/review/issue of work products;
7. decisions, approvals and change;
8. project/commercial status, billing and cash consequences;
9. performance, closeout and lessons;
10. continuing customer/client relationship.

## Business constraints

The product must not rely on:

- unmanaged customer-specific code forks;
- undocumented manual controls for critical business integrity;
- unsupported security/privacy assumptions;
- a requirement to replace every specialist tool;
- a fixed software taxonomy before customer/work requirements are understood.

## Business acceptance of the requirements baseline

The BRD can move to In Review when:

- primary customer evidence has been collected;
- each requirement has a source and owner;
- priority outcomes are selected;
- conflicts/gaps are documented;
- measurable success/acceptance concepts exist;
- rejected/deferred requirements are traceable.

## References

- `software_project_docs/D_Project_Initiation/High-level_scope_statement.md`
- `software_project_docs/D_Project_Initiation/Objectives_and_success_criteria.md`
- `software_project_docs/F_Requirements_Analysis/Stakeholder_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial candidate business requirements derived from controlled programme baseline |
