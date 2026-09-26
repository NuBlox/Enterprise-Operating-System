# High-level schedule

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-013  
**Document Type:** High-level schedule  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Programme  
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
**Related Documents:** `Project_initiation_document_PID.md`, `Initial_dependency_log.md`, `Milestone_list.md`, `High-level_budget.md`, `High-level_resource_plan.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/High-level_schedule.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/High-level_schedule.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the initial project sequence and stage dependencies without creating false calendar precision before resources, customer access and funding are baselined.

## Scheduling principle

Version 0.1 is **sequence-based, not date-based**. Calendar dates will be introduced only after resource availability, funding, dependencies and customer-research access are sufficiently known.

## High-level sequence

### Phase 1 — Project initiation

Outputs:
- charter, brief and PID;
- high-level scope/objectives;
- stakeholder/governance/RACI;
- assumptions/constraints;
- initial risks/dependencies;
- budget/resource/schedule baseline.

Exit condition: authority and resources exist to enter requirements discovery.

### Phase 2 — Customer discovery and requirements

Outputs:
- primary customer research;
- representative end-to-end scenarios;
- business requirements;
- functional requirements;
- non-functional requirements;
- data/integration/reporting requirements;
- business rules;
- acceptance criteria and traceability.

Exit condition: requirements are sufficiently complete and prioritised to support architecture.

### Phase 3 — Architecture and product design

Outputs:
- system context;
- solution architecture;
- security/privacy architecture;
- data/integration/migration architecture;
- UX/information architecture;
- deployment/operability design;
- ADRs;
- implementation slice plan.

Exit condition: architecture is approved and major risks have evidence-backed treatments.

### Phase 4 — Foundation implementation

Outputs:
- engineering standards/environments;
- CI/CD;
- production-quality core platform foundations;
- security/data/audit/observability foundations;
- first representative end-to-end slice.

Exit condition: architecture and delivery model are proven in working software.

### Phase 5 — Initial market solution

Outputs:
- priority end-to-end workflows;
- necessary integrations;
- migration/onboarding capability;
- user experience and operational controls;
- automated verification.

Exit condition: product is suitable for controlled pilot use.

### Phase 6 — Pilot and benefits validation

Outputs:
- pilot deployment;
- migration/onboarding;
- user adoption;
- measured outcomes;
- defect/support evidence;
- product/business-case updates.

Exit condition: pilot acceptance and evidence support or reject production progression.

### Phase 7 — Production readiness and launch decision

Outputs:
- security/performance/accessibility evidence;
- operational readiness;
- support/runbooks;
- release/cutover/rollback plan;
- commercial readiness;
- launch decision.

## Parallel workstreams

The following run across multiple phases:

- customer/market research;
- risk and compliance;
- security/privacy;
- data governance;
- documentation/traceability;
- financial/TCO modelling;
- benefit measurement;
- commercial/legal preparation.

## Critical dependencies

The schedule cannot be calendared credibly until at least:

- decision authorities are appointed;
- delivery capacity is known;
- funding envelope exists;
- customer research access is established;
- priority initial scope is selected.

## Schedule control

Once baselined, the schedule shall distinguish:

- baseline dates;
- actual dates;
- current forecast;
- approved changes;
- critical path/dependencies;
- milestone confidence.

## References

- `software_project_docs/D_Project_Initiation/Initial_dependency_log.md`
- `software_project_docs/D_Project_Initiation/Milestone_list.md`
- `software_project_docs/D_Project_Initiation/High-level_resource_plan.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial sequence-based schedule; calendar dates intentionally deferred |
