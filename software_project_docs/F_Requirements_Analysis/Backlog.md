# Backlog

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-025  
**Document Type:** Backlog  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product  
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
**Related Documents:** `Themes.md`, `Epics.md`, `User_stories.md`, `Requirements_prioritisation_MoSCoW_WSJF_etc.md`, `Requirements_traceability_matrix_RTM.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Backlog.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Backlog.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the controlled candidate requirements backlog. Version 0.1 is a **discovery backlog**, not an approved development sprint/release backlog.

## Backlog states

- `DISCOVERY` — hypothesis requiring evidence/refinement.
- `READY_FOR_REVIEW` — sufficiently defined for product/requirements review.
- `APPROVED` — approved for the stated scope/priority.
- `READY_FOR_DELIVERY` — satisfies Definition of Ready and delivery dependencies.
- `IN_DELIVERY` — implementation underway.
- `VERIFICATION` — implementation complete enough for planned verification.
- `DONE` — satisfies approved Definition of Done.
- `DEFERRED` — deliberately postponed.
- `REJECTED` — deliberately excluded with rationale.

## Candidate epic backlog

All epics are currently `DISCOVERY`.

| Epic | Outcome | Provisional sequence dependency | State |
|---|---|---|---|
| `EP-004` Governed business subjects | Establish authoritative information/context needed by selected workflow | Required before reliable work/reporting | DISCOVERY |
| `EP-001` Understand my work | Participant can see required attention/context/outcome | Depends on selected workflow/context | DISCOVERY |
| `EP-002` Initiate and coordinate work | Trigger, route and progress work | Depends on work/context semantics | DISCOVERY |
| `EP-005` Create and govern work products | Outputs tied to work/review/history | Depends on selected professional output | DISCOVERY |
| `EP-006` Review, decide and approve | Governed decisions and authority evidence | Depends on authority/rule discovery | DISCOVERY |
| `EP-003` Controlled handoffs | Preserve context across stages/participants | Depends on workflow validation | DISCOVERY |
| `EP-011` Trusted management views | Current governed status/exceptions with drill-through | Depends on authoritative operational data | DISCOVERY |
| `EP-007` Controlled external participation | Secure low-friction external collaboration | Pilot-dependent | DISCOVERY |
| `EP-008` Connect commercial context to delivery | Connect delivery to commitments/consequences | Pilot-dependent | DISCOVERY |
| `EP-009` Manage exceptions and change | Govern risks/issues/change/blocking | Cross-cutting/pilot-dependent | DISCOVERY |
| `EP-010` Search and navigate business context | Find permitted information/relationships | Depends on information model | DISCOVERY |
| `EP-012` Measure outcomes and benefits | Prove workflow/customer value | Required for pilot acceptance | DISCOVERY |
| `EP-013` Integrate retained specialist systems | Controlled coexistence/integration | Depends on selected system boundary | DISCOVERY |
| `EP-014` Migrate customer information safely | Repeatable migration/reconciliation | Required before customer implementation where migration applies | DISCOVERY |
| `EP-015` Govern customer configuration | Supported configurable variation | Architecture/product integrity dependency | DISCOVERY |
| `EP-016` Administer identity/access/configuration | Safe product administration | Required for controlled pilot/production | DISCOVERY |
| `EP-017` Operate and support the service | Production operability/support | Required before production release | DISCOVERY |
| `EP-018` Secure and audit the platform | Cross-cutting security/privacy/audit | Applies to every delivery increment | DISCOVERY |

## Story backlog

The current candidate story set is maintained in `User_stories.md` as `US-001` through `US-030`. Stories remain discovery items until customer/user evidence and acceptance criteria exist.

## Backlog entry requirements

A backlog entry shall record or link to:

- unique ID;
- outcome/user value;
- requirement/epic links;
- status;
- priority;
- acceptance criteria;
- business rules;
- relevant NFR/security/data/interface obligations;
- dependencies;
- estimate/size only when meaningful;
- evidence/source;
- release/pilot target when approved;
- verification result when completed.

## Scope-control rule

No item should enter delivery merely because it sounds useful or is common in an incumbent product. It must trace to an approved requirement, obligation, risk treatment or explicit architecture enabler.

## References

- `Themes.md`
- `Epics.md`
- `User_stories.md`
- `Requirements_prioritisation_MoSCoW_WSJF_etc.md`
- `Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product | Initial controlled discovery backlog and lifecycle |