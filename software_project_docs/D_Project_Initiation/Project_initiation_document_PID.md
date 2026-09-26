# Project initiation document (PID)

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-003  
**Document Type:** Project initiation document (PID)  
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
**Related Documents:** `Project_charter.md`, `Project_brief.md`, `High-level_scope_statement.md`, `Objectives_and_success_criteria.md`, `Stakeholder_register.md`, `Governance_structure.md`, `RACI_chart.md`, `Assumptions_log.md`, `Constraints_log.md`, `Initial_risk_register.md`, `Initial_dependency_log.md`, `High-level_schedule.md`, `High-level_budget.md`, `High-level_resource_plan.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Project_initiation_document_PID.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define how the initial NuBlox product-definition and delivery project will be governed, planned, controlled and progressed from initiation into requirements, architecture and implementation.

## Project context

The project operates under the NuBlox Enterprise Operating System Programme Charter. The programme is investigating and developing a software product intended to reduce the operational divide between managing an organisation and delivering the work through which it creates value.

The current initial-market hypothesis is UK mid-market multidisciplinary built-environment consultancies and project-services firms. Customer evidence remains incomplete and may change the initial market or scope.

## Project objectives

The project shall:

1. validate the customer problem and initial target segment;
2. define end-to-end business outcomes and priority user scenarios;
3. baseline business, functional and non-functional requirements;
4. establish data, integration, reporting, security and compliance requirements;
5. design and approve a coherent solution architecture;
6. implement production-quality foundations and representative end-to-end slices;
7. verify quality, security, performance and operability;
8. pilot the product with measurable success criteria;
9. establish repeatable migration, onboarding, support and operations;
10. make an evidence-based production-launch recommendation.

## Scope control

Detailed scope is controlled by `High-level_scope_statement.md` and later requirements baselines.

Material additions that affect budget, architecture, schedule, regulatory exposure or customer-market focus require controlled change and decision records.

## Delivery lifecycle

The project shall progress through these controlled phases:

### 1. Initiation
- project governance;
- scope and objectives;
- stakeholder/RACI baseline;
- assumptions, constraints, risks and dependencies;
- high-level budget/resource/schedule.

### 2. Requirements and analysis
- customer/user research;
- business requirements;
- process/workflow scenarios;
- functional requirements;
- non-functional requirements;
- data/integration/reporting requirements;
- business rules;
- acceptance criteria and traceability.

### 3. Architecture and design
- system context;
- solution architecture;
- information/data design;
- security/privacy architecture;
- integration and migration architecture;
- UX/information architecture;
- deployment/operability design;
- ADRs.

### 4. Implementation and verification
- development standards and environments;
- production-quality foundations;
- incremental end-to-end delivery;
- automated testing;
- security/performance/accessibility verification;
- documentation.

### 5. Pilot and release readiness
- controlled pilot deployment;
- onboarding/migration;
- support/runbooks;
- operational readiness;
- benefits measurement;
- release/launch decision.

## Governance

Proposed governance roles:

- Programme Sponsor / investment authority — [TBD]
- Product Owner — [TBD]
- Project Manager — [TBD]
- Architecture authority — [TBD]
- Engineering Lead — [TBD]
- Security/Privacy authority — [TBD]
- Quality/Acceptance authority — [TBD]
- Data Governance authority — [TBD]
- Commercial/Customer authority — [TBD]

No placeholder role is considered appointed until explicitly confirmed.

## Decision controls

Material decisions shall be recorded through controlled documents or ADR/decision records and include:

- decision owner;
- date;
- options considered;
- rationale;
- evidence;
- consequences;
- review conditions where applicable.

## Change control

Changes are assessed against:

- customer/business value;
- requirements impact;
- architecture impact;
- budget/resource impact;
- schedule impact;
- security/privacy/compliance impact;
- migration/operational impact;
- benefits and risk impact.

High-impact changes require approval by the relevant authority.

## Risk and issue management

The project will maintain separate risk and issue controls. Risks have probability/impact/mitigation/owner; issues represent events that have occurred and require action.

The Enterprise Risk Register remains a strategic parent input. Project-specific risks will be maintained in `Initial_risk_register.md` and later operational project controls.

## Quality management

Quality will be defined by approved requirements and acceptance criteria. The project shall maintain traceability from requirement to implementation and verification evidence.

Expected quality dimensions include:

- functional correctness;
- security/privacy;
- availability/reliability;
- performance/scalability;
- accessibility/usability;
- maintainability;
- data integrity;
- auditability/traceability;
- operability/recoverability.

## Configuration and repository control

- `main` is the authoritative integration branch for completed work.
- Completed work is not considered done while residual PRs/branches remain unless explicitly retained for an approved reason.
- Controlled documents and source code are versioned in Git.
- Material document status changes require metadata/change-history updates.

## Reporting

At minimum project reporting should cover:

- scope/change status;
- schedule/milestones;
- budget/forecast;
- risks/issues/dependencies;
- requirements status;
- architecture decisions;
- build/test/security evidence;
- customer research/pilot evidence;
- benefits/acceptance status.

## Exit criteria from initiation

Project Initiation can move to review when:

- charter, brief and PID are drafted;
- high-level scope/objectives are defined;
- stakeholders/governance/RACI are documented;
- assumptions/constraints/risks/dependencies are captured;
- high-level schedule, budget and resource plan exist;
- next-phase requirements activities are planned;
- funding/authority to proceed is confirmed.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Programme_charter.md`
- `software_project_docs/D_Project_Initiation/Project_charter.md`
- `software_project_docs/D_Project_Initiation/Project_brief.md`
- `software_project_docs/D_Project_Initiation/High-level_scope_statement.md`
- `software_project_docs/D_Project_Initiation/Objectives_and_success_criteria.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial controlled PID |
