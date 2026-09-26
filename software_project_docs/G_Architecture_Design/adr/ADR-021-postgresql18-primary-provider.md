# ADR-021 — Initial relational database provider

**Section:** G_Architecture_Design  
**ADR:** ADR-021  
**Decision:** Initial production relational database provider/profile  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Data Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-005–FR-008; NFR-DATA-001–004; NFR-SEC-001–002; NFR-RES-001–003; NFR-PORT-002  
**Related ADRs:** ADR-002, ADR-007, ADR-012, ADR-017, ADR-019, ADR-020  
**Evidence:** `Evaluation_matrix_for_technology_selection.md`; SPIKE-001 through SPIKE-009; PostgreSQL 18 current support/documentation  
**Supersedes:** None

## Context

ADR-002 accepts transactional relational persistence as the authoritative model while deliberately leaving product/provider selection separate. ADR-007 selects a tenant-aware logical model with a shared-database/shared-schema default profile and dedicated-database option. ADR-017 requires module-owned data boundaries and permits provider-specific capabilities behind infrastructure adapters.

The architecture spike programme used PostgreSQL 18 and directly exercised:

- atomic relational transactions;
- composite tenant-aware keys and foreign keys;
- effective-dated history;
- exclusion constraints preventing overlapping effective periods;
- row-level security for shared-schema tenant isolation;
- schemas/namespaces;
- deterministic ordered migrations;
- migration staging/reconciliation;
- reporting/query-plan evidence;
- durable outbox/work recovery.

A production provider should maximise proven integrity/security capability while preserving the possibility of future provider support where a validated product requirement exists.

## Decision

NuBlox will use **PostgreSQL 18** as the **initial production primary relational database provider**.

The initial supported server baseline is the current PostgreSQL 18 minor release and will follow PostgreSQL's supported-minor servicing policy rather than pinning indefinitely to one minor version.

As of 2026-09-26:

- current PostgreSQL 18 minor: `18.6`;
- PostgreSQL 18 support end: `2030-11-14`;
- PostgreSQL 19 is still Beta 4 and is not approved for production.

The .NET provider baseline is **Npgsql `10.0.3`**, currently the stable Npgsql release compatible with `net10.0`.

## Why PostgreSQL 18

### NuBlox-specific evidence

PostgreSQL is the only database provider currently exercised across all nine NuBlox architecture spikes. The selection therefore relies on measured NuBlox evidence rather than capability comparison alone.

### Tenant isolation

PostgreSQL row-level security provides database-enforced defence in depth for the ADR-007 shared-database/shared-schema profile. Policies can scope reads and writes and can fail closed when no applicable policy permits access.

Application-level tenant context remains mandatory; RLS is an additional enforcement layer, not a replacement for ADR-007.

### Effective history/integrity

PostgreSQL exclusion constraints directly support the non-overlap invariant already proven in SPIKE-002 for effective-dated facts.

The database also provides mature foreign keys, unique/check constraints, transactions and indexes required by ADR-002.

### Module ownership

PostgreSQL schemas provide a useful physical namespace for module-owned persistence under ADR-017. Schema privileges can strengthen ownership where practical.

The application architecture remains provider-neutral at domain/application boundaries; PostgreSQL-specific SQL/features live in infrastructure/migration code.

### .NET integration

Npgsql provides the approved .NET 10 application with direct ADO.NET PostgreSQL access and integrates with .NET logging/diagnostic patterns.

NuBlox does not require an ORM to adopt PostgreSQL. ORM selection, if any, is a separate implementation dependency decision.

## Provider profile

### Shared SaaS profile

Default initial SaaS profile:

```text
PostgreSQL cluster/database
  -> module-owned schemas/namespaces
  -> tenant-aware tables
  -> tenant-aware keys/constraints
  -> RLS where applicable
  -> restricted runtime role
  -> separate migration/admin role
```

### Dedicated tenant profile

Where ADR-007 requires a dedicated database, the same logical migrations/module ownership model is applied to the tenant-dedicated PostgreSQL database.

The logical tenant key remains present even in a dedicated database so the product model and verification remain consistent.

## Runtime roles

At minimum production distinguishes:

- **runtime role** — least-privilege DML required by the application; no broad schema-DDL rights;
- **migration role** — controlled DDL/migration capability used by deployment tooling;
- **administrative/operator role** — separately controlled and audited; not used by normal application traffic.

Exact role grants are defined with the production schema.

## Version policy

NuBlox follows a supported-major/current-minor policy:

- production stays on a supported PostgreSQL major;
- current security/fix minor releases are evaluated and adopted promptly;
- minor upgrades do not change the NuBlox logical schema version;
- major PostgreSQL upgrades follow a controlled compatibility, backup/restore and rehearsal plan under ADR-020;
- pre-release PostgreSQL versions are not used for production.

PostgreSQL 19 may be evaluated after general availability but does not replace this ADR automatically.

## Npgsql policy

Npgsql is isolated to PostgreSQL infrastructure/persistence projects. Domain/application/kernel projects must not directly reference Npgsql.

Direct version is centrally controlled in `Directory.Packages.props` once production persistence code is introduced.

The initial baseline is `Npgsql 10.0.3`; updates follow dependency/security review and production verification.

## Alternatives considered

### MySQL 8.4 LTS

MySQL remains a credible relational database and NuBlox maintains `packages/mastered/mysql` for governed Node.js MySQL client capability.

It is not selected as the initial primary provider because:

- the full NuBlox spike programme has direct PostgreSQL evidence, not equivalent MySQL evidence;
- PostgreSQL's native row-level security directly supports the selected shared-schema tenant defence-in-depth design;
- PostgreSQL exclusion constraints directly support the effective-dated non-overlap invariant already proven in NuBlox;
- changing providers now would add risk without a validated customer/product requirement.

The mastered MySQL package remains useful for integrations, tooling or future provider support. It does not imply primary-database selection.

### PostgreSQL 19 pre-release

Rejected for production because PostgreSQL 19 remains beta as of this decision and the PostgreSQL project explicitly advises against beta use in production.

### Database-per-tenant only

Rejected by ADR-007 as the universal default. PostgreSQL supports both shared and dedicated profiles.

### Multi-provider production abstraction from day one

Rejected.

NuBlox will not build/maintain multiple authoritative database implementations without a validated market/customer requirement. The architecture preserves provider seams, but PostgreSQL 18 is the one production provider initially implemented and tested.

## Backup, recovery and availability

Provider-specific production design must define:

- automated backups/PITR appropriate to approved RPO/RTO;
- restore testing;
- HA/failover topology for the chosen hosting environment;
- encryption in transit and at rest per environment/security controls;
- capacity/connection/pooling limits;
- maintenance/minor-upgrade process.

This ADR selects the database technology, not a particular cloud/managed PostgreSQL vendor.

## Portability

NuBlox accepts intentional PostgreSQL-specific capabilities where they materially improve integrity/security/operability.

Portability is preserved at product/module contract boundaries and through explicit migration/data-export capability, not by avoiding valuable database features.

If a future customer requirement mandates another relational provider, NuBlox will implement a tested provider adapter/migration profile rather than weakening the PostgreSQL implementation pre-emptively.

## Consequences

### Positive
- directly reuses the architecture programme's database evidence;
- strong native isolation/integrity features align to accepted ADRs;
- current supported major has a long support window;
- mature .NET provider available;
- one initial provider keeps testing/migration/operations tractable.

### Costs / risks
- PostgreSQL-specific migrations/features create provider-switching cost;
- RLS/schema privileges require careful role configuration and testing;
- major PostgreSQL upgrades require rehearsed operational work;
- customers requiring another database cannot be promised support until an explicit provider implementation exists.

## Verification requirements

Production persistence CI/integration verification must include:

- PostgreSQL `18.x` service using a supported current minor;
- migrations from empty database and previous supported baseline;
- runtime role cannot perform prohibited DDL/privileged actions;
- cross-tenant reads/writes fail as required;
- effective-history non-overlap constraint tests;
- module schema/ownership checks;
- Npgsql/application build against the centrally pinned supported provider version;
- migration replay/journal/reconciliation checks under ADR-020.

## References

- PostgreSQL versioning policy: https://www.postgresql.org/support/versioning/
- PostgreSQL 18 documentation: https://www.postgresql.org/docs/18/
- PostgreSQL constraints: https://www.postgresql.org/docs/18/ddl-constraints.html
- PostgreSQL row security: https://www.postgresql.org/docs/18/ddl-rowsecurity.html
- Npgsql: https://www.nuget.org/packages/Npgsql

## Decision outcome

**Accepted:** PostgreSQL 18 is the initial NuBlox production primary relational database provider, maintained on a current supported 18.x minor release. Npgsql 10.0.3 is the initial .NET provider baseline. Provider-specific capabilities are deliberately used behind module infrastructure boundaries; MySQL and other providers remain future explicit product decisions rather than lowest-common-denominator targets.
