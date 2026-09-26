# Technical spikes

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-013  
**Document Type:** Technical spikes  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture / Engineering  
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
**Related Documents:** `Evaluation_matrix_for_technology_selection.md`, `Architecture_decision_records_ADRs.md`, `High-level_design_HLD.md`, `Data_architecture.md`, `Integration_design.md`, `Security_architecture.md`, `Data_migration_design.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Technical_spikes.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Technical_spikes.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define bounded technical experiments used to reduce material architecture uncertainty. A spike proves or falsifies an assumption; it does not become production architecture merely because code exists.

## Spike rules

1. Every spike must identify the ADR/requirement/risk it informs.
2. Success/failure criteria are defined before implementation.
3. Spike code is disposable unless separately reviewed and promoted through the normal delivery controls.
4. Results, limitations and measurements are retained in a proof-of-concept/spike report.
5. Spikes must not invent customer/business semantics that requirements have deliberately left unresolved.

## SPIKE-001 — Cohesive modular core + relational transactions

**Informs:** `ADR-001`, `ADR-002`, `ADR-017`  
**Candidate stack:** .NET 10 LTS / C# + PostgreSQL 18  
**Goal:** Prove that explicit internal module ownership and strong transactional invariants can be implemented cleanly without premature service distribution.

### Scenario

Implement a deliberately generic but typed test scenario:

```text
Customer/organisation context
→ governed business subject
→ work request
→ decision/approval
→ state transition
→ audit/evidence
```

The spike must not define final NuBlox Party/Position/Function taxonomy.

### Must prove

- explicit module boundaries in code;
- one module cannot arbitrarily mutate another module's state;
- atomic business transaction across the required consistency boundary;
- database constraints protect core invariants;
- application tests can execute the transaction end to end;
- schema migrations are version controlled;
- no generic object engine is necessary to express the scenario.

### Failure signals

- architecture requires widespread shared-table manipulation;
- module boundaries exist only as folders with no enforceable dependency discipline;
- ORM/data access makes invariants or transaction boundaries unclear;
- basic domain change requires excessive framework ceremony.

## SPIKE-002 — Historical/effective-dated data

**Informs:** `ADR-002`, `ADR-018`; `BR-014`; `DATA-004`, `DATA-005`

### Goal

Prove reconstruction of both current and historical business meaning without treating `updated_at` as sufficient history.

### Scenario

A typed relationship/state has:

- creation timestamp;
- business effective-from / effective-to;
- one or more changes;
- supersession or closure;
- decision/audit evidence.

Queries must answer:

- what is current now?;
- what was effective at an earlier business date?;
- what change caused the transition?;
- who/what authorised the change?;
- what record existed technically at a chosen audit point where applicable?

### Must prove

- clear distinction between technical timestamps and business effective time;
- constraints prevent invalid overlapping/effective state where the model requires exclusivity;
- historical query is understandable and testable;
- indexing/query performance is plausible on representative synthetic volume.

## SPIKE-003 — Customer/isolation context

**Informs:** `ADR-007`; security architecture; `API-014`

### Goal

Compare isolation patterns and prove that accidental cross-customer access is prevented through more than developer convention.

### Options to prototype

1. shared schema with mandatory customer discriminator and database-enforced row policies where practical;
2. separate schema per customer;
3. database per customer;
4. hybrid strategy design only if evidence justifies it.

### Test cases

- normal customer-scoped query/write;
- guessed/modified record ID from another customer;
- background job with wrong/missing isolation context;
- integration callback with ambiguous customer mapping;
- privileged platform/support access;
- backup/restore/export implications;
- migration/schema upgrade across multiple customers.

### Evidence

Document security strength, developer ergonomics, operational complexity, restore/export characteristics, migration burden, query/reporting implications and likely cost.

## SPIKE-004 — Permission vs business authority

**Informs:** `ADR-009`; `BR-006`, `BR-007`; `RULE-001`, `RULE-002`

### Goal

Prove a server-side decision flow where a user can technically access a decision operation but may only approve when business authority conditions are satisfied.

### Scenario

Authority may depend on:

- customer/organisation context;
- decision type;
- monetary/other threshold;
- business subject/work context;
- effective period;
- delegation;
- segregation-of-duties rule.

### Must prove

- access permission and authority check are distinct;
- authority result is explainable/auditable;
- change/delegation is effective-dated;
- denied and approved attempts are tested;
- no UI-only enforcement.

## SPIKE-005 — Durable asynchronous integration / outbox

**Informs:** `ADR-003`, `ADR-011`; integration design; `AC-007`

### Goal

Prove that a committed local business action cannot silently lose its required external consequence and that retries do not create duplicate business effects.

### Scenario

1. local state transition commits;
2. durable integration intent is committed atomically;
3. worker attempts delivery;
4. provider fails/transiently times out;
5. retry occurs;
6. provider returns success/rejection;
7. result is reconciled to the business operation.

### Must prove

- crash after local commit but before delivery does not lose intent;
- crash during delivery can recover safely;
- duplicate processing is idempotent where required;
- error becomes actionable business/support state;
- correlation connects business record, job and provider request/result;
- integration payload secrets/sensitive data are not leaked in logs.

## SPIKE-006 — Controlled configuration / extension

**Informs:** `ADR-005`; `BR-011`; `AC-010`

### Goal

Prove supported variation without customer code forks or a universal untyped metadata model.

### Scenario

Configure a typed field/rule/reference choice for one customer while mandatory product invariants remain fixed.

### Must prove

- typed validation;
- configuration scope/customer ownership;
- version/change history;
- safe effective activation;
- upgrade/schema compatibility;
- query/report access to configured information where supported;
- mandatory security/data rules cannot be overridden.

## SPIKE-007 — Migration staging and reconciliation

**Informs:** `ADR-019`; `BR-013`; `AC-008`

### Goal

Prove repeatable migration from a synthetic legacy source with bad/ambiguous data.

### Source dataset must include

- stable master records;
- relationships;
- effective-dated history;
- duplicate/invalid references;
- external identifiers;
- status-code mapping;
- binary attachment references;
- deliberately unmappable examples.

### Must prove

- raw/source staging separated from authoritative state;
- versioned mapping/transformation;
- validation and exception classification;
- deterministic rerun;
- source/target reconciliation;
- retained source identity/migration batch evidence;
- failed rows do not corrupt accepted target state.

## SPIKE-008 — Operational reporting / drill-through

**Informs:** `ADR-015`; `BR-008`, `BR-016`; `REP-*`

### Goal

Prove operational management views can initially be served from governed relational/read-model structures without introducing an analytical platform prematurely.

### Must prove

- scoped access is enforced;
- measure definition is explicit;
- aggregate can drill through to source context;
- current vs historical/as-at query semantics are distinguishable;
- representative synthetic data volume is acceptable;
- query plan/indexing evidence is captured.

## SPIKE-009 — Build/test/operability developer experience

**Informs:** `ADR-012`, `ADR-016`, later CI/CD decisions.

### Goal

Validate that the proposed stack has a simple reproducible development/test workflow on supported developer platforms, including macOS.

### Must prove

- clean checkout → restore/build/test using documented commands;
- database dependency can be started reproducibly;
- schema migration command is deterministic;
- unit/integration test separation is clear;
- structured logs/correlation are available;
- app/worker shutdown/restart does not lose durable work;
- dependency versions are pinned/control-reviewed.

## Spike sequence

Recommended order:

```text
SPIKE-001 modular transaction core
        ↓
SPIKE-002 historical/effective data
        ↓
SPIKE-003 isolation context
        ↓
SPIKE-004 authority
        ↓
SPIKE-005 durable async integration
        ↓
SPIKE-006 controlled configuration
        ↓
SPIKE-007 migration
        ↓
SPIKE-008 reporting
        ↓
SPIKE-009 build/operability review throughout
```

Some spikes can share one disposable spike codebase, but results must remain separately measurable.

## First spike implementation boundary

The first code scaffold is authorised only for:

- .NET 10 LTS spike solution;
- PostgreSQL 18 development dependency;
- typed sample module(s), not final enterprise taxonomy;
- migrations;
- tests;
- minimal API/test harness;
- background worker when `SPIKE-005` begins;
- no production UI;
- no product branding/navigation;
- no claim that the code is the NuBlox production application.

## References

- `Evaluation_matrix_for_technology_selection.md`
- `Architecture_decision_records_ADRs.md`
- `High-level_design_HLD.md`
- `Data_architecture.md`
- `Integration_design.md`
- `Security_architecture.md`
- `Data_migration_design.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Engineering | Defined nine bounded spikes and authorised .NET 10 + PostgreSQL 18 only for the first architecture experiment |