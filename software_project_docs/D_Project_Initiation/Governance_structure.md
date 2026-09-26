# Governance structure

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-007  
**Document Type:** Governance structure  
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
**Related Documents:** `Project_initiation_document_PID.md`, `Stakeholder_register.md`, `RACI_chart.md`, `../A_Enterprise_Pre_Project/Programme_charter.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Governance_structure.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Governance_structure.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the decision and oversight structure for the NuBlox initial product-definition and delivery project.

## Governance objectives

Governance shall ensure that:

- strategic intent remains connected to delivery;
- funding and scope decisions are explicit;
- product/customer evidence can change direction;
- architecture, security, data and quality decisions have clear authority;
- risks/issues are escalated at the right level;
- approvals and decisions are traceable;
- implementation cannot silently override approved requirements or controls.

## Proposed governance layers

### 1. Programme / Investment Authority

Responsibilities:

- approve programme direction and funding tranches;
- approve material changes to market scope or investment case;
- accept strategic residual risk;
- approve transition between major programme gates.

Participants: Sponsor / investment authority plus advisers as required.

### 2. Product & Customer Authority

Responsibilities:

- own product outcomes and prioritisation;
- approve product requirements and scope trade-offs;
- ensure customer evidence is represented;
- maintain roadmap coherence.

Primary accountable role: Product Owner.

### 3. Project Delivery Authority

Responsibilities:

- plan and coordinate delivery;
- maintain schedule, budget, RAID and dependencies;
- run change control;
- escalate exceptions;
- coordinate cross-discipline delivery.

Primary accountable role: Project Manager.

### 4. Architecture & Engineering Authority

Responsibilities:

- approve material architecture decisions;
- maintain engineering standards;
- ensure requirements traceability and technical quality;
- control technical debt and platform integrity;
- review implementation changes with systemic impact.

Primary accountable roles: Architecture Authority and Engineering Lead.

### 5. Security, Privacy & Data Authority

Responsibilities:

- define and approve security/privacy/data controls;
- assess regulatory and information risks;
- approve security-sensitive architecture and release evidence;
- own data-governance exceptions.

### 6. Quality & Acceptance Authority

Responsibilities:

- define verification and acceptance evidence;
- ensure requirement-to-test traceability;
- assess quality/security/performance/accessibility evidence;
- recommend acceptance/release readiness.

### 7. Customer / Pilot Governance

Responsibilities:

- agree pilot scope and measurable outcomes;
- resolve customer-specific decisions without creating ungoverned product forks;
- review migration/onboarding risk;
- accept pilot outcomes.

## Decision classes

| Decision class | Example | Required authority |
|---|---|---|
| Strategic | target market, major funding, launch | Programme / Investment |
| Product | priority, product scope, customer outcome | Product & Customer |
| Delivery | schedule sequencing, routine execution | Project Delivery |
| Architecture | data boundaries, platform/deployment, systemic tech choice | Architecture Authority |
| Security/Privacy/Data | sensitive-data control, risk acceptance | Relevant specialist authority |
| Quality/Release | acceptance evidence, readiness | Quality + accountable release authority |
| Customer/Pilot | pilot scope/outcome acceptance | Product + customer sponsor |

## Escalation rules

Escalation is required where a decision materially changes:

- approved scope;
- budget or funding need;
- milestone/launch commitment;
- security/privacy/compliance exposure;
- architecture principles;
- customer-market focus;
- contractual obligations;
- accepted residual risk.

## Governance cadence

Proposed initial cadence:

- delivery/RAID review — weekly once active delivery begins;
- product/requirements review — weekly or per discovery cadence;
- architecture/security review — at decision points, minimum fortnightly during design/build;
- programme/investment review — monthly and at stage gates;
- pilot steering — cadence agreed with pilot customer.

Cadence may change with project scale.

## Decision records

Material decisions must record:

- decision ID/date;
- decision owner;
- issue/question;
- options;
- evidence;
- rationale;
- consequences;
- affected requirements/documents;
- review trigger if conditional.

## Current status

This governance model is proposed. Named individuals and formal terms of reference remain to be approved.

## References

- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`
- `software_project_docs/D_Project_Initiation/RACI_chart.md`
- `software_project_docs/A_Enterprise_Pre_Project/Programme_charter.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial project governance model |
