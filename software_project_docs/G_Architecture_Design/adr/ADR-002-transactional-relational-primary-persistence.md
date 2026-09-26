# ADR-002 — Primary persistence model

**Section:** G_Architecture_Design  
**ADR:** ADR-002  
**Decision:** Primary persistence model  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-005–FR-008, FR-016, FR-022, FR-035, FR-037–FR-042  
**Related ADRs:** ADR-001, ADR-004, ADR-007, ADR-015, ADR-017, ADR-018, ADR-019, ADR-020  
**Evidence:** SPIKE-001, SPIKE-002, SPIKE-003, SPIKE-006, SPIKE-007, SPIKE-008, SPIKE-009  
**Supersedes:** None  

## Context

NuBlox must preserve authoritative enterprise records, relationships, business state, approvals, effective history, migration identity, audit evidence and management views. These requirements need strong integrity, explicit transactions, predictable querying and controlled evolution.

The architecture spikes demonstrated relational constraints, composite relationship integrity, effective-dated history, deterministic migrations, row-scoped isolation, typed configuration, migration reconciliation and operational reporting. The evidence supports a relational model as the authoritative system-of-record foundation.

This ADR deliberately separates the **persistence model** from the **database product/vendor**. Successful PostgreSQL spike evidence does not by itself approve PostgreSQL as the only or final supported provider, and the existence of NuBlox-mastered MySQL capability does not by itself make MySQL the primary database.

## Decision

NuBlox will use **transactional relational persistence as the primary authoritative persistence model** for governed business state.

### Core rules

1. **Relational state is authoritative for core governed records and relationships.**
2. **Business invariants belong in both application behaviour and database constraints where practical.**
3. **Module-owned persistence boundaries follow ADR-001/ADR-017.**
4. **Transactions are explicit and aligned to business consistency boundaries.**
5. **Stable identifiers are separated from mutable/effective-dated facts where historical reconstruction requires it.**
6. **Relationships use explicit keys and constraints; opaque embedded documents must not replace relationships that require referential integrity.**
7. **Structured extension/configuration data may use JSON or equivalent provider features only behind typed validation and governed ownership.**
8. **Binary work products/content are not stored in relational rows by default; metadata/authority/state remain relational while binary/object content follows ADR-004.**
9. **Operational read models may be relational projections/views where they satisfy the workload; analytical stores are introduced only when ADR-015 evidence requires them.**
10. **Provider-specific capabilities may be used behind explicit persistence boundaries when they materially improve integrity, performance or operations. Portability must not force NuBlox to the lowest common SQL denominator.**

## Logical persistence shape

```text
stable business identity
        │
        ├── current governed state
        ├── effective-dated facts/history
        ├── explicit relationships
        ├── work / decision state
        ├── audit/evidence references
        └── integration/configuration references

binary/object content ── separate content store where appropriate
analytics/search      ── derived/specialised stores only when justified
```

## Alternatives considered

### Document database as the primary store

Rejected for the authoritative core.

Document stores can be valuable for selected workloads, but the current NuBlox requirements place unusually high value on relationship integrity, cross-record constraints, transactional consistency, effective history, reporting and migration reconciliation. Those needs are a stronger default fit for relational persistence.

### Graph database as the primary store

Rejected as the default authoritative store.

NuBlox will contain rich relationships, but relationship richness alone does not justify moving transactional records, approvals, state and financial/commercial continuity into a graph-first persistence model. Graph technology may later support specialised traversal/search workloads if evidence demonstrates value.

### Event sourcing as the universal primary model

Rejected.

NuBlox requires audit/history and may use events, but universal event sourcing would add reconstruction, schema evolution and operational complexity that the current requirements do not justify. Material business events/evidence remain first-class without making every domain aggregate event-sourced.

### Polyglot persistence from the outset

Rejected as a default strategy.

Specialised stores can be introduced when a bounded workload demonstrates the need. Starting with multiple authoritative persistence technologies would increase consistency, backup, migration, security and operational complexity before evidence justifies it.

## Evidence

The spike programme demonstrated:

- atomic cross-module writes and rollback;
- composite keys preventing invalid cross-customer relationships;
- effective-from/effective-to history distinct from technical recording time;
- overlap prevention through database constraints;
- customer isolation using database policies in the PostgreSQL candidate;
- typed effective configuration with mandatory invariants;
- deterministic migration staging/reconciliation;
- governed operational reporting and drill-through;
- repeatable ordered migration execution in CI.

This evidence validates the relational persistence **model**. It does not settle the final production provider, hosting topology or every schema pattern.

## Provider strategy

The production provider decision must consider:

- integrity features required by the accepted logical model;
- tenant-isolation capabilities and ADR-007;
- migration/upgrade tooling;
- supported runtime/data libraries;
- operational maturity, backup/restore and HA/DR;
- customer hosting/residency expectations;
- cloud portability and managed-service availability;
- licensing/support economics;
- performance under representative NuBlox workloads.

NuBlox may support multiple SQL dialects/providers where a validated product requirement exists. Provider support must be explicit and tested; it must not emerge accidentally through ad-hoc SQL branches.

`packages/mastered/mysql` is a governed implementation asset for MySQL connectivity/capability. Its presence is independent of the primary-provider decision.

## Consequences

### Positive

- strong integrity and transaction semantics for enterprise records;
- mature query/reporting capability;
- clear migration/reconciliation patterns;
- explicit history and relationship modelling;
- broad runtime/provider tooling ecosystem;
- supports both cohesive modular transactions and future service extraction boundaries.

### Costs / risks

- schema evolution requires disciplined migrations;
- poorly governed shared schemas could undermine module ownership;
- provider-specific features can create portability cost;
- large analytical/search workloads may eventually require specialised stores;
- effective/history modelling must be applied intentionally rather than automatically to every table.

## Implementation implications

- production schema is created fresh from approved product semantics; spike migrations are not copied wholesale;
- module persistence ownership is documented and testable;
- migrations are ordered, repeatable and controlled by ADR-020;
- SQL-provider abstraction is introduced only at a boundary that preserves integrity and observability;
- direct cross-module table writes are prohibited;
- audit/event evidence is designed separately from technical logs under ADR-018.

## Review triggers

Review this ADR if:

- validated workloads cannot meet required scale/latency with the relational design;
- a domain has a demonstrated graph/document/event-sourced workload that materially benefits from specialised persistence;
- regulatory/customer deployment requirements force different authoritative-store characteristics;
- multi-provider support materially compromises integrity or delivery velocity.

## Decision outcome

**Accepted:** transactional relational persistence is NuBlox's primary authoritative persistence model. Database product/provider selection remains a separate controlled implementation decision.
