# Proof of concept report

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-014  
**Document Type:** Proof of concept report  
**Version:** 0.5  
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
**Supersedes:** Version 0.4  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Proof_of_concept_report.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Proof_of_concept_report.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history / GitHub Actions runs in the CI evidence table below  

## Purpose

Record reproducible evidence from the foundation architecture spikes and distinguish verified technical facts from architecture decisions that still require approval.

## Proof-of-concept scope

The disposable experiment under `spikes/foundation-architecture/` currently tests:

- .NET 10 LTS / C#;
- PostgreSQL 18;
- explicit modules for Subjects, Work, Decisions, Audit, Authority, Integration, Configuration, Migration and Reporting;
- an Application orchestration layer;
- PostgreSQL transaction-session abstractions;
- minimal HTTP and independent verification harnesses;
- ordered SQL migrations;
- customer-context relational constraints;
- business-effective history with as-of reconstruction;
- shared-schema row-level customer isolation under a restricted application role;
- technical permission separated from business decision authority;
- durable local outbox intent and simulated provider retry/recovery;
- typed effective customer configuration;
- synthetic migration staging, exceptions and reconciliation;
- operational measures over authoritative Work records with source drill-through;
- GitHub Actions CI.

The experiment does **not** define the final NuBlox business taxonomy, tenancy model, identity model, UI, navigation, deployment topology or approved production architecture.

## SPIKE-001 — Modular transaction core

### POC-E-001 — .NET 10 build viability
CI restored and built the spike projects in Release configuration. **Result: PASS.**

### POC-E-002 — PostgreSQL 18 execution viability
PostgreSQL 18 accepted the versioned migrations with fail-fast execution. **Result: PASS.**

### POC-E-003 — Atomic modular transaction
Subject, work, decision and audit writes committed atomically across explicit module APIs. **Result: PASS.**

### POC-E-004 — Cross-customer relationship integrity
Composite foreign keys rejected a cross-customer subject/work relationship. **Result: PASS.**

### POC-E-005 — Rollback leaves no partial state
Explicit rollback removed all partial subject/work writes. **Result: PASS.**

### POC-E-006 — CI reproducibility
GitHub Actions run `36256385149` completed successfully. **Result: PASS.**

## SPIKE-002 — Historical / effective-dated data

### POC-E-007 — Ordered schema evolution
Migration `0002_effective_history.sql` applies after the foundation schema. **Result: PASS.**

### POC-E-008 — Business-effective vs technical recording time
Effective-from/to and recorded-at are distinct and independently queryable. **Result: PASS.**

### POC-E-009 — Historical as-of reconstruction
March 2025 reconstructed `Original Name`; July 2025 reconstructed `Renamed Subject`. **Result: PASS.**

### POC-E-010 — Overlap prevention
A GiST exclusion constraint rejected an overlapping effective period. **Result: PASS.**

### POC-E-011 — SPIKE-002 CI reproducibility
GitHub Actions run `36256979645` completed successfully. **Result: PASS.**

## SPIKE-003 — Customer/isolation context: shared-schema RLS candidate

This spike evaluates one option only: shared schema with mandatory customer context and PostgreSQL row-level security. It does not complete the comparison with schema-per-customer or database-per-customer.

### POC-E-012 — Restricted application role
`nublox_app` is non-superuser and receives only required schema/table rights. **Result: PASS.**

### POC-E-013 — Own-customer visibility / cross-customer read isolation
A customer-scoped session saw its own records and could not see another customer's subject/work/decision/audit rows. **Result: PASS.**

### POC-E-014 — Missing context fails closed
The restricted role without customer context exposed no customer rows. **Result: PASS.**

### POC-E-015 — Wrong-customer write rejected
RLS rejected a write whose `customer_id` did not match the active customer context. **Result: PASS.**

### POC-E-016 — Layered protection
Composite keys protect relationship integrity while RLS protects visibility and writes. **Result: PASS.**

### POC-E-017 — SPIKE-003 CI reproducibility
GitHub Actions run `36257231581` completed successfully. **Result: PASS.**

## SPIKE-004 — Permission versus business authority

### Scenario

The spike deliberately separates:

```text
technical operation permission
        ≠
business authority to approve
```

The test decision type is `COMMERCIAL_COMMITMENT`; authority is effective-dated and may carry a monetary threshold and delegation evidence.

### POC-E-018 — Permission alone is insufficient
A principal with `DECISION.APPROVE` permission but no matching business authority was denied. The result explicitly recorded `BUSINESS_AUTHORITY_MISSING_OR_THRESHOLD_EXCEEDED`.

**Result: PASS.**

### POC-E-019 — Authority alone is insufficient
A principal with valid business authority but no technical operation permission was denied with `OPERATION_PERMISSION_MISSING`.

**Result: PASS.**

### POC-E-020 — Permission plus in-scope authority is allowed
A principal holding both the operation permission and effective authority for a £5,000-equivalent test commitment within a configured £10,000 threshold was allowed.

**Result: PASS.**

### POC-E-021 — Authority threshold is enforced
The same principal was denied when the requested amount exceeded the configured authority threshold.

**Result: PASS.**

### POC-E-022 — Authority effective dates are enforced
A principal retained technical permission but had an authority grant that expired before the evaluation date. The approval was denied.

**Result: PASS.**

### POC-E-023 — Authority evaluations produce evidence
Every evaluation writes a separate `authority.evaluations` record containing customer, principal, operation, decision type, requested amount, outcome and reason.

**Result: PASS.**

### POC-E-024 — SPIKE-004 CI reproducibility
GitHub Actions workflow `Foundation architecture spike`, run `36257548295`, completed successfully. It applied migrations through `0004_permission_authority.sql`, built the existing foundation verifier and the independent authority verifier, and ran both successfully.

**Result: PASS.**

## SPIKE-005 — Durable asynchronous integration

The verifier commits an external handoff intent in the same transaction as
business work, then exercises the outbox worker against a simulated provider.

- Committed work has a durable `PENDING` intent; rollback removes both.
- A transient failure moves to retry and later succeeds with one provider
  business effect; a permanent rejection remains an actionable failed state.
- A simulated crash after provider success is recovered and redelivered using
  the same idempotency key. Two calls produce one simulated external effect.
- Worker claims and outbox reads remain customer scoped.

**Result: PASS** in [foundation workflow run 36258214502](https://github.com/NuBlox/Enterprise-Operating-System/actions/runs/36258214502).
This verifies the local mechanism and a fake provider, not a real integration
contract or provider's idempotency guarantee.

## SPIKE-006 — Governed configuration

The independent configuration verifier checks two effective versions of a
typed customer work policy, separate customer values and RLS isolation. Invalid
typed values are rejected and a mandatory audit flag cannot be disabled even
by a direct application-role insert.

**Result: PASS** in [configuration workflow run 36258663919](https://github.com/NuBlox/Enterprise-Operating-System/actions/runs/36258663919).
Only this one policy shape and its sample invariants were tested.

## SPIKE-007 — Migration staging and reconciliation

The independent migration verifier stages three synthetic source records,
loads two valid subjects with source-to-target identity/history, and retains
one invalid row as a controlled exception. Reprocessing preserves identities
and counts. Customer B cannot see Customer A's migration batch.

**Result: PASS** in [migration workflow run 36258793007](https://github.com/NuBlox/Enterprise-Operating-System/actions/runs/36258793007).
Real source relationships, attachments and large-volume performance remain
outside this narrow fixture.

## SPIKE-008 — Operational reporting and drill-through

The reporting experiment defines two provisional, owned measures sourced from
`work.work_requests`: `WORK_CREATED_THROUGH` uses an exclusive UTC technical
creation cutoff, and `WORK_CURRENT_OPEN` reads current state. Both expose paged
source work records through a restricted customer session. Their different time
bases are disclosed so a creation cutoff is never mistaken for historical work
state. The verifier checks count/detail reconciliation, cross-customer and
missing-context denial, and count/first-page query plans at 20,003 customer
records (20,000 synthetic additions).

**Result: PASS** in [reporting workflow run 36265786034](https://github.com/NuBlox/Enterprise-Operating-System/actions/runs/36265786034)
on commit `437e2e05e0b2e4ca67c1d8e9fcd06d48c39291aa`. The verifier restored,
built and ran against PostgreSQL 18 after all migrations through `0008`.

On the 20,003-record customer fixture, the application count call took
**2.68 ms** and the first 50 source rows took **0.92 ms** in that CI run.
`EXPLAIN (ANALYZE, BUFFERS)` measured **4.481 ms** for the count and
**0.053 ms** for the first-page query. After `ANALYZE`, PostgreSQL chose a
sequential scan for the broad count (all 20,003 rows matched) and an index
scan on `ix_work_created_through` for the first page. Both calls met the
5-second smoke-test bound. These measurements are one synthetic CI sample,
not a production workload benchmark or SLO.

The verifier has no concurrent updates. Across separate reads or pages the
current report can change; snapshot consistency, historical work states and
stable published reports still require explicit design and testing.

## What the POC does not yet prove

The evidence does not yet establish:

- production performance or representative workload behaviour;
- the final customer/tenant isolation strategy or its operational comparison;
- privileged support/platform-access governance;
- final authentication/identity architecture;
- the complete production authority model, including all decision contexts, segregation-of-duties patterns and delegation revocation semantics;
- long-running workflow/orchestration;
- a real provider's idempotency, delivery SLA or operational recovery;
- bi-temporal reconstruction for every business object;
- historical work-state reporting, field-level report permissions or reproducible published snapshots;
- production observability, DR or availability targets;
- real customer migration complexity or high-volume load/reconciliation;
- suitability of the sample terminology for the final NuBlox product model;
- formal production approval of .NET, PostgreSQL or the modular-monolith deployment hypothesis.

## Architectural implication

Current evidence supports continuing the hypothesis that NuBlox can use:

- strongly typed modules;
- relational transactions and database constraints;
- stable identity separated from effective-dated attributes/state;
- explicit historical query semantics;
- layered customer-context protection;
- separate technical permission and business-authority evaluation with auditable outcomes;
- durable local integration intent and simulated idempotent retry;
- typed, effective customer configuration with mandatory invariants;
- migration exception accounting and deterministic rerun for a small source fixture;
- customer-scoped operational counts with source drill-through at modest synthetic volume.

These approaches are technically credible, but remain evidence rather than final production decisions.

## Next evidence required

Continue the bounded spike sequence with:

1. complete isolation-options comparison and privileged/support-access model;
2. test real provider contracts and realistic migration sources/volume;
3. define historical work-state and report snapshot requirements;
4. run SPIKE-009 build/operability assessment and review material ADRs.

## CI evidence

| Spike | Workflow run | Commit | Conclusion |
|---|---:|---|---|
| SPIKE-001 | `36256385149` | `6bb6d0595dd8f50853416de439e1fed96d611594` | success |
| SPIKE-002 | `36256979645` | `44ef93d99319894337de3108588afca81c444885` | success |
| SPIKE-003 shared-schema/RLS candidate | `36257231581` | `bb6142073d06ab6669692339b625c05479086741` | success |
| SPIKE-004 permission vs authority | `36257548295` | `d7d3675d24bbad20f9e5217f007c5b8a0e62e15d` | success |
| SPIKE-005 simulated durable outbox | `36258214502` | `b2fdb05` | success |
| SPIKE-006 typed configuration | `36258663919` | `a3be15d` | success |
| SPIKE-007 synthetic migration | `36258793007` | `6bb6146c003a334ef5d71b0c42ebfa0a800d2757` | success |
| SPIKE-008 operational reporting | `36265786034` | `437e2e05e0b2e4ca67c1d8e9fcd06d48c39291aa` | success |

## Recommendation

Retain `spikes/foundation-architecture/` as disposable evidence. Progress to
SPIKE-009 and the unresolved isolation,
identity and production-volume questions. Promotion requires review of the
material ADRs and normal product delivery controls.

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Engineering | Recorded successful foundation transaction CI verification and evidence boundaries |
| 0.2 | 2026-09-26 | NuBlox Architecture / Engineering | Added successful effective-dated history evidence |
| 0.3 | 2026-09-26 | NuBlox Architecture / Engineering | Added shared-schema row-level isolation evidence |
| 0.4 | 2026-09-26 | NuBlox Architecture / Engineering | Added verified separation of operation permission and effective business authority |
| 0.5 | 2026-09-26 | NuBlox Architecture / Engineering | Reconciled SPIKE-005 to 007 evidence and recorded verified SPIKE-008 findings and limits |
