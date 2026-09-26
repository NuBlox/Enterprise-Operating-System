# Data architecture

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-008  
**Document Type:** Data architecture  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture / Data  
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
**Related Documents:** `Solution_architecture_document.md`, `High-level_design_HLD.md`, `../F_Requirements_Analysis/Data_requirements.md`, `../A_Enterprise_Pre_Project/Data_governance_policy.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Data_architecture.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Data_architecture.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the first architecture for authoritative, historical, migratable and reportable NuBlox information without prematurely freezing the final business object taxonomy or physical schema.

## Data architecture goals

- preserve explicit business meaning and identity;
- enforce required relationships/invariants transactionally;
- make authority/source ownership visible;
- preserve history/effective state where required;
- support customer isolation/security consistently;
- enable repeatable migration/reconciliation;
- support governed configuration/extension;
- support operational reporting without duplicate uncontrolled truth;
- enable later scale/search/analytics specialisation when justified.

## Primary persistence hypothesis

The leading architecture is:

```text
Explicit business/domain records
        +
Explicit relationship/state/history structures
        +
Governed configuration / extension data
        ↓
Transactional relational persistence
```

A universal generic `Thing / Field / Value / Relationship` persistence model is **not the default hypothesis** because current requirements include strong type-specific invariants, authority, lifecycle, reporting and transactional relationships. Generic metadata may be used for controlled extension where it adds value without erasing core semantics.

## Information categories

### 1. Authoritative operational data
Structured business state managed by NuBlox for approved scope. Must satisfy transactional/integrity/history requirements.

### 2. Reference/master data
Governed reusable definitions/codes/classifications used by operational records. Ownership and effective lifecycle must be clear.

### 3. Configuration data
Customer/product settings and definitions controlling supported behaviour. Material configuration should be versioned/auditable and validated.

### 4. Business audit/evidence
Protected records needed to reconstruct material actions, decisions and state changes. This is distinct from ordinary technical logs.

### 5. Integration/migration state
Correlation, external identifiers, delivery status, source metadata, mapping/reconciliation evidence and exceptions.

### 6. Binary/content data
Files/models/attachments or other binary assets stored by NuBlox where required. Transactional metadata links them to business context.

### 7. Derived search/reporting/analytics data
Read-optimised representations that can be rebuilt/reconciled from authoritative data and governed definitions.

## Identity strategy — Candidate

Material business records should use stable internal identifiers independent of mutable human-readable codes. Where external/customer/source identifiers matter, retain them explicitly with source/context rather than overloading the internal key.

Candidate principles:

- internal identity is immutable;
- business/reference numbers may be unique under defined scope but can have lifecycle rules;
- external IDs include source-system/context;
- identifiers are never silently reused in a way that makes history ambiguous;
- URLs/API identifiers should not depend on mutable display names unless deliberately designed.

Exact ID technology is an ADR/implementation choice.

## Relationship strategy

Relationships that carry business meaning, lifecycle, authority, dates or provenance should be represented explicitly rather than hidden in arbitrary JSON or inferred from display structure.

Examples requiring later domain validation include:

- participant-to-organisation relationships;
- work-to-context relationships;
- work-to-output relationships;
- decision-to-subject/evidence relationships;
- contract/commitment-to-delivery relationships;
- revision/supersession relationships;
- source/external identity mappings.

## History and time

Different historical needs must be distinguished:

- **technical created/updated timestamps**;
- **business effective/valid time**;
- **revision/version history**;
- **state-transition history**;
- **audit/event history**;
- **snapshot/as-at reporting**.

Not every record requires all forms. The domain/data design shall choose the minimum sufficient model per requirement.

## Deletion, retention and privacy

Deletion is not simply a technical row delete. The architecture must reconcile:

- business-record retention;
- contractual/regulatory retention;
- audit/evidence integrity;
- personal-data minimisation/deletion obligations;
- legal hold;
- external/binary copies;
- derived/search/analytics copies;
- backup retention.

Policies shall distinguish hard deletion, anonymisation/pseudonymisation, logical closure, archival and immutable retention where applicable.

## Customer isolation — Open architecture decision

`ADR-007` must select an isolation model. Candidate patterns include:

1. shared DB/shared schema with mandatory customer discriminator and database/application enforcement;
2. shared DB/separate schema;
3. database per customer;
4. hybrid tiers by customer/regulatory requirement.

Evaluation criteria:

- security blast radius;
- query/data-leak risk;
- operational complexity;
- migration/backup/restore;
- analytics/reporting;
- cost/scale;
- data residency;
- customer-specific restore/export;
- schema evolution;
- support access.

No model is accepted yet.

## Schema modularity

Even with one relational database, schema ownership should follow logical module/domain responsibility. Cross-module access should be through owned APIs/application contracts or intentionally governed shared concepts rather than arbitrary table coupling.

Future service extraction should not require first untangling uncontrolled cross-module writes.

## Configuration and extension data

Configuration must:

- have explicit type/schema/validation;
- state scope (platform/customer/work context etc.);
- state effective lifecycle where material;
- preserve change history;
- support safe upgrade/migration;
- not override mandatory security/integrity invariants;
- expose dependency/impact where changes affect active workflows.

Custom fields/metadata, if introduced, should use typed definitions, validation, classification and indexing/reporting policies rather than unbounded opaque JSON by default.

## Transaction design

A transaction boundary should align with a business consistency requirement. Operations that must succeed/fail together should commit atomically where feasible.

Cross-system effects cannot share the local transaction; they require durable intent plus asynchronous delivery/reconciliation.

## Data-access rules

- application/business modules own writes to their data;
- raw database access is not a normal end-user/admin interface;
- migrations/scripts are controlled/versioned/reviewed;
- production support access follows least privilege and audit;
- analytics/search projections do not write back to authoritative state except through approved application contracts.

## Reporting/read-model approach

Start with governed relational queries/read models for operational reporting. Introduce specialised projection/warehouse/search stores only when measured workload, history or analytical needs justify them.

Every derived representation must have:

- source/lineage;
- refresh/rebuild path;
- reconciliation expectations;
- security/isolation controls;
- definition/version ownership.

## Data architecture validation spikes

Before persistence approval, test representative cases for:

- complex cross-domain transaction;
- effective-dated relationship/state;
- high-volume audit/event history;
- customer isolation query enforcement;
- configurable fields/reporting;
- integration external-ID/correlation mapping;
- migration bulk load/reconciliation;
- operational report with drill-through;
- backup/restore at customer isolation unit.

## Open decisions

- isolation/tenancy model;
- database engine;
- migration framework/tooling;
- history/temporal modelling patterns;
- audit store/tamper protection;
- object storage/content model;
- custom field/metadata mechanism;
- search/index need;
- analytics store need;
- retention policy implementation;
- encryption/key strategy.

## References

- `../F_Requirements_Analysis/Data_requirements.md`
- `../F_Requirements_Analysis/Data_dictionary.md`
- `../A_Enterprise_Pre_Project/Data_governance_policy.md`
- `Solution_architecture_document.md`
- `Architecture_decision_records_ADRs.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Data | Initial requirements-derived data architecture and persistence/isolation hypotheses |