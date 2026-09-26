# Integration requirements

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-010  
**Document Type:** Integration requirements  
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
**Related Documents:** `Interface_requirements.md`, `API_requirements.md`, `Data_requirements.md`, `Business_rules.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Integration_requirements.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Integration_requirements.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the candidate business and operational obligations for integrating NuBlox with external systems while preserving control, traceability and clear system boundaries.

## Integration objectives

Integration should be used when it improves the complete business outcome. It shall not exist merely to reproduce all data from another platform inside NuBlox.

## Candidate requirements

| ID | Requirement |
|---|---|
| `INT-001` | Each integration shall have a documented business outcome, owner and approved source/target boundary. |
| `INT-002` | Integration mappings shall preserve the intended business meaning of identifiers, states, units, dates, amounts and relationships. |
| `INT-003` | The integration shall define create/update/delete authority for each material information class. |
| `INT-004` | Where cross-system completion is required, NuBlox shall distinguish requested, accepted, completed, rejected and failed outcomes as applicable. |
| `INT-005` | Integrations shall provide controlled retry/recovery behaviour and shall prevent uncontrolled duplicate effects. |
| `INT-006` | Reconciliation shall be available where failure or drift could create material business inconsistency. |
| `INT-007` | Integration errors requiring human intervention shall be surfaced with sufficient context for controlled resolution. |
| `INT-008` | Sensitive information shall be exchanged only where necessary and under approved security/privacy controls. |
| `INT-009` | Credentials, keys and secrets used for integration shall be managed outside source code and protected through approved secret-management controls. |
| `INT-010` | Material integration activity shall be observable through technical telemetry and business-level status/evidence where applicable. |
| `INT-011` | Integration contracts shall support controlled versioning and backward-compatibility policy appropriate to their consumers. |
| `INT-012` | The product shall support coexistence with retained specialist systems without making customer-specific point-to-point coupling the default architecture. |
| `INT-013` | Integration design shall consider bulk/migration, scheduled/batch, request/response and event-driven patterns according to the workflow need rather than by default preference. |
| `INT-014` | External-service unavailability shall not corrupt authoritative NuBlox state; degraded behaviour shall be defined for critical workflows. |
| `INT-015` | Integration decommissioning/replacement shall include ownership transfer, data reconciliation and dependency-removal controls. |

## Integration decision test

Before a new integration enters scope, the project shall answer:

1. Which approved business requirement does it satisfy?
2. Why is integration preferable to native capability or manual exclusion?
3. Which system owns the information and lifecycle?
4. What failure can occur and what is the business consequence?
5. What data/security obligations apply?
6. Is the pattern reusable across customers or a customer-specific exception?
7. How is the integration tested, monitored, supported and retired?

## Anti-patterns to avoid

- synchronising every field because it exists;
- two systems independently mastering the same information without conflict rules;
- integrations whose only monitoring is a technical HTTP success code;
- customer-specific scripts with no product ownership/support model;
- manual database correction as the normal recovery mechanism;
- embedding provider credentials in application configuration repositories.

## References

- `Interface_requirements.md`
- `API_requirements.md`
- `Data_requirements.md`
- `Business_rules.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial controlled integration requirements |