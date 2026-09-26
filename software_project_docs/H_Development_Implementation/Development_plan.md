# Development plan

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-001  
**Document Type:** Development plan  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Engineering  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `../G_Architecture_Design/Architecture_decision_records_ADRs.md`, `../G_Architecture_Design/Technical_spikes.md`, `../G_Architecture_Design/Proof_of_concept_report.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/H_Development_Implementation/Development_plan.md`  
**Storage Location:** `software_project_docs/H_Development_Implementation/Development_plan.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the controlled transition from completed architecture spikes into NuBlox product implementation. The plan prevents experimental spike code or candidate technology choices from becoming production architecture by momentum.

## Starting evidence

The architecture spike programme has completed `SPIKE-001` through `SPIKE-009`. The evidence includes modular transaction boundaries, effective-dated data, isolation controls, business-authority separation, durable asynchronous work, governed configuration, migration staging/reconciliation, operational reporting, reproducible build/migration execution, structured correlation logging and worker recovery.

`SPIKE-009` was merged to `main` in PR #2 after the Build and operability and Operational reporting workflows both passed on the reconciled head.

The evidence reduces architecture uncertainty; it does not by itself approve a production stack or production schema.

## Development principles

1. **Requirements remain authoritative.** Product code traces to controlled requirements and acceptance criteria rather than to spike implementation details.
2. **Architecture decisions are explicit.** Material implementation choices must reference an ADR with a recorded status and evidence.
3. **No direct spike promotion.** Code under `spikes/` is disposable. Proven patterns may be reimplemented after design review; files are not copied into product source merely because the spike passed.
4. **First-party package preference.** Where NuBlox maintains a mastered package under `packages/mastered/`, implementation should prefer that governed package when it satisfies the requirement. Direct upstream use requires a recorded rationale and provenance.
5. **Provider decisions are deliberate.** A mastered connector or successful spike provider does not implicitly make that provider the enterprise-wide production standard.
6. **One vertical outcome at a time.** Each implementation wave should produce a runnable, testable business or platform outcome with traceability and rollback/forward-fix considerations.
7. **Security, isolation and audit are server-side controls.** UI behaviour must never be the sole enforcement boundary.

## Entry gates for product implementation

The following decisions must be explicit before the affected production capability is implemented:

| Decision | Required before | Current register state |
|---|---|---|
| `ADR-001` application decomposition | production solution/module structure | PROPOSED |
| `ADR-002` primary persistence model | authoritative production schema | PROPOSED |
| `ADR-007` customer/tenant isolation | customer-scoped persistence and jobs | PROPOSED |
| `ADR-008` identity/authentication | production sign-in and service identity | PROPOSED |
| `ADR-011` API standards | external/stable API contracts | PROPOSED |
| `ADR-012` runtime/language/framework | production runtime/toolchain | DEFERRED |
| `ADR-016` observability stack | production telemetry/export/alerting | DEFERRED |
| `ADR-020` release/config/schema evolution | production upgrade/rollback mechanism | PROPOSED |

Other ADRs are promoted as their corresponding product slice reaches implementation.

## Delivery waves

### Wave 0 — decision promotion and development controls

- create individual ADR records for the implementation-critical decisions;
- attach spike/benchmark evidence and credible alternatives;
- explicitly accept, defer or reject each decision through architecture governance;
- establish the H-section development baseline, backlog, build instructions, dependency governance and CI policy;
- keep frontend and cloud/provider choices separate until their requirements are sufficiently evidenced.

**Exit:** product implementation no longer depends on an unstated architecture choice.

### Wave 1 — production repository and toolchain foundation

After the runtime decision is accepted:

- create production `src/` and `tests/` boundaries separate from `spikes/`;
- pin the selected SDK/runtime and package-management policy;
- add deterministic restore/build/test commands;
- add CI gates for build, tests, dependency/provenance checks and controlled migrations where applicable;
- establish structured configuration and secret boundaries;
- expose only platform health/diagnostic behaviour required by the accepted design;
- do not introduce final business taxonomy merely to populate the scaffold.

**Exit:** a clean checkout can reproducibly restore, build and verify the production foundation on a supported developer platform and in CI.

### Wave 2 — first governed vertical slice

Select the first slice from approved requirements and implement it end to end through:

```text
request/use case
→ server-side identity/isolation/authority context
→ typed application/domain behaviour
→ authoritative persistence
→ audit/evidence
→ API/UX boundary as applicable
→ automated verification
```

The slice must demonstrate the production architecture without importing the synthetic business subjects used by the spike.

### Wave 3 — enterprise capability expansion

Expand capability by traced backlog priority. Functional governance, functional delivery and built-environment domain delivery are introduced from controlled product requirements rather than pre-creating 29 disconnected modules.

## Data and SQL strategy

The spike demonstrates that relational persistence is a strong fit, but the production database decision remains governed by `ADR-002` and `ADR-007`.

NuBlox may support more than one SQL dialect where there is a product requirement and a tested compatibility boundary. SQL portability must not reduce integrity to the lowest common denominator. Provider-specific capabilities may be used behind explicit persistence boundaries when justified.

The presence of `packages/mastered/mysql` establishes governed MySQL client source/provenance; it is not, by itself, approval of MySQL as the primary NuBlox database.

## Dependency governance

For every production dependency record:

- purpose and owning component;
- exact version/range and lock strategy;
- source/provenance;
- licence;
- vulnerability/update process;
- whether a NuBlox-mastered package exists;
- replacement/exit implications for material dependencies.

No package is introduced solely because it appeared in spike code.

## Quality gates

Every merge that affects production code must, as applicable, pass:

- deterministic restore/build;
- automated unit tests;
- automated integration/contract tests for changed boundaries;
- migration validation and forward/reconciliation checks;
- formatting/lint/static analysis;
- dependency/provenance and vulnerability checks;
- tenant/isolation and authority-negative tests where those controls are affected;
- structured log/correlation checks for operationally material flows;
- documentation/traceability updates for material design changes.

A green build is necessary but not sufficient for promotion to a release.

## Definition of the first production-foundation increment

The first increment is complete when:

1. implementation-critical ADRs for that increment have explicit statuses;
2. production source is physically separated from disposable spikes;
3. clean-checkout build/test instructions are reproducible;
4. CI enforces the same build/test path;
5. dependency provenance and versions are controlled;
6. no synthetic spike taxonomy has leaked into product semantics;
7. the next vertical slice is traceable to controlled requirements and acceptance criteria.

## Out of scope for version 0.1

- declaring spike technologies production-approved without ADR governance;
- selecting a frontend by preference alone;
- selecting a cloud provider before deployment/residency/economic requirements justify it;
- copying the spike schema or synthetic entities into production source;
- implementing all enterprise functions before a traced vertical slice proves the production foundation.

## References

- `../G_Architecture_Design/Architecture_decision_records_ADRs.md`
- `../G_Architecture_Design/Evaluation_matrix_for_technology_selection.md`
- `../G_Architecture_Design/Technical_spikes.md`
- `../G_Architecture_Design/Proof_of_concept_report.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `Product_backlog.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Engineering | Established the controlled transition from architecture spikes into Development & Implementation |
