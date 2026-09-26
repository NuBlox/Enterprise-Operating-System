# ADR-009 — Business authority model

**Section:** G_Architecture_Design  
**ADR:** ADR-009  
**Decision:** Contextual business authority separate from technical permission  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Governance Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** BR-006, BR-007; FR-015–FR-018; UC-003; NBEOS-H-004  
**Related ADRs:** ADR-007, ADR-008, ADR-011, ADR-018  
**Evidence:** SPIKE-004 permission-versus-authority experiment; first governed Work Product approval workflow  
**Supersedes:** None

## Context

NuBlox already separates authentication from technical access. The first governed Work Product slice now requires an Approver to make a business decision whose validity cannot be inferred merely from read/write access, a role name, a route or successful authentication.

The system therefore needs a minimum production authority contract before approval/rejection is implemented. The contract must be usable now without prematurely fixing the final enterprise job/position/delegation taxonomy.

## Decision

NuBlox will evaluate **business authority as an explicit, server-side, context-specific decision separate from permission**.

Authority evaluation is expressed against:

```text
verified Tenant
+ authenticated Principal
+ governed action
+ governed subject
= authority decision
```

A successful authority decision returns an attributable **authority reference** identifying the mandate/delegation/rule under which the Principal may make that decision.

### Core rule

```text
authentication != permission != business authority
```

Technical permission determines whether a Principal may reach/attempt an operation. Business authority determines whether that Principal may validly make the governed business decision.

## Authority requirement

The production contract identifies at minimum:

- `TenantId`;
- `PrincipalId`;
- governed action code;
- governed subject type;
- governed subject identity.

For the first Work Product slice the initial governed actions are:

- `work-products.revision.approve`;
- `work-products.revision.reject`.

The subject is the exact governed Work Product Revision receiving the decision.

## Evaluation result

An authority evaluator returns either:

- **Granted** — including an `AuthorityReference` that can be retained with decision evidence; or
- **Denied** — including an internal governed denial reason code suitable for diagnostics/audit without exposing sensitive authority-policy internals to an external caller.

A missing evaluator, unknown authority rule or ambiguous context fails closed.

## Authority source

This ADR does not yet select the final source model for enterprise authority.

A later implementation may derive authority from controlled combinations of:

- position/deployment;
- organisational mandate;
- delegated authority;
- approval limits;
- project/function/domain assignment;
- temporary delegation;
- policy/rule configuration.

Those sources sit behind the authority-evaluation contract. WorkProducts must not hard-code job titles, grades or organisation-specific structures.

## Decision evidence

A material business decision must retain enough evidence to reconstruct:

- exact Tenant;
- exact governed subject;
- review/decision request;
- acting Principal;
- outcome;
- rationale/conditions where required;
- decision timestamp;
- authority reference;
- correlation identifier where available.

The decision evidence is authoritative business evidence under ADR-018 and is not replaced by logs or traces.

For the first Work Product slice, the state transition, completion of the review request and creation of decision evidence occur atomically in the same authoritative relational transaction.

## Access before authority

The application evaluates technical access before business authority.

This prevents a Principal with no permission to act on the review request from probing authority-policy behaviour. Passing the access check still does not grant authority.

## Tenant boundary

Authority evaluation always uses the already verified internal Tenant context from ADR-007/ADR-008. Caller-supplied tenant slugs/IDs are never authority proof.

An authority reference valid in one Tenant does not implicitly apply in another Tenant.

## Alternatives considered

### Treat technical permission as approval authority

Rejected. Permission answers whether an operation is technically accessible, not whether the actor has a valid business mandate to bind the organisation.

### Hard-code approver job titles/grades in WorkProducts

Rejected. It would couple one generic governed workflow to customer-specific organisational taxonomy and make future delegation models difficult to evolve.

### Put authority logic only in the UI

Rejected. Authority is a server-side control and must be enforced for API, UI, integration and background execution paths.

### Record only the final revision state

Rejected. The resulting state would not explain who decided, under what authority, against which request or with what rationale.

## Consequences

### Positive
- preserves the distinction between access and business mandate;
- gives every material decision an attributable authority basis;
- keeps WorkProducts independent from the final enterprise organisation/position authority model;
- supports later delegation/limit/rule engines behind one contract;
- produces reconstructable decision evidence.

### Costs / risks
- authority policy becomes another runtime dependency for governed decisions;
- authority-reference lifecycle and revocation semantics require later design;
- customer-specific authority models require mapping/configuration rather than hard-coded roles;
- policy denial must be observable without leaking sensitive internal policy details.

## Verification requirements

The first production use must prove:

- access permission alone cannot complete approval;
- access denial prevents authority evaluation;
- authority denial leaves revision/request state unchanged;
- granted authority records the exact authority reference;
- decision actor, subject, outcome, rationale and time are immutable evidence;
- a decision cannot be applied twice to the same open request;
- Tenant B cannot read or mutate Tenant A decision evidence;
- state transition + request completion + decision evidence are atomic.

## Review triggers

Review this ADR when:

- the enterprise position/job/deployment model becomes production-authoritative;
- approval limits or monetary thresholds are implemented;
- temporary/delegated authority is implemented;
- external-party authority is introduced;
- regulatory signature/non-repudiation requirements exceed authenticated attributable approval evidence;
- authority evaluation becomes distributed across independently deployed services.

## Decision outcome

**Accepted:** NuBlox evaluates business authority explicitly and server-side against verified Tenant, Principal, action and subject context. Authority is separate from technical permission, fails closed, returns an attributable authority reference when granted, and material decisions retain immutable evidence of the authority used.
