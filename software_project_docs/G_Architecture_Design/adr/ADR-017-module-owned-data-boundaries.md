# ADR-017 — Schema and data modularity

**Section:** G_Architecture_Design  
**ADR:** ADR-017  
**Decision:** Module-owned data and persistence boundaries  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Data Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-005–FR-008, FR-032–FR-042; data/history/isolation NFRs  
**Related ADRs:** ADR-001, ADR-002, ADR-003, ADR-007, ADR-015, ADR-018, ADR-019, ADR-020  
**Evidence:** SPIKE-001/002/003/006/007/008/009; accepted modular-application and relational-persistence decisions  
**Supersedes:** None  

## Context

NuBlox begins as a cohesive modular application (ADR-001) using transactional relational persistence for authoritative state (ADR-002). A shared deployment and relational database can simplify strong transactions, but it also creates a major architectural risk: modules can become nominal folders while arbitrary code reads and writes any table.

Data ownership therefore has to be explicit and enforceable enough that the initial modular application can evolve without becoming a shared-table monolith.

This ADR defines **logical ownership and access rules**. It deliberately avoids selecting the physical database product or assuming that every provider supports PostgreSQL-style schemas/roles.

## Decision

Every authoritative production table/collection of relational records has **one owning NuBlox module or platform capability**.

Only the owner may perform unrestricted writes to its authoritative data. Other modules collaborate through the owner's typed application contracts, read contracts or explicitly governed shared read projections.

### Core rule

```text
module behaviour
    ↓
module-owned persistence adapter
    ↓
module-owned authoritative data

other module
    ── typed contract ──> owner
    NOT arbitrary write ──> owner's tables
```

## Ownership model

For every persisted object/table the implementation must be able to answer:

- which module/platform capability owns it?;
- who may create/update/delete it?;
- which module contract exposes required behaviour?;
- which consumers may read it directly, if any?;
- which tenant/isolation rules apply?;
- which migration set owns its schema evolution?;
- which audit/evidence obligations apply?;

A table without an owner is an architecture defect.

## Physical namespace strategy

The logical design uses module-owned database namespaces.

The physical representation depends on the selected relational provider:

- PostgreSQL may map modules to schemas;
- another provider may use database/schema/catalog conventions, table prefixes, permissions or another explicit namespace mechanism.

The invariant is ownership and controlled access, not a provider-specific keyword.

Provider mappings must remain visible and deterministic in migration/configuration code.

## Persistence adapters

Product modules do not issue arbitrary SQL through a global database helper.

Each module owns its persistence adapter/repository/query implementation behind module contracts.

Shared persistence infrastructure may provide technical primitives such as:

- connection/transaction creation;
- tenant-context propagation;
- command execution abstraction;
- migration orchestration;
- tracing/metrics;
- provider capability abstraction.

It must not become a universal repository that knows every module table or business rule.

## Transactions across modules

ADR-001 permits an application use case to coordinate one relational transaction across multiple module-owned writes when a business invariant genuinely requires atomic consistency.

The transaction coordinator may provide the shared transaction/connection, but it does not grant Module A permission to mutate Module B's tables directly.

Correct pattern:

```text
Application use case
  ├─ Module A command/API -> A persistence
  ├─ Module B command/API -> B persistence
  └─ one explicit transaction boundary
```

Rejected pattern:

```text
Module A repository
  -> UPDATE module_b_table ...
```

## Cross-module relationships and foreign keys

Cross-module relationships are permitted when they represent a real invariant, but their physical constraint strategy must be explicit.

A cross-module foreign key may be used when:

- both modules share the same transactional datastore/profile;
- the relationship is stable and materially benefits from database integrity enforcement;
- the owning modules agree on lifecycle semantics;
- the constraint does not grant write ownership to the referencing module;
- extraction implications are understood.

Cross-module cascading deletes/updates are prohibited by default because they allow one module's persistence operation to mutate another module's authoritative state invisibly.

Where later service extraction or dedicated storage makes a physical foreign key impossible, the same business relationship must retain application-level validation/reconciliation and durable failure handling appropriate to its consistency needs.

## Tenant keys

ADR-007 applies to every tenant-owned module.

Tenant identity is part of the logical key/security context for tenant-owned data. Module ownership never permits bypassing tenant isolation.

Cross-module tenant-owned relationships must not permit an identifier from one tenant to reference an object belonging to another tenant.

The selected provider should enforce tenant-aware relationships/row isolation at the database layer where practical, in addition to application enforcement.

## Read access

### Module contracts

The preferred cross-module read path is an explicit typed query/read contract owned by the source module.

### Shared operational read models

A controlled read model may combine data from multiple modules for reporting, search, dashboards or management views where ADR-015 permits it.

Such a read model is derived/non-authoritative unless explicitly designated otherwise. It must not become a backdoor write model.

### Direct cross-module SQL reads

Direct reads of another module's authoritative tables are prohibited by default in product code.

A narrowly approved read-only dependency may exist for performance/reporting/migration reasons when documented, but it must have:

- named owner and consumer;
- stable contract/view/projection where practical;
- tenant/security enforcement;
- compatibility responsibility;
- extraction/migration consequence understood.

## Shared reference data

Truly platform-wide reference definitions may live in a shared platform-owned module rather than being duplicated into each business module.

A concept is not "shared" merely because many modules use it. The shared owner still has an explicit contract and lifecycle.

## Migrations

Each migration has an owning module/platform capability.

The repository may execute migrations through one global ordered orchestrator, but migrations remain logically grouped by owner and must not casually rewrite another module's tables.

Cross-module migration steps that are necessary for a coordinated product change require an explicit migration/reconciliation plan under ADR-019/ADR-020.

## Database roles and permissions

Where the selected provider permits practical enforcement, NuBlox should strengthen module ownership with database permissions/roles, separate migration credentials and restricted runtime credentials.

The initial cohesive application may need one runtime connection to preserve atomic cross-module transactions. Even then, application code structure, persistence registrations and automated architecture tests must enforce ownership so the database credential does not become licence for arbitrary writes.

## Provider abstraction

NuBlox will not build a universal SQL dialect abstraction before a real multi-provider requirement is implemented.

Provider-specific persistence may use capabilities such as row-level security, exclusion constraints, JSON types, generated columns or provider-native indexing when those improve correctness/operations.

Provider differences are isolated in infrastructure/persistence implementations rather than leaking into domain/application contracts.

The existence of `packages/mastered/mysql` does not require .NET production modules to depend on it or force lowest-common-denominator SQL.

## Alternatives considered

### One shared repository/data-access layer for all tables

Rejected.

It would centralise technical code at the cost of destroying module data ownership and making every business change coupled to one shared persistence model.

### No cross-module database constraints ever

Rejected as an absolute rule.

The initial cohesive application intentionally values strong integrity. Stable cross-module constraints may be appropriate when they protect a material invariant, provided ownership and future extraction consequences are explicit.

### Separate database per module immediately

Rejected as the default.

It would prevent simple atomic transactions, increase operational/migration burden and create service-like distribution before module boundaries have enough production evidence.

Separate storage remains a future extraction option where scale, resilience, regulation or service independence justify it.

### Provider-neutral lowest-common-denominator SQL

Rejected.

NuBlox values correctness and operability more than superficial portability. Provider-specific capabilities may be used behind explicit adapters while application contracts remain provider-neutral.

## Automated enforcement

The production codebase should evolve architecture tests that verify:

- module projects cannot reference prohibited implementation/persistence projects;
- domain/application projects do not reference database provider packages;
- modules do not reference `spikes/`;
- provider packages live only in infrastructure/persistence projects;
- known cross-module dependencies match the architecture dependency map.

Database integration tests verify tenant and relationship constraints independently of application checks.

## Consequences

### Positive

- protects modular architecture while retaining one deployment/database where useful;
- preserves strong local transactions;
- makes future service/storage extraction possible from explicit seams;
- prevents accidental shared-table ownership;
- keeps provider technology out of core product contracts.

### Costs / risks

- modules need explicit query/command contracts rather than convenient arbitrary joins/writes;
- some reporting/query workloads require projections/read models;
- architecture tests and migration governance add discipline overhead;
- cross-module foreign keys must be assessed case by case rather than applied mechanically.

## Implementation implications

The production repository may now introduce module/project layering such as:

```text
NuBlox.<Module>.Domain
NuBlox.<Module>.Application
NuBlox.<Module>.Infrastructure
```

where justified by module complexity, without requiring every module to use identical project count.

The first persistence provider scaffold must include:

- explicit module ownership;
- controlled transaction primitive;
- tenant-context enforcement;
- migration ownership;
- architecture tests preventing spike/provider dependency leakage into the kernel/domain layer.

## Review triggers

Review this ADR when:

- independently deployed services are extracted;
- a module moves to separate storage;
- multi-provider persistence becomes a committed product requirement;
- cross-module query performance forces new read-model architecture;
- database permission/runtime constraints prevent the desired ownership enforcement.

## Decision outcome

**Accepted:** NuBlox uses module-owned authoritative data and persistence adapters inside the cohesive modular application. Cross-module behaviour goes through explicit contracts; unrestricted cross-module reads/writes are prohibited; strong relational constraints may cross modules only when explicitly justified, and provider-specific capabilities remain behind infrastructure boundaries.
