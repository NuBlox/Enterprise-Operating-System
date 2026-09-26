# Architecture decision records (ADRs)

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-005  
**Document Type:** Architecture decision records (ADR register)  
**Version:** 0.1  
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
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the authoritative register of material architecture decisions. Individual ADRs may later be split into separate files; this register provides stable IDs, status and dependency order from the outset.

## ADR lifecycle

- `PROPOSED` — option/decision under analysis.
- `ACCEPTED` — approved current architecture decision.
- `SUPERSEDED` — replaced by a later ADR.
- `REJECTED` — explicitly considered and rejected.
- `DEFERRED` — decision intentionally postponed.

No ADR in version 0.1 is Accepted unless explicitly shown.

## ADR register

| ADR | Decision topic | Current leading position / question | Status | Primary drivers |
|---|---|---|---|---|
| `ADR-001` | Initial application decomposition | Cohesive modular application is leading hypothesis over first-release microservices | PROPOSED | Integrity, evolving semantics, delivery/ops complexity |
| `ADR-002` | Primary persistence model | Transactional relational persistence is leading hypothesis | PROPOSED | Relationships, consistency, querying, history, migration |
| `ADR-003` | Durable asynchronous processing | Use durable jobs/messages and transactional-outbox-or-equivalent for external/long-running work | PROPOSED | Reliability, retry, reconciliation, no silent loss |
| `ADR-004` | Binary content/work-product storage | Keep business metadata/state separate from binary/object content where appropriate | PROPOSED | Scale, content lifecycle, specialist-system coexistence |
| `ADR-005` | Configuration/extensibility model | Typed/governed configuration should extend domain semantics rather than universal generic-object runtime | PROPOSED | Integrity, supportability, upgradeability |
| `ADR-006` | Information authority | Every material integration/domain boundary must declare authoritative source/update rights | PROPOSED | Data integrity, reconciliation, BR-004 |
| `ADR-007` | Customer/tenant isolation | Compare shared database/schema, row-level isolation, database-per-customer and hybrid models | PROPOSED | Security, economics, scale, residency, operations |
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

Some decisions intentionally depend on others:

```text
Validated workflow / isolation / NFRs
        ↓
ADR-001 decomposition
ADR-002 persistence
ADR-007 isolation
ADR-008 identity
        ↓
ADR-003 async processing
ADR-010 workflow
ADR-011 API
ADR-017 schema modularity
        ↓
ADR-012 runtime/framework
ADR-013 cloud/deployment
ADR-016 observability
ADR-020 release/evolution
```

## Individual ADR location

When a decision receives enough analysis to stand alone, create:

```text
software_project_docs/G_Architecture_Design/adr/ADR-###-short-title.md
```

and retain this register as the index.

## References

- `Architecture_vision.md`
- `Architecture_definition.md`
- `Solution_architecture_document.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Established stable ADR register with first 20 proposed/deferred decisions |