# Assumptions log

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-009  
**Document Type:** Assumptions log  
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
**Related Documents:** `Constraints_log.md`, `Initial_risk_register.md`, `Initial_dependency_log.md`, `Project_initiation_document_PID.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Assumptions_log.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Assumptions_log.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain explicit assumptions that influence project scope, planning, architecture or investment so they can be validated, rejected or converted into requirements rather than remaining hidden.

## Assumptions register

| ID | Assumption | Impact if false | Validation route | Owner | Status |
|---|---|---|---|---|---|
| ASM-001 | The target segment experiences material operational fragmentation | Market/product case weakens | Primary customer research | Product | Open |
| ASM-002 | Customers will consider changing core operating workflows if value is sufficient | Adoption/sales risk rises | Buyer/user interviews and pilots | Product/Commercial | Open |
| ASM-003 | A reusable product can serve multiple organisations without customer-specific forks | Product economics fail if false | Requirements/configuration analysis and pilots | Product/Architecture | Open |
| ASM-004 | Specialist tools can remain integrated where replacement adds little value | Integration becomes central dependency | Workflow/integration discovery | Architecture | Open |
| ASM-005 | A cloud-hosted service is plausible for the initial market | Deployment/security scope changes if false | Customer IT/security discovery | Architecture/Security | Open |
| ASM-006 | Initial customers can provide data/workflow access sufficient for discovery and pilots | Validation and migration planning delayed | Customer recruitment | Product/Commercial | Open |
| ASM-007 | Business value can be demonstrated through measurable workflow outcomes | Benefits case weakens | Baselines and pilot measurement | Product/Customer | Open |
| ASM-008 | Required security/privacy controls are achievable within plausible cost and schedule | Feasibility changes | Security/compliance architecture | Security | Open |
| ASM-009 | Data migration can be made repeatable enough for a product business | Implementation cost may become unsustainable | Migration discovery/prototypes | Data/Architecture | Open |
| ASM-010 | The project can recruit/retain sufficient engineering and domain capability | Schedule/quality affected | Resource planning | Programme | Open |
| ASM-011 | Financial/commercial model can support enterprise-grade support and operations | Business case fails if false | TCO/pricing/unit economics | Commercial | Open |
| ASM-012 | End-to-end coherence will create more customer value than a thin integration layer alone | Product positioning changes | Customer testing and solution-option comparison | Product | Open |

## Assumption lifecycle

Each assumption should move through one of:

- Open;
- Validating;
- Validated;
- Rejected;
- Converted to Requirement;
- Converted to Risk/Issue;
- No Longer Relevant.

## Review rules

- Review assumptions at each major project gate.
- Any assumption affecting architecture, budget, compliance or launch must have a named validation route.
- Rejected assumptions require impact assessment and updates to affected controlled documents.
- Assumptions should not remain Open indefinitely once implementation depends on them.

## References

- `software_project_docs/D_Project_Initiation/Constraints_log.md`
- `software_project_docs/D_Project_Initiation/Initial_risk_register.md`
- `software_project_docs/D_Project_Initiation/Project_initiation_document_PID.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial explicit project assumptions baseline |
