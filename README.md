# NuBlox Enterprise Operating System

NuBlox is being designed from first principles as an enterprise operating system: a single governed environment in which organisations can operate the business and deliver the work they exist to perform.

This repository is the **clean-slate product foundation**. It deliberately does not inherit application code, database schema, routes, navigation, Function catalogues, activity catalogues, or implementation assumptions from earlier NuBlox repositories.

## Clean-slate rule

Earlier NuBlox work is evidence, not authority.

Nothing is canonical here because it existed in V1, V2 or V3. Concepts are promoted only after they survive review against business reality, the enterprise model, observed work, market evidence and implementation constraints.

## Product questions, in order

1. What is NuBlox?
2. What kinds of parties participate in an enterprise?
3. How is an organisation structured and governed?
4. What work does the enterprise need to perform?
5. What objects exist because of that work?
6. What capabilities, methods and tools are required?
7. How are authority, permissions, decisions, evidence and change controlled?
8. Only then: what software architecture, persistence, routes and UI implement the model?

## Governing documents

- [`docs/00-product-definition.md`](docs/00-product-definition.md)
- [`docs/01-enterprise-model.md`](docs/01-enterprise-model.md)
- [`docs/02-work-model.md`](docs/02-work-model.md)
- [`docs/03-capability-model.md`](docs/03-capability-model.md)
- [`docs/04-object-model.md`](docs/04-object-model.md)
- [`docs/05-control-model.md`](docs/05-control-model.md)
- [`docs/06-evidence-reconciliation.md`](docs/06-evidence-reconciliation.md)

## Current repository phase

**Architecture before application.**

There is intentionally no application scaffold, database migration history, runtime framework or UI in this repository yet.

## Evidence

Prior NuBlox repositories, market-tool research, industry references, user/job research, standards and product benchmarks will be brought into `docs/evidence/` only as evidence to test this architecture. They do not define it by default.

## Licence

This repository is proprietary. See [`LICENSE`](LICENSE).
