# ADR-011 — API standards

**Section:** G_Architecture_Design  
**ADR:** ADR-011  
**Decision:** Internal/external API contract and HTTP standards baseline  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** API-001–API-015; FR-032–FR-035; FR-039–FR-042  
**Related ADRs:** ADR-001, ADR-003, ADR-007, ADR-008, ADR-016, ADR-018, ADR-020  
**Evidence:** API requirements; SPIKE-005/008/009; accepted .NET server/runtime baseline  
**Supersedes:** None  

## Context

NuBlox needs stable synchronous API contracts for its web experience, external integrations, platform administration and future ecosystem use without turning database tables or internal module structures into accidental public contracts.

The application begins as a cohesive modular application under ADR-001, so an HTTP call is not required merely to cross an internal module boundary. External and remotely consumed contracts nevertheless require consistent semantics, security, lifecycle, errors, idempotency and documentation.

## Decision

NuBlox will use **resource/capability-oriented HTTPS APIs with JSON representations and OpenAPI-described contracts** as the default synchronous remote API style.

The decision applies to network APIs. In-process module collaboration inside the cohesive modular application uses typed C# contracts rather than loopback HTTP.

## Contract principles

1. APIs expose business/platform capabilities, not tables.
2. Stable external identifiers and business semantics are used instead of persistence implementation details.
3. Authentication, permission, tenant isolation and applicable business Authority are enforced server-side.
4. Public/integration contracts have an explicit owner and consumer purpose.
5. State-changing operations expose explicit success/business outcome and stable error semantics.
6. Retryable effects define idempotency semantics before release.
7. Contracts are documented and compatibility-tested.
8. Sensitive/personal data is minimised.
9. Pagination/filtering is bounded.
10. Internal module refactoring must not automatically create a breaking remote API change.

## HTTP and representation baseline

- HTTPS is mandatory outside explicitly controlled local development/test environments.
- JSON is the default request/response representation for structured synchronous APIs.
- UTF-8 is the default JSON encoding.
- HTTP methods and status codes are used according to their standard semantics.
- Content negotiation may be introduced where a real consumer need exists; it is not required for every endpoint.

## Contract description

HTTP APIs exposed beyond a private implementation detail are described with **OpenAPI 3.x**, using the newest specification version that the approved NuBlox tooling can generate and validate reliably.

As of this decision, OpenAPI 3.2.0 is the current published specification. NuBlox does not make product compatibility depend on consumers supporting a newer OpenAPI minor version before the toolchain does.

Generated documentation must be treated as a contract artefact, not a substitute for meaningful operation descriptions, business semantics and security notes.

## API versioning

Externally depended-on HTTP APIs use an explicit **major contract version**.

Initial external/public contract paths use the form:

```text
/api/v1/...
```

Rules:

- additive backwards-compatible changes stay within the current major version;
- breaking semantic/schema changes require a new major version or an approved migration/deprecation path;
- implementation releases do not create API versions automatically;
- internal product UI endpoints may remain unversioned only while they are demonstrably private to the same release boundary and are not externally depended upon;
- version support/deprecation dates are documented before a version is withdrawn.

## Error semantics

NuBlox uses **RFC 9457 Problem Details for HTTP APIs** for machine-readable error responses where an HTTP API returns an error body.

NuBlox-defined problem types identify stable categories such as:

- validation failure;
- unauthenticated;
- forbidden/insufficient permission;
- business authority denied;
- tenant/context mismatch;
- not found;
- concurrency/conflict;
- idempotency conflict;
- downstream dependency unavailable;
- rate/limit exceeded;
- unexpected server failure.

Error responses must not leak credentials, tokens, secrets, internal stack traces or unnecessary personal/business-sensitive data.

## Authentication and tenant context

ADR-008 defines authentication and Principal semantics. ADR-007 defines Tenant isolation.

Remote APIs accept standards-based authentication credentials/tokens appropriate to the client class. They do **not** treat a route slug, body tenant ID or arbitrary tenant header as proof of tenant access.

The authenticated Principal and requested route/context are resolved to a verified NuBlox Principal/Tenant context before protected application operations execute.

## Idempotency and retries

Operations that can create duplicate business effects when retried must define idempotency behaviour.

For applicable synchronous mutation APIs, the default external convention is an `Idempotency-Key` supplied by the caller and scoped to the authenticated consumer/tenant/operation. The server retains sufficient result/fingerprint state for the operation's documented retry window.

Idempotency is not applied mechanically to every request. Naturally idempotent HTTP operations and business actions with different semantics may use another explicitly documented strategy.

## Concurrency

Where concurrent modification matters, APIs expose explicit optimistic concurrency semantics rather than silently overwriting newer state.

ETags / `If-Match`, a version token or a domain-specific expected-version field may be used according to the resource model. The contract must define the conflict response.

## Pagination and query controls

Collection/query endpoints use bounded pagination. Cursor-based pagination is preferred for large or changing collections where stable offset paging cannot be guaranteed.

Every list/search API defines:

- maximum page size;
- allowed filter/sort fields;
- tenant/security scope;
- behaviour for invalid/expired cursors where cursors are used.

Arbitrary client-supplied SQL/query expressions are prohibited.

## Correlation and tracing

HTTP boundaries participate in distributed trace/correlation propagation using standard trace context where supported by the selected observability tooling.

A human-support correlation/request identifier may also be surfaced in responses/logs. Correlation identifiers are operational metadata, not authentication or idempotency proof.

ADR-016 defines the complete telemetry standard.

## API classes

### Product/web API

Supports the NuBlox product experience. It may contain private release-coupled endpoints plus stable versioned contracts where required.

### External/integration API

Versioned and documented for customer/partner/tool integration, with explicit lifecycle, consumer and support obligations.

### Platform/administrative API

Separated by permission/scope and protected against accidental tenant-admin access to platform-level operations.

### Bulk/import/export

May use asynchronous job resources rather than long-running synchronous requests. Large data movement follows migration/integration controls.

## Events and webhooks

Asynchronous domain/integration messages are not forced into the HTTP request/response model.

Where NuBlox exposes webhooks:

- deliveries are attributable and observable;
- authenticity is verifiable;
- retries are expected;
- duplicate/replay handling is defined;
- payload versioning is explicit;
- sensitive data is minimised.

A later event-contract decision may standardise envelope/schema technology without changing this HTTP API ADR.

## In-process module contracts

ADR-001 remains authoritative: modules in the initial cohesive application call typed in-process contracts.

Prohibited pattern:

```text
Module A -> localhost HTTP -> Module B
```

when both modules are part of the same application and no independent deployment boundary exists.

Network contracts are introduced for actual remote/deployment/integration boundaries, not to imitate microservices internally.

## Alternatives considered

### GraphQL as the universal API

Not selected as the default.

GraphQL can be valuable for selected read/composition use cases, but making it universal would add schema/resolver complexity, authorisation/query-cost concerns and another abstraction before current workflow/query needs justify it. It may be introduced later for a bounded consumer need.

### gRPC as the universal API

Not selected as the public/default HTTP contract.

gRPC remains available for later service-to-service or high-throughput typed RPC boundaries if independent services emerge. The initial cohesive modular application does not need network RPC between modules.

### Generic CRUD/table APIs

Rejected.

They expose persistence design as product contract, weaken business invariants and create long-lived compatibility debt.

### Unversioned external APIs

Rejected.

Externally depended-on contracts require an explicit compatibility/deprecation model.

## Consequences

### Positive

- conventional, interoperable HTTP integration surface;
- machine-readable contracts through OpenAPI;
- consistent error handling through RFC 9457;
- clean separation between internal modules and network APIs;
- explicit compatibility and idempotency expectations.

### Costs / risks

- versioned external contracts create long-term support obligations;
- idempotency storage and compatibility tests add implementation work;
- poor API design can still leak internal architecture if reviews focus only on OpenAPI syntax;
- API documentation must remain synchronised with runtime behaviour.

## Implementation implications

The production foundation may now add an ASP.NET Core API host with:

- `/api/v1` as the initial stable version prefix for remotely depended-on product/integration APIs;
- RFC 9457-compliant problem details;
- generated OpenAPI for approved contracts;
- central authentication/Tenant context middleware/boundary;
- bounded request sizes and collection pagination controls;
- no direct persistence exposure.

Health/readiness endpoints are operational endpoints and need not be placed under the business API major-version path.

## Verification requirements

API verification must include as applicable:

- OpenAPI/schema contract tests;
- unauthenticated and forbidden tests;
- cross-tenant negative tests;
- validation/problem-details tests;
- idempotency replay/conflict tests;
- concurrency tests;
- version compatibility tests;
- bounded pagination/query tests;
- sensitive-data/error leakage tests.

## Standards references

- OpenAPI Specification 3.2.0: https://spec.openapis.org/oas/v3.2.0.html
- RFC 9457 Problem Details for HTTP APIs: https://www.rfc-editor.org/rfc/rfc9457.html
- RFC 9700 OAuth 2.0 Security Best Current Practice: https://www.rfc-editor.org/rfc/rfc9700.html
- OpenID Connect Core 1.0: https://openid.net/specs/openid-connect-core-1_0.html

## Decision outcome

**Accepted:** NuBlox uses HTTPS + JSON + OpenAPI-described, capability-oriented HTTP APIs as its default synchronous remote contract, with explicit major versioning for externally depended-on contracts, RFC 9457 errors, server-side security/context enforcement and defined idempotency/compatibility semantics. In-process modules continue to use typed C# contracts rather than internal HTTP.
