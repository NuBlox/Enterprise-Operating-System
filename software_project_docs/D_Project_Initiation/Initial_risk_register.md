# Initial risk register

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-011  
**Document Type:** Initial risk register  
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
**Related Documents:** `../A_Enterprise_Pre_Project/Enterprise_risk_register.md`, `Assumptions_log.md`, `Constraints_log.md`, `Initial_dependency_log.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Initial_risk_register.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Initial_risk_register.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Translate the strategic enterprise risk baseline into the initial risks that must be actively managed during project initiation, requirements, architecture and early implementation.

## Scoring

- Likelihood: Low / Medium / High
- Impact: Low / Medium / High / Critical
- Exposure: qualitative combined judgement

## Initial project risks

| ID | Risk | Likelihood | Impact | Exposure | Response / Next action | Owner |
|---|---|---|---|---|---|---|
| PR-001 | Customer discovery is insufficient or unrepresentative | High | Critical | Critical | Recruit representative cohort; track role/organisation coverage | Product |
| PR-002 | Scope expands before requirements are prioritised | High | Critical | Critical | Scope gate; trace every candidate capability to approved outcome | Product / PM |
| PR-003 | Requirements remain solution-led or software-category-led | Medium | High | High | Outcome-based workshops; requirements review criteria | Product / BA |
| PR-004 | Governance roles remain unappointed, delaying decisions | High | High | High | Appoint named authorities before formal baselines | Programme |
| PR-005 | Architecture is selected before non-functional requirements are known | Medium | Critical | High | Baseline NFRs before architecture approval | Architecture |
| PR-006 | Data migration complexity is discovered too late | High | High | High | Early data profiling and migration scenarios | Data / Architecture |
| PR-007 | Integration surface becomes too large for initial delivery | High | High | High | Prioritise interfaces by end-to-end outcome; defer low-value integrations | Architecture |
| PR-008 | Security/privacy controls constrain design late | Medium | Critical | High | Threat/privacy analysis during requirements and design | Security |
| PR-009 | Implementation becomes customer-specific | High | Critical | Critical | Configuration rules; no unmanaged forks; product review of exceptions | Product / Engineering |
| PR-010 | Delivery capacity is lower than required | High | High | High | Resource plan and staged scope; capacity-based planning | PM / Programme |
| PR-011 | Budget remains uncertain while implementation commitments grow | Medium | High | High | Baseline resource/cost model before major build | Programme |
| PR-012 | No measurable pilot baseline exists | Medium | High | High | Benefits measures agreed before pilot configuration | Product / Customer |
| PR-013 | Technical foundation cannot support required audit/history | Medium | Critical | High | Include traceability/data-history requirements and architecture tests | Architecture |
| PR-014 | Poor usability reduces adoption | Medium | Critical | High | Continuous user research and usability testing | Product / UX |
| PR-015 | Release/operations needs are deferred until late | Medium | High | High | Operability NFRs and runbook/monitoring planning from architecture stage | Engineering / Ops |
| PR-016 | Product documents drift out of sync with implementation | Medium | High | High | Traceability, review gates and controlled updates | PM / QA |

## Link to enterprise risks

Project risks should reference or roll up into the Enterprise Risk Register where they represent the same strategic exposure. The project register focuses on actionable delivery risks rather than duplicating every enterprise-level risk.

## Escalation

Escalate a risk when it:

- threatens a major stage gate;
- requires additional funding;
- changes customer-market scope;
- creates Critical security/privacy/compliance exposure;
- requires a material architecture reversal;
- exceeds project-level authority to accept.

## Review cadence

During active delivery, the risk register should be reviewed at least weekly with changes recorded in Git/project controls.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Enterprise_risk_register.md`
- `software_project_docs/D_Project_Initiation/Assumptions_log.md`
- `software_project_docs/D_Project_Initiation/Constraints_log.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial project-level risk baseline derived from strategic risks |
