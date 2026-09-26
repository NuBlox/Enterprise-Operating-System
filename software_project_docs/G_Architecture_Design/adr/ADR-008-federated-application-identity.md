# ADR-008 — Identity and authentication

**Section:** G_Architecture_Design  
**ADR:** ADR-008  
**Decision:** Application identity, authentication and service identity boundary  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Security Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-001–FR-004; API-002, API-007, API-013, API-014; security/NFR requirements  
**Related ADRs:** ADR-001, ADR-007, ADR-009, ADR-011, ADR-016, ADR-018  
**Evidence:** Canonical enterprise model; control model; SPIKE-003/004/009; current OIDC/OAuth security standards  
**Supersedes:** None  

## Context

NuBlox must identify human and machine actors without collapsing application credentials into the enterprise identity model.

The canonical enterprise model already establishes:

```text
Person
!= User Account / Principal
!= Authentication Credential
!= Session
```

It also establishes that a Tenant is an isolation/configuration/commercial context rather than a Person, Organisation or Party. A Person may exist without login credentials and a service identity must not be forced into the Person model.

NuBlox must support enterprise federation, multi-tenant participation, external participants and machine-to-machine access. Authentication must therefore be standards-based and separable from the internal Person/Organisation/Work Relationship model.

## Decision

NuBlox will use a **federated application-identity model**.

Authentication is delegated to standards-compliant identity providers. NuBlox owns the application principal, account-linking, tenant participation, permission and business-authority relationships that sit behind authenticated identities.

### Identity concepts

NuBlox will keep these concepts separate:

```text
External authentication identity
    issuer + subject
        ↓
NuBlox Principal
        ↓
optional controlled link
        ↓
Person

NuBlox Principal
        ↓
Tenant participation / access scope
        ↓
Permission
        ↓
Business Authority (separate control)
```

A service/machine principal is a first-class application principal and does not require a Person record.

## Human authentication

### Primary protocol

OpenID Connect 1.0 is the primary interactive authentication/federation protocol for NuBlox web applications.

OAuth 2.0 access-token usage follows the current OAuth 2.0 Security Best Current Practice (RFC 9700) and related standards.

Interactive browser clients use an authorization-code flow with PKCE where applicable. Implicit flow is not part of the approved NuBlox baseline.

### Enterprise federation

Tenant enterprise identity providers should federate through OIDC where available.

Where a customer requires SAML 2.0 or another enterprise federation protocol, NuBlox should terminate that compatibility at an identity-broker/federation adapter and normalise the resulting identity into the same internal Principal model. Product modules must not depend directly on provider-specific claims or SAML semantics.

No identity-provider vendor is selected by this ADR.

## Principal identity

NuBlox assigns an internal immutable Principal identifier independent of the external provider.

External account identity is keyed by the trusted combination of:

```text
issuer (`iss`)
+
subject (`sub`)
```

Email address, display name or tenant slug must not be used as the immutable authentication identity key.

An authenticated external identity may be linked to an existing Person only through a controlled account-linking/provisioning process. Authentication claims must not silently create or overwrite canonical enterprise identity facts.

## Tenant context

Authentication answers **who the principal is**. It does not by itself establish which tenant or business context the principal may act within.

Tenant context follows ADR-007:

1. an external tenant slug may identify the requested tenant route/discovery context;
2. the server resolves the slug to the stable internal Tenant identity;
3. the authenticated Principal must have an approved participation/access relationship to that Tenant;
4. the server establishes the working Tenant context;
5. downstream application and persistence access use the verified internal Tenant identity, never the untrusted slug alone.

A principal that can participate in multiple tenants retains one application identity and selects/enters an authorised working tenant context rather than receiving duplicated Person identities.

## External participants

External users such as candidates, clients, suppliers or collaborators use the same application-identity principles but receive only the minimum tenant/context participation required for their workflow.

An external participant must not become an internal employee identity merely because they authenticate successfully.

Business relationship roles remain enterprise/work relationships, not authentication roles.

## Service and machine identities

Machine-to-machine operations use governed service principals or workload identities.

Requirements:

- no shared human credentials;
- attributable service identity;
- explicit owning NuBlox component/integration;
- least-privilege permissions and tenant scope;
- credential/key lifecycle independent of human accounts;
- audit/correlation sufficient to identify the service principal responsible for material actions.

Client credentials, workload federation or stronger sender-constrained mechanisms may be selected per deployment/integration risk. Long-lived static secrets are not the preferred default.

## Authentication versus permission versus authority

NuBlox explicitly preserves:

```text
Authentication = confidence in who is acting
Permission     = what the principal may technically access/do
Authority      = what business decision/commitment the actor may validly make
```

Authentication middleware must not be treated as the complete authorisation model.

Permission and business Authority are evaluated server-side against the current Tenant/work context. ADR-009 governs the business-authority model.

## Session and token principles

- tokens are validated for issuer, audience, signature, expiry and other protocol-required conditions;
- application modules consume a normalised NuBlox Principal/context abstraction rather than raw provider claims;
- access tokens are not persisted as enterprise identity records;
- refresh/session handling follows the chosen identity-provider/client architecture and current security guidance;
- logout/revocation/disablement must not delete canonical Person or historical enterprise records;
- security-sensitive authentication events are observable without logging tokens, credentials or unnecessary personal data.

## Tenant provisioning and bootstrap

A new Tenant must have a controlled bootstrap administration path that creates or links the first authorised Principal without weakening normal federation controls.

Tenant onboarding may initially use a NuBlox platform identity provider/broker and later add the tenant's enterprise federation configuration. The concrete hosted identity product is a deployment/procurement decision, not part of this ADR.

## Alternatives considered

### Build and store all passwords directly in NuBlox

Rejected as the default architecture.

A bespoke credential platform would create avoidable security, recovery, MFA, federation, breach-response and lifecycle responsibilities. NuBlox should integrate standards-compliant identity services rather than make password storage a core product differentiator.

### Treat Person as the login account

Rejected.

It conflicts with the canonical enterprise model, breaks service identities, complicates multi-account/federation cases and would make credential lifecycle corrupt business identity history.

### Tenant-specific duplicate user records

Rejected as the default identity model.

Tenant access is contextual participation. A person/principal participating in multiple tenants should not require duplicated application identity merely to preserve isolation.

### Provider-specific claims throughout product code

Rejected.

Provider claims are normalised at the authentication boundary so product modules depend on NuBlox Principal and context semantics.

## Security standards

The implementation baseline follows:

- OpenID Connect Core 1.0 incorporating current approved errata;
- OAuth 2.0 Security Best Current Practice, RFC 9700;
- PKCE for applicable authorization-code clients;
- provider metadata/discovery where supported and safe;
- TLS for authentication/token/resource traffic.

Later security decisions may require DPoP, mTLS, passkeys/WebAuthn, FAPI profiles or other stronger mechanisms for specific risk classes without changing the internal Principal model.

## Consequences

### Positive

- enterprise federation is supported without polluting enterprise Person semantics;
- users may participate across tenants without identity duplication;
- service identities are first-class;
- identity-provider replacement/federation changes stay behind one boundary;
- permission and Authority remain independent controls.

### Costs / risks

- account-linking and tenant-provisioning workflows require careful design;
- multiple IdPs require issuer/subject lifecycle and federation configuration management;
- identity-broker/provider outages become an availability dependency;
- support tooling requires secure, auditable recovery and federation diagnostics.

## Implementation implications

The production foundation may now introduce provider-neutral abstractions for:

- `PrincipalId`;
- human versus service principal kind;
- external issuer/subject binding;
- authenticated principal context;
- verified Tenant context;
- controlled Person/account link.

The initial production API must not accept a caller-supplied tenant identifier as proof of Tenant access.

Concrete OIDC middleware/provider configuration is implemented behind the authentication boundary and remains environment/configuration driven.

## Verification requirements

Tests must cover at minimum:

- valid authenticated Principal;
- invalid issuer/audience/token rejection;
- missing authentication where required;
- principal without access to requested Tenant;
- multi-tenant principal selecting an authorised Tenant;
- untrusted tenant slug/identifier cannot override verified context;
- service principal cannot masquerade as a Person;
- disabled/unlinked principal loses application access without deleting enterprise Person history.

## Review triggers

Review this ADR when:

- a selected identity provider cannot support the required federation/tenant model;
- passkey-only or phishing-resistant authentication becomes a universal product requirement;
- customer contracts require customer-owned authentication infrastructure without a broker;
- cross-tenant identity matching/privacy requirements change materially;
- machine identity requirements outgrow the service-principal model.

## References

- `../../../docs/01-enterprise-model.md`
- `../../../docs/05-control-model.md`
- `../../F_Requirements_Analysis/Functional_requirements_specification.md`
- `../../F_Requirements_Analysis/API_requirements.md`
- https://openid.net/specs/openid-connect-core-1_0.html
- https://www.rfc-editor.org/rfc/rfc9700.html

## Decision outcome

**Accepted:** NuBlox separates enterprise identity from application identity, authenticates humans through standards-based federation (OIDC primary), represents service identities separately, normalises external identities to an internal Principal, and verifies tenant/context participation server-side before product access.
