# Architecture decision records (ADRs)

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-005  
**Document Type:** Architecture decision records (ADR register)  
**Version:** 0.4  
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
**Supersedes:** Version 0.3  
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
| [`ADR-008`](adr/ADR-008-federated-application-identity.md) | Identity/authentication | Separate enterprise identity from application Principal; OIDC-primary federation, service principals and verified tenant participation | ACCEPTED | Enterprise federation, security, FR-001–FR-004 |
| `ADR-009` | Business authority model | Business authority/decision mandate must be enforceable separately from technical permission where required | PROPOSED | BR-006, BR-007, audit/control |
| `ADR-010` | Workflow/orchestration | Determine coded use-case/state-machine patterns vs workflow engine vs hybrid by process characteristics | PROPOSED | Changeability, observability, long-running work, complexity |
| [`ADR-011`](adr/ADR-011-http-api-standards.md) | API standards | HTTPS/JSON/OpenAPI capability-oriented remote APIs; explicit major versions, RFC 9457 errors and defined idempotency/compatibility | ACCEPTED | Integration, security, developer experience, compatibility |
| [`ADR-012`](adr/ADR-012-dotnet10-server-runtime.md) | Runtime/language/framework | .NET 10 LTS / C# is the production server/core runtime; ASP.NET Core is the default HTTP server framework; frontend remains separate | ACCEPTED | Maintainability, productivity, ecosystem, support, completed spike evidence |
| `ADR-013` | Cloud/deployment provider | Determine provider/topology after residency, customer, resilience and cost requirements are clearer | DEFERRED | NFRs, compliance, economics |
| `ADR-014` | Search | Use relational/native search first vs dedicated search index based on validated search/load requirements | DEFERRED | Search UX, scale, operations |
| `ADR-015` | Analytics architecture | Begin from operational read/query models; add analytical store when workload/retention needs justify it | PROPOSED | REP/ANA requirements, isolation, cost |
| `ADR-016` | Observability stack | Standardise structured logs, metrics, tracing/correlation and alerting after runtime/deployment choice | DEFERRED | Supportability, NFR-OPS |
| [`ADR-017`](adr/ADR-017-module-owned-data-boundaries.md) | Schema/data modularity | Every authoritative data set has one owning module; cross-module behaviour uses explicit contracts and provider details stay behind infrastructure boundaries | ACCEPTED | Modular integrity, future extraction, maintainability |
| `ADR-018` | Audit/event evidence | Distinguish business audit/evidence from technical logs; define protected retained evidence mechanisms | PROPOSED | NFR-AUD, BR-014, BR-015 |
| `ADR-019` | Migration architecture | Treat migration as product/architecture capability with staging, mapping, validation and reconciliation | PROPOSED | BR-013, implementation repeatability |
| `ADR-020` | Release/config/schema evolution | Define backward-safe application/schema/configuration migration and rollback/forward-fix model | PROPOSED | Operability, customer upgrades, integrity |

## Accepted production foundation

The accepted decisions now establish:

```text
cohesive modular application
        +
module-owned data / persistence boundaries
        +
transactional relational authoritative persistence
        +
layered tenant-aware logical data model
        +
.NET 10 LTS / C# server and core runtime
        +
federated Principal identity separate from Person
        +
HTTPS/JSON/OpenAPI remote API standard
```

These decisions deliberately do **not** yet select the relational database product, identity-provider vendor, frontend framework, cloud provider or observability vendor.

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
ACCEPTED FOUNDATION:
ADR-001 decomposition
ADR-002 persistence model
ADR-007 tenant isolation
ADR-008 identity/authentication boundary
ADR-011 API standards
ADR-012 .NET 10 server/core runtime
ADR-017 module-owned data boundaries
        ↓
NEXT IMPLEMENTATION GATES:
ADR-016 observability
ADR-020 release/schema evolution
relational database provider selection
ADR-018 audit/evidence as production audit work begins
        ↓
PERSISTENCE / IDENTITY / API PRODUCTION FOUNDATION
```

Domain/process decisions such as ADR-003, ADR-005, ADR-009, ADR-010, ADR-015 and ADR-019 are promoted as the relevant product slice requires them.

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
- `Evaluation_matrix_for_technology_selection.md`
- `Technical_spikes.md`
- `Proof_of_concept_report.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Established stable ADR register with first 20 proposed/deferred decisions |
| 0.2 | 2026-09-26 | NuBlox Architecture | Accepted ADR-001 cohesive modular application, ADR-002 relational primary persistence model and ADR-007 layered tenant isolation |
| 0.3 | 2026-09-26 | NuBlox Architecture | Accepted ADR-012 .NET 10 LTS/C# as the production server/core runtime and ASP.NET Core as the default server HTTP framework |
| 0.4 | 2026-09-26 | NuBlox Architecture | Accepted ADR-008 federated application identity, ADR-011 HTTP API standards and ADR-017 module-owned data boundaries |
