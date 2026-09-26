# NuBlox Requirements & Analysis — Controlled Index

This directory contains the live controlled **Requirements & Analysis** documents for the NuBlox Enterprise Operating System programme.

Templates remain under [`../../software_project_docs_templates/F_Requirements_Analysis/`](../../software_project_docs_templates/F_Requirements_Analysis/).

## Governing requirements baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-F-001` | [Business requirements document (BRD)](Business_requirements_document_BRD.md) | Draft | 0.1 |
| `NBEOS-F-002` | [Stakeholder requirements specification](Stakeholder_requirements_specification.md) | Draft | 0.1 |
| `NBEOS-F-003` | [Software requirements specification (SRS)](Software_requirements_specification_SRS.md) | Draft | 0.1 |
| `NBEOS-F-004` | [Functional requirements specification](Functional_requirements_specification.md) | Draft | 0.1 |
| `NBEOS-F-005` | [Non-functional requirements specification](Non-functional_requirements_specification.md) | Draft | 0.1 |

## Supporting requirements controls

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-F-006` | [Business rules](Business_rules.md) | Draft | 0.1 |
| `NBEOS-F-007` | [Data requirements](Data_requirements.md) | Draft | 0.1 |
| `NBEOS-F-008` | [Data dictionary](Data_dictionary.md) | Draft | 0.1 |
| `NBEOS-F-009` | [Interface requirements](Interface_requirements.md) | Draft | 0.1 |
| `NBEOS-F-010` | [Integration requirements](Integration_requirements.md) | Draft | 0.1 |
| `NBEOS-F-011` | [API requirements](API_requirements.md) | Draft | 0.1 |
| `NBEOS-F-012` | [Reporting requirements](Reporting_requirements.md) | Draft | 0.1 |
| `NBEOS-F-013` | [Analytics requirements](Analytics_requirements.md) | Draft | 0.1 |
| `NBEOS-F-014` | [Acceptance criteria](Acceptance_criteria.md) | Draft | 0.1 |
| `NBEOS-F-015` | [Requirements traceability matrix](Requirements_traceability_matrix_RTM.md) | Draft | 0.1 |
| `NBEOS-F-016` | [Requirements prioritisation](Requirements_prioritisation_MoSCoW_WSJF_etc.md) | Draft | 0.1 |
| `NBEOS-F-017` | [Business glossary](Business_glossary.md) | Draft | 0.1 |

## Behavioural analysis

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-F-018` | [Themes](Themes.md) | Draft | 0.1 |
| `NBEOS-F-019` | [Epics](Epics.md) | Draft | 0.1 |
| `NBEOS-F-020` | [User stories](User_stories.md) | Draft | 0.1 |
| `NBEOS-F-021` | [Use cases](Use_cases.md) | Draft | 0.1 |
| `NBEOS-F-022` | [User journeys](User_journeys.md) | Draft | 0.1 |
| `NBEOS-F-023` | [Process maps](Process_maps.md) | Draft | 0.1 |
| `NBEOS-F-024` | [BPMN diagrams / modelling control](BPMN_diagrams.md) | Draft | 0.1 |

## Delivery/readiness controls

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-F-025` | [Backlog](Backlog.md) | Draft | 0.1 |
| `NBEOS-F-026` | [Definition of Ready](Definition_of_Ready.md) | Draft | 0.1 |
| `NBEOS-F-027` | [Definition of Done](Definition_of_Done.md) | Draft | 0.1 |
| `NBEOS-F-028` | [Requirements sign-off](Requirements_sign-off.md) | Draft — **NOT READY FOR SIGN-OFF** | 0.1 |

## Current requirements position

The requirements estate provides a controlled chain from business intent to candidate software behaviour and verification:

```text
BR → SR → FR/NFR/supporting specifications
   → Themes/Epics/Stories/Use Cases/Processes
   → Acceptance + RTM + Backlog controls
```

Material gaps deliberately remain open:

- primary customer/user validation;
- selected initial end-to-end pilot workflow(s);
- quantitative availability/performance/recovery/scale targets;
- approved identity/authority/data semantics;
- named initial integrations;
- migration source evidence;
- customer/pilot success thresholds;
- formal requirement approval authorities.

## Architecture entry condition

Draft architecture may now be explored to test feasibility and expose requirement gaps, but no architecture decision should be treated as approved merely because the requirements documents exist.

Architecture decisions must:

- trace to controlled requirements;
- make assumptions/TBDs explicit;
- avoid silently deciding unresolved business semantics;
- remain reversible where evidence is weak;
- feed requirement gaps/conflicts back into this baseline.
