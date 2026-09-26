# Requirements prioritisation (MoSCoW, WSJF, etc.)

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-016  
**Document Type:** Requirements prioritisation  
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
**Related Documents:** `Business_requirements_document_BRD.md`, `Requirements_traceability_matrix_RTM.md`, `Acceptance_criteria.md`, `../D_Project_Initiation/High-level_scope_statement.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Requirements_prioritisation_MoSCoW_WSJF_etc.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Requirements_prioritisation_MoSCoW_WSJF_etc.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define how NuBlox requirements will be prioritised and record the first provisional priority view for requirements definition. This document does **not** constitute an approved release backlog.

## Prioritisation principles

1. Priority is based on business outcome, risk/control need, dependency and evidence—not feature popularity.
2. A requirement necessary for security, integrity, regulatory compliance or architecture viability may be high priority even when users do not explicitly request it.
3. A broad requirement can be strategically important while its detailed implementation is deferred.
4. Customer evidence may raise, lower, split or reject requirements.
5. Priority and sequence are different: a Must requirement may be implemented later if the selected pilot does not yet exercise it and no architecture/control dependency requires it earlier.
6. Scope decisions must remain traceable to business requirements and acceptance evidence.

## Provisional MoSCoW meanings

- **Must** — required for the coherent product/pilot architecture, a material control, or the central value proposition. Failure to address it would invalidate the current programme hypothesis or create a material dead end.
- **Should** — important to target users/outcomes, but the initial pilot may be credible with a controlled limitation.
- **Could** — useful candidate value that may be deferred without invalidating the current pilot/product hypothesis.
- **Won't yet** — deliberately outside the current planning horizon; retained for traceability.

## Provisional business-requirement priority

This is a **requirements-definition priority**, pending primary customer validation.

| Requirement | Provisional priority | Rationale |
|---|---|---|
| `BR-001` End-to-end operational continuity | Must | Central product hypothesis. |
| `BR-002` Work must be executable | Must | Product cannot be only a reporting shell. |
| `BR-003` Organisational and delivery context connected | Must | Required for end-to-end continuity and coherent information. |
| `BR-004` Authoritative business information | Must | Data integrity and management trust dependency. |
| `BR-005` Work and outputs connected | Must | Core traceability/professional-delivery requirement. |
| `BR-006` Responsibility, authority and access understandable | Must | Business integrity/security dependency. |
| `BR-007` Decisions and approvals evidenced | Must | Governance/control dependency. |
| `BR-008` Management information from operational work | Should | Important value proposition; depth can follow pilot workflow. |
| `BR-009` Cross-functional work | Must | Central fragmentation problem. |
| `BR-010` Controlled external participation | Should | Likely material for target segment; exact pilot need must be validated. |
| `BR-011` Variation without customer forks | Must | Product commercial/supportability constraint. |
| `BR-012` Specialist-system interoperability | Must | Market/coexistence strategy and practical adoption dependency. |
| `BR-013` Feasible migration | Must | Enterprise adoption constraint even if full migration scope is staged. |
| `BR-014` Preserve historical meaning | Must | Audit/business continuity/data-integrity requirement. |
| `BR-015` Security, privacy and auditability | Must | Non-negotiable enterprise control. |
| `BR-016` Measurable priority workflows | Must | Programme must prove customer value. |
| `BR-017` Commercially implementable product | Must | Prevents bespoke/non-viable delivery. |
| `BR-018` Outcome-led scope | Must | Governs the scope-management method itself. |

## Detailed prioritisation criteria

When detailed requirements/stories are prioritised, assess:

- customer/problem evidence strength;
- contribution to selected end-to-end outcome;
- legal/security/control obligation;
- dependency/architecture criticality;
- value/risk reduction;
- repeatability across target customers;
- implementation/migration complexity;
- operational/support cost;
- integration dependency;
- ability to verify/measure outcome;
- reversibility of the decision;
- learning value for the pilot.

## WSJF or scoring use

Weighted scoring may be used later for competing backlog items once approximate size and business value can be assessed credibly. The project shall not invent numeric precision before enough evidence exists.

A future WSJF-style model may consider:

```text
(cost of delay / job size)
```

where cost of delay is itself evidence-based and may combine user/business value, time criticality and risk/opportunity enablement.

## Pilot prioritisation gate

Before a pilot backlog is approved:

- one or more end-to-end target workflows must be selected;
- customer evidence must support the problem;
- Must/Should items must be mapped to workflow acceptance;
- NFR/security/data/migration dependencies must be included;
- explicit Won't-yet boundaries must be recorded;
- the RTM must show no unexplained implementation scope.

## References

- `Business_requirements_document_BRD.md`
- `Requirements_traceability_matrix_RTM.md`
- `Acceptance_criteria.md`
- `../D_Project_Initiation/High-level_scope_statement.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial prioritisation method and provisional BR-level MoSCoW for definition planning |