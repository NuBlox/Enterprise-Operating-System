# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.19 |
| `NBEOS-H-003` | [Dependency management document](Dependency_management_document.md) | Draft | 0.4 |
| `NBEOS-H-004` | [First vertical slice definition](First_vertical_slice_definition.md) | Draft | 0.1 |
| `NBEOS-H-005` | [First vertical slice verification](First_vertical_slice_verification.md) | Draft | 0.1 |

## Production foundation status

The controlled production foundation is physically separate from disposable spikes and includes:

```text
src/NuBlox.Kernel/
src/NuBlox.Identity/
src/NuBlox.Authority/
src/NuBlox.Audit/
src/NuBlox.Observability/
src/NuBlox.Persistence.PostgreSql/
src/NuBlox.Api/
src/NuBlox.WorkProducts.Domain/
src/NuBlox.WorkProducts.Application/
src/NuBlox.WorkProducts.Infrastructure.PostgreSql/
        ↓
tests/*
        ↓
scripts/verify-production.sh
        ↓
.github/workflows/production-foundation.yml
```

The repository root pins the approved .NET SDK through `global.json`, defines shared production build rules through `Directory.Build.props`, and centrally controls direct NuGet package versions through `Directory.Packages.props`.

Production verification runs with:

```bash
bash scripts/verify-production.sh
```

GitHub Actions runs the identical production verifier with PostgreSQL 18.6 available for integration tests.

## First governed vertical slice

`DEV-201` selects **Governed Work Product — Create, Review, Approve and Issue** as the first representative production workflow. Its controlled definition is [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md).

The implementation through `DEV-207` now provides:

```text
governed Work Product + Revision
        ↓
submission + ReviewRequest routing
        ↓
contextual authority-backed decision
        ↓
approval-linked issue evidence + supersession
        ↓
Principal-scoped attention + source drill-through
        ↓
transactional durable issue consequence
        ↓
lease recovery / retry / reconciliation
```

`DEV-208` has begun the formal acceptance/traceability gate. The evidence report is [`First_vertical_slice_verification.md`](First_vertical_slice_verification.md).

That verification correctly found two remaining end-to-end composition gaps rather than falsely declaring the slice complete:

- `DEV-209` — compose WorkProducts into the versioned HTTP boundary using only verified Principal/Tenant request context, with RFC 9457 and request-tampering negative tests;
- `DEV-210` — compose material WorkProducts operations with authoritative `NuBlox.Audit` evidence and correlated `NuBlox.Observability` telemetry.

The controlled completion sequence is therefore:

```text
DEV-209 WorkProducts HTTP + verified request context
DEV-210 WorkProducts audit + telemetry integration
        ↓
DEV-208 final twelve-criterion acceptance/traceability verification
```

## Development rule

Spike code remains disposable experimental code. It is not the production application and must not be copied into production by momentum. Proven patterns are reimplemented through accepted ADRs, controlled product requirements, module ownership, production tests and normal security/quality gates.

The first vertical slice remains an implementation/validation baseline. Draft/Candidate requirements remain subject to controlled business/customer validation and refinement; implementation must not silently convert every candidate requirement into permanent product semantics.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`](../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md)
- [`Development_plan.md`](Development_plan.md)
- [`Product_backlog.md`](Product_backlog.md)
- [`Dependency_management_document.md`](Dependency_management_document.md)
- [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md)
- [`First_vertical_slice_verification.md`](First_vertical_slice_verification.md)
