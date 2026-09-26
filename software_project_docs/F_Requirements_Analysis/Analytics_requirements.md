# Analytics requirements

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-013  
**Document Type:** Analytics requirements  
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
**Related Documents:** `Reporting_requirements.md`, `Data_requirements.md`, `Business_requirements_document_BRD.md`, `Non-functional_requirements_specification.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Analytics_requirements.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Analytics_requirements.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define candidate requirements for diagnostic, predictive and optimisation-oriented analysis using governed NuBlox information without allowing analytics to become an uncontrolled parallel data truth.

## Analytics principles

1. Operational truth remains governed by authoritative business data and definitions.
2. Derived metrics/models shall retain provenance to source data and model/rule versions where material.
3. Analytical outputs that affect decisions or automated actions require defined accountability and validation.
4. Personal/sensitive data shall be minimised and protected according to approved purpose and classification.
5. Model quality and analytical usefulness shall be measured rather than assumed.

## Candidate analytics requirements

| ID | Requirement |
|---|---|
| `ANA-001` | Analytical datasets shall have traceable lineage to governed source data and transformation logic. |
| `ANA-002` | Derived measures/features shall have controlled definitions and ownership where they are used for material business decisions. |
| `ANA-003` | Analytical processing shall preserve customer/tenant isolation and applicable access restrictions. |
| `ANA-004` | Analytical outputs shall expose the applicable time period, source currency/freshness and material assumptions where needed for interpretation. |
| `ANA-005` | Predictive/recommendation models shall have documented purpose, training/validation basis, version and accountable owner before material operational use. |
| `ANA-006` | Automated or AI-assisted recommendations with material effect shall retain sufficient explanation/provenance and human-review controls appropriate to risk. |
| `ANA-007` | Model/analysis performance shall be monitored for drift, degradation or invalid assumptions where ongoing use materially affects outcomes. |
| `ANA-008` | Analytics shall support measurement of approved product/business outcomes so benefit realisation can be evaluated. |
| `ANA-009` | Customer-configurable analytics shall use governed semantic definitions and shall not silently redefine core measures. |
| `ANA-010` | Analytics exports/integrations shall follow the same classification, minimisation, audit and interface controls as other information exchanges. |
| `ANA-011` | Where analytics uses snapshots or denormalised stores, refresh and reconciliation behaviour shall be explicit. |
| `ANA-012` | Analytical features shall not be considered accepted solely because a model produces output; usefulness, correctness and operational impact require validation. |

## Initial analytical questions to investigate

- Where is work delayed and why?
- Which handoffs create repeatable bottlenecks?
- Which commitments are at risk of late or incomplete delivery?
- How do workload, capacity and competence relate to planned work?
- Where do commercial/financial outcomes diverge from delivery performance?
- Which changes/issues drive rework?
- Which control failures recur?
- Which product workflows create measurable customer benefit?

These are discovery questions, not approved analytical products.

## Acceptance evidence

Approved analytical capabilities should have applicable evidence for:

- source-data quality and lineage;
- calculation/model validation;
- access/privacy controls;
- reproducibility;
- performance/freshness;
- business usefulness;
- monitoring and change/version management.

## References

- `Reporting_requirements.md`
- `Data_requirements.md`
- `Non-functional_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial analytics requirements baseline |