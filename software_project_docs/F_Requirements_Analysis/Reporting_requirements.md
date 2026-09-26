# Reporting requirements

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-012  
**Document Type:** Reporting requirements  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Data_requirements.md`, `Analytics_requirements.md`, `Acceptance_criteria.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Reporting_requirements.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Reporting_requirements.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define candidate requirements for operational, management, assurance and external reporting from governed NuBlox information.

## Reporting principles

1. Reports shall be traceable to governed definitions and authoritative data.
2. Reporting shall not create a second uncontrolled version of business truth.
3. Access controls applying to source information shall also constrain reports and exports.
4. Current-state and historical/as-at reporting requirements shall be distinguished explicitly.
5. Material measures shall have documented calculation rules, ownership and refresh expectations.

## Candidate reporting requirements

| ID | Requirement |
|---|---|
| `REP-001` | Users shall be able to obtain operational views relevant to their permitted scope without reconstructing data manually from disconnected sources. |
| `REP-002` | Material measures shall have a controlled definition, owner, source and calculation method. |
| `REP-003` | Reports shall disclose applicable reporting period/as-at basis and material filters where omission could mislead interpretation. |
| `REP-004` | Reporting shall respect row/object/field-level access constraints where applicable and prevent unauthorised inference through aggregates or exports. |
| `REP-005` | Reports used for assurance, approval or contractual/regulatory purposes shall retain sufficient provenance/version context to reproduce the result where required. |
| `REP-006` | The system shall distinguish live operational dashboards from controlled published/snapshotted reports where business meaning differs. |
| `REP-007` | Users shall be able to export permitted report data in approved formats where a justified downstream need exists. |
| `REP-008` | Exported sensitive information shall remain subject to classification, access, retention and handling controls. |
| `REP-009` | Report failures, stale data or incomplete refreshes shall be detectable for reports where timeliness is material. |
| `REP-010` | Cross-functional reporting shall use reconciled/common definitions for shared dimensions and measures rather than independently redefined versions. |
| `REP-011` | Historical reporting shall use the applicable historical/effective information where the question is explicitly as-at or period based. |
| `REP-012` | Customer-specific reports shall use governed configuration/semantic definitions and shall not require unsupported code forks by default. |

## Initial reporting perspectives to investigate

- personal work/attention;
- team workload and exceptions;
- project/service/delivery status;
- commercial and financial status where in scope;
- customer/client commitments;
- resource/capacity;
- quality/review/approval status;
- risk/issue/change;
- compliance/assurance;
- executive performance and outcome views;
- platform/service operations.

These are investigation perspectives, not approved dashboard modules.

## Reporting definition record

Each material report/measure should define:

- business question;
- audience and owner;
- source requirements;
- measure/dimension definitions;
- filters/scope;
- time basis;
- refresh/freshness expectation;
- security classification;
- retention/publication behaviour;
- reconciliation/control checks;
- acceptance criteria.

## References

- `Business_requirements_document_BRD.md`
- `Data_requirements.md`
- `Analytics_requirements.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial reporting requirements baseline |