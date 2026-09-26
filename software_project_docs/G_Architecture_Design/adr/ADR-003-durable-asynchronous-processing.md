# ADR-003 — Durable asynchronous processing

**Section:** G_Architecture_Design  
**ADR:** ADR-003  
**Decision:** Durable asynchronous processing and external consequence delivery  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Integration Engineering  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** FR-018, FR-030, FR-032–FR-035; NFR-RES-001–003; NFR-AUD-001–004  
**Related ADRs:** ADR-001, ADR-002, ADR-007, ADR-011, ADR-016, ADR-017, ADR-018, ADR-020, ADR-021  
**Evidence:** SPIKE-005, SPIKE-009, `Technical_spikes.md`, `Proof_of_concept_report.md`  
**Supersedes:** None

## Context

NuBlox contains business operations whose authoritative local state may commit before an external or long-running consequence can safely complete. Examples include notifications, external-system publication, integration calls, asynchronous projections and other consequences that must not be silently lost because a process stops after the local transaction commits.

Directly calling an external provider inside the authoritative business transaction couples database availability to provider availability and cannot make a remote side effect participate atomically in the local PostgreSQL transaction. Conversely, committing local state and then creating an in-memory job leaves a failure window in which the business operation succeeds but the required consequence disappears.

SPIKE-005 directly exercised a durable integration/outbox pattern. SPIKE-009 then verified process-level cancellation, lease expiry, restart recovery, completion and correlated operational evidence across separate processes. The spikes establish sufficient evidence to make the production architectural decision while keeping the spike code disposable.

## Decision

NuBlox will use **durable asynchronous intents**, implemented with a **transactional outbox or an equivalent transactional durable-work record**, whenever a committed NuBlox business operation requires an external, delayed or independently retried consequence that cannot participate in the same authoritative transaction.

The authoritative business change and the durable intent are committed atomically in the same relational transaction when loss of the consequence would violate the business or operational requirement.

A separate worker/process claims durable intents and attempts delivery outside the originating business transaction.

```text
business command
     ↓
authoritative state change
+ durable consequence intent
     ↓
ONE local database transaction commits
     ↓
worker claims durable intent
     ↓
provider / integration / notification attempt
     ↓
success | retry | terminal failure / reconciliation
```

## Core invariants

1. **No silent-loss window.** A required consequence is durably recorded before the originating transaction is considered complete.
2. **Atomic local intent.** Where the consequence belongs to the same business operation, state and outbox intent commit together.
3. **At-least-once execution is assumed.** Workers and providers may observe retries; product behaviour must therefore tolerate duplicate delivery attempts.
4. **Idempotency is explicit.** Every externally meaningful delivery has a stable intent/idempotency identity or another documented duplicate-control mechanism.
5. **Claims are recoverable.** A worker claim/lease must expire or otherwise become safely recoverable after process failure.
6. **Failure is visible.** Repeated or terminal failure becomes queryable operational/business support state; work must not disappear into logs alone.
7. **Reconciliation is first-class.** Delivery outcome can be related back to the originating Tenant, business subject and durable intent.
8. **Tenant isolation applies.** Durable intents and delivery state carry Tenant identity and are subject to the same server/database isolation discipline as the originating records.
9. **Audit and telemetry remain distinct.** Delivery telemetry supports operations; authoritative business evidence remains governed by ADR-018.
10. **Secrets are not payload defaults.** Sensitive credentials/tokens are resolved at delivery time from controlled configuration/secret facilities rather than persisted casually in outbox payloads.

## Durable intent model

A production durable intent must carry enough information to recover and reconcile without reconstructing trust from transient process memory.

Minimum platform semantics:

- durable intent identifier;
- Tenant identifier;
- consequence/integration type;
- originating subject type and identifier;
- correlation identifier where available;
- idempotency/deduplication key where required;
- payload or governed payload reference containing only the information needed for delivery;
- creation timestamp;
- current delivery state;
- attempt count;
- next-attempt timestamp where retryable;
- claim/lease owner and expiry while processing;
- last failure classification/safe diagnostic where applicable;
- completed/reconciled timestamp where applicable.

The exact table/module contract is implementation-specific. A domain module may own its business-specific intent while shared infrastructure supplies claiming, retry and telemetry primitives. A universal untyped enterprise event model is not required by this ADR.

## Delivery states

The production implementation may use names appropriate to the owning module, but it must preserve the following semantic states:

```text
PENDING
   ↓ claim
PROCESSING
   ├─ success → COMPLETED
   ├─ retryable failure → PENDING / RETRY_SCHEDULED
   └─ terminal/exhausted failure → FAILED / ATTENTION_REQUIRED
```

A process crash while `PROCESSING` must not strand the record permanently. Lease expiry or another explicit recovery mechanism returns the work to a claimable/reconcilable condition.

## Transaction boundary

The outbox intent is written in the **same PostgreSQL transaction** as the authoritative state change when the external consequence is required because of that state transition.

Rejected pattern:

```text
COMMIT business state
then
insert outbox row
```

because a process failure between those steps loses the required consequence.

Accepted pattern:

```text
BEGIN
  mutate authoritative state
  append required durable intent
COMMIT
```

The later provider call is deliberately outside the database transaction.

## Idempotency and duplicate effects

NuBlox does not assume exactly-once distributed delivery.

The delivery boundary must use one or more of:

- stable provider idempotency key;
- NuBlox intent identifier accepted by the provider;
- provider-result reconciliation before repeating a non-idempotent action;
- receiver-side deduplication contract;
- business-specific duplicate detection where the remote system lacks idempotency support.

A retry-safe worker is not sufficient if the remote side effect itself can be duplicated.

## Claiming and concurrency

Workers must claim work atomically so multiple workers do not intentionally process the same available row at the same time.

For PostgreSQL the implementation may use provider capabilities such as row locking with `FOR UPDATE SKIP LOCKED`, atomic state transitions and lease timestamps. Provider-specific SQL remains behind infrastructure boundaries under ADR-017/021.

A claim must include a bounded lease/recovery policy. Permanent process ownership of an intent is prohibited.

## Retry policy

Retry behaviour is governed by failure classification rather than an unbounded tight loop.

The implementation distinguishes at minimum:

- success;
- retryable/transient failure;
- terminal/provider rejection or invalid request;
- ambiguous outcome requiring reconciliation.

Retry scheduling must be bounded and observable. Backoff/jitter policy may vary by provider and consequence class.

## Reconciliation and support

Operators and authorised product workflows must be able to determine:

- what business operation created the intent;
- whether delivery is pending, processing, completed or failed;
- how many attempts occurred;
- the last safe failure classification;
- whether a provider outcome was recorded;
- whether manual/reconciliation action is required.

Logs alone are not the reconciliation system of record.

## Security and tenant isolation

Durable work is tenant-owned unless explicitly platform-global.

Production implementation must:

- carry verified `TenantId` from the originating application context;
- enforce tenant-aware persistence and RLS where applicable;
- prevent a worker from manufacturing Tenant context from untrusted payload values;
- avoid storing credentials/secrets in intent payloads;
- avoid logging sensitive payload content;
- preserve least-privilege provider/runtime credentials.

## Observability

Every delivery attempt should correlate:

```text
Tenant
→ originating business subject
→ durable intent
→ worker claim/attempt
→ provider request/result
```

ADR-016 governs technical traces/metrics/logs. Useful metrics include queue depth, oldest pending age, retry count, failed/attention-required count, processing lease age and delivery latency.

Telemetry does not replace authoritative business/audit evidence under ADR-018.

## Module ownership

The first implementation for the Governed Work Product slice may place the durable issue consequence in the WorkProducts-owned schema because the intent is created atomically by the Work Product issue transaction.

Shared worker infrastructure may later be extracted when multiple modules need the same claim/retry mechanics. That extraction must not transfer ownership of each module's business semantics to a universal integration repository.

## Alternatives considered

### Synchronous external call inside the business transaction

Rejected as the general solution. Remote systems cannot join the local PostgreSQL transaction reliably, provider latency/failure extends transaction duration and rollback cannot reliably undo a completed remote effect.

### Commit then enqueue in memory

Rejected because a crash after commit and before enqueue silently loses the consequence.

### Database polling with no durable state machine

Rejected. A boolean `sent` flag does not provide sufficient claim recovery, retry classification, failure visibility or reconciliation semantics.

### Distributed transaction / exactly-once assumption

Rejected as the platform baseline. It is not generally available across external providers and creates false guarantees. NuBlox designs for at-least-once attempts plus explicit idempotency/reconciliation.

### Introduce a message broker as mandatory system of record now

Rejected as a required first step. A broker may later be introduced for scale/fan-out, but the originating durable intent still requires a loss-safe transactional boundary unless the broker participates in an equivalently reliable mechanism.

## Consequences

### Positive

- required external consequences survive process crashes;
- business commit and intent have one local integrity boundary;
- retries and failures become explicit/queryable;
- workers can scale independently from request processing;
- provider outages do not require holding business database transactions open;
- architecture remains compatible with future brokers or specialised integration infrastructure.

### Costs / risks

- at-least-once execution requires explicit duplicate-control design;
- outbox growth requires retention/archive policy;
- stuck/failed work requires operational attention tooling;
- provider-specific ambiguous outcomes can require reconciliation logic;
- worker claim/lease implementation must be carefully concurrency-tested.

## Verification requirements

Production durable-consequence verification must include:

- authoritative state and durable intent commit atomically;
- rollback leaves neither state transition nor intent partially committed;
- process restart does not lose pending/processing work;
- an expired/abandoned claim is recoverable;
- concurrent workers do not intentionally claim the same available intent;
- retryable failure schedules another attempt without creating a second intent;
- stable idempotency identity survives retries;
- successful delivery becomes completed/reconciled state;
- terminal failure becomes visible/actionable state;
- tenant isolation prevents cross-tenant claim/read/mutation;
- correlation links subject, intent and attempt without exposing secrets.

## Review triggers

Review this ADR when:

- a broker/event-stream becomes a committed platform dependency;
- multiple modules require shared worker infrastructure;
- a provider offers transactional/idempotency guarantees that materially change the pattern;
- delivery volume/latency requires partitioning or specialised queue storage;
- retention/residency rules require a different payload/evidence strategy.

## Decision outcome

**Accepted:** NuBlox uses transactional durable intents/outbox-or-equivalent for required external or independently retried consequences. The originating business state and durable intent commit atomically; workers operate with recoverable claims, at-least-once delivery semantics, explicit idempotency, retry/failure visibility and reconciliation. The pattern is implemented inside explicit module/infrastructure boundaries and may later integrate with a broker without weakening the local no-silent-loss guarantee.
