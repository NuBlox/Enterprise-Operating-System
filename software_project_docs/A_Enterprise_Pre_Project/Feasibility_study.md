# Feasibility study

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-007  
**Document Type:** Feasibility study  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product  
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
**Related Documents:** `Product_vision_statement.md`, `Product_strategy.md`, `Market_analysis.md`, `Competitor_analysis.md`, `Customer_research_summary.md`, `Business_case.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Feasibility_study.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Feasibility_study.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Assess whether the NuBlox Enterprise Operating System appears feasible enough to justify continued discovery and controlled product development, and identify the questions that must be resolved before implementation can be authorised at scale.

## Feasibility conclusion at version 0.1

**Conditionally feasible for continued discovery and prototyping. Not yet proven feasible for full product investment or production implementation.**

There is no obvious technical reason why a platform connecting enterprise operations, project/professional delivery, workflow, information and governance cannot be built. The main uncertainties are product scope, customer value, migration/implementation complexity, operating economics and the depth required to compete with mature incumbents.

## Product feasibility

### Positive indicators

- The proposed problem is understandable and maps to recognised software categories and customer workflows.
- Market products demonstrate that enterprise ERP, CRM, HCM, project delivery, workflow, information management and industry-specific capabilities are technically achievable at scale.
- A focused initial segment creates a possible path to limit first-release breadth.
- The vision supports staged capability development rather than requiring every enterprise capability at launch.

### Unresolved issues

- The minimum coherent product boundary has not yet been defined.
- It is not yet proven which workflows create enough value to cause customers to switch.
- It is not yet known how much specialist professional-delivery functionality must be native.
- Customer-specific variation may be substantial.

**Current product feasibility:** AMBER — proceed with discovery, not full-scale build commitment.

## Technical feasibility

### Expected technical requirements

A credible enterprise operating system will likely require:

- secure multi-organisation identity and access management;
- reliable transactional persistence;
- configurable workflow/process execution;
- business-object lifecycle and relationships;
- audit/history/evidence capability;
- document/file/object storage;
- search;
- event/integration mechanisms;
- reporting/analytics;
- configurable metadata and business rules;
- APIs and integration tooling;
- scalable background processing;
- observability, backup, recovery and resilience;
- secure web/mobile user experiences;
- data import/migration tooling.

These are requirement categories, not approved technology choices.

### Technical risks

- over-generalised metadata models can become difficult to govern and optimise;
- over-specialised schemas can make the platform hard to configure across customers;
- workflow flexibility can create unmaintainable customer variants;
- enterprise permissions/authority can become complex quickly;
- auditability and historical reconstruction must be designed in rather than added later;
- integrations with finance, payroll, tax, banking, identity and specialist authoring systems may be numerous;
- data migration quality may determine implementation success.

**Current technical feasibility:** GREEN/AMBER — technically achievable, architecture choices still require requirements evidence.

## Operational feasibility

A production NuBlox service would require mature operational capabilities including:

- customer onboarding and configuration;
- support and incident management;
- environment management;
- release/change management;
- security monitoring;
- backup/restore and disaster recovery;
- data migration support;
- service-level management;
- customer administration;
- documentation and training;
- tenant/customer lifecycle management.

Operational feasibility is likely but will depend heavily on product configurability and implementation model.

**Current operational feasibility:** AMBER.

## Commercial feasibility

Commercial feasibility is not yet proven.

Questions requiring evidence include:

- total addressable and serviceable market for the launch segment;
- willingness to pay;
- achievable annual contract value;
- implementation/service revenue and cost;
- gross margin after hosting/support;
- sales-cycle length;
- customer acquisition cost;
- retention/expansion potential;
- number of customers required to support sustainable development;
- price/value relationship versus incumbent system estates.

**Current commercial feasibility:** AMBER/RED pending primary evidence and financial model.

## Customer adoption feasibility

Adoption may be difficult because NuBlox potentially touches critical business processes and existing systems of record.

A practical adoption strategy may require:

- phased deployment;
- coexistence with incumbent systems;
- strong import/migration capability;
- configurable integrations;
- measurable early-value workflows;
- role-based training and change management;
- clear rollback/transition planning;
- evidence of security and reliability.

The initial product should therefore avoid requiring an all-or-nothing enterprise replacement unless customer evidence clearly supports that strategy.

**Current adoption feasibility:** AMBER.

## Security, privacy and regulatory feasibility

Enterprise customers will expect strong controls around:

- authentication and access control;
- encryption;
- audit logging;
- privacy and data protection;
- data residency where applicable;
- secure development and vulnerability management;
- backup/recovery;
- incident response;
- third-party risk;
- regulatory evidence relevant to customer industries.

For a UK launch, UK GDPR/Data Protection Act obligations and general cybersecurity expectations will require formal assessment. Construction/built-environment customers may also impose project/client-specific information-security requirements.

No certification commitment is made in this draft.

**Current security/regulatory feasibility:** GREEN/AMBER — achievable, but scope and assurance programme must be defined early.

## Delivery feasibility

A credible delivery plan requires staged scope and evidence gates.

A feasible sequence is likely to be:

1. customer discovery and scenario definition;
2. prioritised business requirements;
3. architecture options and proof-of-concept spikes;
4. thin end-to-end pilot covering a complete business outcome;
5. customer validation;
6. controlled expansion of capability;
7. production-readiness work;
8. early adopter deployment;
9. measured iteration.

Trying to implement the whole enterprise vision before proving a small number of complete outcomes would materially increase delivery risk.

**Current delivery feasibility:** GREEN if staged; RED if attempted as a single full-suite build.

## Resourcing feasibility

The programme will require skills spanning:

- product/domain analysis;
- enterprise architecture;
- software engineering;
- data architecture;
- security engineering;
- UX/product design;
- QA/test automation;
- DevOps/platform engineering;
- implementation/data migration;
- customer success/support;
- legal/privacy/compliance;
- sales and marketing.

Resource quantity, sequencing and cost are not yet estimated.

## Feasibility gates before major implementation

Major implementation should not be authorised until the programme has:

1. validated a target customer segment;
2. documented several high-value end-to-end scenarios;
3. defined measurable customer outcomes;
4. established an initial requirements baseline;
5. completed architecture option assessment;
6. estimated build/operate costs;
7. completed an initial security/privacy assessment;
8. built a commercial model;
9. defined pilot acceptance criteria;
10. established governance, funding and accountable ownership.

## Recommendation

Continue the pre-project programme and begin requirements-oriented discovery once direct customer evidence is available. Conduct technical spikes only where they reduce a material feasibility uncertainty; do not allow technology experiments to become de facto architecture decisions before the requirements and option analysis are controlled.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Product_vision_statement.md`
- `software_project_docs/A_Enterprise_Pre_Project/Product_strategy.md`
- `software_project_docs/A_Enterprise_Pre_Project/Market_analysis.md`
- `software_project_docs/A_Enterprise_Pre_Project/Competitor_analysis.md`
- `software_project_docs/A_Enterprise_Pre_Project/Customer_research_summary.md`
- `software_project_docs/A_Enterprise_Pre_Project/Business_case.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product | Initial controlled feasibility draft |