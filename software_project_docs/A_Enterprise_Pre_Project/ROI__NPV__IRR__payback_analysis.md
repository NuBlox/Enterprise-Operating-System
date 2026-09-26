# ROI / NPV / IRR / payback analysis

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-014  
**Document Type:** ROI / NPV / IRR / payback analysis  
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
**Related Documents:** `Business_case.md`, `Cost-benefit_analysis.md`, `Total_cost_of_ownership_TCO_model.md`, `Budget__funding_model.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/ROI__NPV__IRR__payback_analysis.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/ROI__NPV__IRR__payback_analysis.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the financial-evaluation method for NuBlox and the evidence required before ROI, NPV, IRR or payback claims can be approved.

## Current status

No approved financial return is stated in version 0.1 because validated development cost, operating cost, pricing, adoption, retention, implementation cost and customer-benefit baselines do not yet exist.

The purpose of this draft is to prevent later investment analysis from changing definitions or selectively excluding material costs.

## Required financial model

The investment model shall include at minimum:

- initial product development expenditure;
- ongoing engineering and product expenditure;
- cloud/platform operating expenditure;
- security, compliance and assurance expenditure;
- customer implementation and onboarding expenditure;
- sales and marketing expenditure;
- support and service operations expenditure;
- third-party licences/services;
- expected subscription or licence revenue;
- implementation/service revenue where applicable;
- churn, retention and expansion assumptions;
- working-capital and timing effects where material;
- tax treatment where relevant to the investment decision.

## Required scenarios

At least three scenarios shall be modelled:

1. **Downside** — slower customer acquisition, higher implementation effort and lower pricing/retention than plan.
2. **Base case** — evidence-supported expected case.
3. **Upside** — stronger adoption, repeatable implementation and expansion revenue.

A scenario may not be labelled Base Case until its core assumptions are supported by evidence.

## ROI

ROI will be calculated using an agreed programme convention and must identify whether the denominator is total investment, cumulative cash cost or another approved basis.

Candidate formula:

```text
ROI = (Cumulative attributable benefit - cumulative attributable cost) / cumulative attributable cost
```

The model must disclose the period covered and whether benefits are customer benefits, NuBlox commercial returns or both. These must not be mixed.

## NPV

NPV shall use explicit dated cash flows and an approved discount rate:

```text
NPV = Σ [Cash Flow_t / (1 + r)^t]
```

The discount rate and terminal assumptions require approval before NPV is used for an investment decision.

## IRR

IRR may be reported only where cash-flow structure makes the measure meaningful. The analysis shall not rely on IRR alone where multiple sign changes or unusual cash-flow timing could create misleading results.

## Payback

Payback shall be reported as the point at which cumulative net cash flow becomes non-negative. Both simple and discounted payback may be used where useful, with the method clearly labelled.

## Evidence requirements

Before this document can move to In Review, the following are required:

- costed delivery/resource plan;
- initial pricing and packaging hypothesis;
- customer acquisition assumptions;
- customer research on willingness to pay and switching conditions;
- implementation effort estimate;
- infrastructure cost model;
- support/operations cost model;
- initial sales pipeline assumptions or pilot evidence;
- approved financial modelling horizon and discount rate.

## Sensitivity analysis

The approved model shall test sensitivity to at least:

- development duration;
- engineering headcount/cost;
- implementation cost per customer;
- average contract value;
- sales cycle;
- customer acquisition cost;
- retention/churn;
- gross margin;
- infrastructure consumption;
- support intensity;
- scope growth.

## Decision rule

No investment recommendation shall be based on a single headline ROI or payback value. Financial metrics must be considered with strategic value, delivery risk, market evidence and option value.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Business_case.md`
- `software_project_docs/A_Enterprise_Pre_Project/Cost-benefit_analysis.md`
- `software_project_docs/A_Enterprise_Pre_Project/Total_cost_of_ownership_TCO_model.md`
- `software_project_docs/A_Enterprise_Pre_Project/Budget__funding_model.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product | Initial controlled financial-return methodology; no unsupported return figures introduced |
