# Third vertical slice definition

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-008  
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
**Related Documents:** `Product_backlog.md`, `Second_vertical_slice_definition.md`, `../F_Requirements_Analysis/Data_requirements.md`, `../F_Requirements_Analysis/Data_dictionary.md`, `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`, `../../docs/01-enterprise-model.md`, `../../docs/07-wave-1-enterprise-identity-reconciliation.md`  
**Supersedes:** None  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/Third_vertical_slice_definition.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define the bounded production implementation of the second canonical Party kind established by ADR-022: a stable **Person** enterprise identity that exists independently of application credentials, employment, Position or contextual business role.

This increment advances the canonical enterprise/workforce spine. It does not claim that workforce/HCM is the next customer-validated market priority and it does not implement Employee semantics.

## Selected bounded outcome

**Governed Person Party — Register and Retrieve**

Primary controlled drivers:

- `EP-004 — Governed business subjects`;
- `DATA-001 — stable identifiers`;
- `DATA-002 — explicit material relationships`;
- `DATA-003 — authoritative ownership/source`;
- canonical clean-slate enterprise model;
- `ADR-008 — federated application identity`;
- `ADR-022 — Canonical Party and Organisation identity`.

## Core invariant

```text
Person != Principal/User Account != Employee != Work Relationship != Position
```

A Person is a stable human enterprise Party identity.

A Principal is an authenticated application identity. A Principal may administer a Person without being that Person, and a Person may exist without any application Principal.

Employee is not a Party kind. It will emerge from a governed Employment Work Relationship when that capability is implemented.

## In scope

- stable `PartyId` for a Person;
- `PartyKind.Person`;
- tenant-owned Person identity;
- bounded governed display identity;
- attributable creator/timestamp;
- provider-neutral Person domain/application contracts;
- module-owned `enterprise.people` PostgreSQL subtype persistence;
- tenant RLS and restricted-role verification;
- verified Principal/Tenant HTTP request context;
- fail-closed configuration-backed Person-administrator access;
- authoritative payload-minimised creation audit evidence;
- production runtime composition and CI verification.

## Out of scope

- User Account / Principal linking to Person;
- authentication credentials;
- Employee as an identity type;
- Employment or Contingent Work Relationship;
- Organisational Unit;
- Position or Position Occupancy;
- reporting relationships;
- legal/HR identity records;
- date of birth, address, phone, email or other contact/PII profile;
- competence/qualification records;
- customer/supplier relationship roles;
- Party matching/merge;
- UI.

Those concerns require separate requirements, privacy/security analysis and controlled implementation increments.

## Authoritative record

Minimum bounded Person attributes:

- `PartyId` — stable enterprise Party identifier;
- `TenantId` — verified tenant ownership/security key;
- `PartyKind` — fixed to `Person`;
- `DisplayName` — bounded human-readable identity label;
- `CreatedByPrincipalId` — administrator/actor who established the enterprise record;
- `CreatedAtUtc`.

`CreatedByPrincipalId` is audit/provenance metadata and is **not** the Person identity.

## HTTP boundary

Initial operations:

```text
POST /api/v1/{tenantSlug}/people
GET  /api/v1/{tenantSlug}/people/{partyId}
```

The route slug is an address into a requested context. Trusted `TenantId` and acting `PrincipalId` come only from the verified server-side request context.

Caller-supplied `TenantId` or `ActorPrincipalId` fields cannot override that verified context.

## Access boundary

DEV-309 uses a deliberately bounded fail-closed configuration list:

```text
NuBlox:Enterprise:PersonAdministratorPrincipals
```

Only configured Principals may create/read Person records in this slice.

This is an interim technical policy. It is not the final responsibility/authority model and does not imply that a Person record is accessible to the Person themselves.

## Persistence boundary

The existing Enterprise module owns Person persistence.

```text
enterprise.parties
        ↓
enterprise.people
```

Required controls:

- `(TenantId, PartyId)` keys;
- `party_kind = PERSON` in the canonical Party row;
- Person-to-Party referential integrity;
- bounded display-name integrity;
- forced PostgreSQL RLS using transaction-local `nublox.tenant_id`;
- restricted runtime credentials;
- ordered migration under the common migration runner.

## Audit boundary

Successful Person creation appends authoritative audit evidence containing only stable control/evidence data:

- verified Tenant;
- acting Principal;
- action `enterprise.person.create`;
- subject type `person`;
- stable `PartyId`;
- timestamp/outcome/source/correlation.

The Person display name or other request payload is not copied into audit evidence merely for convenience.

## Acceptance criteria

DEV-309 is technically complete only when all of the following are demonstrated:

1. creating a Person produces a stable non-empty `PartyId` and `PartyKind.Person`;
2. Person `PartyId` is independent of the acting administrator `PrincipalId`;
3. blank/invalid Person display names fail validation;
4. creation is denied for an unconfigured Principal;
5. verified server Tenant/Principal context overrides forged body Tenant/actor identifiers;
6. Tenant A can retrieve its Person through the real HTTP/application/PostgreSQL graph;
7. Tenant B cannot retrieve Tenant A's Person by changing Tenant context;
8. restricted PostgreSQL runtime credentials cannot insert a Tenant B Person Party while the session is scoped to Tenant A;
9. persisted Party kind is exactly `PERSON`, and no Employee/UserAccount/Position identity shortcut is introduced;
10. successful creation appends one tenant-scoped attributable audit event without copying Person display-name payload;
11. the common production verifier builds/tests the Person increment alongside existing WorkProducts/Organisation suites with zero warnings/errors.

## Next dependency

Only after Person and Organisation identities are trustworthy should a later controlled increment introduce **Work Relationship** semantics such as Employment or Contingent Engagement.

That later relationship must connect a Person Party to an Organisation Party with effective-dated legal/contractual meaning rather than converting Person into an `Employee` Party kind.

## References

- `Product_backlog.md`
- `Second_vertical_slice_definition.md`
- `../F_Requirements_Analysis/Data_requirements.md`
- `../F_Requirements_Analysis/Data_dictionary.md`
- `../G_Architecture_Design/adr/ADR-008-federated-application-identity.md`
- `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`
- `../../docs/01-enterprise-model.md`
- `../../docs/07-wave-1-enterprise-identity-reconciliation.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-27 | NuBlox Product / Engineering | Defined the bounded governed Person Party production increment and explicit Person/Principal/Employee/Position separation |
