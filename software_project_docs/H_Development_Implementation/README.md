# Development & Implementation controlled documents

This directory contains the live controlled Development & Implementation documents for the NuBlox Enterprise Operating System programme.

The corresponding reusable templates are maintained under [`../../software_project_docs_templates/H_Development_Implementation/`](../../software_project_docs_templates/H_Development_Implementation/). Templates are reference structures only; NuBlox decisions, plans and implementation evidence belong here.

## Current baseline

| ID | Document | Status | Version |
|---|---|---|---|
| `NBEOS-H-001` | [Development plan](Development_plan.md) | Draft | 0.1 |
| `NBEOS-H-002` | [Product backlog](Product_backlog.md) | Draft | 0.23 |
| `NBEOS-H-003` | [Dependency management document](Dependency_management_document.md) | Draft | 0.4 |
| `NBEOS-H-004` | [First vertical slice definition](First_vertical_slice_definition.md) | Draft | 0.1 |
| `NBEOS-H-005` | [First vertical slice verification](First_vertical_slice_verification.md) | Draft | 0.3 |
| `NBEOS-H-006` | [Second vertical slice definition](Second_vertical_slice_definition.md) | Draft | 0.1 |

## Production foundation status

The controlled production foundation is physically separate from disposable spikes and includes:

```text
src/NuBlox.Kernel/
src/NuBlox.Identity/
src/NuBlox.Authority/
src/NuBlox.Audit/
src/NuBlox.Observability/
src/NuBlox.Persistence.PostgreSql/
src/NuBlox.Runtime.PostgreSql/
src/NuBlox.Api/
src/NuBlox.WorkProducts.Domain/
src/NuBlox.WorkProducts.Application/
src/NuBlox.WorkProducts.Infrastructure.PostgreSql/
src/NuBlox.Enterprise.Domain/
src/NuBlox.Enterprise.Application/
src/NuBlox.Enterprise.Infrastructure.PostgreSql/
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

GitHub Actions runs the identical verifier with PostgreSQL 18.6 available for integration and real runtime-composition verification.

## First governed vertical slice

`DEV-201` selected **Governed Work Product — Create, Review, Approve and Issue** as the first representative production workflow. Its controlled definition is [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md).

`DEV-202` through `DEV-211` provide and verify:

```text
verified HTTP Principal/Tenant context
        ↓
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
        ↓
minimised authoritative audit + correlated telemetry
        ↓
real production runtime composition
        ↓
restricted-role PostgreSQL persistence + tenant RLS
```

`DEV-208` is complete. [`First_vertical_slice_verification.md`](First_vertical_slice_verification.md) version 0.3 records all twelve NBEOS-H-004 acceptance criteria as technically satisfied for this bounded slice.

DEV-211 also demonstrated why real composition-root verification is mandatory: the first runtime registration selected the base Work Product repository, allowing create/submit but causing the real decision route to return `404`. The production graph now registers the governed repository and the complete real HTTP create → submit → approve → issue path passes against PostgreSQL.

## Governed Organisation Party increment

`DEV-308` is the current bounded Wave-3 engineering increment. Its controlled definition is [`Second_vertical_slice_definition.md`](Second_vertical_slice_definition.md), backed by accepted [`ADR-022`](../G_Architecture_Design/adr/ADR-022-canonical-party-organisation-identity.md).

The invariant is:

```text
Tenant != Party

Party
├── Person
└── Organisation

Employee / Customer / Supplier / Partner / Contractor
= relationship or contextual role, not Party kind
```

DEV-308 implements **Organisation only** and is still **In Progress** until the branch passes the complete production verifier. The implementation boundary includes stable `PartyId`, module-owned PostgreSQL persistence, tenant RLS, verified-context create/read HTTP operations, fail-closed configured administration access and payload-minimised authoritative audit evidence.

Person, workforce structures and customer/supplier/commercial relationship semantics remain outside this increment.

## Current controlled direction

The first bounded production slice is technically verified; this does not approve the complete product or convert Draft/Candidate requirements into an approved product baseline.

DEV-308 establishes a canonical Organisation identity prerequisite already supported by the accepted enterprise-model evidence. It does **not** claim that primary customer discovery has selected commercial continuity or another Wave-3 capability as the market priority.

After DEV-308 is verified, the next relationship-bearing business outcome must be selected from controlled product/customer evidence. Commercial/project continuity remains a strong candidate because it tests NuBlox's end-to-end operating/delivery thesis, but that priority remains evidence-gated.

## Development rule

Spike code remains disposable experimental code. It is not the production application and must not be copied into production by momentum. Proven patterns are reimplemented through accepted ADRs, controlled product requirements, module ownership, production tests and normal security/quality gates.

Draft/Candidate requirements remain subject to controlled business/customer validation and refinement; implementation must not silently convert every candidate requirement into permanent product semantics.

## References

- [`../G_Architecture_Design/Architecture_decision_records_ADRs.md`](../G_Architecture_Design/Architecture_decision_records_ADRs.md)
- [`../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md`](../F_Requirements_Analysis/Requirements_traceability_matrix_RTM.md)
- [`Development_plan.md`](Development_plan.md)
- [`Product_backlog.md`](Product_backlog.md)
- [`Dependency_management_document.md`](Dependency_management_document.md)
- [`First_vertical_slice_definition.md`](First_vertical_slice_definition.md)
- [`First_vertical_slice_verification.md`](First_vertical_slice_verification.md)
- [`Second_vertical_slice_definition.md`](Second_vertical_slice_definition.md)
