# Epics

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-019  
**Document Type:** Epics  
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
**Related Documents:** `Themes.md`, `Business_requirements_document_BRD.md`, `Functional_requirements_specification.md`, `User_stories.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Epics.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Epics.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define candidate outcome-oriented epics that decompose the controlled requirement themes into meaningful product/discovery slices. Epics are not approved software modules and do not imply package or service boundaries.

## Candidate epics

| ID | Epic | Theme(s) | Outcome |
|---|---|---|---|
| `EP-001` | Understand my work | `TH-001` | A participant can see what requires attention, why, required context and expected outcome. |
| `EP-002` | Initiate and coordinate work | `TH-001`, `TH-004` | Approved triggers can create/route work with ownership, status and dependencies. |
| `EP-003` | Controlled handoffs | `TH-001`, `TH-005` | Work/results move between participants or stages with required context and acceptance. |
| `EP-004` | Governed business subjects | `TH-002` | Priority workflows use stable, authoritative business information with required history. |
| `EP-005` | Create and govern work products | `TH-003` | Outputs are created/registered, revised, reviewed, issued and traced to work/context. |
| `EP-006` | Review, decide and approve | `TH-004`, `TH-010` | Reviews/decisions apply authority rules and retain evidence/provenance. |
| `EP-007` | Controlled external participation | `TH-005`, `TH-010` | External participants can contribute to approved workflows without broader access. |
| `EP-008` | Connect commercial context to delivery | `TH-006` | Work/change/progress remains related to relevant commitments and financial/commercial consequences. |
| `EP-009` | Manage exceptions and change | `TH-001`, `TH-004`, `TH-006` | Risks/issues/changes/blocks are visible, owned and resolved through controlled work. |
| `EP-010` | Search and navigate business context | `TH-002`, `TH-007` | Users can find permitted information and move through its relationships/context. |
| `EP-011` | Trusted management views | `TH-007` | Managers/executives see governed current status, exceptions and measures with drill-through. |
| `EP-012` | Measure outcomes and benefits | `TH-007` | Priority workflows produce measurable operational/customer outcome evidence. |
| `EP-013` | Integrate retained specialist systems | `TH-008` | NuBlox can coordinate/exchange with approved systems while preserving authority and recovery. |
| `EP-014` | Migrate customer information safely | `TH-008`, `TH-010` | Data can be profiled, transformed, loaded and reconciled with retained migration evidence. |
| `EP-015` | Govern customer configuration | `TH-009`, `TH-010` | Approved variation is configurable, traceable, testable and upgrade-safe. |
| `EP-016` | Administer identity/access/configuration | `TH-009`, `TH-010` | Administrators can operate required controls without uncontrolled direct data manipulation. |
| `EP-017` | Operate and support the service | `TH-009`, `TH-010` | Health, failures, diagnostics, backup/recovery and support evidence are available. |
| `EP-018` | Secure and audit the platform | `TH-010` | Security/privacy/audit controls apply consistently across product behaviour. |

## Epic quality gate

Before an epic enters an approved delivery backlog it must have:

- traceable business/stakeholder requirements;
- an identified user/business outcome;
- candidate acceptance measures;
- dependencies and NFR/control obligations;
- explicit in/out boundaries;
- enough discovery evidence to justify delivery;
- no assumption that the epic maps one-to-one to a software component.

## Candidate first-pilot epic combination

The first credible end-to-end pilot is expected to need a **combination** of epics rather than one epic in isolation. A candidate sequence to validate is:

```text
EP-004 Governed business subjects
→ EP-001 Understand my work
→ EP-002 Initiate and coordinate work
→ EP-005 Create and govern work products
→ EP-006 Review, decide and approve
→ EP-003 Controlled handoffs
→ EP-011 Trusted management views
```

This is a discovery hypothesis, not an approved pilot scope.

## References

- `Themes.md`
- `Business_requirements_document_BRD.md`
- `Functional_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial candidate epics derived from requirement themes |