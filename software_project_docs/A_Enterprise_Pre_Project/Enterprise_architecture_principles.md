# Enterprise architecture principles

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-012  
**Document Type:** Enterprise architecture principles  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture  
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
**Related Documents:** `Product_vision_statement.md`, `Product_strategy.md`, `Feasibility_study.md`, `Enterprise_risk_register.md`, `Compliance__regulatory_register.md`, `Data_governance_policy.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Enterprise_architecture_principles.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Enterprise_architecture_principles.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the initial architecture principles that should govern later requirements, solution architecture and technical design without prematurely choosing a technology stack or implementation pattern.

## Status and authority

These are Draft principles. They define decision criteria, not a completed architecture. Specific architecture choices require requirements, options analysis and Architecture Decision Records.

## Principles

### AP-01 — Business outcomes drive architecture

Architecture exists to enable complete business outcomes. Technology, modules, services and data stores must not become the starting point for product scope.

**Implication:** requirements and scenarios should describe what must be achieved before prescribing implementation components.

### AP-02 — One business fact should have clear authority

Where a business fact has an authoritative source, the system should make that authority explicit and avoid uncontrolled duplicates.

**Implication:** synchronised copies, projections and caches may exist, but ownership and reconciliation rules must be defined.

### AP-03 — Information and work remain connected

The system should preserve the relationship between work, the business subject of that work, outputs produced, decisions made and evidence retained.

**Implication:** task/workflow records must not become disconnected from the business outcome they exist to support.

### AP-04 — Security and privacy by design and default

Security/privacy requirements must be considered throughout design and delivery, not appended after implementation.

**Implication:** threat modelling, least privilege, strong authentication, secure defaults, data minimisation, logging and vulnerability management become lifecycle requirements.

### AP-05 — Traceability is designed in

Material changes, decisions, approvals and business events should be reconstructable to the level required by business, legal and assurance needs.

**Implication:** audit/history cannot depend solely on mutable application logs or user memory.

### AP-06 — Configuration before customer-specific forks

Variation between customers should be handled through governed configuration where economically and technically sensible.

**Implication:** customer-specific code branches or schema forks require exceptional justification and should not be the normal implementation model.

### AP-07 — Strong domain semantics over generic abstraction

Generic platform capability is valuable only when it preserves the meaning and rules of the business concepts it supports.

**Implication:** avoid designs that reduce every business concept to untyped generic data merely to maximise configurability.

### AP-08 — Interoperability is a core requirement

NuBlox will coexist with specialist systems, external authorities, banks, payroll providers, identity services, customer systems and other platforms.

**Implication:** APIs, events, imports/exports and integration governance are architectural concerns from the outset.

### AP-09 — Replace only where replacement creates value

Specialist software should not be reimplemented solely for product completeness.

**Implication:** build, integrate, govern or exclude decisions should be based on customer outcome, control needs, differentiation, risk and economics.

### AP-10 — Data migration is part of product architecture

Enterprise adoption depends on moving, reconciling and proving existing information safely.

**Implication:** import, mapping, validation, provenance, reconciliation and rollback/exception handling must be considered early.

### AP-11 — API/integration contracts are products

External and internal integration contracts require versioning, ownership, compatibility and observability.

**Implication:** interfaces should not be informal implementation details.

### AP-12 — Resilience is proportional to business criticality

Availability, recovery, backup, consistency and continuity requirements should be derived from business impact.

**Implication:** service objectives must be evidence-based rather than copied uniformly across all features.

### AP-13 — Prefer reversible decisions under uncertainty

Where requirements are immature, choose approaches that preserve options and minimise irreversible commitments.

**Implication:** prototype or isolate uncertain choices before embedding them across the product.

### AP-14 — Automate repeatable controls

Build, test, deployment, security checks, schema changes and operational controls should be automated wherever repeatability and evidence improve.

**Implication:** manual production processes require explicit justification and control.

### AP-15 — Observability is part of operability

A production system must expose enough metrics, logs, traces, health information and business/technical events to operate it safely.

**Implication:** operational visibility is designed alongside functionality.

### AP-16 — Accessibility and usability are architectural quality attributes

The system should support inclusive use and consistent user interaction without requiring each feature team to rediscover accessibility requirements.

**Implication:** shared design/system controls and automated/manual accessibility testing should be planned.

### AP-17 — Separate policy from mechanism where practical

Rules likely to vary by organisation, jurisdiction or time should not be unnecessarily hard-coded into infrastructure or application flow.

**Implication:** configurable policy may be appropriate, but must remain governed, versioned and testable.

### AP-18 — Historical meaning must survive change

Configuration, organisational structure, permissions, workflows and business definitions may change over time; historical records must remain interpretable in their original context where required.

**Implication:** effective dating, versioning or immutable evidence may be needed depending on the domain.

### AP-19 — Simplicity is a control

Unnecessary complexity increases defects, security risk, support cost and onboarding burden.

**Implication:** architecture reviews should challenge abstractions, distributed components and custom mechanisms that lack a clear requirement.

### AP-20 — Architecture decisions are explicit and testable

Important design choices should record context, options, decision, consequences and validation criteria.

**Implication:** ADRs and architecture tests should accompany material implementation decisions.

## Secure-development alignment

The NCSC advises organisations to embed security throughout development, secure repositories and pipelines, continually test security and plan for flaws. These principles should inform later secure-development standards and CI/CD design.

## Principle governance

Exceptions to an approved architecture principle should record:

- principle affected;
- requirement/business reason;
- alternatives considered;
- risk/consequence;
- compensating controls;
- accountable approver;
- review/expiry condition.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Product_vision_statement.md`
- `software_project_docs/A_Enterprise_Pre_Project/Feasibility_study.md`
- `software_project_docs/A_Enterprise_Pre_Project/Enterprise_risk_register.md`
- NCSC secure-development principles: https://www.ncsc.gov.uk/collection/developers-collection/principles
- NCSC Software Security Code guidance: https://www.ncsc.gov.uk/collection/software-security-code-of-practice-implementation-guidance/theme-1-secure-design-development

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Initial enterprise architecture principles draft |