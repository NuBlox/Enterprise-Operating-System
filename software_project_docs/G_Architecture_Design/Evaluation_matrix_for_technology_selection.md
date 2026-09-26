# Evaluation matrix for technology selection

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-012  
**Document Type:** Evaluation matrix for technology selection  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture / Engineering  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Solution_architecture_document.md`, `Architecture_decision_records_ADRs.md`, `Technical_spikes.md`, `../F_Requirements_Analysis/Non-functional_requirements_specification.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Evaluation_matrix_for_technology_selection.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Evaluation_matrix_for_technology_selection.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Evaluate candidate implementation technologies against the current controlled architecture and requirements. The purpose of version 0.1 is to choose a **technical spike candidate**, not to approve the production stack.

## Current-version evidence

As of 2026-09-26, official vendor/project sources indicate:

- .NET 10 is an active LTS release, supported to November 2028;
- Java 25 is the current Java LTS release;
- Spring Boot 4.1.x is the current stable Spring Boot line, with 4.2 still milestone/pre-release;
- Node.js 24 is an LTS release; Node 26 is Current rather than LTS;
- TypeScript 6.0 is the current documented TypeScript release line;
- Go 1.27 is the current Go release line;
- PostgreSQL 18 is the current supported PostgreSQL major version; PostgreSQL 19 remains pre-release/beta.

Version numbers shall be revalidated at implementation approval rather than frozen from this draft.

## Core-runtime evaluation criteria

Scores are 1 (poor fit) to 5 (strong fit) for the **initial enterprise-core architecture hypothesis**. They are architecture-team provisional judgements to direct spikes, not market facts.

| Criterion | Weight | .NET 10 / C# | Java 25 + Spring Boot 4.1 | Node 24 + TypeScript 6 | Go 1.27 |
|---|---:|---:|---:|---:|---:|
| Rich domain / transactional application fit | 20 | 5 | 5 | 4 | 3 |
| Type system / maintainable large-codebase modelling | 15 | 5 | 5 | 4 | 4 |
| Enterprise identity / security ecosystem | 10 | 5 | 5 | 4 | 4 |
| Background work / integration / reliability patterns | 10 | 5 | 5 | 4 | 5 |
| Observability / diagnostics / production operations | 10 | 5 | 5 | 4 | 5 |
| Relational data / ORM / migrations ecosystem | 10 | 5 | 5 | 4 | 3 |
| Developer productivity for business applications | 10 | 4 | 3 | 5 | 4 |
| Cross-platform/container/cloud portability | 5 | 5 | 5 | 5 | 5 |
| LTS / enterprise support predictability | 5 | 5 | 5 | 4 | 4 |
| Shared full-stack language opportunity | 5 | 3 | 2 | 5 | 2 |
| **Weighted result / 5** | **100** | **4.75** | **4.60** | **4.20** | **3.85** |

## Candidate assessment

### .NET 10 / C#

Strengths for NuBlox:
- strong static typing and domain modelling;
- mature ASP.NET Core application/security stack;
- strong relational/data tooling ecosystem;
- first-class background-service patterns;
- mature OpenTelemetry/diagnostics support;
- cross-platform development and deployment;
- current LTS support window suitable for a new enterprise product baseline.

Risks / questions:
- frontend still requires separate choice unless Blazor is selected later;
- ORM convenience must not hide inefficient queries or domain-boundary leakage;
- product/team capability in C# must be built or sourced if not already present.

**Current use:** preferred core spike candidate.

### Java 25 + Spring Boot 4.1

Strengths:
- extremely mature enterprise/data/security/integration ecosystem;
- strong type system and modular architecture support;
- broad enterprise skills/tooling ecosystem;
- robust runtime/observability options.

Risks / questions:
- comparatively heavier framework/ecosystem complexity;
- developer feedback/configuration overhead can be higher;
- Spring Boot 4.1 supports Java versions below the current Java 25 LTS as well, so exact JVM baseline must be chosen deliberately rather than assumed.

**Current use:** principal alternative to .NET; retain for benchmark/spike comparison if .NET reveals material issues.

### Node.js 24 + TypeScript 6

Strengths:
- high full-stack productivity and one-language opportunity;
- excellent web/API ecosystem;
- strong developer tooling and rapid iteration;
- natural fit with modern frontend stacks.

Risks / questions for this specific core:
- TypeScript's structural type system requires strong discipline for rich domain invariants;
- package/ecosystem variance is higher;
- long-running transactional/integration-heavy enterprise design is achievable but needs more deliberate convention than .NET/Java;
- runtime blocking/CPU-heavy work requires careful isolation.

**Current use:** strong candidate for web experience and possibly application services, but not selected as first core-risk spike.

### Go 1.27

Strengths:
- simple deployment and strong runtime footprint;
- excellent concurrency/network-service characteristics;
- strong operational simplicity.

Risks / questions for this specific core:
- less ergonomic ecosystem for rich transactional/domain-heavy enterprise modelling;
- less mature batteries-included business application/data stack than .NET/Java;
- frontend remains separate.

**Current use:** suitable for specialised infrastructure/integration services if future requirements justify it; not preferred for the initial enterprise core.

## Primary relational database evaluation

The requirements strongly favour relational transactional persistence. PostgreSQL 18 is the leading **spike database** because it provides:

- mature ACID transactions and constraints;
- rich SQL/query support;
- JSON support where controlled extension data is useful;
- broad migration/ORM/tool support across candidate runtimes;
- open-source licensing and wide cloud portability;
- current supported major release.

Alternative products should still be evaluated before production approval where licensing, customer hosting, cloud-provider or enterprise-support requirements make them material.

**Spike position:** PostgreSQL 18 — Proposed for technical spike, not production-approved.

## Frontend framework decision

Frontend selection is intentionally deferred from the core spike.

Evaluation criteria later should include:

- work-centred application UX complexity;
- accessibility;
- data/forms/state patterns;
- SSR/client-side requirements;
- TypeScript support;
- component/design-system ecosystem;
- performance;
- testing;
- long-term maintenance/team capability.

Candidate families may include Svelte/SvelteKit and React/Next.js among others, but no framework is approved by this document.

## Initial spike selection

The proposed first technology spike is:

```text
Backend/runtime: .NET 10 LTS / C#
Persistence: PostgreSQL 18
Architecture shape: cohesive modular application
Spike scope: API/domain/persistence/background processing only
Frontend: deferred
Cloud/provider: deferred
```

The spike shall validate architecture risk, not create the production application by momentum.

## Selection evidence required before production approval

- successful representative transaction spike;
- persistence/history/effective-date proof;
- customer-isolation strategy comparison;
- authority/access enforcement proof;
- durable async/outbox retry/reconciliation proof;
- migration bulk/reconciliation proof;
- representative reporting/query test;
- deployment/observability proof;
- developer experience/build/test evidence;
- security review of selected approach;
- TCO/operational implications.

## Official references

- .NET support policy: https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core
- .NET downloads/releases: https://dotnet.microsoft.com/en-us/download/dotnet
- Java downloads / LTS information: https://www.oracle.com/java/technologies/downloads/
- Spring Boot system requirements: https://docs.spring.io/spring-boot/system-requirements.html
- Node.js release status: https://nodejs.org/en/about/previous-releases
- TypeScript documentation: https://www.typescriptlang.org/docs/
- Go releases: https://go.dev/doc/devel/release
- PostgreSQL versioning/support: https://www.postgresql.org/support/versioning/

## References

- `Solution_architecture_document.md`
- `Architecture_decision_records_ADRs.md`
- `Technical_spikes.md`
- `../F_Requirements_Analysis/Non-functional_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture / Engineering | Initial technology evaluation; .NET 10 + PostgreSQL 18 selected only for first architecture spike |