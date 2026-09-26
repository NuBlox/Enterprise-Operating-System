# High-level resource plan

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-016  
**Document Type:** High-level resource plan  
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
**Related Documents:** `High-level_budget.md`, `High-level_schedule.md`, `Milestone_list.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/High-level_resource_plan.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/High-level_resource_plan.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the initial capabilities and roles required to deliver the NuBlox project before individual appointments, headcount or cost commitments are approved.

## Resource-planning principles

- Plan for capabilities, not just job titles.
- Separate accountable decision roles from delivery capacity.
- Do not assume one person can sustainably cover several high-demand specialist roles during active delivery.
- Security, QA, data, migration, UX and operations are core delivery capabilities, not optional late-stage support.
- Resource levels should follow staged scope and evidence gates.

## Core project capabilities

| Capability | Primary responsibilities | Initial need |
|---|---|---|
| Product leadership | product outcomes, priority, customer value, roadmap | Required from initiation |
| Programme / project management | plan, RAID, dependencies, governance, budget | Required from initiation |
| Business analysis / domain analysis | workflows, requirements, rules, traceability | Required from requirements |
| User research / UX | research, journeys, interaction design, usability/accessibility | Required from discovery |
| Solution architecture | system context, architecture decisions, cross-cutting quality | Required before architecture baseline |
| Software engineering | implementation, tests, maintainability | Required from foundation build |
| Data architecture / engineering | data model, governance, migration, reporting foundations | Required from requirements/design |
| Integration engineering | interfaces, APIs, external-system coordination | Required as interfaces are selected |
| Security / privacy | threat/privacy analysis, security architecture, assurance | Required from requirements onward |
| Quality engineering | test strategy, automation, acceptance evidence | Required before implementation scales |
| DevOps / platform engineering | CI/CD, environments, cloud, observability, reliability | Required from foundation build |
| Operations / support design | service model, runbooks, incidents, continuity | Required before pilot readiness |
| Commercial / customer success | customer recruitment, pricing, contracts, pilots, value evidence | Required throughout |
| Legal / compliance advice | contracts, privacy, regulatory interpretation | As required by decision gates |

## Stage-based resource profile

### Initiation / discovery

Higher emphasis on:
- product;
- project management;
- business/domain analysis;
- customer research/UX;
- commercial;
- architecture/security advisory input.

### Requirements / architecture

Increase:
- business analysis;
- solution/data/security architecture;
- UX/product design;
- QA/test planning;
- migration/integration analysis.

### Foundation / implementation

Increase:
- software engineering;
- platform/DevOps;
- data/integration engineering;
- quality engineering;
- security engineering.

### Pilot / production readiness

Increase:
- implementation/migration;
- customer success/training;
- operations/support;
- QA/security/performance assurance;
- commercial/legal support.

## Appointment priorities

Before formal Project Initiation approval, appoint or explicitly assign:

1. Programme Sponsor / investment authority.
2. Product Owner.
3. Project Manager.
4. Architecture authority.
5. Engineering Lead or engineering authority.
6. Security/Privacy authority.
7. Quality/Acceptance authority.
8. Data Governance authority.
9. Commercial/Customer authority.

Delivery roles may initially be combined where capacity allows, but authority and accountability must remain clear.

## Capacity planning

Detailed planning should capture:

- role/capability;
- named resource;
- availability/FTE;
- start/end need;
- cost/rate;
- internal/external status;
- dependencies;
- critical skill risks;
- backup/continuity.

## Critical resource risks

- key-person dependency;
- insufficient domain knowledge;
- insufficient enterprise software engineering depth;
- security/privacy skills engaged too late;
- migration/integration effort underestimated;
- UX/user research capacity squeezed by implementation;
- QA treated as an end-stage activity;
- support/operations capability not established before pilot.

## Sourcing principles

Use external specialists where they accelerate evidence or provide independent assurance, but retain enough internal product/architecture knowledge that the product is not dependent on consultancy knowledge that NuBlox cannot own or maintain.

## Approval dependencies

A baselined resource plan requires:

- agreed initial scope;
- high-level schedule;
- budget/funding envelope;
- delivery model;
- make/buy/sourcing decisions;
- named governance roles.

## References

- `software_project_docs/D_Project_Initiation/High-level_budget.md`
- `software_project_docs/D_Project_Initiation/High-level_schedule.md`
- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial capability-based project resource plan |
