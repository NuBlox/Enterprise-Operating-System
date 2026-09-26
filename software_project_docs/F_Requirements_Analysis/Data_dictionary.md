# Data dictionary

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-008  
**Document Type:** Data dictionary  
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
**Related Documents:** `Data_requirements.md`, `Business_requirements_document_BRD.md`, `Software_requirements_specification_SRS.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Data_dictionary.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Data_dictionary.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain a controlled preliminary business-data vocabulary for requirements work. This is **not** the physical database schema and does not make any candidate term a canonical software object merely by listing it.

## Dictionary rules

- Terms must be defined in business language first.
- Synonyms and ambiguous meanings must be recorded rather than silently normalised.
- A term becomes an approved product concept only through the relevant requirements/architecture decision.
- Physical storage names, keys and datatypes are defined later in data/database design.

## Preliminary terms

| Term | Working definition | Status / notes |
|---|---|---|
| Organisation | A business, legal, public, charitable or other organised body relevant to an approved NuBlox workflow. | Candidate umbrella term; legal-entity semantics require later definition. |
| Person | An individual relevant to an approved business/work context. | Candidate; application account is not assumed to be identical to Person. |
| Participant | A person, organisation or other approved actor participating in a business outcome. | Candidate contextual term. |
| Customer / Client | An organisation or person receiving or commissioning value/work from the operating organisation. | Relationship/context term; exact distinction requires domain validation. |
| Supplier / Partner | An external organisation/person providing goods, services, professional input or another contribution. | Relationship/context term. |
| Opportunity | A potential future commercial/customer engagement being evaluated or pursued. | Candidate; scope depends on target workflows. |
| Commitment | A business obligation, promise or authorised undertaking with relevant scope/terms. | Candidate cross-domain term. |
| Contract / Appointment | A governed agreement defining obligations, scope, commercial terms or rights between parties. | Candidate; jurisdiction/domain variants expected. |
| Project | A temporary delivery context with defined objectives/scope and governance where applicable. | Candidate; not every work context is assumed to be a Project. |
| Service | An ongoing or repeatable provision of value/work under defined expectations. | Candidate. |
| Work | Activity performed to achieve or contribute to a business outcome. | Business concept; detailed runtime semantics deferred. |
| Work Item | A trackable unit of coordinated work where explicit assignment/status/deadline or queue management is required. | Candidate; not assumed to represent all execution. |
| Assignment | A relationship allocating work/responsibility to an approved participant or organisational context. | Candidate. |
| Work Product | An output created or changed through work, such as a report, drawing, model, specification, submission or record. | Candidate umbrella term. |
| Document | A controlled information artifact represented as a document/file/record where document semantics are relevant. | Candidate; not all business information is a Document. |
| Model | A structured representation used to describe/design/analyse a subject, potentially held in a specialist environment. | Candidate; technical formats out of scope here. |
| Decision | A governed conclusion or determination that has business meaning/effect. | Candidate. |
| Approval | An authorised decision that permits, accepts, confirms or advances something under defined conditions. | Candidate subset/type of decision; detailed semantics later. |
| Evidence | Information retained to support or prove an action, state, decision, control or outcome. | Candidate. |
| Authority | Business power/mandate to make a particular commitment, approval or decision within a defined scope. | Candidate; distinct from software permission. |
| Permission / Access | System-enforced ability to see or perform an operation on specified information/functionality. | Candidate security term; distinct from business authority. |
| Responsibility | Accountability or expected ownership for work, information or outcome. | Candidate; exact responsibility models later. |
| State | The business condition/status of a subject at a point in its lifecycle. | Candidate generic concept. |
| Lifecycle | The allowed sequence/rules governing changes in state for a business subject. | Candidate. |
| Event | A recorded occurrence relevant to business or system behaviour. | Candidate; event architecture not implied. |
| Risk | An uncertain event/condition that may affect objectives or outcomes. | Candidate. |
| Issue | A current problem or condition requiring management/action. | Candidate. |
| Change | A controlled modification to an approved baseline, scope, state, commitment or output. | Candidate. |
| Configuration | Governed customer/product settings or definitions that vary behaviour without unmanaged source-code forks. | Candidate. |
| Integration | Controlled exchange or coordination between NuBlox and another system/service. | Candidate. |
| Authoritative Source | The source recognised as governing the current/official representation of specified information within an agreed scope. | Candidate data-governance term. |
| Effective Date | The business date/time from which information or a relationship is considered applicable, which may differ from its creation timestamp. | Candidate temporal term. |
| Audit Record | Retained evidence about relevant actions/events sufficient for agreed assurance or reconstruction. | Candidate. |

## Terms requiring explicit resolution

The following terms are likely to be important but should not be normalised prematurely:

- tenant;
- legal entity;
- organisational unit;
- position / job / role;
- employee / worker / contractor;
- function / capability / process / activity / task;
- programme / portfolio;
- asset / site / location;
- deliverable / submission / transmittal / issue;
- fee / price / cost / budget / forecast / commitment;
- client vs customer;
- vendor vs supplier vs subcontractor;
- record vs document vs evidence.

## Change control

Whenever a material term is added or materially redefined, affected requirements, rules, interfaces, reports and later architecture/data designs shall be reviewed for semantic impact.

## References

- `Data_requirements.md`
- `Business_requirements_document_BRD.md`
- `Software_requirements_specification_SRS.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial controlled business-data vocabulary; no physical schema implied |