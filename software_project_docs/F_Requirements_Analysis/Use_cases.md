# Use cases

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-021  
**Document Type:** Use cases  
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
**Related Documents:** `User_stories.md`, `Functional_requirements_specification.md`, `Business_rules.md`, `Acceptance_criteria.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Use_cases.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Use_cases.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Describe candidate system interactions at a level suitable for validating functional requirements, business rules, data needs, exceptions and acceptance criteria before detailed design.

All use cases are Candidate until customer/business validation.

---

## UC-001 — Initiate and complete controlled work

**Primary actor:** Authorised work participant  
**Supporting actors:** Work owner/manager; reviewer/approver where required  
**Links:** `US-001`–`US-005`; `FR-009`–`FR-014`

### Preconditions
- actor is authenticated and authorised;
- approved business trigger/context exists;
- required business information is available or explicitly identified as missing.

### Main flow
1. A valid business trigger initiates work.
2. NuBlox records the work context and applicable ownership/routing.
3. The assigned participant sees the work and required inputs/outcome.
4. The participant performs the work and records/relates required outputs.
5. Required validations/reviews are completed.
6. Work is handed to the next participant/stage or completed.
7. Relevant downstream consequences are triggered.
8. The final state and evidence are retained.

### Alternate/exception flows
- missing prerequisite;
- actor lacks permission/authority;
- work is blocked/overdue;
- required review is rejected;
- dependent integration fails;
- work is cancelled/superseded.

### Postconditions
- current work state is known;
- ownership/outcome/evidence are traceable;
- no required control is silently bypassed.

---

## UC-002 — Create, review and issue a governed work product

**Primary actor:** Professional/operational contributor  
**Supporting actors:** Reviewer; approver; recipient  
**Links:** `US-008`–`US-014`; `FR-019`–`FR-022`; `RULE-003`, `RULE-006`

### Main flow
1. Contributor creates/registers a work product in the correct business/work context.
2. NuBlox records identity, ownership and applicable status.
3. Contributor submits the work product for required review/approval.
4. Reviewer accesses the applicable version and supporting context.
5. Reviewer records comments/outcome.
6. Where approval is required, authority is validated and decision recorded.
7. Approved information is issued/distributed to authorised recipients.
8. Issue evidence and relationships to work/decision/recipient are retained.

### Exceptions
- review requests changes;
- a newer revision supersedes the submitted version;
- approver lacks authority;
- recipient access/distribution fails;
- issue is withdrawn/superseded.

### Acceptance focus
Revision integrity, decision provenance, controlled issue and historical reconstruction.

---

## UC-003 — Make an authorised business decision

**Primary actor:** Decision maker  
**Supporting actors:** Requestor; assurance stakeholder  
**Links:** `US-011`–`US-014`; `FR-015`–`FR-018`; `AC-004`

### Main flow
1. A decision request is raised against an identified subject.
2. Required evidence and conditions are presented.
3. System evaluates applicable permission and authority rules.
4. Decision maker records outcome and required rationale/conditions.
5. NuBlox records actor, time, context, authority and evidence.
6. Approved downstream consequences occur.

### Exceptions
- incomplete evidence;
- no valid authority;
- delegated/escalated decision required;
- conflict/segregation-of-duties rule prevents action;
- decision expires/is superseded/appealed where applicable.

---

## UC-004 — Collaborate with an external participant

**Primary actor:** Internal work owner  
**Supporting actor:** External collaborator  
**Links:** `US-015`, `US-016`; `FR-004`; `RULE-008`

### Main flow
1. Internal owner identifies required external contribution.
2. External participant is invited/associated with the approved context.
3. External access is limited to required information/actions.
4. External participant receives context and performs/submits required contribution.
5. Internal workflow records receipt/review/acceptance.
6. External access changes/ends according to lifecycle rules.

### Acceptance focus
Least privilege, low-friction collaboration, attributable contributions and access lifecycle.

---

## UC-005 — Manage a delivery/commercial change

**Primary actor:** Project/commercial stakeholder  
**Supporting actors:** Delivery owner; approver; finance/integration actor  
**Links:** `US-017`–`US-019`; `FR-023`–`FR-025`

### Main flow
1. Change is identified against delivery/commercial context.
2. Scope, reason, impact and supporting evidence are recorded.
3. Required review/authority is determined.
4. Decision is made and evidenced.
5. Approved change updates the applicable controlled state/commitments.
6. Required work, reporting and finance-system handoff occur.

### Exceptions
Change rejected; commercial authority exceeded; external finance handoff fails; conflicting concurrent change exists.

---

## UC-006 — Investigate and resolve an integration failure

**Primary actor:** Support/operations user  
**Supporting actor:** Business work owner  
**Links:** `US-024`, `US-025`; `INT-004`–`INT-007`; `AC-007`

### Main flow
1. Integration failure/inconsistency is detected.
2. Support user locates correlated business and technical evidence.
3. System shows request/result/retry/reconciliation state.
4. User determines safe recovery action.
5. Recovery is performed using approved mechanism.
6. Result is reconciled and status updated.
7. Resolution evidence remains attributable.

### Prohibited normal recovery
Direct uncontrolled database mutation.

---

## UC-007 — Migrate and reconcile customer information

**Primary actor:** Implementation/data specialist  
**Supporting actors:** Customer data owner; product/domain owner  
**Links:** `US-026`; `DATA-009`, `DATA-010`; `AC-008`

### Main flow
1. Source scope/profile is agreed.
2. Source data is extracted and mapped to target semantics.
3. Validation/transformation rules are applied.
4. Trial load runs.
5. Exceptions are classified/resolved.
6. Reconciliation compares agreed counts/values/relationships/history.
7. Customer/data owner accepts or rejects migration evidence.
8. Approved cutover is executed and evidenced.

### Acceptance focus
Semantic correctness, completeness, reconciliation, retained source identifiers/history and rollback/cutover evidence.

---

## UC-008 — Configure supported customer variation

**Primary actor:** Authorised administrator  
**Supporting actors:** Product/support owner  
**Links:** `US-027`, `US-028`; `FR-036`–`FR-038`; `AC-010`

### Main flow
1. Administrator selects approved configuration area.
2. Current configuration/version is shown.
3. Proposed change is validated against mandatory invariants.
4. Change is saved/published according to approval rules.
5. Change history and actor are recorded.
6. Affected behaviour uses the new effective configuration.

### Exceptions
Invalid configuration; prohibited security/control override; incompatible version; approval required.

---

## UC-009 — Review trusted management information

**Primary actor:** Manager/executive  
**Links:** `US-020`–`US-023`; `FR-028`, `FR-029`, `FR-041`; `REP-*`

### Main flow
1. User opens a permitted management view.
2. System scopes data to approved context.
3. Current measures/exceptions are presented with defined time basis.
4. User drills into supporting permitted context.
5. User can identify definition/source for material measures.
6. Any stale/incomplete data condition is surfaced where material.

### Acceptance focus
Scope enforcement, measure definitions, provenance, drill-through and reconciliation.

---

## UC-010 — Reconstruct a historical business event

**Primary actor:** Assurance/support stakeholder  
**Links:** `US-030`; `BR-014`, `BR-015`; `NFR-AUD-*`

### Main flow
1. User identifies the business subject/time/event to investigate.
2. System authorises access to historical/audit evidence.
3. Relevant past states/events/decisions are retrieved in time order.
4. User can identify actors/services, effective/current distinctions and evidence.
5. Investigation/export is retained according to policy where required.

### Acceptance focus
Historical integrity, attribution, time ordering, access controls and tamper resistance.

## Use-case validation gate

Before a use case is approved:

- representative users/business owners must validate the flow;
- business rules and exceptions must be complete enough for the stage;
- data/interface dependencies must be identified;
- acceptance criteria and NFRs must be linked;
- unresolved conflicts/variations must be recorded.

## References

- `User_stories.md`
- `Functional_requirements_specification.md`
- `Business_rules.md`
- `Acceptance_criteria.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial 10 candidate use cases for requirements validation |