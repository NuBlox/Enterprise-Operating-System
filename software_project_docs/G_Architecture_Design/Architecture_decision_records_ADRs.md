# Architecture decision records (ADRs)

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-005  
**Document Type:** Architecture decision records (ADR register)  
**Version:** 0.8  
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
**Supersedes:** Version 0.7  
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
| [`ADR-003`](adr/ADR-003-durable-asynchronous-processing.md) | Durable asynchronous processing | Commit required external/independently retried consequences as transactional durable intents; deliver at least once with recoverable claims, idempotency and reconciliation | ACCEPTED | Reliability, retry, reconciliation, no silent loss |
| `ADR-004` | Binary content/work-product storage | Keep business metadata/state separate from binary/object content where appropriate | PROPOSED | Scale, content lifecycle, specialist-system coexistence |
| `ADR-005` | Configuration/extensibility model | Typed/governed configuration should extend domain semantics rather than universal generic-object runtime | PROPOSED | Integrity, supportability, upgradeability |
| `ADR-006` | Information authority | Every material integration/domain boundary must declare authoritative source/update rights | PROPOSED | Data integrity, reconciliation, BR-004 |
| [`ADR-007`](adr/ADR-007-layered-tenant-isolation.md) | Customer/tenant isolation | One tenant-aware logical model; shared database/schema is the default profile with layered enforcement and dedicated-database profile when justified | ACCEPTED | Security, economics, scale, residency, operations |
| [`ADR-008`](adr/ADR-008-federated-application-identity.md) | Identity/authentication | Separate enterprise identity from application Principal; OIDC-primary federation, service principals and verified tenant participation | ACCEPTED | Enterprise federation, security, FR-001–FR-004 |
| [`ADR-009`](adr/ADR-009-contextual-business-authority.md) | Business authority model | Evaluate business authority server-side against verified Tenant, Principal, governed action and subject; authority is separate from technical permission and granted decisions retain an attributable authority reference | ACCEPTED | BR-006, BR-007, FR-015–FR-018, audit/control |
| `ADR-010` | Workflow/orchestration | Determine coded use-case/state-machine patterns vs workflow engine vs hybrid by process characteristics | PROPOSED | Changeability, observability, long-running work, complexity |
| [`ADR-011`](adr/ADR-011-http-api-standards.md) | API standards | HTTPS/JSON/OpenAPI capability-oriented remote APIs; explicit major versions, RFC 9457 errors and defined idempotency/compatibility | ACCEPTED | Integration, security, developer experience, compatibility |
| [`ADR-012`](adr/ADR-012-dotnet10-server-runtime.md) | Runtime/language/framework | .NET 10 LTS / C# is the production server/core runtime; ASP.NET Core is the default HTTP server framework; frontend remains separate | ACCEPTED | Maintainability, productivity, ecosystem, support, completed spike evidence |
| `ADR-013` | Cloud/deployment provider | Determine provider/topology after residency, customer, resilience and cost requirements are clearer | DEFERRED | NFRs, compliance, economics |
| `ADR-014` | Search | Use relational/native search first vs dedicated search index based on validated search/load requirements | DEFERRED | Search UX, scale, operations |
| `ADR-015` | Analytics architecture | Begin from operational read/query models; add analytical store when workload/retention needs justify it | PROPOSED | REP/ANA requirements, isolation, cost |
| [`ADR-016`](adr/ADR-016-opentelemetry-observability-baseline.md) | Observability stack | OpenTelemetry traces/metrics/logs with OTLP preferred export, standard .NET diagnostics and separate business audit evidence | ACCEPTED | Supportability, performance, security diagnostics, vendor independence |
| [`ADR-017`](adr/ADR-017-module-owned-data-boundaries.md) | Schema/data modularity | Every authoritative data set has one owning module; cross-module behaviour uses explicit contracts and provider details stay behind infrastructure boundaries | ACCEPTED | Modular integrity, future extraction, maintainability |
| [`ADR-018`](adr/ADR-018-business-audit-evidence.md) | Audit/event evidence | Separate append-oriented authoritative business audit/evidence from technical telemetry; atomic evidence where required | ACCEPTED | NFR-AUD, accountability, integrity |
| `ADR-019` | Migration architecture | Treat migration as product/architecture capability with staging, mapping, validation and reconciliation | PROPOSED | BR-013, implementation repeatability |
| [`ADR-020`](adr/ADR-020-release-schema-configuration-evolution.md) | Release/config/schema evolution | Expand/migrate/contract; ordered migration journal; separate migration execution; rollback only within compatibility/recovery rules | ACCEPTED | Operability, customer upgrades, integrity |
| [`ADR-021`](adr/ADR-021-postgresql18-primary-provider.md) | Initial relational database provider | PostgreSQL 18 current supported minor as initial primary provider; Npgsql 10.0.3 for .NET; provider-specific features behind infrastructure boundaries | ACCEPTED | Proven spike evidence, tenant RLS, history constraints, support horizon |
| [`ADR-022`](adr/ADR-022-canonical-party-organisation-identity.md) | Canonical enterprise Party identity | Tenant is separate from Party; canonical Party kinds are Person and Organisation; implement tenant-isolated Organisation identity first | ACCEPTED | Accepted enterprise model, DATA-001–003, EP-004, commercial/workforce prerequisites |

## Accepted production foundation

The accepted decisions now establish:

```text
cohesive modular application
+ module-owned data / persistence boundaries
+ transactional relational authoritative persistence
+ transactional durable asynchronous intents / outbox-or-equivalent
+ PostgreSQL 18 initial primary provider
+ layered tenant-aware logical data model
+ .NET 10 LTS / C# server and core runtime
+ federated Principal identity separate from Person
+ contextual business authority separate from technical permission
+ HTTPS/JSON/OpenAPI remote API standard
+ OpenTelemetry/OTLP observability boundary
+ append-oriented business audit/evidence
+ expand/migrate/contract release and schema evolution
+ canonical enterprise Party identity: Person / Organisation, separate from Tenant and contextual business roles
```

These decisions deliberately do **not** select an identity-provider vendor, frontend framework, cloud provider, message-broker product, monitoring backend/vendor or final enterprise authority-source taxonomy.

ADR-022 also deliberately does not implement Person, Work Relationship, Position or customer/supplier relationship-role lifecycles in DEV-308; those remain separately governed increments.

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
IMPLEMENTATION FOUNDATION ACCEPTED:
ADR-001 decomposition
ADR-002 persistence model
ADR-003 durable asynchronous consequences
ADR-007 tenant isolation
ADR-008 identity boundary
ADR-009 business authority boundary
ADR-011 API standards
ADR-012 .NET 10 runtime
ADR-016 observability
ADR-017 data modularity
ADR-018 audit/evidence
ADR-020 release/schema evolution
ADR-021 PostgreSQL 18 provider
ADR-022 canonical Party / Organisation identity
        ↓
PRODUCTION IMPLEMENTATION:
API host + Principal/Tenant context
PostgreSQL persistence/migrations
OpenTelemetry telemetry
business audit boundary
contextual authority evaluation
durable consequence/outbox processing
canonical Organisation Party foundation
complete CI quality gates
```

Remaining domain/process decisions such as ADR-005, ADR-010, ADR-015 and ADR-019 are promoted as the relevant product slice requires them.

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
- `../../docs/01-enterprise-model.md`
- `../../docs/07-wave-1-enterprise-identity-reconciliation.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Established stable ADR register with first 20 proposed/deferred decisions |
| 0.2 | 2026-09-26 | NuBlox Architecture | Accepted ADR-001 cohesive modular application, ADR-002 relational primary persistence model and ADR-007 layered tenant isolation |
| 0.3 | 2026-09-26 | NuBlox Architecture | Accepted ADR-012 .NET 10 LTS/C# as the production server/core runtime and ASP.NET Core as the default server HTTP framework |
| 0.4 | 2026-09-26 | NuBlox Architecture | Accepted ADR-008 federated application identity, ADR-011 HTTP API standards and ADR-017 module-owned data boundaries |
| 0.5 | 2026-09-26 | NuBlox Architecture | Accepted ADR-016 OpenTelemetry observability, ADR-018 business audit evidence, ADR-020 release/schema evolution and ADR-021 PostgreSQL 18 provider |
| 0.6 | 2026-09-26 | NuBlox Architecture | Accepted ADR-009 contextual business authority with permission/authority separation and attributable authority references for governed decisions |
| 0.7 | 2026-09-26 | NuBlox Architecture | Accepted ADR-003 transactional durable asynchronous intents with recoverable claims, at-least-once delivery, idempotency and reconciliation |
| 0.8 | 2026-09-27 | NuBlox Architecture | Accepted ADR-022 to implement the canonical clean-slate Party/Organisation identity invariant and reject Tenant/employee/client/vendor Party-type shortcuts |
