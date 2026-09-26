# First vertical slice definition

**Section:** H_Development_Implementation  
**Document ID:** NBEOS-H-004  
**Document Type:** First vertical slice definition  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Engineering  
**Reviewer:** [TBD]  
**Approver:** NuBlox programme owner  
**Approval Date:** 2026-09-26  
**Effective Date:** 2026-09-26  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** Product lifetime + [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Product_backlog.md`, `../F_Requirements_Analysis/Use_cases.md`, `../F_Requirements_Analysis/User_stories.md`, `../F_Requirements_Analysis/Functional_requirements_specification.md`, `../F_Requirements_Analysis/Business_rules.md`, `../F_Requirements_Analysis/Acceptance_criteria.md`, `../G_Architecture_Design/Architecture_decision_records_ADRs.md`  
**Supersedes:** None  
**Superseded By:** None  
**Storage Location:** `software_project_docs/H_Development_Implementation/First_vertical_slice_definition.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Baseline the first representative NuBlox production workflow so development can move from platform foundations into an end-to-end governed business outcome without prematurely implementing the complete enterprise taxonomy.

This definition completes `DEV-201` by selecting a bounded workflow already present in the controlled requirements set and defining the implementation boundary for `DEV-202` through `DEV-208`.

## Selected workflow

**Governed Work Product — Create, Review, Approve and Issue**

Primary source use case: `UC-002 — Create, review and issue a governed work product`.

Supporting use cases:

- `UC-001 — Initiate and complete controlled work` for work ownership/state;
- `UC-003 — Make an authorised business decision` for review/approval authority and evidence;
- `UC-010 — Reconstruct a historical business event` for retained history/audit evidence.

The slice is deliberately **work-product-type neutral**. A work product may later represent a document, drawing, model, specification, report, submission, configuration record or another governed output. Version 0.1 does not create discipline-specific semantics.

## Why this is the first slice

The workflow exercises the broadest set of NuBlox foundation controls with minimal invented domain detail:

```text
authenticated Principal + verified Tenant context
        ↓
create/register governed Work Product
        ↓
create immutable Revision
        ↓
submit Revision for Review
        ↓
record Review / Approval Decision
        ↓
issue approved Revision
        ↓
retain current state + history + authoritative audit evidence
        ↓
query permitted current/history state through versioned API
```

It validates the product premise that a person can perform work in NuBlox and produce a controlled work product, while remaining reusable across functional governance, functional delivery and construction/built-environment domains.

## Actors

### Contributor

An authenticated human Principal with access to the verified Tenant context and permission to create/maintain the relevant work product.

Responsibilities:

- create/register a work product;
- create a revision;
- update mutable draft information;
- submit the revision for review.

### Reviewer

An authenticated Principal authorised to review the submitted revision.

Responsibilities:

- inspect the submitted revision/context;
- record review comments/outcome;
- request changes or progress the revision according to the defined state rules.

### Approver

An authenticated Principal with the required business authority for the approval action.

Responsibilities:

- approve or reject the submitted revision;
- provide required rationale/conditions;
- create attributable decision evidence.

Permission to access the work product does not itself establish approval authority.

### Recipient

A Principal or later governed external participant permitted to receive an issued revision. Recipient/distribution expansion beyond an internal authenticated Principal is deferred until the external-participant slice is implemented.

## Authoritative records

### WorkProduct

Stable governed identity for the business output.

Minimum version-0.1 attributes:

- `WorkProductId` — stable internal identifier;
- `TenantId` — logical tenant/security key;
- `Title` — human-readable name;
- `ProductType` — controlled semantic discriminator, initially a constrained generic value rather than a full enterprise taxonomy;
- `OwnerPrincipalId` — accountable current owner;
- `CreatedAtUtc`;
- `CreatedByPrincipalId`;
- `CurrentRevisionNumber` — derived/convenience pointer where appropriate;
- current lifecycle state.

### WorkProductRevision

Immutable identity for one revision of a Work Product. A revision becomes immutable for controlled content metadata once submitted; a later change creates a new revision rather than rewriting the submitted historical state.

Minimum attributes:

- `WorkProductRevisionId`;
- `TenantId`;
- `WorkProductId`;
- `RevisionNumber`;
- `Title`/description snapshot required for reconstruction;
- `State`;
- `CreatedAtUtc` / `CreatedByPrincipalId`;
- `SubmittedAtUtc` / `SubmittedByPrincipalId` where applicable;
- `IssuedAtUtc` / `IssuedByPrincipalId` where applicable.

Binary/file content is not required for DEV-202. The first implementation governs record/revision metadata and evidence; binary content storage remains behind ADR-004 and a later backlog item.

### ReviewRequest

Represents a controlled request to review/approve a specific revision.

Minimum attributes:

- `ReviewRequestId`;
- `TenantId`;
- `WorkProductRevisionId`;
- request kind (`Review` or `Approval`);
- requested participant/authority reference;
- requested timestamp;
- current request state.

### DecisionEvidence

Authoritative decision evidence follows ADR-018 and links the decision to the exact review request/revision.

Minimum semantics:

- subject identity;
- decision type/outcome;
- actor Principal;
- Tenant/context;
- timestamp;
- rationale/conditions where required;
- correlation identifier;
- immutable audit/event identity.

## Lifecycle

### Work Product lifecycle

Initial lifecycle:

```text
ACTIVE
  ↓
SUPERSEDED | WITHDRAWN
```

The Work Product remains the stable identity while revisions progress independently.

### Revision lifecycle

Version 0.1 state machine:

```text
DRAFT
  ↓ submit
IN_REVIEW
  ├─ request changes → CHANGES_REQUIRED
  │                      ↓ new revision/draft work
  ├─ reject          → REJECTED
  └─ approve         → APPROVED
                         ↓ issue
                      ISSUED
                         ↓ later revision issued
                      SUPERSEDED
```

Rules:

1. only `DRAFT` may be submitted;
2. submission records an immutable submitted snapshot/evidence point;
3. only a revision in `IN_REVIEW` can receive the active review/approval outcome;
4. approval requires separate authority evaluation, not merely access permission;
5. only `APPROVED` may be issued;
6. issuing a newer revision supersedes the previously issued revision of the same Work Product atomically where applicable;
7. historical revision/decision evidence is never overwritten to simulate the new state;
8. invalid transitions fail explicitly and do not partially mutate authoritative state.

## Tenant and security boundary

Every record in this slice is tenant-owned.

The request path uses the existing DEV-105 boundary:

```text
external authentication identity
→ AuthenticatedPrincipal
→ requested tenant route/context
→ IdentityContextResolver
→ AuthenticatedRequestContext
→ application operation
```

A route slug, body `TenantId`, query value or tenant header can request/identify context but can never manufacture trusted tenant access.

Persistence uses the verified `TenantId` and the PostgreSQL tenant session/RLS controls established by DEV-104.

## Authority boundary

Three distinct concepts remain separate:

- **authentication** — who the Principal is;
- **permission/access** — what information/actions the Principal may reach;
- **business authority** — whether that Principal may make the specific approval/decision.

DEV-204 will introduce the minimum authority contract needed by this slice. Version 0.1 must not encode job titles, organisational grades or a final enterprise authority taxonomy directly into the Work Product module.

## API boundary

The slice uses the DEV-108 versioned HTTP contract foundation.

Candidate initial operations for implementation are:

```text
POST   /api/v1/{tenantSlug}/work-products
GET    /api/v1/{tenantSlug}/work-products/{workProductId}
POST   /api/v1/{tenantSlug}/work-products/{workProductId}/revisions
GET    /api/v1/{tenantSlug}/work-products/{workProductId}/revisions/{revisionNumber}
POST   /api/v1/{tenantSlug}/work-products/{workProductId}/revisions/{revisionNumber}/submit
POST   /api/v1/{tenantSlug}/review-requests/{reviewRequestId}/decisions
POST   /api/v1/{tenantSlug}/work-products/{workProductId}/revisions/{revisionNumber}/issue
```

These are implementation candidates for the first slice, not permission shortcuts or table CRUD. Each operation must resolve the verified request context before application behaviour executes.

HTTP error bodies follow RFC 9457 and the NuBlox problem categories established by DEV-108.

## Persistence ownership

The first business module is named **WorkProducts** for the bounded slice.

Target layering:

```text
NuBlox.WorkProducts.Domain
NuBlox.WorkProducts.Application
NuBlox.WorkProducts.Infrastructure.PostgreSql
```

A smaller project count may be used if the boundary remains explicit; the invariant is that WorkProducts owns its authoritative tables/migrations and other modules do not mutate those tables directly.

Proposed PostgreSQL namespace: `work_products`.

DEV-202 establishes the authoritative schema and repository/application contracts. Provider-native SQL stays in the PostgreSQL infrastructure boundary.

## Audit and observability

Material business events include at minimum:

- work product created;
- revision created;
- revision submitted;
- review requested;
- review changes requested/rejected;
- approval granted/rejected;
- revision issued;
- prior issued revision superseded;
- work product withdrawn/superseded where implemented.

Authoritative business evidence uses `NuBlox.Audit`; technical traces/metrics/log correlation use `NuBlox.Observability`. Telemetry is not the authoritative decision history.

## Acceptance criteria for the first slice

The completed DEV-202–DEV-208 slice must prove:

1. an authenticated authorised contributor in Tenant A can create a Work Product and revision;
2. Tenant B cannot read, mutate, submit, decide or issue Tenant A records by changing route/body/header identifiers;
3. invalid lifecycle transitions are rejected without partial state changes;
4. submitted/decided/issued history remains reconstructable;
5. review/approval decisions are attributable to the verified Principal and exact revision;
6. approval cannot be completed solely because the Principal has read/write access;
7. only an approved revision may be issued;
8. issuing a newer revision does not rewrite the previously issued revision/evidence;
9. API failures use stable RFC 9457 problem semantics and do not leak stack traces/secrets;
10. material operations produce authoritative audit evidence plus correlated technical telemetry;
11. clean checkout CI verifies unit, API, PostgreSQL integration, tenant-negative and lifecycle-negative paths;
12. requirement → design/ADR → code → test traceability is recorded by DEV-208.

## DEV-202–DEV-208 allocation

| Backlog item | First-slice interpretation |
|---|---|
| `DEV-202` | WorkProduct + Revision domain model, lifecycle invariants, PostgreSQL schema/persistence and creation/read operations |
| `DEV-203` | submission/review work initiation and state/routing primitives |
| `DEV-204` | review/approval decision and business-authority contract |
| `DEV-205` | explicit work-product/revision/evidence relationships and issue evidence |
| `DEV-206` | bounded contributor/reviewer attention view plus source drill-through |
| `DEV-207` | durable post-approval/issue consequence/notification intent where required |
| `DEV-208` | consolidated automated acceptance evidence and traceability |

## Explicitly out of scope

Version 0.1 does not yet define:

- a final list of document/drawing/model/work-product types;
- CBE discipline-specific metadata;
- binary/object storage implementation;
- rich-content authoring/editor tooling;
- external-recipient invitation/access lifecycle;
- electronic/digital signature policy beyond attributable authenticated approval evidence;
- configurable workflow designer;
- final organisation/job/position authority matrix;
- notifications as an authoritative business record;
- arbitrary customer-defined lifecycle states;
- search/indexing implementation beyond direct retrieval needed by the slice.

These remain later governed capabilities rather than shortcuts added to the first vertical slice.

## Validation position

The underlying F-section requirements/use cases remain Draft/Candidate programme baselines. Selecting this workflow for the first production implementation does not falsely convert every linked candidate requirement into a generally approved final product requirement.

The slice is approved as the **representative implementation/validation workflow**. Findings from implementation and user/business validation may refine the Draft requirement set through normal controlled changes.

## References

- `Product_backlog.md`
- `Development_plan.md`
- `../F_Requirements_Analysis/Use_cases.md`
- `../F_Requirements_Analysis/User_stories.md`
- `../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../F_Requirements_Analysis/Business_rules.md`
- `../F_Requirements_Analysis/Acceptance_criteria.md`
- `../G_Architecture_Design/adr/ADR-001-cohesive-modular-application.md`
- `../G_Architecture_Design/adr/ADR-007-layered-tenant-isolation.md`
- `../G_Architecture_Design/adr/ADR-008-federated-application-identity.md`
- `../G_Architecture_Design/adr/ADR-011-http-api-standards.md`
- `../G_Architecture_Design/adr/ADR-017-module-owned-data-boundaries.md`
- `../G_Architecture_Design/adr/ADR-018-business-audit-evidence.md`
- `../G_Architecture_Design/adr/ADR-021-postgresql18-primary-provider.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Engineering | Selected and baselined Governed Work Product — Create, Review, Approve and Issue as the first representative production vertical slice |
