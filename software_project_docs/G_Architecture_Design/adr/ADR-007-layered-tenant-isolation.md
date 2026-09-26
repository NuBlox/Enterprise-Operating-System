# ADR-007 — Customer / tenant isolation

**Section:** G_Architecture_Design  
**ADR:** ADR-007  
**Decision:** Customer / tenant isolation  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-001–FR-004, FR-005–FR-008, FR-026–FR-029, FR-032–FR-042  
**Related ADRs:** ADR-001, ADR-002, ADR-006, ADR-008, ADR-011, ADR-017, ADR-018, ADR-020  
**Evidence:** SPIKE-003 plus cross-customer checks throughout SPIKE-001, SPIKE-005, SPIKE-006, SPIKE-007 and SPIKE-008  
**Supersedes:** None  

## Context

NuBlox is a multi-tenant enterprise product. Tenant isolation must protect business information, relationships, work, decisions, integrations, configuration, reporting and background processing from accidental or malicious cross-tenant access.

The architecture spike proved one strong implementation option: a shared PostgreSQL schema with mandatory customer context, composite customer-scoped keys and row-level security under a restricted application role. Missing context failed closed, cross-customer reads returned no rows and wrong-customer writes were rejected.

The spike did not compare every physical deployment topology. NuBlox also needs room for customer-specific residency, contractual or operational isolation requirements. The architecture therefore needs a tenant-isolation invariant that survives provider and deployment choices rather than depending entirely on PostgreSQL RLS.

## Decision

NuBlox will use a **layered tenant-isolation architecture** with a **shared logical data model as the default product model** and a **dedicated-database deployment profile available where justified**.

Isolation is a product invariant, not a routing convention or UI feature.

### Mandatory logical rules

1. Every tenant-owned authoritative record carries an explicit immutable tenant identifier.
2. Tenant identity is resolved from trusted authenticated/application context; clients cannot select arbitrary tenant IDs for protected operations.
3. Tenant-owned relationships use tenant-aware keys/constraints so a relationship cannot silently cross tenant boundaries.
4. Data-access APIs require tenant context explicitly or through a verified request/job scope.
5. Missing tenant context fails closed for tenant-owned operations.
6. Background jobs, integrations, reporting and migrations carry tenant context with the same isolation requirements as interactive requests.
7. Privileged platform/support access is a separate audited capability; it must not bypass tenant controls implicitly.
8. Tenant slug/routing is an external addressing mechanism only. The stable tenant identifier remains the internal isolation key.

## Physical deployment profiles

### Profile A — shared database / shared logical schema

This is the default SaaS efficiency profile where legal, regulatory and customer requirements permit it.

Required controls include:

- tenant key on every tenant-owned row;
- tenant-aware uniqueness and foreign keys where relationships require them;
- mandatory scoped data-access APIs;
- server-side access and authority evaluation;
- database-enforced row/write isolation when the selected provider offers a robust capability suitable for NuBlox;
- negative cross-tenant tests in CI.

The PostgreSQL spike demonstrated this profile using RLS successfully.

### Profile B — dedicated database per tenant

NuBlox may deploy the same logical product schema in a tenant-dedicated database where justified by:

- regulatory or contractual isolation;
- data residency/sovereignty;
- customer-controlled backup/restore requirements;
- unusually large workload or maintenance window;
- enterprise risk/commercial requirement.

The tenant key remains in the logical model even in a dedicated database. Removing it would create divergent product schemas and weaken portability, restore/export and cross-profile verification.

### Schema-per-tenant

Not selected as the primary isolation profile.

It provides more physical separation than a shared schema but creates migration, connection/search-path, reporting and operational fan-out complexity while retaining a shared database failure domain. It may be reconsidered only if a concrete hosting requirement shows it materially outperforms Profiles A and B.

## Why hybrid rather than one topology

A single mandatory database-per-tenant model would maximise physical separation but create high operational cost for a SaaS product with many tenants. A shared-only model would be economically efficient but could exclude customers with stronger isolation/residency requirements.

The accepted architecture therefore separates:

```text
one logical tenant-aware product model
        ↓
multiple controlled physical isolation profiles
```

This allows NuBlox to preserve one implementation model while varying deployment isolation when the commercial/security case requires it.

## Database-provider independence

Tenant isolation must not depend on a single database feature.

Where a provider supports robust row-level security, NuBlox should use it as an additional database enforcement layer. Where it does not, equivalent protection must be provided through restricted database identities, generated/scoped repository queries, tenant-aware schema constraints and exhaustive negative tests.

Provider selection must explicitly demonstrate how Profile A meets this ADR before it can be production-approved.

## Evidence

SPIKE-003 demonstrated with PostgreSQL 18:

- a restricted non-superuser application role;
- own-tenant visibility;
- cross-tenant read isolation;
- missing-context fail-closed behaviour;
- wrong-tenant write rejection;
- layered protection using composite tenant keys plus RLS.

Other spikes also demonstrated tenant-scoped behaviour in durable work, configuration, migration and reporting. The evidence establishes that strong shared-schema isolation is technically credible, while the dedicated-database profile remains an architectural deployment option rather than a completed production proof.

## Alternatives considered

### Application-only tenant filters

Rejected.

Developer convention alone is insufficient for an enterprise multi-tenant system. Data relationships, persistence access and database capabilities must reinforce the application context.

### Database per tenant for every tenant

Rejected as the universal default.

It offers strong physical separation but increases provisioning, migration fan-out, connection management, backup/restore, observability and cost. It remains an approved deployment profile when requirements justify it.

### Shared schema with tenant ID but no database enforcement

Rejected as the preferred final posture when the chosen provider can enforce stronger controls.

Application/repository scoping remains mandatory, but database enforcement should be added where robustly available.

## Consequences

### Positive

- isolation semantics remain consistent across product modules;
- efficient shared SaaS operation remains possible;
- enterprise/dedicated isolation can be offered without a separate product codebase;
- tenant-scoped backup/export/migration semantics remain explicit;
- database security can add defence in depth without becoming the only boundary.

### Costs / risks

- every tenant-owned schema and query must preserve the tenant key correctly;
- CI needs systematic cross-tenant negative tests;
- supporting more than one physical profile increases deployment/migration testing;
- dedicated-database fleet operations require automation before being offered at scale;
- provider differences in row-security features must be addressed explicitly.

## Implementation implications

- tenant context becomes a first-class platform primitive;
- tenant slug lookup resolves to a stable internal tenant ID before protected application work;
- repository/query interfaces accept or inherit only validated tenant context;
- jobs/outbox messages persist tenant identity explicitly;
- composite tenant-aware relationship constraints are preferred where they protect invariants;
- privileged support tooling must use explicit audited elevation rather than unrestricted shared credentials;
- migration and deployment tooling must be capable of operating both shared and dedicated database profiles if/when Profile B is enabled.

## Verification requirements

Every production tenant-owned capability must include relevant negative tests for:

- guessed/modified identifiers from another tenant;
- missing tenant context;
- wrong tenant on write;
- background job with missing/wrong tenant;
- integration callback with ambiguous tenant mapping;
- report/export cross-tenant leakage;
- privileged access path and audit evidence.

## Review triggers

Review this ADR when:

- a selected database provider cannot meet the shared-profile enforcement requirements;
- customer/residency demand makes dedicated databases a common rather than exceptional profile;
- fleet scale changes the economics of shared versus dedicated persistence;
- cross-region tenancy or cell/shard architecture becomes necessary;
- regulatory requirements demand stronger physical separation.

## Decision outcome

**Accepted:** NuBlox uses one tenant-aware logical product model with layered isolation. Shared database/shared logical schema is the default profile; dedicated database per tenant is an approved deployment profile when justified. Database-level row security is defence in depth, not the sole tenant boundary.
