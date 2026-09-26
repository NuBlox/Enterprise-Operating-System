# RACI chart

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-008  
**Document Type:** RACI chart  
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
**Related Documents:** `Governance_structure.md`, `Stakeholder_register.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/RACI_chart.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/RACI_chart.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define provisional responsibility and accountability for core project decisions and deliverables until named individuals are appointed.

## Role abbreviations

- `SP` — Programme Sponsor / Investment Authority
- `PO` — Product Owner
- `PM` — Project Manager
- `AR` — Architecture Authority
- `EL` — Engineering Lead
- `SEC` — Security / Privacy Authority
- `DATA` — Data Governance Authority
- `QA` — Quality / Acceptance Authority
- `COM` — Commercial / Customer Authority
- `CUST` — Customer Sponsor / Representative

`A` = Accountable, `R` = Responsible, `C` = Consulted, `I` = Informed.

## RACI

| Activity / Decision | SP | PO | PM | AR | EL | SEC | DATA | QA | COM | CUST |
|---|---|---|---|---|---|---|---|---|---|---|
| Product vision / strategy | A | R | I | C | I | C | C | I | C | C |
| Funding tranche approval | A | C | R | C | I | C | C | I | C | I |
| Target market / segment | A | R | C | C | I | I | I | I | R/C | C |
| Customer research plan | I | A | R | C | I | I | I | C | C | C |
| Project plan / RAID | I | C | A/R | C | C | C | C | C | C | I |
| High-level scope | C | A | R | C | C | C | C | C | C | C |
| Requirements baseline | I | A | R | C | C | C | C | C | C | C |
| Architecture baseline | I | C | C | A/R | R | C | C | C | I | I |
| Security/privacy controls | I | C | C | C | R | A/R | C | C | I | C |
| Data governance/model decisions | I | C | C | C | R | C | A/R | C | I | C |
| Engineering standards | I | I | C | A | R | C | C | C | I | I |
| Implementation delivery | I | C | A | C | R | C | C | C | I | I |
| Test/acceptance strategy | I | C | C | C | R | C | C | A/R | I | C |
| Pilot scope | I | A | R | C | C | C | C | C | C | C/A* |
| Customer migration/onboarding | I | C | R | C | R | C | C | C | A/R | C |
| Benefits measurement | I | A | R | I | I | I | C | C | R | C |
| Production readiness | C | C | R | C | R | C | C | A | C | I |
| Production launch decision | A | R | C | C | C | C | C | C | C | I |

`*` Customer accountability applies to acceptance of the customer's own pilot scope/outcomes; NuBlox Product remains accountable for product scope.

## RACI rules

1. Each material deliverable or decision should have one clear NuBlox accountable role.
2. Accountability may not be delegated implicitly by implementation activity.
3. `Consulted` means input is actively sought before decision; `Informed` is notification after/before as appropriate.
4. Security, privacy, data and quality authorities must be consulted where their domain is materially affected even if not shown in every row.
5. Named-role assignment must replace placeholder roles before formal approval.

## References

- `software_project_docs/D_Project_Initiation/Governance_structure.md`
- `software_project_docs/D_Project_Initiation/Stakeholder_register.md`
- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial provisional project RACI |
