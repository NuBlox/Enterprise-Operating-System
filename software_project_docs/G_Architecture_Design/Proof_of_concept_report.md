# Proof of concept report

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-014  
**Document Type:** Proof of concept report  
**Version:** 0.2  
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
**Related Documents:** `Technical_spikes.md`, `Architecture_decision_records_ADRs.md`, `High-level_design_HLD.md`, `Data_architecture.md`, `Security_architecture.md`, `Data_migration_design.md`  
**Supersedes:** Version 0.1  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Proof_of_concept_report.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Proof_of_concept_report.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history / GitHub Actions runs `36256385149`, `36256979645`  

## Purpose

Record reproducible evidence from the foundation architecture spikes and distinguish verified technical facts from architecture decisions that still require approval.

## Proof-of-concept scope

The disposable experiment under `spikes/foundation-architecture/` currently tests:

- .NET 10 LTS / C#;
- PostgreSQL 18;
- explicit module projects for Subjects, Work, Decisions and Audit;
- an Application orchestration layer;
- PostgreSQL transaction-session abstraction;
- minimal HTTP and console verification harnesses;
- ordered SQL migrations;
- customer-context relational constraints;
- business-effective history with as-of reconstruction;
- GitHub Actions CI.

The experiment does **not** define the final NuBlox business taxonomy, tenancy model, identity model, UI, navigation, deployment topology or approved production architecture.

## SPIKE-001 — Modular transaction core

### Scenario

```text
customer context
→ create typed sample business subject
→ create work request linked to subject
→ create decision linked to work
→ append audit evidence
→ commit all or none
```

### POC-E-001 — .NET 10 build viability

CI installed .NET 10, restored and built the API and verification projects in Release configuration.

**Result:** PASS.

### POC-E-002 — PostgreSQL 18 execution viability

PostgreSQL 18 started successfully and accepted the versioned foundation migration with `ON_ERROR_STOP` enabled.

**Result:** PASS.

### POC-E-003 — Atomic modular transaction

One use case writes a subject, work record, decision and audit event through separate module APIs inside one shared database transaction. All four records were present after commit.

**Result:** PASS.

### POC-E-004 — Database-enforced customer-context relationship integrity

A work record for one customer was deliberately made to reference a subject belonging to another customer. The composite foreign key rejected the relationship.

**Result:** PASS.

### POC-E-005 — Rollback leaves no partial module state

Subject/work state was written inside a transaction and explicitly rolled back. Subsequent queries confirmed neither record remained.

**Result:** PASS.

### POC-E-006 — CI reproducibility

GitHub Actions run `36256385149` completed successfully.

**Result:** PASS.

## SPIKE-002 — Historical / effective-dated data

### Scenario

A sample business subject has a stable identity and separately controlled effective-dated names. The experiment creates:

```text
2025-01-01  Original Name
2025-06-01  Renamed Subject
```

Technical recording occurs later than the historical business-effective dates.

### POC-E-007 — Ordered schema evolution

CI applies all numbered spike migrations in lexical order. `0002_effective_history.sql` adds the effective-history structure after the foundation schema.

**Result:** PASS.

### POC-E-008 — Business-effective time separated from technical recording time

`subjects.business_subject_names` stores:

- `effective_from`;
- optional `effective_to`;
- `recorded_at`;
- change reason.

The verification harness proved that technical recording time remains distinct from historical business-effective time.

**Result:** PASS.

### POC-E-009 — Historical as-of reconstruction

The verification harness queried the same subject at two business dates:

- March 2025 returned `Original Name`;
- July 2025 returned `Renamed Subject`.

The first version closed exactly at the second version's effective start and the current version remained open-ended.

**Result:** PASS.

### POC-E-010 — Overlapping effective periods rejected

The database uses a GiST exclusion constraint over customer, subject and effective timestamp range. A deliberately overlapping version was rejected by PostgreSQL.

**Result:** PASS.

### POC-E-011 — SPIKE-002 CI reproducibility

GitHub Actions workflow `Foundation architecture spike`, run `36256979645`, completed successfully on commit `44ef93d99319894337de3108588afca81c444885`.

The run successfully:

1. started PostgreSQL 18;
2. applied `0001_foundation.sql` and `0002_effective_history.sql`;
3. restored and built the API harness with warnings treated as errors;
4. restored and built the verification harness;
5. executed SPIKE-001 and SPIKE-002 verification successfully.

**Result:** PASS.

## What the POC does not yet prove

The evidence does not yet establish:

- production performance or representative-volume history-query performance;
- final customer/tenant isolation strategy;
- final authentication/identity architecture;
- business-authority evaluation;
- long-running workflow/orchestration;
- durable asynchronous delivery/outbox behaviour;
- bi-temporal audit reconstruction for every business object;
- production observability;
- disaster recovery or availability targets;
- real customer migration complexity;
- suitability of the sample domain terminology for the NuBlox product model;
- formal production approval of .NET, PostgreSQL or a modular-monolith deployment model.

## Architectural implication

Current evidence supports continuing the hypothesis that NuBlox can use:

- strongly typed modules;
- relational transactions and constraints;
- stable identity separated from effective-dated attributes/state;
- explicit historical query semantics.

The successful spikes make these approaches credible. They do **not** make them final production decisions.

## Next evidence required

Continue the bounded spike sequence with:

1. customer/isolation context options;
2. permission versus business authority;
3. durable asynchronous integration/outbox;
4. governed configuration;
5. migration staging and reconciliation;
6. operational reporting/drill-through;
7. build/operability assessment throughout.

The history design must also be tested later at representative synthetic volume before an NFR/performance conclusion is made.

## CI evidence

| Spike | Workflow run | Commit | Conclusion |
|---|---:|---|---|
| SPIKE-001 | `36256385149` | `6bb6d0595dd8f50853416de439e1fed96d611594` | success |
| SPIKE-002 | `36256979645` | `44ef93d99319894337de3108588afca81c444885` | success |

## Recommendation

Retain `spikes/foundation-architecture/` as disposable evidence and proceed to SPIKE-003. Do not promote the current spike code into the production application until the material ADRs have been reviewed after the remaining high-risk uncertainties are tested.

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Engineering | Recorded successful foundation transaction CI verification and evidence boundaries |
| 0.2 | 2026-09-26 | NuBlox Architecture / Engineering | Added successful effective-dated history, as-of reconstruction and overlap-prevention evidence |
