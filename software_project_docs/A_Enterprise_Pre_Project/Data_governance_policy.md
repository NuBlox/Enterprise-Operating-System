# Data governance policy

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-013  
**Document Type:** Data governance policy  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Data Governance  
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
**Related Documents:** `Compliance__regulatory_register.md`, `Security_classification_policy.md`, `Enterprise_architecture_principles.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Data_governance_policy.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Data_governance_policy.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the initial governance principles for data and information used by NuBlox and, later, by customer organisations operating through the product.

## Scope

This draft covers programme and product-level governance principles for:

- business data;
- master/reference data;
- personal data;
- transactional data;
- documents/files and structured work products;
- metadata;
- configuration data;
- audit/evidence data;
- integration/imported data;
- analytical/reporting data.

Specific domain ownership and retention schedules will be established during requirements and design.

## Governance principles

### DG-01 — Data has accountable ownership

Material data domains must have an accountable owner responsible for definition, quality expectations, access policy, lifecycle and issue resolution.

### DG-02 — Meaning is governed

Important terms, codes, statuses and relationships require controlled definitions. Different teams should not silently assign different meanings to the same business concept.

### DG-03 — Authoritative sources are explicit

Where a fact has one authoritative source, that authority must be documented. Copies or projections must be reconcilable to it.

### DG-04 — Data is collected for a purpose

Data should be created or collected because a defined business, legal, operational or analytical purpose exists. Personal data must follow applicable data-protection principles including purpose limitation and minimisation.

### DG-05 — Quality is measurable

Data-quality expectations should be defined in terms relevant to the use case, such as completeness, validity, accuracy, timeliness, uniqueness and consistency.

### DG-06 — History is preserved where business meaning requires it

If a changed value affects legal, financial, operational or audit interpretation, the system should retain sufficient history to reconstruct the relevant context.

### DG-07 — Classification drives handling

Information classification should influence access, export, sharing, storage, retention and disposal controls.

### DG-08 — Access follows business need and policy

Data access should be based on approved responsibilities and least privilege, with additional controls for sensitive information.

### DG-09 — Retention and disposal are controlled

Data should not be kept indefinitely by default. Retention must consider law, contract, customer requirements, business need, audit/evidence obligations and privacy principles.

### DG-10 — Data lineage and provenance matter

For imported, transformed or analytically derived data, the product should retain enough provenance to understand origin and significant transformation where required.

### DG-11 — Integration does not remove accountability

Receiving data from an external system does not make it trustworthy automatically. Validation, ownership and reconciliation responsibilities remain necessary.

### DG-12 — Analytics must not redefine transactional truth silently

Reporting and analytics may derive metrics and aggregates, but definitions and calculation logic should be governed and traceable.

### DG-13 — Configuration data is governed data

Configuration can materially change product behaviour and therefore requires versioning, access controls, review and audit proportionate to impact.

### DG-14 — Test/development data is controlled

Production or identifiable personal data should not be copied into non-production environments without documented need and appropriate controls. Synthetic or de-identified data should be preferred where practical.

### DG-15 — Customer data remains distinguishable from NuBlox operational data

Product telemetry, support data, billing data and customer business content may have different purposes, ownership and retention rules and must not be conflated.

## Data roles to establish

The programme should later define responsibilities such as:

- Data Owner;
- Data Steward;
- System/Product Owner;
- Information Security Owner;
- Privacy Owner / Data Protection responsibility;
- Records/Retention Owner;
- Data Consumer;
- Data Processor/Integration Owner.

Role names may change; accountable responsibilities must not remain ambiguous.

## Data lifecycle

Governance should cover:

1. definition;
2. creation/collection;
3. validation;
4. storage;
5. use;
6. sharing/integration;
7. change/correction;
8. analysis/reporting;
9. archival/legal hold where required;
10. retention/disposal.

## Minimum controls for important data domains

Each significant data domain should eventually define:

- business definition;
- authoritative source;
- accountable owner;
- classification;
- required fields/validation;
- quality rules;
- access rules;
- retention/disposal rules;
- external sharing/integration rules;
- audit/history expectations;
- regulatory/contractual obligations;
- critical reports/metrics dependent on the data.

## Personal data

Personal-data governance must align with current UK data-protection law and ICO guidance. The ICO identifies seven UK GDPR principles: lawfulness/fairness/transparency, purpose limitation, data minimisation, accuracy, storage limitation, security, and accountability.

Product requirements should allow customers and NuBlox to meet their respective legal roles and obligations without assuming that the same lawful basis, retention period or access rule applies to every customer/process.

## Metadata and extensibility

If NuBlox provides customer-configurable fields, types or rules, that metadata must be governed. Configurability does not justify uncontrolled schema proliferation or ambiguous meaning.

Metadata changes should support:

- ownership;
- naming/definition standards;
- type/validation rules;
- version/history where required;
- dependency/impact analysis;
- access/change control;
- migration when definitions change.

## Immediate actions

1. create an initial business glossary during requirements discovery;
2. identify candidate master/reference data domains;
3. define data ownership during process/scenario modelling;
4. create a data inventory alongside the privacy assessment;
5. establish retention-policy ownership;
6. define data-quality measures for pilot workflows;
7. include migration/provenance requirements in early architecture work;
8. define governance for customer-configurable metadata before implementation.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Compliance__regulatory_register.md`
- `software_project_docs/A_Enterprise_Pre_Project/Security_classification_policy.md`
- `software_project_docs/A_Enterprise_Pre_Project/Enterprise_architecture_principles.md`
- ICO data-protection principles: https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/data-protection-principles/a-guide-to-the-data-protection-principles/

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Data Governance | Initial data-governance policy draft |