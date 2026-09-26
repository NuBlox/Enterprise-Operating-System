# User stories

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-020  
**Document Type:** User stories  
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
**Related Documents:** `Epics.md`, `Stakeholder_requirements_specification.md`, `Functional_requirements_specification.md`, `Acceptance_criteria.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/User_stories.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/User_stories.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Capture candidate user-centred behaviours derived from the controlled stakeholder and functional requirements. These stories are discovery/backlog inputs only; they are not approved implementation scope until validated, prioritised and linked to acceptance evidence.

## Story conventions

- `As a ...` describes the stakeholder perspective, not an approved security role.
- `I want ...` describes the need/behaviour, not a specific UI.
- `So that ...` states the outcome/value.
- Each story remains **Candidate** in version 0.1.

## Candidate stories

### Work context and execution

**US-001 — Understand required work**  
As a person performing work, I want to see what requires my attention, why it exists, its relevant context and expected outcome, so that I can act without reconstructing the task from disconnected systems.  
Links: `EP-001`, `SR-001`, `FR-027`.

**US-002 — Start work from a business trigger**  
As an authorised participant, I want work to be initiated from an approved trigger with the relevant context attached, so that the resulting work begins from controlled information.  
Links: `EP-002`, `FR-009`.

**US-003 — Know ownership and dependencies**  
As a person performing or managing work, I want to know who owns the work and what it depends on, so that I can coordinate completion and identify blockers.  
Links: `EP-002`, `SR-006`, `FR-010`, `FR-011`.

**US-004 — See blocked/overdue work**  
As a manager or work owner, I want blocked, failed and overdue work surfaced with reasons, so that I can intervene before the business outcome is missed.  
Links: `EP-009`, `SR-008`, `FR-014`.

**US-005 — Hand work over with context**  
As a person handing work to another participant, I want the required result, context, evidence and acceptance state to travel with the handoff, so that the recipient can continue without re-discovery.  
Links: `EP-003`, `SR-005`, `FR-013`.

### Business information and outputs

**US-006 — Find the authoritative business record**  
As an authorised user, I want to identify the authoritative record for important information, so that I do not rely on conflicting copies.  
Links: `EP-004`, `BR-004`, `FR-005`–`FR-008`.

**US-007 — Navigate related business context**  
As an authorised user, I want to navigate relevant relationships between business subjects, work, outputs and decisions, so that I can understand the complete context.  
Links: `EP-004`, `EP-010`, `FR-006`, `FR-026`.

**US-008 — Create/register a work product**  
As a professional/operational user, I want to create or register the work product required by my workflow and relate it to its context, so that the output is governed as part of the business outcome.  
Links: `EP-005`, `SR-003`, `FR-019`.

**US-009 — Revise a controlled work product**  
As an authorised contributor, I want a revised work product to remain related to previous revisions and its status, so that people can distinguish current, historical and superseded information.  
Links: `EP-005`, `FR-020`, `BR-014`.

**US-010 — Issue/distribute approved information**  
As an authorised participant, I want approved information issued to identified recipients with a retained issue record where required, so that distribution is controlled and traceable.  
Links: `EP-005`, `FR-021`.

### Review, authority and decisions

**US-011 — Request review/approval**  
As a work owner, I want to request the required review or approval against the correct subject and evidence, so that completion is governed rather than informal.  
Links: `EP-006`, `FR-015`.

**US-012 — Decide within authority**  
As a decision maker, I want the system to tell me whether I have the required authority for the decision/context, so that I do not accidentally make an unauthorised commitment.  
Links: `EP-006`, `SR-016`, `FR-017`.

**US-013 — Record decision evidence**  
As an assurance stakeholder, I want decisions to retain who decided, when, under what context/authority and with what evidence, so that the outcome can later be understood and relied upon.  
Links: `EP-006`, `SR-016`, `SR-017`, `FR-016`, `FR-022`.

**US-014 — Act on an approved decision**  
As a work owner, I want an approved decision to trigger the authorised next state/work/notification/integration where configured, so that the process progresses consistently.  
Links: `EP-006`, `FR-018`.

### External collaboration

**US-015 — Invite an external participant only to relevant work**  
As an internal work owner, I want to involve an external participant in a specific approved context without granting wider access, so that collaboration is useful and secure.  
Links: `EP-007`, `SR-031`, `FR-004`.

**US-016 — External contributor sees only what they need**  
As an external collaborator, I want a low-friction view of the information/actions relevant to my contribution, so that I can participate without navigating unrelated internal systems.  
Links: `EP-007`, `SR-032`, `RULE-008`.

### Commercial/delivery continuity

**US-017 — Understand delivery in commercial context**  
As a commercial or project stakeholder, I want delivery work/progress/change related to the applicable commercial commitment, so that delivery and commercial position are not reconciled manually after the fact.  
Links: `EP-008`, `SR-010`, `FR-023`.

**US-018 — Control commercial change**  
As an authorised commercial stakeholder, I want material commercial changes recorded and governed before downstream consequences occur, so that commitments remain traceable.  
Links: `EP-008`, `EP-009`, `FR-024`.

**US-019 — Hand approved financial information to finance**  
As a commercial/finance user, I want approved billing/financial information handed to the retained finance system where applicable, so that financial processing uses controlled delivery context.  
Links: `EP-008`, `FR-025`, `EP-013`.

### Management information and outcomes

**US-020 — See scoped team/project status**  
As a manager, I want current work, commitments, blockers and exceptions for my legitimate scope, so that I can manage outcomes proactively.  
Links: `EP-011`, `SR-006`, `SR-009`, `FR-028`.

**US-021 — Drill from metric/exception to source**  
As an executive or manager, I want to drill from an aggregate/exception to permitted supporting business context, so that I can understand what is driving the result.  
Links: `EP-011`, `SR-014`, `FR-029`.

**US-022 — Trust a reported measure**  
As an executive/assurance stakeholder, I want material measures to show governed definitions and sources, so that decisions are based on explainable information.  
Links: `EP-011`, `EP-012`, `REP-002`, `REP-003`.

**US-023 — Measure whether the workflow improved**  
As a product/customer sponsor, I want agreed outcome metrics for the pilot workflow, so that we can determine whether NuBlox created measurable value.  
Links: `EP-012`, `SR-027`, `BR-016`, `AC-015`.

### Integration, migration and administration

**US-024 — Know whether an external integration completed**  
As a work/support user, I want to know whether a material cross-system action was requested, accepted, completed or failed, so that I can recover without guessing.  
Links: `EP-013`, `FR-034`, `INT-004`.

**US-025 — Reconcile failed integration activity**  
As a support/operations user, I want failed or inconsistent integration activity surfaced with correlation/evidence and recovery actions, so that cross-system integrity can be restored safely.  
Links: `EP-013`, `INT-006`, `INT-007`.

**US-026 — Validate migrated information**  
As an implementation/data stakeholder, I want source-to-target reconciliation and exceptions for migrated information, so that migration success is evidenced rather than assumed.  
Links: `EP-014`, `DATA-009`, `AC-008`.

**US-027 — Configure supported customer variation**  
As an authorised administrator, I want to configure supported customer variation with change history and validation, so that the product can adapt without an unmanaged code fork.  
Links: `EP-015`, `SR-022`, `FR-036`, `FR-037`.

**US-028 — Administer access safely**  
As an authorised administrator, I want access/configuration controls that are attributable and governed, so that I do not need uncontrolled direct data manipulation.  
Links: `EP-016`, `SR-019`, `NFR-SEC-005`.

**US-029 — Diagnose service health/failure**  
As a support/operations user, I want health, logs, metrics and correlation for critical services/dependencies, so that incidents can be diagnosed efficiently.  
Links: `EP-017`, `SR-021`, `NFR-OPS-001`, `NFR-OPS-002`.

**US-030 — Investigate material history**  
As an authorised assurance/support stakeholder, I want to inspect relevant historical and audit evidence, so that I can reconstruct material events without altering production data.  
Links: `EP-018`, `SR-018`, `SR-026`, `FR-040`.

## Story readiness

A candidate story may move toward delivery only when it has:

- validated/approved requirement links;
- user/context evidence;
- clear acceptance criteria;
- applicable business rules/NFRs;
- data/interface dependencies;
- priority;
- explicit out-of-scope boundaries where needed.

## References

- `Epics.md`
- `Stakeholder_requirements_specification.md`
- `Functional_requirements_specification.md`
- `Acceptance_criteria.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial 30 candidate user stories derived from controlled requirements and epics |