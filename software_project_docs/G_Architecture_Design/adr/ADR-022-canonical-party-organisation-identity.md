# ADR-022 — Canonical Party and Organisation identity

**Section:** G_Architecture_Design  
**ADR:** ADR-022  
**Decision:** Separate Tenant platform context from canonical Party identity; begin production Party implementation with Organisation  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-27  
**Owner:** NuBlox Architecture / Enterprise Model  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** BR-003, BR-004, BR-006; DATA-001–DATA-003; EP-004; DEV-308  
**Related ADRs:** ADR-001, ADR-002, ADR-007, ADR-008, ADR-017, ADR-018, ADR-020, ADR-021  
**Evidence:** `docs/01-enterprise-model.md`; `docs/07-wave-1-enterprise-identity-reconciliation.md` accepted clean-slate baseline  
**Supersedes:** V3 Party-Type shortcuts only; no prior Enterprise Operating System ADR

## Context

NuBlox needs stable enterprise identities before later commercial, workforce, project and relationship slices can refer to customers, suppliers, employers or other organisational participants without duplicating business identities inside each module.

The accepted enterprise-model reconciliation already establishes that a software Tenant is not a business Party and that labels such as employee, client/customer and vendor/supplier describe relationships or contextual roles rather than identity kinds.

DEV-308 is the first production implementation of that accepted conceptual baseline. This ADR records the implementation invariant before commercial or workforce modules depend on it.

## Decision

NuBlox will maintain the following canonical distinction:

```text
Tenant != Party

Party
├── Person
└── Organisation
```

A **Tenant** remains the NuBlox platform isolation, configuration and subscription context.

A **Party** is a stable enterprise participant identity.

The initial Party kinds are exactly:

- `PERSON`;
- `ORGANISATION`.

DEV-308 implements the **Organisation** branch only. Person, Work Relationship, Organisational Unit, Position, Occupancy and business relationship-role lifecycles remain separate future increments.

## Organisation identity

An Organisation is a stable organisational Party identity inside a verified Tenant context.

The first production representation requires:

- stable `PartyId`;
- verified `TenantId`;
- `PartyKind = ORGANISATION`;
- governed display name;
- attributable creation Principal;
- creation timestamp.

Later legal-entity identifiers, registrations, addresses, names, classifications and relationship roles may extend this identity through owned models; they must not require recreating the Organisation as a different Party merely because its business role changes.

## Business-role rule

The following are **not Party kinds**:

- Tenant;
- Employee;
- Client / Customer;
- Vendor / Supplier;
- Partner;
- Contractor.

Those meanings are represented through the appropriate platform context, Work Relationship or contextual business relationship when that capability is implemented.

For example, the same Organisation may later be both a customer and a supplier in different governed relationships while retaining one Organisation Party identity.

## Ownership and persistence

Canonical Party/Organisation data is owned by the Enterprise module under ADR-017.

The initial PostgreSQL profile uses module-owned `enterprise` schema tables with:

- `(TenantId, PartyId)` authoritative keys;
- database constraints for Party kind and basic Organisation integrity;
- transaction-local verified tenant propagation;
- PostgreSQL row-level security with forced tenant policies;
- ordered migrations under ADR-020/ADR-021.

Other modules reference stable enterprise identifiers through contracts; they do not duplicate authoritative Organisation records as local customer/vendor master identities.

## Access and request context

External route/body/query values never establish trusted Party ownership or tenant access.

Organisation operations use the verified `AuthenticatedRequestContext` established by ADR-007/ADR-008. DEV-308 uses a bounded, configuration-backed Organisation administrator list that **denies by default**. It is an interim technical access policy and must not be mistaken for the final organisational responsibility/authority model.

## Audit

Material Organisation creation appends authoritative audit evidence under ADR-018 using stable identifiers and actor context. Arbitrary request payloads such as the Organisation display name are not copied into the audit contract merely for convenience.

## Alternatives considered

### Treat Tenant as an Organisation/Party

Rejected. A Tenant is a software/customer isolation and configuration boundary; one Tenant may contain multiple Organisations and legal entities.

### Preserve employee/client/vendor/supplier as Party types

Rejected. Those labels describe how a Person or Organisation participates in a relationship/context. Encoding them as identity kinds creates duplicates and prevents one Party from holding multiple simultaneous roles.

### Let each business module create its own customer/supplier identity

Rejected. It would fragment authoritative identity and force later commercial/project/workforce reconciliation between duplicates.

### Implement the complete enterprise/workforce model in one increment

Rejected. Person, Work Relationship, Position and relationship lifecycles have distinct semantics and controls. DEV-308 deliberately proves only the smallest canonical Organisation identity needed as a prerequisite for later slices.

## Consequences

### Positive

- later commercial/project/workforce modules can reference a stable Organisation identity;
- changing business roles does not duplicate enterprise identity;
- Tenant/platform concerns remain separate from real-world Organisation semantics;
- module ownership and tenant isolation are explicit from the first implementation;
- production code now aligns with the accepted clean-slate enterprise model rather than V3 shortcuts.

### Costs / risks

- relationship roles require explicit future models rather than convenient type flags;
- duplicate/matching/merging rules for Organisations remain future work;
- legal-entity and reference-data semantics remain intentionally incomplete;
- configuration-backed administrator access must later be replaced or enriched by governed responsibility/authority evidence.

## Verification requirements

DEV-308 must prove:

- Organisation creation produces a stable non-empty `PartyId` and `PartyKind.Organisation`;
- invalid/blank Organisation names fail validation;
- creation is denied without configured administration access;
- verified Tenant/Principal context overrides any untrusted body identifiers;
- Tenant B cannot read Tenant A Organisation records;
- a restricted runtime database role cannot bypass tenant RLS by supplying another TenantId;
- Party kind is persisted as `ORGANISATION`;
- material creation writes attributable, tenant-scoped audit evidence without copying display-name payload;
- the production HTTP → application → PostgreSQL composition resolves and executes without replacing the real operation boundary.

## Review triggers

Review this ADR when:

- Person identity enters production;
- customer/supplier/partner relationship roles are implemented;
- Organisation matching/merging or global/shared Party identity is required;
- legal-entity/jurisdiction requirements materially change Organisation semantics;
- a Tenant must participate in cross-tenant enterprise relationships;
- authoritative enterprise identity is moved to or reconciled with an external master-data service.

## Decision outcome

**Accepted:** Tenant remains separate from Party identity; canonical Party kinds are Person and Organisation; DEV-308 implements tenant-isolated Organisation Party identity first, while employee/client/vendor/supplier semantics remain explicit relationships or contextual roles rather than Party types.
