# Themes

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-018  
**Document Type:** Themes  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Requirements_prioritisation_MoSCoW_WSJF_etc.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Themes.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Themes.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Group candidate requirements into outcome-oriented themes for discovery and backlog organisation without turning the themes into software modules or architecture boundaries.

## Candidate themes

| ID | Theme | Outcome intent | Principal sources |
|---|---|---|---|
| `TH-001` | Work and context continuity | Preserve why work exists, who is involved, what is required and how it progresses across handoffs. | `BR-001`, `BR-002`, `BR-003`, `BR-009`; `SR-001`–`SR-005` |
| `TH-002` | Governed business information | Maintain authoritative identity, relationships, state, history and provenance for information required by priority workflows. | `BR-004`, `BR-014`; `SR-013`, `SR-018` |
| `TH-003` | Work products and professional outputs | Keep outputs connected to the work, review, issue and evidence that gives them business meaning. | `BR-005`; `SR-003`, `SR-004`, `SR-017` |
| `TH-004` | Responsibility, authority and decisions | Make access, responsibility, authority, review, approval and decision evidence explicit. | `BR-006`, `BR-007`, `BR-015`; `SR-016`, `SR-023` |
| `TH-005` | External collaboration | Allow clients, suppliers, partners and other external participants to join approved work with controlled scope and low unnecessary friction. | `BR-010`; `SR-031`, `SR-032` |
| `TH-006` | Commercial and delivery continuity | Keep relevant delivery commitments, progress and change connected to commercial/financial context. | `BR-003`, `BR-008`; `SR-010`–`SR-012` |
| `TH-007` | Management visibility and measurable outcomes | Derive trusted current reporting, exceptions and outcome measures from governed operational information. | `BR-008`, `BR-016`; `SR-006`, `SR-013`–`SR-015`, `SR-027` |
| `TH-008` | Interoperability and migration | Coexist with specialist systems and migrate information without losing authority, meaning or control. | `BR-012`, `BR-013`; `SR-020`, `SR-028`, `SR-030` |
| `TH-009` | Configurable and supportable product | Support legitimate customer variation while protecting upgradeability, operability and repeatable implementation. | `BR-011`, `BR-017`, `BR-018`; `SR-022`, `SR-029` |
| `TH-010` | Enterprise security, privacy and assurance | Apply secure access, classification, auditability, evidence and operational controls across all themes. | `BR-015`; `SR-023`–`SR-026` |

## Theme rules

- Themes organise product discovery/backlog; they are not modules.
- One requirement/story may contribute to multiple themes.
- Themes may be merged, split or rejected as customer evidence develops.
- Cross-cutting NFRs apply across themes even where not repeated explicitly.

## References

- `Business_requirements_document_BRD.md`
- `Stakeholder_requirements_specification.md`
- `Requirements_prioritisation_MoSCoW_WSJF_etc.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial outcome-oriented requirement themes |