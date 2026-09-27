# Second vertical slice verification

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-007  
**Document Type:** Verification and traceability report  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering / Quality  
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
**Related Documents:** `Second_vertical_slice_definition.md`, `Product_backlog.md`, `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`, `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`  
**Supersedes:** None  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/Second_vertical_slice_verification.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Consolidate objective technical evidence for the bounded **Governed Organisation Party — Register and Retrieve** increment (`DEV-308`) and assess the ten acceptance criteria defined in NBEOS-H-006.

This report records technical verification only. It does not approve the wider Draft/Candidate requirement catalogue, customer segment, commercial-continuity priority or the complete enterprise/workforce model.

## Verified architecture boundary

DEV-308 implements the accepted ADR-022 invariant:

```text
Tenant != Party

Party
├── Person
└── Organisation

Employee / Customer / Supplier / Partner / Contractor
= relationship or contextual role, not Party kind
```

Only **Organisation** is implemented in this increment.

The production path under verification is:

```text
verified HTTP Principal/Tenant context
        ↓
Organisation HTTP operation
        ↓
Organisation application service
        ↓
fail-closed configured administration access
        ↓
Enterprise PostgreSQL repository
        ↓
transaction-local Tenant context + forced RLS
        ↓
enterprise.parties + enterprise.organisations
        ↓
minimised authoritative audit evidence
```

## Principal implementation

### Domain/application

- `src/NuBlox.Enterprise.Domain/PartyModel.cs`
- `src/NuBlox.Enterprise.Application/OrganisationApplication.cs`

The model provides a stable `PartyId`, explicit `PartyKind.Organisation`, tenant ownership, governed display name and attributable creation metadata.

### Persistence

- `src/NuBlox.Enterprise.Infrastructure.PostgreSql/`
- `Migrations/ent_0001_parties_organisations.sql`

The Enterprise module owns `enterprise.parties` and `enterprise.organisations`. Both tables use tenant-aware keys and forced PostgreSQL row-level security.

### Runtime/API

- `src/NuBlox.Runtime.PostgreSql/EnterpriseRuntimePolicies.cs`
- `src/NuBlox.Runtime.PostgreSql/RuntimeServiceCollectionExtensions.cs`
- `src/NuBlox.Api/OrganisationEndpoints.cs`

The provider-specific runtime composition registers the concrete Enterprise persistence boundary. Organisation administration denies by default unless the acting Principal is explicitly configured for this bounded slice.

The API exposes:

```text
POST /api/v1/{tenantSlug}/organisations
GET  /api/v1/{tenantSlug}/organisations/{partyId}
```

Tenant/Principal identity comes only from the verified server request context. Body `TenantId` / `ActorPrincipalId` values do not override that context.

## Automated verification evidence

Production foundation run **36310775633** passed on commit `206deb6217b6faa7de9769b4fedacd7496e30716` using .NET SDK 10.0.401 and PostgreSQL 18.6.

Observed suite results:

| Suite | Result |
|---|---:|
| Kernel | 2 passed |
| Identity | 9 passed |
| Audit | 8 passed |
| Observability | 6 passed |
| Enterprise unit/application | **3 passed** |
| API including real Organisation runtime smoke path | **17 passed** |
| WorkProducts unit | 12 passed |
| Enterprise PostgreSQL integration | **2 passed** |
| Common PostgreSQL persistence integration | 2 passed |
| WorkProducts PostgreSQL integration | 11 passed |

All production builds completed with **0 warnings and 0 errors**.

The PostgreSQL logs also contain the expected negative-path rejection:

```text
new row violates row-level security policy for table "parties"
```

That error is intentionally produced by the restricted-role test attempting to insert a Tenant B Party while the database session is scoped to Tenant A.

## Initial failed gate retained as evidence

The first PR-head verification run **36310679572** failed before runtime tests because analyzer rule `CA1822` rejected a computed `Organisation.Kind` getter that did not access instance state.

The model was corrected so Party kind is stored as immutable instance state when the Organisation is constructed/restored. The complete verifier then passed in run 36310775633.

No failed run is treated as acceptance evidence.

## NBEOS-H-006 acceptance assessment

| # | Acceptance criterion | Evidence | Result |
|---|---|---|---|
| 1 | Creating an Organisation produces stable non-empty `PartyId` and `PartyKind.Organisation` | Enterprise domain unit test and real API contract | **Pass** |
| 2 | Blank/invalid Organisation names fail validation | Domain validation in `Organisation.Create/Restore`; production build/unit boundary | **Pass** |
| 3 | Creation denied for unconfigured Principal | `ApplicationDeniesCreationWithoutConfiguredAccess`; real API denied-principal path | **Pass** |
| 4 | Verified server Tenant/Principal overrides forged body identifiers | real API smoke test posts forged Tenant B/actor values while verified context is Tenant A/admin A; persisted audit actor is admin A | **Pass** |
| 5 | Tenant A retrieves its Organisation through real HTTP/application/PostgreSQL graph | API runtime-composition test | **Pass** |
| 6 | Tenant B cannot retrieve Tenant A Organisation by changing context/route | real Tenant B admin GET returns not found under Tenant B RLS context | **Pass** |
| 7 | Restricted runtime role cannot insert another Tenant's Party under Tenant A context | Enterprise PostgreSQL integration; expected RLS violation visible in PostgreSQL logs | **Pass** |
| 8 | Persisted Party kind is exactly `ORGANISATION`; no employee/client/vendor Party kinds introduced | database constraint + integration query + ADR-022 model | **Pass** |
| 9 | Successful creation appends one tenant-scoped attributable audit event without display-name payload | real API smoke test verifies action/actor/count and zero display-name leakage | **Pass** |
| 10 | Common verifier builds/tests Enterprise module with existing production suites at zero warnings/errors | production CI 36310775633 | **Pass** |

## Verification conclusion

All ten NBEOS-H-006 acceptance criteria are technically satisfied on the implementation head tested by CI run **36310775633**.

`DEV-308` can therefore be treated as **technically complete**, subject to the repository's normal final-documentation-head CI rerun and merge workflow.

The resulting capability is deliberately narrow: NuBlox now has a production-grade Organisation Party identity foundation, not a finished Party/master-data/workforce/customer/supplier subsystem.

## Remaining product/model gaps

DEV-308 does not establish:

- Person production identity;
- Work Relationships;
- Organisational Units;
- Positions/occupancies;
- customer/client or supplier/vendor relationship lifecycles;
- legal-entity profile;
- Party matching/merge;
- commercial engagement/contract/fee/billing semantics;
- primary-customer validation of the next Wave-3 business priority.

The next relationship-bearing slice must reference canonical Organisation `PartyId` rather than create a competing module-local customer/supplier master or extend `PartyKind` with business-role labels.

## References

- `Second_vertical_slice_definition.md`
- `Product_backlog.md`
- `../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`
- `../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md`
- `../../docs/01-enterprise-model.md`
- `../../docs/07-wave-1-enterprise-identity-reconciliation.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-27 | NuBlox Product / Engineering / Quality | Recorded DEV-308 implementation-head verification, all ten NBEOS-H-006 acceptance criteria passing, and the retained analyzer/RLS evidence |
