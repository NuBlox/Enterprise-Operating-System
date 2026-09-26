# High-level budget

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-015  
**Document Type:** High-level budget  
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
**Related Documents:** `../A_Enterprise_Pre_Project/Budget__funding_model.md`, `High-level_resource_plan.md`, `High-level_schedule.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/High-level_budget.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/High-level_budget.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Establish the initial project budget structure and approval rules while avoiding unsupported cost figures before the resource plan and delivery schedule are baselined.

## Current status

No numeric project budget is approved in version 0.1.

This document defines the project cost structure that must be populated and approved before material delivery commitments are made.

## Budget categories

| Category | Typical contents | Current value | Maturity |
|---|---|---:|---|
| Product / business analysis | product, BA, domain research, customer discovery | TBD | Conceptual |
| Architecture / engineering | architecture, software engineering, data/integration engineering | TBD | Conceptual |
| UX / design | user research, interaction/product design, accessibility | TBD | Conceptual |
| Quality / testing | QA, automation, performance, accessibility testing | TBD | Conceptual |
| Security / privacy / compliance | engineering, testing, legal/privacy/compliance support | TBD | Conceptual |
| Cloud / environments / tooling | cloud, CI/CD, observability, dev/test tools | TBD | Conceptual |
| Migration / integration | profiling, migration tooling, connectors, test environments | TBD | Conceptual |
| Pilot / implementation | customer onboarding, configuration, training, pilot support | TBD | Conceptual |
| Operations / support readiness | monitoring, support tools, runbooks, service setup | TBD | Conceptual |
| Commercial / legal | contracts, insurance, sales/solution support | TBD | Conceptual |
| Contingency / management reserve | risk-based reserve | TBD | Conceptual |

## Budget phases

The budget should be separated by delivery phase:

1. Project initiation and customer discovery.
2. Requirements and architecture.
3. Foundation implementation.
4. Initial market solution.
5. Pilot and remediation.
6. Production readiness and launch.

This allows staged approval and prevents later-phase assumptions being treated as committed spend.

## Budget control fields

Once numeric values are introduced, reporting shall include:

- approved baseline;
- approved changes;
- committed spend;
- actual spend;
- current forecast;
- estimate to complete;
- variance;
- contingency remaining;
- forecast confidence.

## Estimate quality

All figures must state estimate maturity:

- Conceptual;
- Indicative;
- Planned;
- Baselined;
- Committed;
- Actual.

## Approval dependencies

A baseline numeric budget requires:

- named delivery roles and rates/costs;
- high-level schedule;
- initial scope baseline;
- customer discovery plan;
- architecture/technical spike allowance;
- security/compliance allowance;
- cloud/tooling assumptions;
- pilot/implementation assumptions;
- contingency linked to risk exposure.

## Budget guardrails

- Do not hide implementation/migration cost inside product engineering.
- Do not exclude security/compliance/operations from the initial cost picture.
- Do not use aspirational future revenue to justify unbounded current spend.
- Major scope changes require budget impact assessment.
- A budget underspend caused by missing scope/evidence is not automatically a success.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Budget__funding_model.md`
- `software_project_docs/D_Project_Initiation/High-level_resource_plan.md`
- `software_project_docs/D_Project_Initiation/High-level_schedule.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial project budget-control structure; numeric budget intentionally deferred |
