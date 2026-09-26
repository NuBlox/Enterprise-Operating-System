# Total cost of ownership (TCO) model

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-015  
**Document Type:** Total cost of ownership model  
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
**Related Documents:** `Business_case.md`, `Cost-benefit_analysis.md`, `ROI__NPV__IRR__payback_analysis.md`, `Budget__funding_model.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Total_cost_of_ownership_TCO_model.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Total_cost_of_ownership_TCO_model.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the cost categories and modelling rules required to understand the full cost of creating, operating, selling, implementing and supporting NuBlox, and to compare customer TCO against incumbent alternatives.

## TCO perspectives

The programme shall maintain two distinct TCO views:

1. **NuBlox provider TCO** — the cost to build, operate, secure, sell, support and evolve the product.
2. **Customer TCO** — the cost to acquire, implement, migrate to, integrate, operate and support NuBlox compared with the customer's current or alternative software estate.

The two perspectives must not be combined into one figure.

## Provider cost model

Provider TCO shall include:

### Product and engineering
- product management;
- domain/business analysis;
- UX and design;
- software engineering;
- quality engineering;
- architecture;
- technical debt and maintenance;
- documentation.

### Platform and operations
- cloud compute/storage/networking;
- databases and managed services;
- monitoring/observability;
- backups and disaster recovery;
- environments;
- CI/CD tooling;
- support tooling.

### Security, privacy and compliance
- security engineering;
- independent testing;
- vulnerability management;
- certification/assurance activities;
- privacy/legal advice;
- regulatory monitoring;
- audit support.

### Go-to-market and customer delivery
- sales;
- marketing;
- solution engineering;
- customer success;
- onboarding;
- implementation;
- migration;
- training;
- support.

### Corporate and third-party
- external licences;
- professional services;
- insurance;
- legal/accounting;
- payment processing;
- partner costs where applicable.

## Customer TCO model

Customer TCO comparison should include:

- subscription/licence cost;
- implementation services;
- migration cost;
- integration cost;
- internal project/change-management effort;
- training;
- configuration/administration;
- support;
- infrastructure where customer-borne;
- specialist systems that remain necessary;
- reporting/reconciliation effort;
- manual process cost;
- upgrade/change effort;
- exit/decommissioning cost.

## Cost horizons

The approved model should report at minimum:

- Year 1 implementation TCO;
- 3-year TCO;
- 5-year TCO;
- steady-state annual run cost where meaningful.

## Unit economics to establish

As evidence matures, the model should derive:

- cost to provision a customer;
- implementation cost by customer archetype;
- monthly infrastructure cost per tenant/customer and per active user where useful;
- support cost per customer;
- gross margin by commercial offer;
- customer acquisition cost;
- lifetime value assumptions;
- migration cost drivers;
- marginal cost of additional capabilities/usage.

## TCO comparison options

Customer comparisons should test at least:

1. current application estate;
2. improved integration of current estate;
3. NuBlox plus retained specialist systems;
4. broader NuBlox consolidation where appropriate.

The comparison must include transition cost and cannot treat displaced licence cost as an automatic saving if the displaced capability remains required elsewhere.

## Evidence status

Version 0.1 defines the model only. No approved TCO values are stated because architecture, deployment model, pricing, staffing and implementation scope are not yet baselined.

## Approval criteria

Before approval, the document requires:

- approved cost categories and accounting treatment;
- initial solution/deployment architecture;
- resource plan;
- cloud/platform assumptions;
- implementation approach;
- customer archetypes;
- migration and integration assumptions;
- pricing/packaging hypotheses;
- benchmark current-estate TCO for representative customers.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Business_case.md`
- `software_project_docs/A_Enterprise_Pre_Project/Cost-benefit_analysis.md`
- `software_project_docs/A_Enterprise_Pre_Project/ROI__NPV__IRR__payback_analysis.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product | Initial controlled TCO framework with separate provider and customer perspectives |
