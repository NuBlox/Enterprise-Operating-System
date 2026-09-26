# 02 — Work Model

## Purpose

The work model defines what the enterprise actually does and how that work is represented independently of any existing application module structure.

## Work is contextual

Work may arise from different enterprise contexts, including:

- operating the organisation;
- serving a customer;
- executing a project or programme;
- fulfilling a contract;
- delivering a service;
- producing or maintaining a product;
- operating or maintaining an asset;
- performing regulatory, assurance or governance obligations;
- responding to an event, issue, risk or change.

The context determines the subject and constraints of work, but not the identity of the worker.

## Core work concepts

### Outcome

The business result the enterprise is trying to achieve.

### Work Package / Scope

A governed grouping of work with defined boundaries, accountability and expected outputs.

### Activity

A business action or unit of work that contributes to an outcome. Activity taxonomy will be derived from evidence rather than imported automatically from earlier repositories.

### Task / Work Item

An executable instance of work assigned to one or more actors in a defined context.

### Method

The governed specification for **how** an Activity is performed.

A Method may define:

- purpose and applicability;
- prerequisites;
- sequence/steps;
- required inputs;
- required competence;
- required authority/permission;
- tools;
- decision points;
- outputs/work products;
- handoffs;
- evidence;
- controls;
- lifecycle/version/effective dates.

### Assignment

The relationship allocating a Work Item to an actor. Assignment does not itself create authority or competence.

### Handoff

The controlled transfer of work, responsibility, information or work product between actors or contexts.

### Completion / Acceptance

Work is complete only when its defined completion conditions are met. Some work also requires acceptance by another actor.

## Execution chain

The initial work execution chain is:

```text
Business need / trigger
→ Outcome
→ Scope / Work Package
→ Activity
→ Method
→ Work Item
→ Assignment
→ Tool use / object change
→ Review / Decision / Approval where required
→ Work Product / Transaction / Record
→ Evidence
→ Handoff / Acceptance / Completion
```

This is a conceptual chain, not a fixed workflow for every type of work.

## Reusable versus contextual work

The architecture must separate:

- **reusable definition** — Activity, Method, work-product type, control requirement;
- **contextual instance** — a particular Work Item, Deliverable, Transaction, Decision or Handoff.

A process definition must not be confused with one execution of that process.

## Functional governance and delivery

Evidence from previous NuBlox work suggests a useful distinction that must be tested rather than assumed:

- **functional governance** — defining and controlling how a capability should operate;
- **functional delivery** — performing the actual work of that capability.

If retained, both must use the same underlying enterprise and work objects rather than form parallel systems.

## Managerial scope

Management visibility should arise from enterprise accountability and reporting relationships, not merely from dashboard sharing. The model must be able to answer:

- What work is mine?
- What work belongs to Positions I manage?
- What work belongs to a project/contract/site I govern?
- Which work am I authorised to approve?
- Which work requires my attention because of risk, exception or escalation?

## Work-product orientation

For every Activity, the architecture must determine whether it:

- creates a work product;
- changes an enterprise object;
- records a transaction;
- makes a decision;
- produces evidence;
- triggers another Activity;
- hands work to another actor/context;
- or some combination of these.

This prevents activity catalogues from becoming lists of verbs with no executable business meaning.

## Open questions

1. What are the universal work-context types?
2. Is `Work Item` the universal executable unit or are several first-class execution types required?
3. How are recurring/continuous activities represented?
4. How do event-driven, case-driven, process-driven and project-driven work differ?
5. Which acceptance semantics are universal?
6. How should work dependencies and cross-functional handoffs be modelled?
7. Which work definitions are global versus tenant-configurable?
