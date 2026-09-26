# Proof of concept report

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-014  
**Document Type:** Proof of concept report  
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
**Related Documents:** `Technical_spikes.md`, `Architecture_decision_records_ADRs.md`, `High-level_design_HLD.md`, `Data_architecture.md`, `Security_architecture.md`, `Data_migration_design.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Proof_of_concept_report.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Proof_of_concept_report.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history / GitHub Actions run 36256385149  

## Purpose

Record the evidence produced by the first foundation architecture proof of concept and distinguish verified technical facts from architecture decisions that still require approval.

## Proof of concept scope

The implemented experiment under `spikes/foundation-architecture/` tests a deliberately generic governed-work transaction using:

- .NET 10 LTS / C#;
- PostgreSQL 18;
- explicit module projects for Subjects, Work, Decisions and Audit;
- an Application orchestration layer;
- a PostgreSQL transaction-session abstraction;
- a minimal HTTP harness;
- a console verification harness;
- versioned SQL migration `0001_foundation.sql`;
- GitHub Actions CI.

The experiment does **not** define the final NuBlox business taxonomy, tenancy model, identity model, UI, navigation, deployment topology or production architecture.

## Scenario exercised

```text
customer context
→ create typed sample business subject
→ create work request linked to subject
→ create decision linked to work
→ append audit evidence
→ commit all or none
```

The database schema uses composite `(customer_id, id)` primary keys and composite foreign keys across the sample module-owned tables.

## Verified evidence

### POC-E-001 — .NET 10 build viability

The CI runner successfully installed .NET 10, restored the API and verification projects, and built both in Release configuration.

**Result:** PASS.

### POC-E-002 — PostgreSQL 18 execution viability

A PostgreSQL 18 service container started successfully and accepted the versioned foundation migration with `ON_ERROR_STOP` enabled.

**Result:** PASS.

### POC-E-003 — Atomic modular transaction

The verification harness executed one use case that writes a subject, work record, decision and audit event through separate module APIs within one shared database transaction.

The harness verified that all four records existed after commit.

**Result:** PASS.

### POC-E-004 — Database-enforced customer-context relationship integrity

The verification harness attempted to create a work record for one customer referencing a subject belonging to another customer.

The database composite foreign key rejected the relationship.

**Result:** PASS.

### POC-E-005 — Rollback leaves no partial module state

The verification harness created subject/work state inside a transaction and explicitly rolled the transaction back.

Subsequent queries confirmed that the rolled-back subject and work records did not exist.

**Result:** PASS.

### POC-E-006 — CI reproducibility

GitHub Actions workflow `Foundation architecture spike`, run `36256385149`, completed successfully on `main`.

The run completed:

1. PostgreSQL container startup;
2. checkout;
3. .NET 10 setup;
4. migration application;
5. API restore/build;
6. verification restore/build;
7. foundation verification harness.

**Result:** PASS.

## What the POC does not prove

This proof of concept does not yet prove:

- production performance or scale;
- final customer/tenant isolation strategy;
- final authentication/identity architecture;
- business-authority evaluation;
- long-running workflow/orchestration;
- durable asynchronous delivery/outbox behaviour;
- effective-dated/historical reconstruction;
- production observability;
- disaster recovery or availability targets;
- real customer migration complexity;
- suitability of the sample domain terminology for the NuBlox product model;
- that a modular monolith or PostgreSQL has been formally approved for production.

## Architectural implication

The experiment provides positive evidence that a cohesive modular application can maintain explicit module write boundaries while sharing a relational transaction and that PostgreSQL constraints can enforce cross-module/customer-context integrity.

This is evidence in favour of continuing the modular-relational hypothesis. It is **not** sufficient by itself to approve the production architecture.

## Next evidence required

Proceed with the next bounded spikes in `Technical_spikes.md`, prioritising:

1. historical/effective-dated data;
2. customer/isolation context options;
3. permission versus business authority;
4. durable asynchronous integration/outbox;
5. governed configuration;
6. migration staging and reconciliation;
7. operational reporting/drill-through.

Each spike must retain independent success/failure criteria and evidence.

## CI evidence

- Workflow: `.github/workflows/foundation-spike.yml`
- Run ID: `36256385149`
- Commit tested: `6bb6d0595dd8f50853416de439e1fed96d611594`
- Conclusion: `success`
- Completed: 2026-09-26

## Recommendation

Retain the foundation spike as disposable evidence and continue the next technical spikes. Do not promote the current code into the production application until the relevant ADRs are reviewed and accepted after the remaining high-risk uncertainties are tested.

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Engineering | Recorded successful foundation architecture CI verification and evidence boundaries |
