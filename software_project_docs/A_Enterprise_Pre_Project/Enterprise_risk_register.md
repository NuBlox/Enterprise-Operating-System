# Enterprise risk register

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-009  
**Document Type:** Enterprise risk register  
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
**Related Documents:** `Business_case.md`, `Feasibility_study.md`, `Product_strategy.md`, `Compliance__regulatory_register.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Enterprise_risk_register.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Enterprise_risk_register.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the initial strategic risk baseline for the NuBlox Enterprise Operating System programme.

## Scoring method

Initial qualitative scoring uses:

- Likelihood: Low / Medium / High
- Impact: Low / Medium / High / Critical
- Exposure: judgement based on combined likelihood and impact

Numeric scoring and formal risk appetite thresholds will be introduced when programme governance is established.

## Risk register

| ID | Risk | Likelihood | Impact | Exposure | Current response | Owner |
|---|---|---|---|---|---|---|
| R-001 | Product scope becomes too broad before a repeatable customer problem is proven | High | Critical | Critical | Stage-gate scope; require customer evidence before major implementation | Product |
| R-002 | NuBlox reproduces incumbent feature sets without materially better end-to-end outcomes | Medium | Critical | High | Scenario-based competitive tests; outcome metrics | Product |
| R-003 | Target customers do not experience enough pain to justify switching | Medium | Critical | High | Primary discovery; quantify current-state cost and switching triggers | Product |
| R-004 | Initial target segment is too narrow or difficult to sell into | Medium | High | High | Segment sizing, buyer analysis, adjacent-market options | Commercial |
| R-005 | Implementation becomes bespoke consultancy rather than repeatable product delivery | High | Critical | Critical | Configuration boundaries, reference operating models, implementation metrics | Product / Delivery |
| R-006 | Data migration complexity causes cost, delay or customer dissatisfaction | High | High | High | Early migration architecture, profiling, tooling and pilot migrations | Engineering / Delivery |
| R-007 | Integration requirements with incumbent/specialist systems become excessive | High | High | High | Integration strategy; prioritise standard connectors and clear system boundaries | Architecture |
| R-008 | Security weakness damages customer trust or causes breach | Medium | Critical | Critical | Secure-by-design programme, threat modelling, security testing, incident readiness | Security |
| R-009 | Privacy/data-protection non-compliance | Medium | Critical | High | Data protection by design, DPIAs, data inventory, legal review | Privacy / Legal |
| R-010 | Architecture becomes over-generic and loses domain depth | Medium | High | High | Validate against complete customer scenarios and professional outputs | Product / Architecture |
| R-011 | Architecture becomes over-specialised and prevents wider expansion | Medium | High | High | Separate universal and segment-specific requirements; test adjacent scenarios | Product / Architecture |
| R-012 | Customer-specific configuration creates unsupported variants | High | High | High | Governed configuration model; prohibit unmanaged forks | Product / Engineering |
| R-013 | Engineering capacity or funding is insufficient for enterprise-grade scope | High | Critical | Critical | Staged roadmap, funding model, resource plan, prioritisation | Programme |
| R-014 | Operating/support costs undermine commercial viability | Medium | High | High | TCO model, observability, automation, support model, gross-margin targets | Operations / Commercial |
| R-015 | Incumbent vendors close the target whitespace | Medium | High | High | Continuous competitive monitoring; focus on workflow/outcome differentiation | Product |
| R-016 | Buyers prefer integration of incumbent tools over platform replacement | High | High | High | Support coexistence; demonstrate measurable consolidation value | Product / Commercial |
| R-017 | Lack of reference customers blocks enterprise adoption | High | High | High | Early adopter programme; measurable pilot outcomes; controlled case studies | Commercial |
| R-018 | Poor user experience prevents adoption despite functional breadth | Medium | Critical | High | Continuous user research, usability testing and work-centred design | Product / UX |
| R-019 | Regulatory or customer-specific assurance requirements delay sales | Medium | High | High | Compliance register, assurance roadmap, evidence pack | Security / Compliance |
| R-020 | AI functionality creates privacy, reliability or governance risks | Medium | High | High | Treat AI as governed capability; human oversight; data/control assessment | Product / Security |
| R-021 | Vendor/service dependency causes lock-in or service interruption | Medium | High | Medium/High | Supplier assessment, portability, resilience, exit planning | Architecture / Operations |
| R-022 | Inadequate audit/history design prevents reliable reconstruction of business events | Medium | Critical | High | Architecture principle: traceability by design; acceptance tests | Architecture |
| R-023 | Programme documentation becomes an end in itself and delays evidence/product work | Medium | Medium | Medium | Use templates proportionately; combine/retire documents where appropriate | Programme |
| R-024 | Requirements are driven by software categories rather than real business outcomes | Medium | High | High | Outcome-based discovery and requirements traceability | Product |

## Risk treatment principles

1. Critical risks require explicit accountable ownership before implementation approval.
2. Risk acceptance must be documented; absence of mitigation is not acceptance.
3. Risks should link to requirements, architecture decisions, tests or operational controls where treatment is technical.
4. New evidence may lower or raise exposure; register changes must remain traceable.
5. Customer/market risks are as important as technical risks during pre-project work.

## Immediate actions

- complete primary customer discovery;
- establish formal programme governance and risk appetite;
- create compliance/regulatory baseline;
- define secure-development expectations;
- establish initial architecture principles;
- quantify development/operating costs;
- create initial data-migration/integration assumptions;
- define pilot success/failure criteria.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Business_case.md`
- `software_project_docs/A_Enterprise_Pre_Project/Feasibility_study.md`
- `software_project_docs/A_Enterprise_Pre_Project/Product_strategy.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial strategic risk baseline |