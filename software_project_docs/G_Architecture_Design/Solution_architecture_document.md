# Solution architecture document

**Section:** G_Architecture_Design  
**Document ID:** NBEOS-G-004  
**Document Type:** Solution architecture document  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Architecture  
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
**Related Documents:** `Architecture_vision.md`, `Architecture_definition.md`, `System_context_diagram.md`, `Architecture_decision_records_ADRs.md`, `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/G_Architecture_Design/Solution_architecture_document.md`  
**Storage Location:** `software_project_docs/G_Architecture_Design/Solution_architecture_document.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Describe the first requirements-derived solution architecture hypothesis for NuBlox and the alternatives that must be tested before architecture approval.

## Architecture status

> **PROVISIONAL — architecture exploration, not an approved implementation baseline.**

The current requirements strongly favour transactional consistency, traceability, controlled customer variation and integration, but key scale/tenancy/customer workflow decisions remain open.

## Primary architectural drivers

- coherent end-to-end transactional/business workflows;
- strong information integrity and history;
- explicit permission/authority/decision controls;
- external integration and reconciliation;
- customer variation without code forks;
- migration feasibility;
- enterprise security/auditability;
- operational supportability;
- ability to evolve product/domain boundaries;
- avoid premature distributed complexity.

## Candidate solution shape

The leading hypothesis for the **first product implementation** is a cohesive application with explicit internal domain/module boundaries, backed by transactional relational persistence, durable asynchronous processing for external/long-running work, separate binary/object storage where needed, and well-defined external APIs/adapters.

```text
Users / External Participants
           ↓
   Web / API Boundary
           ↓
 Application Use Cases
           ↓
 Explicit Business Modules / Domain Logic
           ↓
 Transactional Persistence + History/Audit
           ↓
 Durable Outbox / Background Processing
           ↓
 Integration Adapters / External Systems

Supporting stores/services where justified:
Object/blob storage • Search/index • Cache • Analytics/read models

Cross-cutting:
Identity • Access • Authority • Configuration • Observability • Security
```

This deliberately **does not** mean one giant unstructured codebase. Internal responsibility boundaries and dependency rules are architectural requirements even if initial deployment is cohesive.

## Decomposition options

### Option A — Cohesive modular application / modular monolith

Characteristics:
- one primary deployable application initially;
- explicit internal modules/boundaries;
- shared transaction capability where business operations require it;
- in-process calls for most internal interactions;
- durable messaging/events for integration/background work as needed.

Advantages:
- lower operational/distributed-system complexity;
- strong transactional consistency;
- easier end-to-end refactoring while domain model is still evolving;
- faster first-product development/testing;
- simpler local development and deployment.

Risks:
- boundaries can erode without dependency enforcement;
- one deployment can grow large;
- poorly designed shared database access can create coupling.

### Option B — Distributed services / microservices from first release

Advantages:
- independent deployment/scaling/technology where genuinely required;
- strong service ownership boundaries if well defined.

Risks at current maturity:
- distributed transactions/consistency complexity;
- network/reliability/versioning overhead;
- slower domain refactoring while semantics remain unresolved;
- higher observability/deployment/test burden;
- premature service boundaries may encode wrong domain assumptions.

### Option C — Generic metadata/workflow platform as primary runtime

Characteristics:
- generic object/field/relationship/workflow definitions drive most application behaviour.

Advantages:
- apparent flexibility/configurability;
- potentially rapid creation of simple information/workflow structures.

Risks:
- weak domain invariants and type semantics;
- business rules become scattered configuration;
- query/reporting/performance complexity;
- difficult refactoring and validation;
- risk of forcing every business concept into one generic abstraction.

### Provisional position

**Option A is the leading implementation hypothesis** because the current requirements prioritise integrity, evolving semantics and product delivery speed over independent service deployment. This is **not accepted** until architecture spikes and NFR assumptions support it.

A metadata/configuration subsystem may still exist, but it should extend controlled business semantics rather than replace them wholesale.

## Persistence hypothesis

A transactional relational data store is the leading primary persistence hypothesis because requirements include:

- stable identities/relationships;
- referential integrity;
- atomic business operations;
- rich querying/reporting;
- history/effective data;
- migrations and reconciliation.

Additional stores should be introduced only for a clear requirement:

- object storage for binary work products/assets;
- search engine/index if relational/search performance is insufficient;
- cache for measured performance needs;
- analytics warehouse/lake when analytical isolation/scale requires it.

No database product is selected by this document.

## Transaction and asynchronous-work hypothesis

Use synchronous/transactional processing for operations that must complete as one business unit.

Use durable asynchronous processing where work:

- crosses unreliable external systems;
- is long-running;
- requires retry/backoff;
- requires decoupled notification;
- performs heavy background work;
- must survive process restarts.

A transactional outbox or equivalent pattern is a candidate mechanism for reliably bridging committed business state to asynchronous integration/event processing. This requires ADR/spike validation.

## Integration architecture hypothesis

Integrations should use adapter boundaries around approved external contracts. Business/application logic should not depend directly on vendor-specific APIs where avoidable.

Integration must retain:

- business correlation;
- authority/source mapping;
- request/result status;
- retry/idempotency controls;
- reconciliation;
- observability/audit evidence.

## Work-product/document architecture hypothesis

Structured metadata/business state should remain governed in the transactional model. Binary files/models may be stored in object/content storage or retained specialist systems depending on the approved workflow.

The architecture must not assume that storing a file equals governing the underlying business object or professional deliverable.

## Reporting architecture hypothesis

Initial operational reporting should favour governed query/read models close to authoritative transactional information. Separate analytical stores should be introduced where analytical workload/latency/scale requires isolation.

Definitions/measures belong to governed semantics, not ad hoc dashboard formulas.

## Security architecture direction

The solution must support:

- external identity integration;
- server-side access enforcement;
- separate business authority checks where applicable;
- customer/organisation isolation model still to be selected;
- protected secrets/credentials;
- immutable/tamper-resistant audit where required;
- environment separation;
- secure administration;
- least-privilege service identities.

## Configuration/extensibility direction

Configuration should be typed/governed, versioned where material, validated before activation and unable to bypass mandatory product/security invariants.

Extension mechanisms should have explicit contracts/sandboxing/upgrade rules if later required. Direct customer source-code forks are not the default model.

## Major unresolved architecture decisions

1. Customer/tenant isolation model.
2. Initial deployment model and scale assumptions.
3. Runtime/language/framework.
4. Primary relational database product.
5. Schema/domain modularisation strategy.
6. Identity provider/authentication design.
7. Business authority/delegation model.
8. Workflow/orchestration implementation.
9. Eventing/background job technology.
10. Object/content storage approach.
11. Search strategy.
12. API/event standards and versioning.
13. Infrastructure/cloud/provider.
14. CI/CD and environment strategy.
15. Observability stack.
16. Analytics architecture.

## Architecture validation spikes required

Before approving the first implementation architecture, create small technical spikes for the highest-risk questions, including:

- representative transaction spanning multiple internal responsibilities;
- effective-dated/history-heavy business data;
- authority/access enforcement;
- durable integration failure/retry/reconciliation;
- configurable business variation;
- customer isolation strategy;
- migration throughput/reconciliation;
- representative reporting/query workload.

## References

- `Architecture_vision.md`
- `Architecture_definition.md`
- `System_context_diagram.md`
- `Architecture_decision_records_ADRs.md`
- `../F_Requirements_Analysis/Software_requirements_specification_SRS.md`
- `../F_Requirements_Analysis/Non-functional_requirements_specification.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Architecture | Initial solution options and provisional cohesive modular architecture hypothesis |