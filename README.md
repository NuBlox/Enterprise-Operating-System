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

The authoritative index is [`docs/DOCUMENT-REGISTER.md`](docs/DOCUMENT-REGISTER.md). Document identity, lifecycle, ownership and dependency rules are governed by [`docs/08-document-governance.md`](docs/08-document-governance.md).

Current governing documents:

- [`docs/00-product-definition.md`](docs/00-product-definition.md)
- [`docs/01-enterprise-model.md`](docs/01-enterprise-model.md)
- [`docs/02-work-model.md`](docs/02-work-model.md)
- [`docs/03-capability-model.md`](docs/03-capability-model.md)
- [`docs/04-object-model.md`](docs/04-object-model.md)
- [`docs/05-control-model.md`](docs/05-control-model.md)
- [`docs/06-evidence-reconciliation.md`](docs/06-evidence-reconciliation.md)
- [`docs/07-wave-1-enterprise-identity-reconciliation.md`](docs/07-wave-1-enterprise-identity-reconciliation.md)
- [`docs/08-document-governance.md`](docs/08-document-governance.md)

## Reconciliation status

### Wave 1 — Enterprise identity and structure — ACCEPTED

The clean-slate baseline now establishes Tenant, Party, Person, Organisation, Organisational Unit, Position, Work Relationship, Position Occupancy, Reporting Relationship and the separation of Responsibility, Permission, Authority and Delegation.

Important clean-slate corrections include:

- Tenant is a platform context, not a Party type;
- Employee is a Work Relationship state/role, not a Party identity type;
- Client and Supplier are contextual business relationships, not Party identity types;
- Work Relationship is separate from assignment/deployment;
- reporting is Position-to-Position and does not itself grant Permission, Authority or record ownership;
- enterprise Person identity is separate from application account/authentication identity.

### Wave 2 — Work execution — NEXT

Activity, Method, Work Item, Assignment, workflow, lifecycle, handoff, work products, Decisions and Evidence will be reconciled next.

## Current repository phase

**Architecture before application.**

There is intentionally no application scaffold, database migration history, runtime framework or UI in this repository yet.

No application implementation is authorised until the clean-slate work/execution model is also accepted.

## Evidence

Prior NuBlox repositories, market-tool research, industry references, user/job research, standards and product benchmarks are evidence to test this architecture. They do not define it by default.

## Licence

This repository is proprietary. See [`LICENSE`](LICENSE).
