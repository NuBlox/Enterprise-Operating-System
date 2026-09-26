# User journeys

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-022  
**Document Type:** User journeys  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Business Analysis / UX  
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
**Related Documents:** `User_stories.md`, `Use_cases.md`, `Stakeholder_requirements_specification.md`, `Process_maps.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/User_journeys.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/User_journeys.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Describe candidate end-to-end user experiences across multiple interactions so requirements can be tested against continuity, cognitive load, handoffs and business outcomes rather than isolated screens.

These journeys are hypotheses pending primary user research.

## Journey JRN-001 — Professional/operational user completes governed work

| Stage | User need | NuBlox obligation | Risks/friction to test |
|---|---|---|---|
| Arrive | Know what needs attention | Present scoped work/obligations and context | Too much noise; wrong prioritisation |
| Understand | Know why the work exists and what is required | Show business context, inputs, outcome, due/priority and dependencies | Context spread across records/systems |
| Perform | Complete real work | Provide/coordinate required information, work actions and specialist-system interaction | Product becomes status-only shell |
| Produce | Create/change output | Relate resulting work product/record/transaction to work/context | Files disconnected from business state |
| Review | Obtain required review/decision | Present correct version/evidence and authority path | Informal email approval; wrong authority |
| Handoff | Transfer result | Preserve result, state, evidence and next recipient/action | Recipient must reconstruct context |
| Complete | Know work is accepted/done | Record completion/acceptance and downstream effect | Work marked complete prematurely |
| Reflect | See outcome/history | Retain traceable result/history and applicable measures | Weak evidence; no measurable benefit |

Links: `UC-001`, `UC-002`, `US-001`–`US-014`.

## Journey JRN-002 — Manager manages delivery by exception

| Stage | User need | NuBlox obligation | Risks/friction to test |
|---|---|---|---|
| Orient | Understand current position | Present permitted commitments/work/status/measure summary | Dashboard disconnected from live work |
| Detect | See material exceptions | Surface blocked, overdue, failed, risk/change/approval dependencies | Alert overload; hidden issues |
| Investigate | Understand cause/context | Drill to permitted work, people, decisions, commercial context and evidence | Static reports without drill-through |
| Act | Assign/escalate/decide | Provide governed action/decision path | Manager action bypasses authority/process |
| Monitor | Know intervention worked | Track resulting state/outcome | Manual follow-up outside system |
| Learn | Understand recurring causes | Provide governed trend/analysis where justified | Analytics without provenance |

Links: `UC-009`, `US-004`, `US-020`–`US-023`.

## Journey JRN-003 — External collaborator contributes to controlled work

| Stage | User need | NuBlox obligation | Risks/friction to test |
|---|---|---|---|
| Invite | Understand why access is requested | Clear invitation/context and identity/access process | Excessive onboarding friction |
| Access | See only relevant context | Least-privilege scoped experience | Information leakage or missing context |
| Contribute | Submit/review/respond | Simple required action with necessary information | Internal product complexity exposed externally |
| Confirm | Know contribution was received | Acknowledgement/status | Email becomes only record |
| Resolve | Respond to rejection/query | Preserve conversation/evidence in context | Fragmented communication |
| Exit | No longer retain unnecessary access | Controlled lifecycle/revocation | Stale external access |

Links: `UC-004`, `US-015`, `US-016`.

## Journey JRN-004 — Administrator configures and supports the product

| Stage | User need | NuBlox obligation | Risks/friction to test |
|---|---|---|---|
| Identify change | Know requested configuration/support problem | Show governed config/service context | Unclear customer-specific state |
| Assess | Understand impact/invariants | Validation, dependencies, version/history | Changes break upgrades/security |
| Apply | Make supported change | Controlled admin mechanism | Direct database edits |
| Verify | Confirm outcome | Test/preview/audit evidence | Changes silently fail |
| Monitor | Observe service behaviour | Health/telemetry/correlation | Logs fragmented across components |
| Recover | Restore service/integrity | Controlled retry/recovery/rollback | Ad hoc production manipulation |

Links: `UC-006`, `UC-008`, `US-027`–`US-030`.

## Journey JRN-005 — Customer sponsor evaluates value

| Stage | Sponsor need | NuBlox obligation | Risks/friction to test |
|---|---|---|---|
| Baseline | Understand current-state problem/cost | Capture agreed baseline measures/evidence | Benefits invented after delivery |
| Select | Agree target workflow/outcome | Clear scope and success measures | Feature-list pilot |
| Implement | Understand change/migration risk | Visible plan, dependencies, controls | Hidden integration/migration cost |
| Pilot | Observe real use/outcomes | Operational measures and issue evidence | Demo success mistaken for adoption |
| Assess | Compare outcome to baseline | Governed benefit measures | Cherry-picked metrics |
| Decide | Continue/expand/stop | Evidence for investment decision | Sunk-cost bias; unclear exit |

Links: `SR-027`–`SR-030`, `EP-012`, `AC-015`.

## Journey validation questions

For each journey customer/user research should test:

- real trigger and starting context;
- actual actors and handoffs;
- current tools and workarounds;
- information required at each stage;
- sources of delay/rework/error;
- authority/control needs;
- emotional/cognitive burden where relevant;
- measurable outcome and current baseline;
- acceptable system boundaries/specialist tools;
- variations across organisation size/discipline/project type.

## References

- `User_stories.md`
- `Use_cases.md`
- `Stakeholder_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis / UX | Initial five candidate user journeys for validation |