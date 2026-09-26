# Data requirements

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-007  
**Document Type:** Data requirements  
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
**Related Documents:** `Software_requirements_specification_SRS.md`, `Business_rules.md`, `Data_dictionary.md`, `../A_Enterprise_Pre_Project/Data_governance_policy.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Data_requirements.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Data_requirements.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the information requirements NuBlox must satisfy for the candidate business outcomes without prescribing the physical database schema.

## Data design principles

1. Business meaning precedes storage design.
2. Material business subjects require stable identity where the workflow needs independent reference, history or control.
3. Relationships with business meaning shall be explicit and traceable.
4. Authoritative source and ownership shall be identifiable for material information.
5. History shall be retained to the level required by business, contractual, regulatory and audit needs.
6. Classification, access, retention and disposal requirements shall travel with governed information.
7. Customer variation shall be accommodated without destroying semantic consistency or integrity.
8. Migration and integration requirements are part of the data design, not afterthoughts.

## Candidate information domains

The following domains are discovery frames, not approved tables or bounded contexts:

| Domain | Information need |
|---|---|
| Organisations and participants | Organisations, internal/external people, participation context and relevant relationships |
| Customer/client relationships | Prospects, customers/clients, contacts, opportunities and relationship history where required |
| Supplier/partner relationships | Suppliers, partners, contacts, qualification/status and relevant commitments |
| Work/delivery context | Projects, services, assignments or other contexts selected by approved workflows |
| Work coordination | Work requests/items, ownership, status, dependencies, due dates and completion evidence where required |
| Professional/work outputs | Documents, reports, drawings, models, specifications, submissions, records or other outputs needed by selected workflows |
| Decisions and approvals | Reviews, decisions, approvals, acceptance/rejection, authority/context and evidence |
| Commercial commitments | Proposals, appointments/contracts, scope, fees/prices, commitments, variations/changes and obligations where in scope |
| Financial consequences | Billing, cost, forecast, cash or accounting references where required by approved system boundaries |
| Risk/issue/change | Risks, issues, changes, actions, owners and outcomes where required by workflows |
| Governance and assurance | Policies, controls, evidence, audit and compliance records where required |
| Reporting/analytics | Governed measures, dimensions, snapshots/aggregates and provenance |
| Security/operations | Identities/accounts as applicable, access decisions, technical audit, incidents and operational telemetry |

## Core data requirements

| ID | Requirement |
|---|---|
| `DATA-001` | Material business subjects shall have stable identifiers that remain unambiguous throughout their retained lifecycle. |
| `DATA-002` | Material relationships between business subjects shall be representable explicitly where they affect work, authority, reporting, commercial consequence or traceability. |
| `DATA-003` | For governed information, the system shall identify the authoritative source/owner or an explicitly external system of record. |
| `DATA-004` | The system shall distinguish current state from historical state where historical reconstruction is required. |
| `DATA-005` | Information requiring effective dating shall support valid/effective periods independently from technical record timestamps where necessary. |
| `DATA-006` | The system shall retain provenance sufficient to understand how material information was created, imported, changed or derived where required. |
| `DATA-007` | Security classification and access-control decisions shall be enforceable against the relevant data scope. |
| `DATA-008` | Retention/disposal rules shall be representable and enforceable for data subject to controlled retention. |
| `DATA-009` | Imported/migrated data shall retain source identifiers and reconciliation evidence where required for verification. |
| `DATA-010` | Integration mappings shall preserve business meaning and shall not silently coerce materially different concepts into one representation. |
| `DATA-011` | Reporting measures shall have documented definitions and traceable source data. |
| `DATA-012` | Configuration/extension data shall be distinguishable from core product data and governed through versioned configuration where material. |
| `DATA-013` | Personal data shall be minimised to what is required for the approved purpose and handled according to applicable privacy requirements. |
| `DATA-014` | Data quality rules shall be defined for fields/relationships whose correctness is necessary to complete or control a business outcome. |
| `DATA-015` | Deletion/anonymisation shall not invalidate retained evidence that must lawfully remain, and any conflict shall be governed explicitly. |

## Quality dimensions to baseline

For each material information class, requirements shall determine applicable thresholds for:

- completeness;
- validity;
- uniqueness;
- consistency;
- accuracy;
- timeliness/currency;
- referential integrity;
- provenance;
- reconciliation.

No universal numeric thresholds are invented in version 0.1.

## Migration requirements to refine

The project must determine for each migration source:

- source system and owner;
- information in scope;
- source quality/profile;
- mapping/transformation;
- retained history;
- attachments/work products;
- reconciliation method;
- exception handling;
- cutover approach;
- rollback/recovery evidence.

## Approval criteria

Before approval, material data requirements shall be traceable to approved workflows/requirements and have identified ownership, classification, history, quality, retention and migration/integration needs.

## References

- `Software_requirements_specification_SRS.md`
- `Business_rules.md`
- `../A_Enterprise_Pre_Project/Data_governance_policy.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial data-requirements envelope; physical data model intentionally deferred |