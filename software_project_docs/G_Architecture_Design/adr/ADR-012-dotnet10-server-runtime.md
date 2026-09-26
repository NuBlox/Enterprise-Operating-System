# ADR-012 — Runtime / language / framework

**Section:** G_Architecture_Design  
**ADR:** ADR-012  
**Decision:** Server/core runtime, language and framework baseline  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** SRS core capability areas; NFR maintainability, security, operability and portability obligations  
**Related ADRs:** ADR-001, ADR-002, ADR-003, ADR-007, ADR-011, ADR-016, ADR-017, ADR-020  
**Evidence:** `Evaluation_matrix_for_technology_selection.md`; SPIKE-001 through SPIKE-009; `Proof_of_concept_report.md`  
**Supersedes:** None  

## Context

NuBlox requires a production server/core runtime that supports a long-lived enterprise codebase, explicit domain/module boundaries, strong transactional behaviour, background processing, identity/security integration, observability, cross-platform development and predictable support.

The technology evaluation considered .NET/C#, Java/Spring Boot, Node.js/TypeScript and Go. The completed architecture spikes then exercised the .NET 10/C# candidate through modular transactions, effective history, tenant isolation, business authority, durable asynchronous work, typed configuration, migration/reconciliation, reporting and operability.

As revalidated on 2026-09-26:

- .NET 10 is an active Microsoft LTS release supported through 2028-11-14;
- .NET 10 runtime 10.0.12 and SDK 10.0.401 were the current servicing baseline published on 2026-09-08;
- Node.js 24 remains an LTS line;
- Java 25 remains the current Java LTS line;
- Spring Boot 4.1.x remains a supported current Spring Boot line.

The alternatives remain technically credible; the decision is based on NuBlox fit plus measured spike evidence rather than on ecosystem availability alone.

## Decision

NuBlox will use **.NET 10 LTS with C# as the production server/core runtime and language baseline** for the initial product implementation.

ASP.NET Core is the default HTTP/server framework for NuBlox server applications where an HTTP boundary is required.

This decision applies to:

- enterprise-core application services;
- server-side APIs;
- background workers/jobs;
- application/domain modules;
- server-side integration adapters;
- server-side platform capabilities.

This decision does **not** select the frontend framework/runtime. Web experience technology remains a separate controlled decision and may use TypeScript where appropriate.

## Runtime servicing policy

NuBlox will target the .NET 10 LTS family and remain current on supported servicing/security patches rather than freezing indefinitely on the SPIKE-009 SDK.

For the initial production foundation:

- SDK baseline: `10.0.401`;
- target framework: `net10.0`;
- language: C# with the language version supported by the pinned SDK;
- preview runtime/language features: prohibited unless separately approved;
- production SDK/runtime patches: reviewed and adopted through dependency/toolchain maintenance, with security patches prioritised.

The pinned SDK is a reproducible build input, not a reason to remain on an insecure patch level.

## Why .NET 10 / C#

### Product architecture fit

- strong static typing for a large domain-rich codebase;
- mature ASP.NET Core hosting and HTTP stack;
- first-class dependency injection/configuration/hosting primitives;
- mature relational-data and transaction ecosystem;
- strong background-service and asynchronous processing support;
- cross-platform macOS/Linux/Windows development support;
- mature OpenTelemetry/diagnostic ecosystem;
- predictable LTS servicing horizon.

### NuBlox-specific evidence

SPIKE-001 through SPIKE-009 built and ran successfully on .NET 10 and demonstrated all of the high-risk server-side patterns evaluated to date.

The evidence includes:

- explicit typed modules and application orchestration;
- atomic relational transactions;
- effective-dated history;
- customer isolation;
- business authority checks;
- durable outbox/recovery behaviour;
- typed configuration;
- migration/reconciliation;
- operational reporting;
- structured correlation logging and process restart recovery.

No alternative runtime currently has equivalent NuBlox-specific implementation evidence in this repository.

## Alternatives considered

### Java 25 + Spring Boot 4.1

Retained as the principal architectural alternative but not selected for the initial core.

It provides excellent enterprise, data, security and observability capabilities. The deciding factors for NuBlox are the slightly stronger current evaluation score, simpler demonstrated implementation path and completed .NET spike evidence. Java remains a credible fallback if future evidence exposes a material .NET constraint.

### Node.js 24 + TypeScript

Not selected as the primary enterprise-core server runtime.

It remains attractive for web/full-stack productivity and may be used for frontend/tooling or bounded services where justified. For the initial NuBlox core, the stronger nominal typing, mature transaction/data patterns and completed .NET experiments provide a clearer baseline for domain-heavy enterprise behaviour.

The presence of `packages/mastered/mysql` does not imply that the NuBlox server runtime must be Node.js. Mastered packages and product runtime decisions are governed independently.

### Go

Not selected for the initial enterprise core.

Go remains suitable for specialised infrastructure/network/concurrency-heavy services if a later product need justifies an independently deployed component.

## Framework boundaries

The .NET decision must not become a framework-heavy product model.

Production code should prefer:

- standard .NET / ASP.NET Core capabilities where sufficient;
- explicit NuBlox application/domain contracts;
- small, reviewed dependencies;
- provider adapters at infrastructure boundaries;
- testable code without requiring the HTTP host for domain/application verification.

ORM, mediator, workflow, validation, event-bus or application-framework libraries are **not** approved by this ADR merely because they exist in the .NET ecosystem.

## Frontend boundary

Frontend selection remains intentionally separate.

The server must expose stable contracts that allow the chosen web experience to evolve independently. A future SvelteKit, React/Next.js or other TypeScript-based frontend does not require moving enterprise-core semantics into Node.js.

## Database boundary

This ADR does not select PostgreSQL, MySQL or another relational database provider.

ADR-002 defines the relational persistence model. The production provider must be chosen against integrity, tenant isolation, migration, operations, hosting/residency and product-economics requirements. The selected .NET runtime must support that provider through a controlled infrastructure adapter.

## Consequences

### Positive

- production implementation can now establish a deterministic server build/test foundation;
- spike evidence is directly reusable as architectural learning without copying spike business semantics;
- one strongly typed runtime covers HTTP, application services and workers;
- current LTS lifecycle is suitable for the first product foundation;
- macOS developer and Linux CI/runtime support are available.

### Costs / risks

- NuBlox must maintain C#/.NET engineering capability;
- frontend code will likely use a different language/runtime;
- careless use of framework/ORM convenience could weaken explicit module or persistence boundaries;
- major .NET upgrades require controlled planning before the .NET 10 support window closes.

## Implementation implications

The production foundation will:

- add a root `global.json` pinned to the approved servicing SDK;
- target `net10.0`;
- create production `src/` and `tests/` boundaries outside `spikes/`;
- use deterministic restore/build/test commands in local development and CI;
- treat warnings as errors for production projects unless an explicit exception exists;
- keep spike projects independently buildable but outside the production solution/build path;
- introduce dependencies only through controlled dependency/provenance records.

## Review triggers

Review this ADR when:

- .NET 10 approaches end of support;
- representative production profiling reveals a material runtime limitation;
- a required platform/provider capability cannot be supported safely;
- organisational/team capability materially changes;
- an independently deployed component has a strong reason to use another runtime.

## Official references

- https://dotnet.microsoft.com/en-us/platform/support/policy
- https://dotnet.microsoft.com/en-us/download/dotnet/10.0
- https://nodejs.org/en/about/previous-releases
- https://www.oracle.com/java/technologies/downloads/
- https://docs.spring.io/spring-boot/system-requirements.html

## Decision outcome

**Accepted:** .NET 10 LTS / C# is the NuBlox production server/core runtime baseline. ASP.NET Core is the default server HTTP framework. Frontend framework and relational database provider remain separate controlled decisions.
