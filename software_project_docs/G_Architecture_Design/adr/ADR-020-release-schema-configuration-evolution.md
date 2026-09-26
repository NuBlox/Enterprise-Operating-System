# ADR-020 — Release, configuration and schema evolution

**Section:** G_Architecture_Design  
**ADR:** ADR-020  
**Decision:** Backward-safe application, schema and configuration evolution baseline  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering / Operations  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-036–FR-038; NFR-MNT-004; NFR-AVL-002–004; NFR-RES-001–003; NFR-INT-002  
**Related ADRs:** ADR-002, ADR-005, ADR-007, ADR-011, ADR-017, ADR-019  
**Evidence:** SPIKE-006, SPIKE-007, SPIKE-009  
**Supersedes:** None

## Context

NuBlox must evolve application code, relational schemas and tenant configuration without corrupting customer state or requiring unsupported per-tenant forks. The product also needs to support shared and dedicated database profiles under ADR-007.

Schema rollback is not always safe once production data has been transformed, so release design cannot rely on a simplistic assumption that every deployment can reverse arbitrary DDL/data changes instantly.

## Decision

NuBlox will use **backward-safe expand / migrate / contract evolution** with ordered, version-controlled migrations and explicit configuration compatibility.

Production database migrations are run as a controlled deployment step, not implicitly by every application process at startup.

## Release sequence

For material persistence/configuration changes, the preferred sequence is:

```text
1. EXPAND
   add backward-compatible schema/config capability
        ↓
2. DEPLOY compatible application
        ↓
3. MIGRATE / backfill / reconcile data
        ↓
4. VERIFY new state and operational evidence
        ↓
5. CONTRACT in a later controlled release
   remove obsolete schema/config only when no supported code depends on it
```

A single release must not normally add a new dependency on a schema element and simultaneously remove the prior representation needed by the previous compatible application version.

## Migration rules

1. All production schema/data migrations are version-controlled and ordered.
2. Every migration has an owning module/platform capability under ADR-017.
3. Migrations are idempotent only where explicitly designed; otherwise the migration journal prevents accidental duplicate execution.
4. Destructive operations require explicit impact/backup/recovery consideration and normally occur only after an earlier compatibility period.
5. Large data backfills are operational jobs with observability, resumability and reconciliation rather than one unbounded transaction where that would create unacceptable lock/recovery risk.
6. Migration credentials are separated from ordinary runtime credentials where the provider permits it.
7. Application startup validates required schema compatibility but does not silently mutate production schema.
8. Shared-database and dedicated-database tenant profiles use the same logical migration versions; fleet orchestration may differ.

## Migration journal

Each physical production database maintains an authoritative migration journal containing at least:

- migration identifier/version;
- checksum or immutable identity of the migration artefact;
- applied timestamp;
- application/tool identity applying it;
- success/failure state where useful for recovery;
- owning module where practical.

A changed migration file whose identifier has already been applied is a deployment error; create a new migration instead.

## Rollback and forward-fix

NuBlox distinguishes application rollback from data/schema reversal.

### Application rollback

Application rollback is permitted when the deployed schema/config remains compatible with the prior approved application version.

### Schema/data reversal

Automatic down migrations are not assumed safe for production.

For a material migration, the release plan states which recovery strategy applies:

- backward-compatible application rollback;
- compensating/forward-fix migration;
- data restore/recovery from backup/snapshot;
- explicit reversible migration where lossless reversal is proven.

Destructive irreversible transformations require verified recovery evidence before production execution.

## Configuration evolution

Tenant/customer configuration is versioned/governed data, not arbitrary executable code.

Configuration changes include:

- schema/version identity where needed;
- validation against typed product invariants;
- effective activation where business semantics require it;
- migration/defaulting rules when product versions add/change supported configuration;
- rejection of unsupported configuration rather than silently ignoring it.

Product releases must define the oldest/newest configuration versions they can safely read/write when more than one version may coexist during deployment.

## API compatibility

ADR-011 controls network contract compatibility. Release evolution must not bypass published API deprecation/version commitments merely because internal code/schema changed.

## Deployment compatibility window

The production system should support a bounded compatibility window between adjacent application versions sufficient for controlled rolling/restart deployment where the eventual deployment topology requires it.

The exact number of simultaneously supported versions is an operational release-policy decision. The default architecture assumption is **current and immediately previous compatible application during rollout**, not indefinite multi-version support.

## Feature activation

Where a change needs staged activation, feature/configuration flags may separate deployment from business activation.

Flags must have:

- owner;
- scope;
- default state;
- expiry/removal plan;
- tenant/security implications;
- test coverage for supported states.

Long-lived unmanaged flags are technical debt.

## Database-provider implications

Provider-specific DDL/locking/online-index behaviour is documented in migration implementation. Provider capabilities may influence the safest expansion/contraction mechanism, but the logical release rules remain the same.

## Alternatives considered

### Auto-migrate on every application startup
Rejected for production because concurrent instances can race, startup failure becomes schema mutation risk, and migration privilege must be granted to normal runtime identities.

### Always rely on down migrations
Rejected because many real data transformations/deletions are not losslessly reversible.

### Big-bang schema replacement
Rejected because it creates unnecessary downtime/risk and makes rollback substantially harder.

### Customer-specific code/schema forks
Rejected as a normal configuration strategy because they undermine upgradeability and product integrity.

## Consequences

### Positive
- supports safer rolling deployment and application rollback;
- migration ownership/checksums make schema state auditable;
- avoids runtime processes requiring broad DDL privileges;
- supports shared and dedicated database profiles;
- controlled config evolution reduces unsupported tenant forks.

### Costs / risks
- expand/contract changes often span multiple releases;
- temporary duplicate columns/representations may be necessary;
- backfill/reconciliation tooling is required for larger changes;
- release discipline is more demanding than startup auto-migration.

## Verification requirements

CI/deployment verification must include as applicable:

- clean database migration from zero/current baseline;
- upgrade from previous supported schema version;
- migration replay/journal/checksum protection;
- previous application compatibility during expand phase where rollback is claimed;
- backfill/reconciliation evidence;
- tenant configuration upgrade/validation;
- destructive-change recovery procedure where applicable.

## Decision outcome

**Accepted:** NuBlox uses controlled expand/migrate/contract evolution, ordered migration journals and separate production migration execution. Application rollback depends on schema/config compatibility; destructive data/schema changes use explicit forward-fix or recovery plans rather than assuming automatic down migrations.
