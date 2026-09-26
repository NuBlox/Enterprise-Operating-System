# Definition of Done

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-027  
**Document Type:** Definition of Done  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering / Quality  
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
**Related Documents:** `Definition_of_Ready.md`, `Acceptance_criteria.md`, `Requirements_traceability_matrix_RTM.md`, `Backlog.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Definition_of_Done.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Definition_of_Done.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the minimum evidence required before a delivered requirement/story/backlog item is considered Done. Done is an evidence state, not a developer assertion that coding has stopped.

## Definition of Done — delivery item

An item may be marked `DONE` only when all applicable conditions are satisfied or an approved exception is recorded.

### Requirement and acceptance
- [ ] Implemented behaviour matches the approved requirement/version.
- [ ] All applicable acceptance criteria pass.
- [ ] Material positive, negative, error and recovery cases are verified.
- [ ] Requirement-to-implementation-to-test traceability is updated.

### Quality
- [ ] Appropriate automated tests are present and passing.
- [ ] Regression impact is covered at the appropriate test level.
- [ ] Relevant NFR/security/privacy/accessibility criteria are satisfied or tracked through an explicit release-level gate.
- [ ] No unresolved defect remains that invalidates the approved acceptance criteria.

### Data and integration
- [ ] Data integrity/history/provenance requirements are satisfied.
- [ ] Interface/API contracts and integration behaviour are tested where applicable.
- [ ] Retry/idempotency/failure/reconciliation paths are tested where material.
- [ ] Migration/configuration changes are reconciled/validated where applicable.

### Security and control
- [ ] Access/authority rules are tested for permitted and denied cases.
- [ ] Material state changes/decisions produce required audit evidence.
- [ ] Secrets/sensitive information are handled according to approved controls.
- [ ] Applicable security review/testing findings are resolved or formally accepted.

### Operations
- [ ] Required logging, metrics, traces, health checks and alerts exist.
- [ ] Operational/support behaviour is documented where new material failure modes are introduced.
- [ ] Rollback/recovery or forward-fix approach is understood for the release mechanism.
- [ ] Configuration/defaults are safe and deployable across intended environments.

### Documentation and product readiness
- [ ] User/admin/API/support documentation is updated where applicable.
- [ ] Change/release notes are identified for externally visible behaviour.
- [ ] Accessibility/usability impact is reviewed where the user experience changes.
- [ ] Product/backlog status and dependencies are updated.

### Evidence
- [ ] Verification evidence is retained and linked.
- [ ] Review/approval required for the item has occurred.
- [ ] Any deviation/waiver is documented with owner and disposition.

## Release Done vs Item Done

An individual item being Done does not by itself mean a release is ready. Release readiness additionally requires integrated regression, security/performance/accessibility evidence as applicable, migration/deployment readiness, operational readiness and go/no-go approval.

## Pilot/customer Done

A pilot feature may be technically Done while the **business hypothesis remains unvalidated**. Pilot/customer outcome acceptance requires the separate measures defined in `Acceptance_criteria.md` and the benefits-realisation plan.

## Prohibited shortcuts

An item is not Done solely because:

- code has been merged;
- the happy path works on a developer machine;
- a demo succeeded;
- a unit test exists;
- the UI appears complete;
- a defect is known but unrecorded;
- a required downstream integration has been mocked without an agreed release limitation;
- operational/security/data evidence is missing.

## References

- `Definition_of_Ready.md`
- `Acceptance_criteria.md`
- `Requirements_traceability_matrix_RTM.md`
- `Backlog.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering / Quality | Initial evidence-based Definition of Done |