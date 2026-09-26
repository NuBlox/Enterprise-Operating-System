# Initial dependency log

**Section:** D_Project_Initiation  
**Document ID:** NBEOS-D-012  
**Document Type:** Initial dependency log  
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
**Related Documents:** `Assumptions_log.md`, `Constraints_log.md`, `Initial_risk_register.md`, `High-level_schedule.md`, `Milestone_list.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/D_Project_Initiation/Initial_dependency_log.md`  
**Storage Location:** `software_project_docs/D_Project_Initiation/Initial_dependency_log.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Make project dependencies explicit so sequencing, risk and accountability can be managed before detailed scheduling.

## Dependency register

| ID | Dependency | Needed by | Dependency owner | Impact if late/unavailable | Status |
|---|---|---|---|---|---|
| DEP-001 | Named programme/project decision authorities | Initiation approval | Programme | Baselines cannot be formally approved | Open |
| DEP-002 | Representative customer-research participants | Requirements discovery | Product/Commercial | Problem and workflow evidence weak | Open |
| DEP-003 | Access to representative current-state workflows/data examples | Requirements/data analysis | Customer/Product | Requirements and migration assumptions remain speculative | Open |
| DEP-004 | Initial budget/resource envelope | Delivery planning | Programme | Scope/schedule cannot be committed | Open |
| DEP-005 | Non-functional requirements | Architecture baseline | Product/Architecture | Architecture risks premature design | Open |
| DEP-006 | Security/privacy requirements and data classifications | Architecture/design | Security/Data | Security controls may be incomplete | Open |
| DEP-007 | Integration/system landscape evidence | Integration architecture | Architecture/Customer IT | Interface scope may be understated | Open |
| DEP-008 | Migration source/data-quality evidence | Migration design | Data/Customer IT | Migration effort/risks uncertain | Open |
| DEP-009 | UX/user research participation | Product design | Product/Customer | Usability/adoption risk increases | Open |
| DEP-010 | Test/acceptance authority and quality thresholds | Verification planning | QA/Product | Completion cannot be objectively accepted | Open |
| DEP-011 | Pilot customer and measurable baseline | Pilot planning | Commercial/Product | Value proof delayed | Open |
| DEP-012 | Production hosting/operations decision | Production readiness | Architecture/Ops | Operational controls cannot be finalised | Open |
| DEP-013 | Legal/commercial terms for pilots/customers | Pilot start | Commercial/Legal | Pilot may be blocked | Open |
| DEP-014 | Support/incident model | Production readiness | Operations | Service cannot launch responsibly | Open |

## Dependency categories

Dependencies should be tagged as appropriate:

- decision/governance;
- customer/evidence;
- resource/funding;
- requirements;
- technical/architecture;
- data/migration;
- security/compliance;
- legal/commercial;
- operational;
- supplier/third-party.

## Management rules

- Dependencies must have an owner even when the owner cannot directly deliver them.
- A dependency with uncertain availability should also be represented as a risk.
- Critical-path dependencies should be visible in the detailed schedule.
- Changes to dependency dates or assumptions require downstream impact review.

## References

- `software_project_docs/D_Project_Initiation/Initial_risk_register.md`
- `software_project_docs/D_Project_Initiation/High-level_schedule.md`
- `software_project_docs/D_Project_Initiation/Milestone_list.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Programme | Initial project dependency baseline |
