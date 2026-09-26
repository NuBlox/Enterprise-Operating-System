# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.18 |
| `NBEOS-H-003` | [Dependency management document](Dependency_management_document.md) | Draft | 0.4 |
| `NBEOS-H-004` | [First vertical slice definition](First_vertical_slice_definition.md) | Draft | 0.1 |
| `NBEOS-H-005` | [First vertical slice verification and traceability](First_vertical_slice_verification_and_traceability.md) | Draft | 0.1 |

## Production foundation status

The controlled production foundation is physically separate from disposable spikes and includes:

```text
src/NuBlox.Kernel/
src/NuBlox.Identity/
src/NuBlox.Audit/
src/NuBlox.Observability/
src/NuBlox.Persistence.PostgreSql/
src/NuBlox.Api/
src/NuBlox.WorkProducts.*
        ↓
tests/*
        ↓
scripts/verify-production.sh
        ↓
.github/workflows/production-foundation.yml
```

The repository root pins the approved .NET SDK through `global.json`, defines shared production build rules through `Directory.Build.props`, and centrally controls direct NuGet package versions through `Directory.Packages.props`.

## Completed production-foundation controls

The implementation baseline has completed the initial production foundation through `DEV-108`, including:

- cohesive modular application and module-owned data boundaries;
- .NET 10 LTS / C# production server runtime;
- PostgreSQL 18 / Npgsql production persistence boundary;
- migration journal/checksum, tenant session and PostgreSQL RLS verification;
- provider-neutral Principal/Tenant request-context resolution;
- append-oriented authoritative audit evidence;
- OpenTelemetry trace/metric/log correlation and health primitives;
- ASP.NET Core HTTP host with `/api/v1`, OpenAPI, RFC 9457 Problem Details and operational health endpoints;
- one integrated production CI verification path.

Production verification runs with:

```bash
bash scripts/verify-production.sh
```

GitHub Actions runs the identical production verifier with PostgreSQL 18.6 available for integration tests.

## First governed vertical slice

`DEV-201` selects **Governed Work Product — Create, Review, Approve and Issue** as the first representative production workflow.

The controlled implementation definition is [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md).

Implementation has progressed through:

```text
DEV-202 governed Work Product + Revision records/persistence
        ↓
DEV-203 submission/review work state
        ↓
DEV-204 authority-backed review/approval decision
        ↓
DEV-205 work-product/revision/evidence linkage and issue
        ↓
DEV-206 attention/management view + drill-through
        ↓
DEV-207 durable downstream consequence
        ↓
DEV-208 consolidated traceability/verification
```

The consolidated requirement → ADR/design → code → test → CI record is [`First_vertical_slice_verification_and_traceability.md`](First_vertical_slice_verification_and_traceability.md).

The slice remains deliberately work-product-type neutral so the same core model can later support documents, drawings, models, specifications, reports, submissions and other governed outputs across functional governance, functional delivery and built-environment domains without prematurely hard-coding the full enterprise taxonomy.

## Development rule

The spike code remains disposable experimental code. It is not the production application and must not be copied into the production codebase by momentum. Proven patterns are reimplemented through accepted ADRs, controlled product requirements, module ownership, production tests and normal security/quality gates.

The first vertical slice is an implementation/validation baseline. Draft/Candidate requirements remain subject to controlled business/customer validation and refinement; implementation must not silently convert every candidate requirement into permanent product semantics.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../F_Requirements_Analysis/`](../F_Requirements_Analysis/)
- [`Development_plan.md`](Development_plan.md)
- [`Product_backlog.md`](Product_backlog.md)
- [`Dependency_management_document.md`](Dependency_management_document.md)
- [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md)
- [`First_vertical_slice_verification_and_traceability.md`](First_vertical_slice_verification_and_traceability.md)
