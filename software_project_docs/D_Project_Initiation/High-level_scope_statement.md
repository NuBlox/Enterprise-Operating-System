# High-level scope statement

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-004  
**Document Type:** High-level scope statement  
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
**Related Documents:** `Project_charter.md`, `Project_brief.md`, `Project_initiation_document_PID.md`, `Objectives_and_success_criteria.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/High-level_scope_statement.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/High-level_scope_statement.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the current project boundary at a level sufficient to control discovery, requirements, architecture and early implementation while leaving detailed feature scope to evidence-led requirements work.

## Scope objective

The project shall define and deliver the initial coherent NuBlox product needed to prove that one software environment can connect enterprise operations with the professional/project work through which the target organisation creates value.

## In scope

### Product and customer definition
- validate initial customer segment;
- define user groups and end-to-end business outcomes;
- identify measurable customer pain/value;
- define adoption and implementation assumptions.

### Business and product requirements
- business requirements;
- user journeys/scenarios;
- functional requirements;
- non-functional requirements;
- business rules;
- data requirements;
- integration/interface requirements;
- reporting/analytics requirements;
- acceptance criteria and traceability.

### Solution design
- solution architecture;
- system context and component responsibilities;
- information/data model;
- security/privacy design;
- integration design;
- migration design;
- UX/information architecture;
- deployment/operability design;
- accessibility requirements;
- architecture decision records.

### Software implementation
- source repository standards;
- development environments and CI/CD;
- production-quality platform foundations;
- controlled persistence and migration capability;
- identity/access controls;
- audit/observability;
- selected end-to-end business workflows;
- automated test infrastructure;
- APIs/integrations required by the agreed initial scope.

### Pilot and readiness
- customer onboarding/configuration;
- data migration for agreed pilot scope;
- training/support materials;
- runbooks and monitoring;
- security/performance/accessibility verification;
- pilot acceptance;
- benefits measurement;
- production-readiness recommendation.

## Out of scope at initiation

Unless later approved through change control:

- serving every business function from day one;
- serving every CBE organisation type from day one;
- serving every geography/jurisdiction;
- replacing specialist authoring/engineering software without proven customer value;
- uncontrolled bespoke customer features;
- on-premises/dedicated deployment variants not justified by requirements;
- broad marketplace/ecosystem development before the core product is validated;
- advanced AI capabilities without specific requirements, data controls and benefit cases;
- production commitments before pilot/readiness gates.

## Initial end-to-end scope hypothesis

Detailed workflow selection will be evidence-led. Candidate chains to investigate include:

```text
lead / opportunity
→ proposal / appointment / contract
→ project mobilisation
→ people / resource planning
→ delivery work and controlled work products
→ review / decision / issue
→ project commercial position
→ invoicing / cash / performance
→ closeout / client relationship / lessons
```

This chain is a discovery hypothesis, not an approved software module structure.

## Scope acceptance tests

A proposed capability belongs in the initial project scope only when it can demonstrate one or more of:

- required to complete a priority end-to-end business outcome;
- required for enterprise security, control or operability;
- required to preserve authoritative business information/context;
- required for a pilot customer's measurable value case;
- required to avoid a material architectural dead end;
- required by legal/regulatory obligation for the selected operating model.

## Scope exclusions test

A capability should normally be deferred when:

- it does not support a priority outcome;
- a specialist system already performs it well and integration is sufficient;
- it creates large complexity before customer value is proven;
- it is customer-specific rather than reusable;
- it belongs to a later market/industry expansion.

## Scope control

Detailed scope shall be governed through requirements, prioritisation and formal change control. Feature count is not a project success metric.

## References

- `software_project_docs/D_Project_Initiation/Project_charter.md`
- `software_project_docs/D_Project_Initiation/Project_brief.md`
- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`
- `software_project_docs/D_Project_Initiation/Objectives_and_success_criteria.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial controlled high-level project scope |
