# Architecture decision records (ADRs)

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-005  
**Document Type:** Architecture decision records (ADR register)  
**Version:** 0.2  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture  
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
**Related Documents:** `Architecture_vision.md`, `Architecture_definition.md`, `Solution_architecture_document.md`  
**Supersedes:** Version 0.1  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the authoritative register of material architecture decisions. Individual ADRs are created under `adr/` once a decision has enough evidence and scope to stand alone.

## ADR lifecycle

- `PROPOSED` — option/decision under analysis.
- `ACCEPTED` — approved current architecture decision.
- `SUPERSEDED` — replaced by a later ADR.
- `REJECTED` — explicitly considered and rejected.
- `DEFERRED` — decision intentionally postponed.

## ADR register

| ADR | Decision topic | Current decision / question | Status | Primary drivers |
|---|---|---|---|---|
| [`ADR-001`](adr/ADR-001-cohesive-modular-application.md) | Initial application decomposition | Start as a cohesive modular application with explicit module/data ownership; extract services only when evidence justifies it | ACCEPTED | Integrity, evolving semantics, delivery/ops complexity |
| [`ADR-002`](adr/ADR-002-transactional-relational-primary-persistence.md) | Primary persistence model | Transactional relational persistence is the primary authoritative model; database product/provider remains separate | ACCEPTED | Relationships, consistency, querying, history, migration |
| `ADR-003` | Durable asynchronous processing | Use durable jobs/messages and transactional-outbox-or-equivalent for external/long-running work | PROPOSED | Reliability, retry, reconciliation, no silent loss |
| `ADR-004` | Binary content/work-product storage | Keep business metadata/state separate from binary/object content where appropriate | PROPOSED | Scale, content lifecycle, specialist-system coexistence |
| `ADR-005` | Configuration/extensibility model | Typed/governed configuration should extend domain semantics rather than universal generic-object runtime | PROPOSED | Integrity, supportability, upgradeability |
| `ADR-006` | Information authority | Every material integration/domain boundary must declare authoritative source/update rights | PROPOSED | Data integrity, reconciliation, BR-004 |
| [`ADR-007`](adr/ADR-007-layered-tenant-isolation.md) | Customer/tenant isolation | One tenant-aware logical model; shared database/schema is the default profile with layered enforcement and dedicated-database profile when justified | ACCEPTED | Security, economics, scale, residency, operations |
| `ADR-008` | Identity/authentication | Determine external IdP/federation model, account linking and privileged/service identity approach | PROPOSED | Enterprise adoption, security, SR-024 |
| `ADR-009` | Business authority model | Business authority/decision mandate must be enforceable separately from technical permission where required | PROPOSED | BR-006, BR-007, audit/control |
| `ADR-010` | Workflow/orchestration | Determine coded use-case/state-machine patterns vs workflow engine vs hybrid by process characteristics | PROPOSED | Changeability, observability, long-running work, complexity |
| `ADR-011` | API standards | Select external/internal API styles, schemas, error/versioning/idempotency conventions | PROPOSED | Integration, developer experience, compatibility |
| `ADR-012` | Runtime/language/framework | Select implementation stack after architecture spikes and team/NFR/product-economics assessment | DEFERRED | Maintainability, productivity, ecosystem, operations |
| `ADR-013` | Cloud/deployment provider | Determine provider/topology after residency, customer, resilience and cost requirements are clearer | DEFERRED | NFRs, compliance, economics |
| `ADR-014` | Search | Use relational/native search first vs dedicated search index based on validated search/load requirements | DEFERRED | Search UX, scale, operations |
| `ADR-015` | Analytics architecture | Begin from operational read/query models; add analytical store when workload/retention needs justify it | PROPOSED | REP/ANA requirements, isolation, cost |
| `ADR-016` | Observability stack | Standardise structured logs, metrics, tracing/correlation and alerting after runtime/deployment choice | DEFERRED | Supportability, NFR-OPS |
| `ADR-017` | Schema/data modularity | Define ownership boundaries and prevent unrestricted cross-module persistence access | PROPOSED | Modular integrity, future extraction, maintainability |
| `ADR-018` | Audit/event evidence | Distinguish business audit/evidence from technical logs; define protected retained evidence mechanisms | PROPOSED | NFR-AUD, BR-014, BR-015 |
| `ADR-019` | Migration architecture | Treat migration as product/architecture capability with staging, mapping, validation and reconciliation | PROPOSED | BR-013, implementation repeatability |
| `ADR-020` | Release/config/schema evolution | Define backward-safe application/schema/configuration migration and rollback/forward-fix model | PROPOSED | Operability, customer upgrades, integrity |

## Accepted foundation invariants

The first accepted decision set establishes three provider-neutral product foundations:

```text
cohesive modular application
        +
transactional relational authoritative persistence
        +
layered tenant-aware logical data model
```

These decisions deliberately do **not** select the production runtime/framework, database product, identity provider, cloud provider or observability vendor.

## ADR approval requirements

Before an ADR is Accepted it should include:

1. problem/context and decision scope;
2. traced requirements/NFRs/constraints;
3. at least the credible alternatives;
4. measurable trade-offs where possible;
5. security/data/operational implications;
6. migration/reversibility consequences;
7. prototype/spike/benchmark evidence where risk warrants it;
8. approval by the accountable architecture governance role.

## Decision dependency order

```text
ACCEPTED:
ADR-001 decomposition
ADR-002 persistence model
ADR-007 tenant isolation model
        ↓
NEXT IMPLEMENTATION GATES:
ADR-008 identity
ADR-011 API
ADR-012 runtime/framework
ADR-016 observability
ADR-020 release/evolution
        ↓
PRODUCTION FOUNDATION
```

Other domain/process decisions continue in parallel as their product slices require them.

## Individual ADR location

Individual records are stored under:

```text
software_project_docs/G_Architecture_Design/adr/
```

The register remains the authoritative index/status view.

## References

- `Architecture_vision.md`
- `Architecture_definition.md`
- `Solution_architecture_document.md`
- `Technical_spikes.md`
- `Proof_of_concept_report.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Established stable ADR register with first 20 proposed/deferred decisions |
| 0.2 | 2026-09-26 | NuBlox Architecture | Accepted ADR-001 cohesive modular application, ADR-002 relational primary persistence model and ADR-007 layered tenant isolation; linked individual records |
