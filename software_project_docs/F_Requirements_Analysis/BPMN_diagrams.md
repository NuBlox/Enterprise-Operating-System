# BPMN diagrams

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-024  
**Document Type:** BPMN diagrams  
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
**Related Documents:** `Process_maps.md`, `Use_cases.md`, `Business_rules.md`, `Requirements_traceability_matrix_RTM.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/BPMN_diagrams.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/BPMN_diagrams.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Govern the creation and approval of formal BPMN process models for NuBlox requirements. The conceptual Mermaid process maps in `Process_maps.md` are discovery aids and shall **not** be represented as formal BPMN 2.0 models.

## BPMN modelling policy

Formal BPMN models should be created only for processes where the additional precision provides value for requirements, control, workflow design, automation, customer validation or operating procedure definition.

A BPMN model shall identify as applicable:

- process start/end events;
- participants/pools and responsibility lanes;
- tasks/activities;
- user/service/manual tasks where the distinction matters;
- exclusive/parallel/event-based gateways;
- messages between participants;
- timer/error/escalation events;
- subprocesses/call activities;
- data objects/stores where necessary for understanding;
- boundary events and exception paths;
- correlation to controlled requirement/use-case IDs.

## Candidate BPMN model register

| BPMN ID | Candidate process | Source process map | Primary reason to model | Status |
|---|---|---|---|---|
| `BPMN-001` | Controlled work initiation-to-completion | `PM-001` | Validate cross-participant work, review and handoff semantics | Planned |
| `BPMN-002` | Work-product review/approval/issue | `PM-002` | Validate revision, review, decision and issue controls | Planned |
| `BPMN-003` | Controlled business change | `PM-003` | Validate decision, change impact and downstream integration | Planned |
| `BPMN-004` | External collaboration | `PM-004` | Validate message flows and access lifecycle across organisation boundary | Planned |
| `BPMN-005` | Migration/reconciliation | `PM-005` | Validate exception/rework/acceptance and cutover responsibilities | Planned |

## Required model metadata

Every formal BPMN model shall record:

- model ID and title;
- version/status;
- process owner;
- purpose/scope;
- participating roles/organisations;
- linked BR/SR/FR/RULE/AC/use-case IDs;
- assumptions/variants;
- authoritative model file/location;
- validation participants/date;
- approval record;
- supersession history.

## BPMN validation rules

Before a BPMN model becomes an Approved requirements artifact:

1. start/end conditions are unambiguous;
2. participant/lane responsibilities are meaningful and validated;
3. decision gateways have defined conditions;
4. required authority/review controls are represented;
5. material failure, cancellation, timeout and rework paths are represented where applicable;
6. system-vs-human work distinctions are intentional;
7. cross-organisation messages/handoffs are explicit;
8. data/work products required by critical tasks are identified;
9. model aligns with business rules and use cases;
10. customer/business representatives have validated the business flow.

## Workflow-engine caution

BPMN documentation shall not automatically imply that the process will be executed by a BPMN workflow engine. Execution technology is an architecture decision based on complexity, transactional/control requirements, change frequency, observability and operational needs.

## Current action

The project should select the first validated customer workflow before investing in detailed BPMN modelling. `BPMN-001` through `BPMN-005` remain Planned until supporting customer/process evidence exists.

## References

- `Process_maps.md`
- `Use_cases.md`
- `Business_rules.md`
- `Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Established BPMN modelling governance and initial candidate model register |