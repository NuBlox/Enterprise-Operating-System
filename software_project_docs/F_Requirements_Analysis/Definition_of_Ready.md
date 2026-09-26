# Definition of Ready

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-026  
**Document Type:** Definition of Ready  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Business Analysis / Engineering  
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
**Related Documents:** `Backlog.md`, `Acceptance_criteria.md`, `Requirements_traceability_matrix_RTM.md`, `Definition_of_Done.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Definition_of_Ready.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Definition_of_Ready.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the minimum evidence required before a requirement/story/epic may enter implementation. Ready means sufficiently understood to begin responsibly; it does not mean every detail is immutable.

## Definition of Ready — delivery item

An item is `READY_FOR_DELIVERY` only when all applicable conditions are satisfied or explicitly waived by the accountable authority:

### Outcome and traceability
- [ ] Unique controlled ID exists.
- [ ] User/business outcome is clear.
- [ ] Item traces to an approved requirement, obligation, risk treatment or architecture enabler.
- [ ] Priority and target increment/pilot are agreed.

### Scope and behaviour
- [ ] In-scope behaviour is clear enough to implement.
- [ ] Material out-of-scope boundaries are stated.
- [ ] Primary/alternate/error flows are understood where applicable.
- [ ] Relevant business rules are linked.

### Acceptance and quality
- [ ] Testable acceptance criteria exist.
- [ ] Relevant NFR/security/privacy/accessibility requirements are identified.
- [ ] Required negative/security/error cases are understood.
- [ ] Verification method/evidence owner is identified.

### Information and interfaces
- [ ] Required business information/data concepts are identified.
- [ ] Authoritative source/ownership is understood for material data.
- [ ] Integration/API/interface dependencies are identified.
- [ ] Migration/history implications are identified where applicable.

### Design/delivery readiness
- [ ] Material architecture/design decisions needed before coding are resolved or deliberately time-boxed as a spike.
- [ ] Dependencies/blockers are acceptable.
- [ ] Required user/customer/design input is available.
- [ ] Delivery size is small enough to implement/verify coherently, or has been decomposed.

### Operations and change
- [ ] Logging/monitoring/audit needs are identified for material behaviour.
- [ ] Configuration/feature-release implications are understood.
- [ ] Documentation/training/support implications are identified where material.

## Waiver rule

A checklist item may be waived only when:

- the reason is documented;
- risk is understood;
- an owner accepts the consequence;
- the waiver does not bypass a mandatory security/legal/control requirement without the appropriate authority.

## Not Ready examples

An item is not ready when it is primarily:

- a feature name with no outcome;
- a screen mock-up with no requirement/behaviour;
- an incumbent-product feature copied without a validated need;
- dependent on undefined authority/data semantics that affect correctness;
- missing acceptance criteria for material behaviour;
- blocked by an unresolved external dependency;
- too broad to test coherently.

## References

- `Backlog.md`
- `Acceptance_criteria.md`
- `Requirements_traceability_matrix_RTM.md`
- `Definition_of_Done.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis / Engineering | Initial requirements delivery-readiness gate |