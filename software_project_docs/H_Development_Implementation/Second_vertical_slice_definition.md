# Second vertical slice definition

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-006  
**Document Type:** Vertical slice definition  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering  
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
**Related Documents:** `Product_backlog.md`, `../F_Requirements_Analysis/Epics.md`, `../F_Requirements_Analysis/Data_requirements.md`, `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`, `../../docs/01-enterprise-model.md`, `../../docs/07-wave-1-enterprise-identity-reconciliation.md`  
**Supersedes:** None  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/Second_vertical_slice_definition.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define a bounded second production increment that establishes a trustworthy Organisation Party identity before later commercial, project, workforce or relationship workflows depend on customer/supplier/employer identifiers.

This slice is an **engineering/product-model prerequisite**, not evidence that primary customer research has selected commercial continuity or any other Wave-3 capability as the approved market priority.

## Selected bounded outcome

**Governed Organisation Party — Register and Retrieve**

Primary controlled drivers:

- `EP-004 — Governed business subjects`;
- `DATA-001 — stable identifiers`;
- `DATA-002 — explicit material relationships`;
- `DATA-003 — authoritative ownership/source`;
- canonical clean-slate enterprise model;
- `ADR-022 — Canonical Party and Organisation identity`.

## Why this increment comes next

The first vertical slice proved controlled Work Product execution but deliberately used only Tenant/Principal identities. The next likely end-to-end differentiator is continuity between delivery work and commercial/project context. That continuity requires a stable enterprise identity for the organisations participating as customers, suppliers, partners, employers or other parties.

Creating separate `CustomerId`, `SupplierId` or module-local organisation records before a canonical Party identity exists would create reconciliation debt and contradict the accepted enterprise model.

DEV-308 therefore introduces only the smallest authoritative Organisation foundation required for later relationship-bearing slices.

## Explicit scope boundary

### In scope

- stable `PartyId`;
- `PartyKind.Organisation`;
- tenant-owned Organisation identity;
- governed Organisation display name;
- attributable creator/timestamp;
- provider-neutral domain/application contracts;
- module-owned PostgreSQL persistence;
- tenant RLS and restricted runtime-role verification;
- verified Principal/Tenant HTTP request context;
- fail-closed configured Organisation-administrator access;
- authoritative payload-minimised creation audit evidence;
- production runtime composition and CI verification.

### Out of scope

- Person production identity;
- Employee as an identity type;
- Work Relationship;
- Organisational Unit;
- Position / Position Occupancy;
- client/customer relationship lifecycle;
- vendor/supplier relationship lifecycle;
- partner/contractor relationship lifecycle;
- legal-entity registration/tax/jurisdiction profile;
- duplicate detection, matching or Party merge;
- addresses/contact points;
- cross-tenant shared Party identities;
- commercial engagement/contract/fee/billing semantics;
- customer-facing UI.

Those concerns must not be smuggled into a generic Party-type field to accelerate delivery.

## Canonical identity rule

```text
Tenant != Party

Party
├── Person
└── Organisation

Employee / Customer / Supplier / Partner / Contractor
= relationship or contextual role, not Party identity kind
```

DEV-308 implements only `Organisation`.

## Authoritative record

### Organisation

Minimum bounded attributes:

- `PartyId` — stable Party identity;
- `TenantId` — verified tenant ownership/security key;
- `PartyKind` — fixed to `Organisation` for this record;
- `DisplayName` — governed human-readable name;
- `CreatedByPrincipalId`;
- `CreatedAtUtc`.

The Party identity and Organisation subtype are owned by the Enterprise module. Later modules may reference `PartyId` but may not create competing authoritative customer/vendor Organisation identities.

## HTTP boundary

Initial versioned operations:

```text
POST /api/v1/{tenantSlug}/organisations
GET  /api/v1/{tenantSlug}/organisations/{partyId}
```

The route slug identifies the requested context but does not establish trust. Operations use only the verified server-side `AuthenticatedRequestContext` for `TenantId` and acting `PrincipalId`.

Caller-supplied `TenantId` or `ActorPrincipalId` fields, where accepted for tamper/compatibility verification, cannot override the trusted request context.

## Access boundary

DEV-308 uses a deliberately bounded configuration-backed administrator list:

```text
NuBlox:Enterprise:OrganisationAdministratorPrincipals
```

Unknown or unconfigured Principals fail closed.

This is not the final enterprise responsibility/authority taxonomy. It creates a safe production boundary while Position, Responsibility, Delegation and wider business authority semantics remain separately governed.

## Persistence boundary

The Enterprise module owns its PostgreSQL schema and migrations.

Initial persistence:

```text
enterprise.parties
enterprise.organisations
```

Required controls:

- composite tenant/Party keys;
- Party-kind constraint;
- Organisation-to-Party referential integrity;
- display-name basic integrity constraint;
- forced PostgreSQL RLS using transaction-local `nublox.tenant_id`;
- restricted runtime role without schema-DDL ownership;
- migration ownership under the common ordered migration runner.

## Audit boundary

Successful Organisation creation appends authoritative business audit evidence with:

- verified Tenant;
- acting Principal;
- action `enterprise.organisation.create`;
- subject type `organisation`;
- subject stable `PartyId`;
- timestamp/outcome/source/correlation.

The Organisation display name and arbitrary request payload are not copied into audit evidence merely for convenience.

Read operations do not create material business audit events.

## Acceptance criteria

DEV-308 is technically complete only when all of the following are demonstrated:

1. creating an Organisation produces a stable non-empty `PartyId` and `PartyKind.Organisation`;
2. blank/invalid Organisation names fail validation;
3. creation is denied for an unconfigured Principal;
4. verified server Tenant/Principal context overrides forged body Tenant/actor identifiers;
5. Tenant A can retrieve its Organisation through the real HTTP/application/PostgreSQL graph;
6. Tenant B cannot retrieve Tenant A's Organisation by changing route/context identifiers;
7. restricted PostgreSQL runtime credentials cannot insert another Tenant's Party while Tenant A context is set;
8. persisted Party kind is exactly `ORGANISATION` and no employee/client/vendor Party kinds are introduced;
9. successful creation appends one tenant-scoped attributable audit event without copying Organisation display-name payload;
10. the common production verifier builds and tests the Enterprise module alongside the existing first-slice production suites with zero warnings/errors.

## Traceability position

This slice advances the canonical business-subject foundation required by later capabilities, especially commercial/project continuity, without claiming that the wider Draft/Candidate product requirement catalogue or initial customer segment has completed approval.

The next relationship-bearing slice should explicitly choose and model a business outcome such as Organisation-to-engagement/customer context rather than extending `PartyKind` with role labels.

## References

- `Product_backlog.md`
- `../F_Requirements_Analysis/Epics.md`
- `../F_Requirements_Analysis/Data_requirements.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`
- `../../docs/01-enterprise-model.md`
- `../../docs/07-wave-1-enterprise-identity-reconciliation.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-27 | NuBlox Product / Engineering | Defined the bounded governed Organisation Party production slice as a prerequisite for later relationship/commercial continuity work |
