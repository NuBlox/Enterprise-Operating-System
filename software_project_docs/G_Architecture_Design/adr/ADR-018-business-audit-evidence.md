# ADR-018 — Audit and event evidence

**Section:** G_Architecture_Design  
**ADR:** ADR-018  
**Decision:** Authoritative business audit/evidence model  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Security / Data Governance  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** NFR-AUD-001–004; NFR-SEC-005–006; FR-016, FR-018, FR-022, FR-039–FR-040  
**Related ADRs:** ADR-003, ADR-007, ADR-008, ADR-016, ADR-017, ADR-020  
**Evidence:** SPIKE-001, SPIKE-004, SPIKE-005, SPIKE-009; canonical control model  
**Supersedes:** None

## Context

NuBlox must preserve attributable evidence of material business changes, decisions and control actions. Technical logs are useful for diagnosis but can be sampled, redacted, rotated or moved between monitoring backends; they are therefore not sufficient as the authoritative record of business accountability.

The architecture must distinguish operational telemetry from business audit evidence while still allowing correlation between them.

## Decision

NuBlox will maintain an **authoritative append-oriented business audit/evidence record** for material actions and decisions, separate from technical logs/traces/metrics.

Audit evidence is written as part of the business consistency boundary where loss of that evidence would make the business transaction incomplete.

## Minimum evidence model

A material audit event records, as applicable:

- stable audit event identifier;
- verified `TenantId`;
- actor application `PrincipalId` or governed service/integration identity;
- linked Person/Position/Authority evidence where business meaning requires it;
- action/event type using a governed stable code;
- subject/object type and stable identifier;
- business/work context identifier(s);
- technical recorded timestamp in UTC;
- business effective time where distinct;
- outcome/status;
- correlation/trace identifier for diagnostic linkage;
- reason/rationale/reference where the business process requires it;
- evidence/document references rather than unnecessary duplicated payloads;
- provenance/source application or integration.

The event schema is typed/governed. It is not an unbounded free-form logging sink.

## Atomicity

When an audit event is required to prove a successful state change, the authoritative state change and corresponding audit event are committed atomically where they share the same consistency boundary.

Rejected outcome:

```text
business change committed
+
audit write silently lost
```

For effects across durable asynchronous boundaries, the local intent/evidence is committed before external delivery and later provider outcomes are appended/reconciled rather than overwriting the original event.

## Immutability and correction

Ordinary product operations do not update or delete existing authoritative audit events.

Corrections are represented by new events/evidence that reference the earlier record and explain supersession/correction. This preserves provenance rather than rewriting history.

Retention/disposal may eventually remove records under approved policy/legal rules, but such disposal is a governed records-management operation, not ordinary CRUD.

## Security and access

- audit evidence is tenant-scoped and subject to ADR-007 isolation;
- only approved application/platform components may append governed event types;
- ordinary application runtime identities receive no unrestricted update/delete rights to the audit store;
- support access is controlled and attributable;
- sensitive audit details follow data-classification/privacy rules;
- audit query/export does not imply access to unrelated business content.

Where practical, database privileges and schema ownership reinforce append-oriented behaviour.

## Audit versus telemetry

```text
Authoritative audit/evidence
- business/accountability purpose
- integrity and retention controlled
- no sampling
- append/correction semantics
- may be transactionally required

Technical telemetry
- operational diagnosis/performance purpose
- sampling/aggregation allowed
- retention can differ
- exported through OpenTelemetry/OTLP
```

Both may share `TraceId`, correlation ID and stable business/work identifiers where classification permits.

## Audit versus domain events

Not every internal domain event is automatically a retained audit record, and not every audit event needs to drive asynchronous integration.

The product explicitly decides whether an occurrence is:

- authoritative audit/evidence;
- internal domain notification;
- durable integration/outbox intent;
- technical telemetry;
- or more than one of these with clear responsibilities.

A universal event bus is not implied by this ADR.

## Payload minimisation

Audit events capture enough information to prove what occurred without indiscriminately copying whole request/response payloads or full object snapshots.

Where detailed evidence is needed, the audit event should reference a governed retained artefact/version/hash where practical.

Secrets, passwords and authentication tokens are prohibited.

## Integrity enhancement

The initial implementation relies on append-oriented database permissions, relational constraints, controlled write APIs and backup/restore protections.

If customer/regulatory evidence later requires cryptographic tamper evidence, NuBlox may add chained hashes, signatures, immutable/WORM storage or external evidence anchoring without replacing the logical audit model.

## Alternatives considered

### Use application logs as the audit trail
Rejected because logs have different retention, access, sampling and integrity semantics and are not safely part of every business transaction.

### Full event sourcing for all state
Rejected by ADR-002. NuBlox needs strong audit/evidence but does not require every domain object to be reconstructed exclusively from events.

### Mutable audit rows
Rejected because updates destroy provenance and make investigations unable to distinguish original from corrected evidence.

## Consequences

### Positive
- strong separation between operational diagnosis and accountable business evidence;
- material state changes can guarantee corresponding evidence;
- corrections preserve history;
- supports later regulatory/records controls without redesigning every domain.

### Costs / risks
- audit storage grows continuously until governed retention is defined;
- event taxonomy and access controls require ownership;
- careless payload design can duplicate sensitive data;
- cross-system evidence still requires durable correlation/reconciliation.

## Implementation implications

The production persistence foundation will include a platform-owned audit/evidence boundary with:

- typed append contract;
- tenant-aware audit records;
- stable actor/subject/correlation fields;
- append-only application semantics;
- restricted database privileges where practical;
- integration with application transaction orchestration.

Audit implementation must not introduce a dependency from domain/kernel projects onto the PostgreSQL provider package; persistence remains behind infrastructure boundaries.

## Verification requirements

Tests must demonstrate as applicable:

- material business write and required audit append commit/rollback together;
- cross-tenant audit access is denied;
- ordinary runtime cannot mutate/delete prior audit records;
- service identities remain attributable distinctly from human Person identity;
- correction/supersession appends new evidence rather than changing old evidence;
- telemetry exporter/logging failure does not remove authoritative audit evidence;
- prohibited secrets/tokens are not persisted in audit payloads.

## Review triggers

Review when:

- legal/regulated record requirements mandate stronger cryptographic immutability;
- retention/legal-hold/disposal policies are approved;
- evidence volumes require partition/archive architecture;
- cross-system non-repudiation becomes a product requirement.

## Decision outcome

**Accepted:** NuBlox maintains separate, append-oriented authoritative business audit/evidence records for material actions and decisions, transactionally coupled where required and never replaced by technical logs. Corrections append new evidence; telemetry and integration events remain distinct concerns even when correlated.
