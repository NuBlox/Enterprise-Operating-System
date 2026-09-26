# Business glossary

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-017  
**Document Type:** Business glossary  
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
**Related Documents:** `Data_dictionary.md`, `Business_requirements_document_BRD.md`, `Stakeholder_requirements_specification.md`, `Software_requirements_specification_SRS.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Business_glossary.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Business_glossary.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Provide a controlled vocabulary for the NuBlox programme so product, business, architecture, engineering and customer discussions use important terms consistently. Definitions are working definitions until approved through the relevant requirements or architecture process.

## Glossary

| Term | Working definition | Notes |
|---|---|---|
| Enterprise Operating System | The NuBlox product concept: a coherent governed environment connecting how an organisation operates with the work through which it creates value. | Product positioning term; not a claim that every enterprise capability must be native. |
| Business Outcome | A result valuable to the organisation/customer that may require multiple activities, participants, systems and controls. | Preferred unit for scope/value discussion over isolated features. |
| End-to-end Workflow | The connected sequence of work, information, decisions and handoffs required to reach a defined business outcome. | May cross teams/systems/organisations. |
| Business Requirement | A statement of the outcome/capability/constraint the business needs, independent of software design. | `BR-*`. |
| Stakeholder Requirement | A statement of what a stakeholder group needs from the product/programme. | `SR-*`. |
| Functional Requirement | A testable statement of behaviour the software must provide. | `FR-*`. |
| Non-functional Requirement | A quality, security, performance, reliability, usability, operability or other cross-cutting obligation. | `NFR-*`. |
| Business Rule | A constraint or rule governing valid business behaviour independent of a particular UI or persistence design. | `RULE-*`. |
| Acceptance Criterion | A condition/evidence used to determine whether a requirement/outcome is acceptable. | `AC-*`. |
| Authoritative Source | The recognised source governing the official/current representation of specified information for an agreed scope. | May be NuBlox or external. |
| System of Record | A system designated to authoritatively maintain specified data. | Use carefully; authority may be scoped per information element/lifecycle. |
| Source of Truth | Informal phrase often used for authoritative information. | Avoid unless precise scope/ownership is stated. |
| Work | Activity performed to achieve/contribute to a business outcome. | Detailed execution model remains to be designed. |
| Work Item | A trackable coordinated unit of work where explicit assignment/status/deadline/queue management is required. | Not assumed to represent all execution. |
| Work Product | An output created or changed through work, such as a document, drawing, model, report, specification, submission or record. | Umbrella term; specialist semantics may apply. |
| Handoff | Transfer of work/result/context between participants, stages, organisations or systems. | May require acceptance/review. |
| Decision | A governed conclusion/determination with business meaning or effect. | May be automated or human subject to controls. |
| Approval | An authorised decision allowing, accepting, confirming or advancing a subject under defined conditions. | Permission to click is not itself business authority. |
| Authority | The business mandate/power to make a commitment, decision or approval within defined scope/conditions. | Distinct from software permission. |
| Permission / Access | System-enforced ability to see information or perform an operation. | Distinct from authority/responsibility. |
| Responsibility | Accountability/expected ownership for work, information or outcome. | Detailed responsibility model later. |
| Evidence | Information retained to support/prove an action, decision, control, state or outcome. | May include system records or external artifacts. |
| Audit Trail | Ordered retained evidence of relevant events/actions sufficient for agreed reconstruction/investigation. | Technical and business audit needs may differ. |
| Configuration | Governed variation of product behaviour/settings/data without unmanaged source-code forks. | Must remain supportable/upgradeable. |
| Extension | Approved mechanism adding/altering supported behaviour beyond standard configuration. | Boundaries/design TBD. |
| Customisation | Customer-specific change. | Prefer governed configuration/extension; unmanaged forks are a strategic risk. |
| Integration | Controlled exchange/coordination between NuBlox and another system/service. | Requires explicit authority and failure semantics. |
| Interface | Defined boundary/contract through which systems/participants exchange information or invoke behaviour. | API is one interface type. |
| API | Machine-consumable application programming interface exposing approved information or behaviour. | Contract/version/security governed. |
| Migration | Controlled movement/transformation/reconciliation of information from one estate/state into another. | Not equivalent to simple import. |
| Reconciliation | Evidence-based comparison used to demonstrate consistency/completeness across sources, integrations or migrations. | Tolerances/rules must be defined. |
| Provenance | Information describing origin, derivation, actor/source and/or transformation history. | Level required depends on business need. |
| Effective Date | Business date/time from which information/relationship/rule applies. | May differ from record creation time. |
| Current State | The state considered applicable now under the governing lifecycle/effective-time rules. | Must not erase required history. |
| Historical Reconstruction | Ability to determine relevant past state/context/evidence for an agreed time or event. | Required depth varies by domain. |
| Reporting | Presentation of governed operational/management information using defined sources/measures. | Current vs published/as-at reporting must be distinguished. |
| Analytics | Analysis/derivation intended to explain, predict or optimise outcomes beyond direct operational reporting. | Derived outputs require provenance/validation. |
| Pilot | Controlled real-world validation of selected product capabilities/outcomes before broader rollout. | Must have measurable success/failure criteria. |
| Baseline | A reviewed/approved reference state used for controlled comparison/change. | Draft documents are not baselines. |
| Candidate | Requirement/concept proposed for validation but not approved. | Current default status. |
| Approved | Formally accepted by the accountable authority for the stated scope/version. | Approval does not imply implemented. |
| Implemented | Delivered in software/configuration/integration. | Does not imply accepted/verified. |
| Verified | Required evidence confirms implementation satisfies specified criterion. | Verification method must be defined. |
| Deferred | Deliberately postponed while remaining traceable. | Requires rationale/owner where material. |
| Tenant / Tenancy | A potential product/isolation concept describing customer/organisational separation. | **Not yet defined or approved** in the current requirements baseline. |
| Module | A software/product grouping. | Not an assumed business architecture. Avoid using as a substitute for business capability. |
| Function | A business or software grouping whose exact meaning is context-dependent. | No canonical Function taxonomy is assumed at this stage. |
| Capability | An ability the organisation/product must possess to achieve an outcome. | Does not automatically imply one subsystem/module. |
| Process | A defined sequence/system of activities used to achieve an outcome. | May cross capabilities/teams/systems. |
| Activity | A unit of business work within a process/outcome context. | Detailed taxonomy not yet approved. |

## Relationship to data dictionary

`Business_glossary.md` governs programme/business terminology. `Data_dictionary.md` focuses on candidate information concepts needed during data requirements. Neither document is a physical data model.

## Terms requiring later controlled definition

- tenancy/isolation;
- organisation/legal entity/organisational unit;
- worker/employee/contractor/person/account;
- role/job/position;
- capability/function/process/activity/task;
- project/programme/portfolio/service/asset;
- contract/appointment/order/commitment;
- deliverable/document/record/evidence;
- revision/version/status/state;
- customer/client and supplier/vendor/subcontractor distinctions;
- commercial and financial vocabulary.

## References

- `Data_dictionary.md`
- `Business_requirements_document_BRD.md`
- `Software_requirements_specification_SRS.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial programme business glossary with deliberate unresolved terminology |