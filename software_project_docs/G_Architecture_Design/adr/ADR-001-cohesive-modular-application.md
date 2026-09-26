# ADR-001 — Initial application decomposition

**Section:** G_Architecture_Design  
**ADR:** ADR-001  
**Decision:** Initial application decomposition  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** BR/SR/SRS capability envelope; FR-005–FR-040 as applicable  
**Related ADRs:** ADR-002, ADR-003, ADR-005, ADR-007, ADR-010, ADR-011, ADR-017  
**Evidence:** SPIKE-001 through SPIKE-009; `Proof_of_concept_report.md`  
**Supersedes:** None  

## Context

NuBlox must support strongly related enterprise workflows whose semantics, authoritative boundaries and cross-capability transactions are still evolving. Premature service distribution would make those boundaries more expensive to change and would introduce distributed consistency, deployment and observability problems before product evidence justifies them.

The architecture spikes demonstrated that explicit typed modules can preserve ownership while one application process coordinates a business transaction through controlled APIs and one relational transaction boundary. The same experimental codebase subsequently supported historical data, tenant isolation, authority, durable asynchronous work, configuration, migration and reporting without requiring independent deployable services for each concern.

## Decision

NuBlox will begin production implementation as a **cohesive modular application** with explicit internal module ownership and enforceable dependency boundaries.

The decision is about **decomposition and deployment shape**, not final business taxonomy. It does not authorise copying the synthetic spike modules into production.

### Required properties

1. **Explicit module ownership** — each module owns its application/domain behaviour and authoritative persistence boundary.
2. **Controlled dependencies** — modules interact through explicit contracts; unrestricted cross-module table mutation is prohibited.
3. **Application orchestration** — cross-module use cases are coordinated by an application layer rather than by hidden database coupling.
4. **Single-process transactions where the business invariant requires them** — NuBlox may use one atomic relational transaction across participating module-owned writes when they belong to one consistency boundary.
5. **Durable asynchronous boundaries for external/long-running effects** — external provider calls and work that cannot safely participate in the local transaction use durable intent/outbox-or-equivalent patterns.
6. **Extraction seams** — module contracts and data ownership must make later extraction into independently deployed services possible when justified by evidence.
7. **No universal generic-object runtime** — shared infrastructure may be generic, but product semantics remain typed and owned.

## Initial production shape

```text
NuBlox application
│
├── API / experience boundary
├── Application orchestration
├── Typed product modules
│   ├── module-owned contracts
│   ├── module-owned behaviour
│   └── module-owned persistence access
├── Shared platform primitives
│   ├── identity/context
│   ├── transactions
│   ├── durable work
│   ├── audit/telemetry
│   └── configuration
└── Primary relational persistence
```

This diagram is structural only. It does not define the final module list.

## Alternatives considered

### First-release microservices

Rejected as the default initial decomposition.

Potential advantages include independent deployment, scaling and technology choice. Those benefits do not currently outweigh the cost of distributed transactions, duplicated platform concerns, network failure modes, versioned contracts and harder refactoring while domain boundaries are still being validated.

Microservices remain a valid later extraction strategy for a module with demonstrated independent scaling, resilience, organisational or deployment needs.

### Single undifferentiated monolith

Rejected.

A single deployment unit is acceptable; unrestricted internal coupling is not. NuBlox requires explicit ownership and dependency rules so the modular application does not become a shared-table/shared-code monolith.

### Universal metadata/object engine

Rejected as the primary application model.

Governed metadata/configuration can extend typed domains, but core business invariants must not be reduced to a universal untyped object runtime.

## Evidence

SPIKE-001 proved:

- explicit module APIs;
- atomic subject/work/decision/audit writes;
- rollback without partial state;
- relational constraints protecting cross-customer relationships.

SPIKE-002 through SPIKE-009 extended the same cohesive application experiment with effective history, isolation, authority, durable asynchronous processing, configuration, migration, reporting and operability checks. This demonstrates that the approach is technically credible for the initial NuBlox foundation.

The spike does not prove production workload limits or final business module boundaries; those remain implementation and later architecture concerns.

## Consequences

### Positive

- domain boundaries can evolve without distributed-system migration overhead;
- strong local consistency is available where required;
- one deployment simplifies early operations and debugging;
- platform capabilities can be implemented once and reused through controlled contracts;
- future service extraction remains possible from explicit module boundaries.

### Costs / risks

- boundary discipline must be enforced in code and persistence, not merely documented;
- independent scaling is coarser until a module is extracted;
- a single process can become overly coupled if dependency rules are ignored;
- build/test architecture must detect prohibited references and cross-module persistence access.

## Guardrails

A new independently deployed service requires evidence of at least one material driver such as:

- independent scaling/load profile;
- independent release cadence with stable contract;
- resilience/failure isolation requirement;
- regulatory/security isolation boundary;
- specialist runtime requirement;
- organisational ownership that cannot be served safely by the modular application.

A service must not be created merely because a business function, department or NuBlox navigation area exists.

## Implementation implications

- production code is created outside `spikes/`;
- solution/project/package structure must make forbidden module dependencies observable;
- repositories/data-access code must be owned by the corresponding module;
- cross-module transactions must be explicit application use cases;
- shared platform primitives must remain semantic-light and must not become a dumping ground for business logic.

## Review triggers

Review this ADR when:

- production profiling demonstrates materially different scaling needs;
- a module requires independent availability/failure isolation;
- deployment frequency or organisational ownership makes shared deployment materially harmful;
- a provider/runtime boundary requires independent execution;
- the modular application becomes too coupled despite enforced boundaries.

## Decision outcome

**Accepted:** NuBlox starts as a cohesive modular application with explicit module and data ownership, using independent services only when later evidence justifies extraction.
