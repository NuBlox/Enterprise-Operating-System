# First vertical slice selection and validation plan

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-004  
**Document Type:** Vertical slice selection and validation plan  
**Version:** 0.1  
**Status:** Draft — Candidate for validation  
**Author / Owner:** NuBlox Product / Engineering  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Product_backlog.md`, `Development_plan.md`, `../F_Requirements_Analysis/Business_requirements_document_BRD.md`, `../F_Requirements_Analysis/Stakeholder_requirements_specification.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../../A_Enterprise_Pre_Project/Customer_research_summary.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** None — programme-specific controlled selection artifact governed by the repository document-control metadata standard  
**Storage Location:** `software_project_docs/H_Development_Implementation/First_vertical_slice_selection.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the candidate first representative business workflow for NuBlox vertical-slice validation, the evidence supporting its selection, the scope that must be validated with customers and the gate that prevents candidate semantics from entering production code prematurely.

This document advances `DEV-201`. It does **not** approve the workflow for product implementation. The current SRS, BRD, stakeholder requirements and functional requirements remain Draft/Candidate, and primary customer discovery has not yet reached the programme's product-approval threshold.

## Current decision state

**Candidate selected for validation:** **Governed work-product review and issue**.

**Implementation state:** **Blocked pending primary workflow validation and approval.**

The candidate is intentionally cross-disciplinary. It represents a recurring class of built-environment and professional-services work in which an output is produced, reviewed, decided upon and issued/handed off with controlled evidence. It does not assume whether the output is a drawing, model, report, schedule, calculation, specification, commercial record or another professional work product.

## Why this candidate

The candidate provides a compact but meaningful test of the product proposition because it crosses the current Candidate requirements without requiring the programme to invent the whole enterprise taxonomy first.

It directly exercises the following controlled hypotheses:

- `BR-002` — users can perform work, not merely report its status;
- `BR-005` — work remains connected to its outputs;
- `BR-006` — responsibility, access and authority are understandable;
- `BR-007` — review/approval decisions retain evidence;
- `BR-009` — work can cross participant/team boundaries without losing context;
- `BR-014` — historical meaning can be reconstructed;
- `BR-015` — security/auditability are part of the business outcome;
- `BR-016` — the workflow can ultimately be measured;
- business outcome area 6 — governed production/review/issue of work products.

It also reflects stakeholder needs for clear work context, work-product continuity, predictable handoffs, decision provenance, control evidence and historical reconstruction (`SR-001`, `SR-003`, `SR-005`, `SR-016`–`SR-018`).

## Candidate workflow boundary

The candidate workflow begins when an authorised participant creates or registers a work product/revision requiring controlled review and ends when the resulting approved work product is issued/handed off to its approved recipient/context, or the review terminates in a recorded non-approval outcome.

Conceptually:

```text
approved work trigger/context
        ↓
create/register work product or revision
        ↓
prepare required review evidence
        ↓
submit for review / approval
        ↓
resolve authorised reviewer / decision authority
        ↓
review and record decision
   ┌────┴──────────────┐
   │                   │
not approved       approved
   │                   │
rework / close     controlled issue / handoff
   │                   │
   └──────────┬────────┘
              ↓
traceable history, audit/evidence and management visibility
```

This is a validation model, not yet a fixed BPMN process or canonical domain object model.

## Candidate actors

The following are **roles in the candidate workflow**, not approved NuBlox job titles or organisation roles:

| Actor concept | Candidate responsibility | Validation required |
|---|---|---|
| Originator / author | Creates or registers the work product/revision and prepares it for review | Who can originate and under what work/project context? |
| Responsible owner | Accountable for the work item/work product reaching the required outcome | Is this distinct from author/originator? |
| Reviewer | Performs technical/discipline/peer/quality review as required | What qualifications, independence or scope rules apply? |
| Approver / issuer | Makes the controlled decision permitting issue/handoff where required | Is reviewer and approver separation required, optional or absent? |
| Recipient / downstream participant | Receives the approved output or next work obligation | Internal, client, supplier or other external participation patterns? |
| Manager / assurance user | Requires visibility of due/blocked/rejected/approved work and evidence | Which measures and drill-through rights are valuable? |

## Candidate information subjects

These names describe information responsibilities for validation. They are not yet canonical production class/table names.

| Subject | Minimum meaning to validate |
|---|---|
| Work context | The organisational/project/service/work context under which the output is produced |
| Work obligation / item | The governed unit of work requiring an outcome |
| Work product | The controlled output being created, changed, reviewed or issued |
| Revision / version | The state of the work product to which review and issue apply |
| Review request | The request defining what is being reviewed, by whom and against what criteria/evidence |
| Decision | The review/approval outcome, actor, authority, time, rationale/evidence and consequence |
| Issue / handoff | The controlled release/distribution/acceptance event following an approved decision |
| Evidence reference | Supporting information required to understand or prove the work/decision/outcome |
| History / audit evidence | Append-oriented evidence required to reconstruct material workflow events |

## Candidate state model

Exact terminology must be validated. The minimum behavioural distinctions to test are:

```text
Draft / In preparation
        ↓
Ready for review
        ↓
In review
        ↓
Approved | Rejected / Changes required | Cancelled
        ↓
Issued / Handed off   (only where approval permits issue)
```

No production enum/state machine should be committed from this document until customer workflow evidence confirms the required states and transition rules.

## Candidate business rules to validate

1. A work product/revision belongs to one verified tenant and approved work context.
2. A submitted review identifies the exact revision/evidence being reviewed.
3. A review/approval action is performed only by a participant with the required access and authority for that context.
4. Decision evidence identifies actor, time, outcome, subject/revision, context and required rationale/evidence.
5. Rejection/changes-required cannot be represented as approval merely by changing a status field later; the historical decision remains reconstructable.
6. Issue/handoff occurs only when the applicable approval condition has been satisfied.
7. A later revision does not rewrite the historical meaning of an earlier review/issue event.
8. Cross-tenant access remains denied even when identifiers or external route values are supplied by the caller.
9. Management/attention views are derived from governed workflow state rather than a disconnected manual tracker.
10. Notification may inform participants but is not the authoritative record of the work or decision.

These are validation propositions, not approved customer-specific rules.

## Candidate requirement coverage

The first slice is deliberately selected to exercise a broad but coherent requirement chain:

- identity/context: `FR-001`–`FR-003`;
- governed business records/history: `FR-005`–`FR-008`;
- work initiation/routing/state/outputs/handoffs/exceptions: `FR-009`–`FR-014`;
- review/decision/authority/consequences: `FR-015`–`FR-018`;
- work-product registration/revisions/issue/evidence: `FR-019`–`FR-022`;
- attention and scoped visibility: `FR-027`–`FR-029`;
- controlled notifications where validated: `FR-030`;
- material business audit/evidence: `FR-039`–`FR-040`;
- governed operational view where validated: `FR-041`.

Commercial/financial continuity, broad search, customer configuration and external integrations are not automatically forced into the first slice unless workflow validation shows they are necessary to complete the selected outcome.

## Candidate vertical-slice acceptance scenarios

These are the minimum product behaviours to turn into detailed acceptance criteria **after validation**:

1. **Create/register** — an authorised participant creates/registers a work product/revision in a verified tenant/work context.
2. **Submit** — the exact revision and required evidence can be submitted for review.
3. **Attention/routing** — the correct authorised reviewer/approver sees the obligation requiring action.
4. **Approve** — an authorised decision maker records an approval with attributable evidence.
5. **Reject/changes required** — a non-approval decision remains traceable and routes/returns work according to validated rules.
6. **Authority negative path** — a participant lacking required authority cannot complete the controlled decision.
7. **Tenant negative path** — another tenant cannot read, change, review or issue the subject.
8. **Issue/handoff** — only an eligible approved revision can be issued/handed off; the event is historically attributable.
9. **Revision continuity** — creating a later revision does not alter the previous review/issue evidence.
10. **Management visibility** — authorised users can identify outstanding, blocked, rejected and issued work and drill to source context.
11. **Audit reconstruction** — authorised assurance users can reconstruct material events without relying on technical logs.
12. **Operational traceability** — technical trace/correlation supports diagnosis without becoming the authoritative business evidence store.

## Primary validation questions

Customer/process discovery must establish, for this candidate workflow:

### Trigger and outcome
- What real event creates the obligation to produce/review/issue the work product?
- What constitutes completion and business value?
- Which outputs are genuinely controlled versus informal working material?

### Roles and authority
- Who authors, owns, reviews, approves and issues?
- When are those roles separate or the same person?
- What competence, delegation, project/discipline or monetary/contractual authority rules apply?

### States and handoffs
- What states/terms are actually used?
- Which transitions require acceptance, review, evidence or explicit approval?
- What happens after rejection/changes-required, cancellation, overdue review or supersession?

### Work-product control
- How are revisions/versions identified?
- What does “issue” mean for different work-product types?
- Are transmittal/distribution/receipt acknowledgements material?
- Which specialist authoring/document/model systems must remain authoritative?

### External participation
- Do clients, suppliers or delivery partners review/accept/receive work directly?
- What information may they see and what account/access friction is acceptable?

### Evidence and assurance
- What evidence must be retained and for how long?
- Which actions require rationale/signature/approval provenance?
- Which contractual/regulatory/quality obligations affect the workflow?

### Value and measures
- Where does the workflow currently wait or fail?
- What is manually tracked/reconciled?
- Which measures would demonstrate improvement: review cycle time, first-pass approval, overdue reviews, rework, issue delay, duplicated entry or another outcome?

## Evidence required before DEV-201 can complete

`DEV-201` must remain **In Progress** until sufficient controlled evidence supports the workflow for the first production business slice. At minimum:

1. direct primary customer/process evidence exists rather than vendor/reference evidence alone;
2. the workflow is observed or confirmed across multiple relevant participants/organisations sufficiently to distinguish repeatable product behaviour from one customer's bespoke process;
3. actor responsibilities and authority rules are defined to the level required for acceptance testing;
4. controlled information subjects and required history are understood without prematurely fixing unnecessary taxonomy;
5. workflow states, review/decision rules and issue/handoff consequence are agreed for the initial slice;
6. security/external-participation assumptions are validated;
7. measurable problem/outcome evidence exists;
8. acceptance scenarios are reviewed and approved by the relevant product/business authority;
9. conflicts or organisation-specific variants are explicitly documented as standard, configuration, integration or deferred scope.

The broader customer-research exit criteria in `NBEOS-A-005` still apply to target-segment/product approval and are not bypassed by this workflow-selection artifact.

## Explicit non-decisions

This document does **not** yet decide:

- a canonical `WorkProduct`, `Review`, `Decision` or `Issue` production object taxonomy;
- a specific construction discipline or document/model format;
- a fixed approval-state vocabulary;
- whether review and approval must be separate stages;
- a document management/file-storage implementation;
- electronic/digital signature requirements;
- client/supplier external-access mechanics;
- email/notification provider;
- a specialist authoring/design system integration;
- commercial/billing consequences;
- a final UX/work-queue design.

Each enters implementation only when required by validated workflow evidence.

## DEV-201 outcome

**Current outcome:** candidate selected for validation; implementation gate remains closed.

`DEV-202`–`DEV-208` must remain blocked until this document's workflow scope, actors, records, material rules and acceptance scenarios have sufficient validation/approval evidence to satisfy `DEV-201` completion criteria.

## References

- `Product_backlog.md`
- `Development_plan.md`
- `../A_Enterprise_Pre_Project/Customer_research_summary.md`
- `../F_Requirements_Analysis/Business_requirements_document_BRD.md`
- `../F_Requirements_Analysis/Stakeholder_requirements_specification.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/BPMN_diagrams.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering | Selected governed work-product review and issue as the first workflow candidate for primary validation; defined scope, evidence needs and implementation gate without approving product semantics |
