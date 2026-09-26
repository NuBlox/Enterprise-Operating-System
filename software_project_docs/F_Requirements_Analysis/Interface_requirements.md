# Interface requirements

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-009  
**Document Type:** Interface requirements  
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
**Related Documents:** `Software_requirements_specification_SRS.md`, `Data_requirements.md`, `Integration_requirements.md`, `API_requirements.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Interface_requirements.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Interface_requirements.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the candidate classes of external interface NuBlox must support and the business/control information each interface definition must contain before technical design.

## Interface principles

1. An interface exists to support an approved business outcome or operational/control need.
2. Every material interface shall have an accountable owner on both sides where possible.
3. The authoritative source and update rights for exchanged information shall be explicit.
4. Interface failure shall not silently create inconsistent business state.
5. Security classification, authentication, authorisation, privacy and audit needs shall be defined before implementation.
6. Transport/protocol decisions are architecture concerns and are not fixed by this document.

## Candidate interface classes

| Class | Candidate examples | Requirement status |
|---|---|---|
| Identity and access | Enterprise identity provider, MFA/SSO, directory/group sources | Candidate |
| Communication | Email, notification, messaging services | Candidate |
| Finance/accounting | Retained ledger/accounting/finance platforms | Candidate |
| Project/professional systems | Design, engineering, BIM/CAD, project controls, specialist analysis | Candidate |
| Information repositories | Document/content repositories, file stores, records systems | Candidate |
| Customer/supplier systems | Customer portals, procurement/supplier platforms, external collaboration | Candidate |
| Analytics/export | BI tools, data warehouse/lake, regulatory/export destinations | Candidate |
| Regulatory/third-party services | Verification, address, tax, sanctions, industry/regulatory services where required | Candidate |
| Device/field | Mobile/field/device interfaces where selected workflows require them | Candidate |

## Interface definition requirements

Each approved interface shall document at least:

- business purpose and supported workflow;
- participating systems/services and owners;
- direction of exchange;
- authoritative source by information element;
- identifiers and correlation rules;
- data classification;
- authentication/authorisation expectations;
- latency/frequency requirement;
- consistency/idempotency expectation;
- validation and rejection rules;
- failure, retry and reconciliation behaviour;
- observability and audit evidence;
- version/change management;
- retention where interface payload/evidence is stored;
- test/acceptance method;
- exit/replacement considerations where the dependency is material.

## Candidate interface requirements

| ID | Requirement |
|---|---|
| `IF-001` | The system shall identify the business purpose and owning workflow for every approved external interface. |
| `IF-002` | The system/interface design shall define which system is authoritative for each material exchanged data set. |
| `IF-003` | An inbound interface shall validate required identifiers, structure and applicable business rules before changing controlled state. |
| `IF-004` | An outbound interface shall provide sufficient correlation information to reconcile a request with its acknowledgement/result where the business outcome depends on completion. |
| `IF-005` | Interface errors shall be observable and recoverable without requiring uncontrolled manual database changes. |
| `IF-006` | Duplicate/replayed messages or requests shall not create unintended duplicate business effects where idempotency is required. |
| `IF-007` | Interface access shall follow least-privilege principles and use approved authentication/credential-management controls. |
| `IF-008` | Material interface transactions shall retain audit evidence sufficient to trace what was exchanged and the resulting outcome, subject to data minimisation. |
| `IF-009` | Interface version changes shall be governed to avoid silent breaking changes. |
| `IF-010` | External dependencies required for critical workflows shall have agreed availability/failure assumptions and degraded-mode behaviour. |

## Interfaces not yet approved

No named vendor/product integration is approved by version 0.1 solely because it appears in market analysis or a candidate workflow. Named interfaces require traceable business justification.

## References

- `Software_requirements_specification_SRS.md`
- `Data_requirements.md`
- `Integration_requirements.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial interface-requirements envelope |