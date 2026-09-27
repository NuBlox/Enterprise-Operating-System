# NuBlox Enterprise Operating System

![NuBlox](brand/NuBlox_Logo_On_Light_Background.svg)

NuBlox is an enterprise operating system being defined and developed through a controlled software-development programme.

## Programme approach

The programme proceeds top-down from business intent through product definition, market/customer evidence, requirements, architecture, design, implementation, verification, release and operation.

The repository separates **document templates** from **live controlled programme documents**:

- [`software_project_docs_templates/`](software_project_docs_templates/) — reusable project-document templates;
- [`software_project_docs/`](software_project_docs/) — live NuBlox controlled documents and current programme decisions/evidence.

Experimental architecture code is kept separately under [`spikes/`](spikes/). Successful spike code is evidence and is **not** production application code by default.

Governed external/package foundations are kept separate from product architecture decisions. Production code must adopt dependencies only through the controlled dependency/architecture path.

## Current controlled programme

The live programme index is:

[`software_project_docs/README.md`](software_project_docs/README.md)

Controlled Draft work spans:

- Enterprise / Pre-Project strategy and evidence;
- Project Initiation;
- Requirements & Analysis;
- Architecture & Design;
- Development & Implementation.

Architecture experimentation completed `SPIKE-001` through `SPIKE-009`; the resulting evidence was converted into explicit production architecture decisions and a controlled .NET 10 / PostgreSQL 18 production foundation.

The Development & Implementation baseline is maintained at:

[`software_project_docs/H_Development_Implementation/`](software_project_docs/H_Development_Implementation/)

Its controlled first-slice evidence includes:

- [`Development_plan.md`](software_project_docs/H_Development_Implementation/Development_plan.md);
- [`Product_backlog.md`](software_project_docs/H_Development_Implementation/Product_backlog.md);
- [`First_vertical_slice_definition.md`](software_project_docs/H_Development_Implementation/First_vertical_slice_definition.md);
- [`First_vertical_slice_verification.md`](software_project_docs/H_Development_Implementation/First_vertical_slice_verification.md).

## Production architecture implemented so far

The controlled production codebase now includes:

```text
NuBlox.Kernel
NuBlox.Identity
NuBlox.Authority
NuBlox.Audit
NuBlox.Observability
NuBlox.Persistence.PostgreSql
NuBlox.Runtime.PostgreSql
NuBlox.Api
NuBlox.WorkProducts.Domain
NuBlox.WorkProducts.Application
NuBlox.WorkProducts.Infrastructure.PostgreSql
```

The production verification entry point is:

```bash
bash scripts/verify-production.sh
```

GitHub Actions executes the same verification path with PostgreSQL 18.6 for persistence, isolation and real runtime-composition tests.

## First governed vertical slice

The first bounded production workflow is **Governed Work Product — Create, Review, Approve and Issue**.

The verified path covers:

```text
verified Principal/Tenant HTTP context
        ↓
Work Product + Revision
        ↓
submit / ReviewRequest
        ↓
contextual authority-backed approval
        ↓
issue evidence / supersession
        ↓
attention / evidence drill-through
        ↓
durable delivery intent + recovery
        ↓
authoritative audit + correlated telemetry
        ↓
real HTTP → application → PostgreSQL runtime graph
```

`NBEOS-H-005` version 0.3 records all twelve controlled first-slice acceptance criteria as technically satisfied. This is bounded technical acceptance, not approval of the entire product or the full Draft/Candidate requirements catalogue.

## Repository areas

| Area | Purpose |
|---|---|
| `src/` | Controlled production source |
| `tests/` | Production unit/integration/composition verification |
| `software_project_docs/` | Live controlled programme documents and decisions |
| `software_project_docs_templates/` | Reusable document templates |
| `spikes/` | Disposable architecture experiments and verification harnesses |
| `packages/` | Governed package assets where applicable |
| `docs/` | Supporting/reference documentation |
| `brand/` | Canonical NuBlox brand assets |

## Working principles

- Define the business and product need before defining the software solution.
- Maintain traceability from strategy through requirements, architecture, implementation, testing, release and operation.
- Keep hypotheses visibly separate from approved decisions.
- Record important assumptions, risks, evidence, approvals and changes.
- Prefer complete business outcomes over disconnected feature/module lists.
- Keep implementation decisions subordinate to controlled business, product and technical requirements.
- Enforce tenant isolation, authority and audit server-side; UI behavior is never the sole enforcement boundary.
- Do not promote spike code into the product without an explicit ADR/design/review/verification path.
- Do not treat component-test success as end-to-end acceptance when real runtime composition has not been exercised.

## Current phase

The programme is in **controlled production implementation and validation**.

The production foundation and first bounded vertical slice are technically verified. The next sequence is intentionally product-evidence-led:

```text
verified first production slice
        ↓
validate / baseline next customer or product priority
        ↓
trace requirement → architecture/design → acceptance criteria
        ↓
implement through production boundaries
        ↓
verify real runtime composition + security/audit/operability
        ↓
controlled capability expansion
```

The next business capability is not selected merely because it is technically convenient. Requirements, customer research and commercial evidence continue to determine downstream priority.

## Brand assets

Canonical NuBlox brand assets are maintained under [`brand/`](brand/). Brand assets should be referenced from that directory rather than duplicated into project-document or application folders.

## Licence

Proprietary. See [`LICENSE`](LICENSE).
