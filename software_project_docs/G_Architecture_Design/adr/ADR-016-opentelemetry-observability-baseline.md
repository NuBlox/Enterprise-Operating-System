# ADR-016 — Observability stack

**Section:** G_Architecture_Design  
**ADR:** ADR-016  
**Decision:** Production observability and telemetry interoperability baseline  
**Status:** ACCEPTED  
**Decision Date:** 2026-09-26  
**Owner:** NuBlox Architecture / Engineering / Operations  
**Approver:** NuBlox programme owner  
**Classification:** Internal  
**Related Requirements:** NFR-SEC-006, NFR-PERF-004, NFR-OPS-001–004; API-006  
**Related ADRs:** ADR-008, ADR-011, ADR-012, ADR-018  
**Evidence:** SPIKE-009; current OpenTelemetry/OTLP standards and .NET implementation  
**Supersedes:** None

## Context

NuBlox needs diagnostic evidence across HTTP requests, background work, persistence and integrations without binding the product to one monitoring vendor. The system must correlate technical telemetry with stable business/integration identifiers while keeping technical logs distinct from business audit evidence.

SPIKE-009 demonstrated correlated structured logging. Production requires a standard model for traces, metrics, logs, health and export.

## Decision

NuBlox will use **OpenTelemetry as the production telemetry interoperability standard** for traces, metrics and logs, with **OTLP as the preferred vendor-neutral export boundary**.

The initial .NET implementation uses standard .NET diagnostics primitives (`ActivitySource`, `Meter`, `ILogger`) integrated with the OpenTelemetry .NET SDK rather than introducing a proprietary instrumentation API throughout product code.

No observability backend/vendor is selected by this ADR.

## Required telemetry model

### Traces

- inbound HTTP requests participate in trace context propagation;
- outbound HTTP/database/integration activity is correlated where instrumentation exists;
- background work starts or continues trace context according to causal semantics;
- custom spans are reserved for materially useful application/platform operations, not every method call.

W3C Trace Context is the propagation baseline where supported.

### Metrics

NuBlox records bounded-cardinality operational measurements such as:

- request rate/latency/error rate;
- background job queue/processing/failure state;
- dependency latency/failure;
- runtime/process health;
- domain/business metrics only where their operational purpose, ownership and cardinality are explicit.

Tenant/user/object IDs are not used as unbounded metric labels.

### Logs

Application logs are structured and emitted through `ILogger` with trace/correlation context available to the logging pipeline.

Logs must not contain:

- credentials or access/refresh tokens;
- raw authentication headers;
- unnecessary request/response payloads;
- secrets;
- high-risk personal/business data merely for convenience.

Sensitive diagnostic fields require explicit justification and classification.

## Correlation

NuBlox distinguishes:

- `TraceId` / `SpanId` — distributed technical trace correlation;
- request/support correlation identifier — optional human-facing diagnostic reference;
- `PrincipalId` — authenticated application actor where safe/required;
- `TenantId` — verified tenant context where safe/required;
- work/integration/business identifiers — only where useful and classification permits.

A correlation identifier is never authentication proof, tenant proof or business audit evidence by itself.

## Health model

Production services expose operational health endpoints separate from business APIs:

- **liveness** — process is running and can service basic execution;
- **readiness** — service can accept the intended workload based on critical dependencies/configuration;
- dependency-specific diagnostics remain access controlled and must not leak secrets/topology unnecessarily.

Health endpoints are not placed under the versioned business API solely for consistency.

## Export and backend

Applications emit OpenTelemetry-compatible telemetry and export through OTLP where practical.

The recommended production topology is:

```text
NuBlox service
  -> OpenTelemetry SDK
  -> OTLP
  -> OpenTelemetry Collector / compatible gateway
  -> approved telemetry backend(s)
```

Direct vendor exporters may be used when operationally justified but product instrumentation must remain based on standard primitives/semantic conventions rather than vendor APIs.

## Audit separation

Technical telemetry is **not** the authoritative business audit/evidence store.

ADR-018 governs business audit evidence. A material action may produce both:

```text
business audit event  -> retained governed evidence
technical telemetry   -> operational diagnosis
```

The two may share correlation identifiers but have separate retention, integrity, access and lifecycle rules.

## Sampling and retention

- trace sampling may be adjusted by environment/load while preserving required incident/debug coverage;
- metrics are aggregated with bounded cardinality;
- log/trace retention is configured by operational/security policy and is not hard-coded into product logic;
- security/audit retention requirements do not rely on sampled traces.

## Initial .NET baseline

The production .NET foundation will use the stable OpenTelemetry .NET packages for required signals/exporters. As of 2026-09-26, OpenTelemetry .NET `1.19.1` is the current stable release.

Package adoption remains centrally versioned and reviewed under `NBEOS-H-003`.

## Alternatives considered

### Vendor-specific SDK everywhere
Rejected because it creates unnecessary provider lock-in and complicates exit/dual-backend operation.

### Logs only
Rejected because logs alone do not provide sufficient distributed causality, performance measurements or standard telemetry correlation.

### Custom NuBlox telemetry protocol
Rejected because OpenTelemetry already provides mature cross-vendor telemetry models, SDKs and export protocols.

## Consequences

### Positive
- vendor-neutral instrumentation and export boundary;
- consistent traces/metrics/logs across .NET components;
- backend choice remains operational/procurement decision;
- direct alignment with supportability/performance/security logging NFRs.

### Costs / risks
- telemetry volume/cardinality needs active control;
- semantic-convention and package changes require review;
- instrumentation can expose sensitive data if developers add uncontrolled attributes/log fields;
- collector/backend operations still require deployment design.

## Verification requirements

Production verification must cover as applicable:

- trace context propagates across HTTP/background boundaries;
- structured log records include expected correlation without payload/secret leakage;
- metrics use approved bounded labels;
- liveness/readiness semantics are deterministic;
- telemetry export failure does not corrupt business state;
- disabling/exporter failure does not make required business audit evidence disappear.

## Standards references

- OpenTelemetry Specification: https://opentelemetry.io/docs/specs/otel/
- OTLP: https://opentelemetry.io/docs/specs/otlp/
- W3C Trace Context: https://www.w3.org/TR/trace-context/
- OpenTelemetry .NET: https://www.nuget.org/packages/OpenTelemetry

## Decision outcome

**Accepted:** OpenTelemetry is the NuBlox production observability interoperability standard, using .NET standard diagnostics primitives and OTLP as the preferred export boundary. Telemetry remains separate from authoritative business audit evidence and no monitoring vendor is selected by this decision.
