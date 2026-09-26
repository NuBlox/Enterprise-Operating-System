# NuBlox Enterprise Operating System

![NuBlox](brand/NuBlox_Logo_On_Light_Background.svg)

NuBlox is an enterprise operating system being defined and developed through a controlled software-development programme.

## Programme approach

The programme proceeds top-down from business intent through product definition, market/customer evidence, requirements, architecture, design, implementation, verification, release and operation.

The repository separates **document templates** from **live controlled programme documents**:

- [`software_project_docs_templates/`](software_project_docs_templates/) — reusable project-document templates;
- [`software_project_docs/`](software_project_docs/) — live NuBlox controlled documents and current programme decisions/evidence.

Experimental architecture code is kept separately under [`spikes/`](spikes/). Successful spike code is evidence and is **not** production application code by default.

NuBlox-maintained third-party foundations and governed forks are maintained under [`packages/mastered/`](packages/mastered/), with provenance and synchronisation controls kept separate from product architecture decisions.

## Current controlled programme

The live programme index is:

[`software_project_docs/README.md`](software_project_docs/README.md)

Controlled Draft work now spans:

- Enterprise / Pre-Project strategy and evidence;
- Project Initiation;
- Requirements & Analysis;
- Architecture & Design;
- Development & Implementation entry planning.

Architecture experimentation has completed the bounded `SPIKE-001` through `SPIKE-009` programme. The current engineering transition is to convert that evidence into explicit architecture decisions and then establish a production source/test/toolchain foundation without copying synthetic spike semantics into the product.

The Development & Implementation baseline is maintained at:

[`software_project_docs/H_Development_Implementation/`](software_project_docs/H_Development_Implementation/)

Its initial controlled documents are:

- [`Development_plan.md`](software_project_docs/H_Development_Implementation/Development_plan.md);
- [`Product_backlog.md`](software_project_docs/H_Development_Implementation/Product_backlog.md).

## Repository areas

| Area | Purpose |
|---|---|
| `software_project_docs/` | Live controlled programme documents and decisions |
| `software_project_docs_templates/` | Reusable document templates |
| `spikes/` | Disposable architecture experiments and verification harnesses |
| `packages/mastered/` | NuBlox-governed/mastered packages with provenance controls |
| `docs/` | Supporting/reference documentation |
| `brand/` | Canonical NuBlox brand assets |

## Working principles

- Define the business and product need before defining the software solution.
- Maintain traceability from strategy through requirements, architecture, implementation, testing, release and operation.
- Use the repository templates as the default structure for controlled project documents.
- Keep hypotheses visibly separate from approved decisions.
- Record important assumptions, risks, evidence, approvals and changes.
- Prefer complete business outcomes over feature lists or module boundaries.
- Avoid creating overlapping documents when one controlled artifact can satisfy the requirement clearly.
- Keep implementation decisions subordinate to approved business, product and technical requirements.
- Treat mastered packages as governed implementation assets, not automatic enterprise architecture decisions.
- Do not promote spike code into the product without an explicit ADR/design/review/verification path.

## Current phase

The programme is entering **Development & Implementation planning**, while requirements, architecture and market/customer evidence remain controlled Drafts and continue to mature in parallel.

The immediate engineering sequence is:

```text
completed architecture spikes
        ↓
implementation-critical ADR decisions
        ↓
production source/test/toolchain foundation
        ↓
first approved end-to-end business workflow
        ↓
verified vertical product slice
        ↓
controlled capability expansion
```

The first production slice will be selected from validated requirements; the synthetic subjects used by architecture spikes will not define the production business taxonomy.

## Brand assets

Canonical NuBlox brand assets are maintained under [`brand/`](brand/). Brand assets should be referenced from that directory rather than duplicated into project-document or application folders.

## Licence

Proprietary. See [`LICENSE`](LICENSE).
