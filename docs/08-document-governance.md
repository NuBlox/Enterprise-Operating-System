# 08 — Document Governance

## Status

Published

## Purpose

This document defines how NuBlox governing documents are identified, created, reviewed, published, superseded and retired.

The clean-slate repository uses documentation to govern architecture and evidence without allowing documentation volume to substitute for product understanding or delivery.

## Core rule

A document enters the governing register only when there is an evidenced reason for it to exist.

The repository must not pre-authorise a large document estate merely because an earlier NuBlox repository, benchmark product or planning exercise proposed one.

## Document role versus lifecycle

Document role and document lifecycle are separate concepts.

### Document role

A role describes what kind of work the document performs. Initial roles are:

- `ARCH` — governing architecture;
- `EVID` — evidence and reconciliation material;
- `DEC` — architecture/product decision record;
- `GOV` — governance policy and assurance;
- `PROD` — product implementation specification, introduced only after architecture permits implementation;
- `OPS` — operational runbook/specification, introduced only when operational capability exists;
- `COM` — commercial material;
- `BRAND` — brand and presentation standards.

No separate `KERN` role is assumed at this stage. If a platform-kernel concept survives capability and object reconciliation, kernel specifications may later become a justified document family.

### Lifecycle status

Every governed document has exactly one lifecycle status:

- `DRAFT` — being authored; not authoritative;
- `IN_REVIEW` — submitted for governance review;
- `PUBLISHED` — authoritative and in force;
- `SUPERSEDED` — replaced by another published document but retained for history;
- `RETIRED` — no longer in force and not replaced directly;
- `PLANNED` — registered because a justified need exists, but not yet authored.

`REFERENCE`, `OPERATIONAL` and `SPECIFIED` are not lifecycle states. Those ideas belong to document role, evidence source or implementation state.

## Identifier scheme

Every governed document receives a unique immutable identifier:

```text
<ROLE>-NNN
```

Examples:

```text
ARCH-001
EVID-003
DEC-004
GOV-002
```

An identifier is never reused, even after a document is retired or superseded.

## Required metadata

Every governed document must state:

- document ID;
- title;
- role;
- lifecycle status;
- version;
- owner Position or accountable governance role;
- reviewing body where applicable;
- effective date when Published;
- dependencies;
- supersedes / superseded-by links where applicable.

The clean-slate repository may temporarily use named governance roles rather than occupied Positions until the operating organisation exists.

## Required structure

A governing document should contain, where applicable:

1. Purpose
2. Scope
3. Definitions
4. Governing model or decision
5. Invariants / mandatory rules
6. Dependencies
7. Evidence / rationale
8. Open questions
9. Change history

Tables should be used where they clarify structured relationships, but prose is preferred when business meaning would otherwise be flattened into labels.

## Authority hierarchy

The following precedence applies:

```text
Published governing architecture / decision
    > published product specification
    > evidence / benchmark / reference material
    > implementation experiment
    > UI or database convenience
```

Evidence can challenge governing architecture and trigger review. It does not silently override it.

## Dependency discipline

Dependencies are explicit.

A downstream document must not redefine an upstream concept without either:

1. changing the upstream governing document through review; or
2. recording a new decision that explicitly supersedes the earlier rule.

Circular document dependencies are not permitted.

## Decision records

A Decision Record is required when a choice:

- establishes or changes an architectural invariant;
- selects between materially different enterprise semantics;
- creates a long-lived technical constraint;
- changes tenancy, security, identity, authority or data-ownership boundaries;
- promotes a major V3 concept into the clean-slate architecture;
- rejects a previously canonical concept that would otherwise be expected to survive.

Decision Records must include:

- context;
- decision;
- alternatives considered;
- evidence;
- consequences;
- affected governing documents.

## Evidence documents

Evidence documents are never authoritative merely because they are detailed.

Evidence should identify:

- source;
- source date/version;
- observed concept or behaviour;
- provenance;
- relevance;
- confidence/limitations;
- reconciliation state where applicable.

The reconciliation states defined in `docs/06-evidence-reconciliation.md` remain authoritative for imported concepts.

## Change control

For a Published document:

1. proposed change is recorded;
2. affected dependencies are identified;
3. evidence and consequences are assessed;
4. revised document enters `IN_REVIEW`;
5. reviewing body approves/rejects;
6. approved version becomes `PUBLISHED` with effective date;
7. replaced version is retained in history or Git history and marked `SUPERSEDED` where separately retained.

Minor editorial changes that do not alter semantics may use a lighter review path, but must not disguise architectural changes.

## Anti-sprawl rule

A document must not be created solely because:

- an earlier suite listed it;
- a competitor has an equivalent manual;
- a possible future module might need it;
- a repository folder would otherwise be empty;
- it makes a roadmap appear more complete.

Before registering a Planned document, answer:

1. What decision, behaviour or evidence does it govern?
2. Which existing document cannot carry that responsibility?
3. What downstream work depends on it?
4. Why is it needed now?

If those questions cannot be answered, do not register the document.

## Relationship to the earlier Full Document Suite

The earlier NuBlox Full Document Suite is retained as evidence for documentation coverage and governance mechanics.

The clean-slate repository promotes from it:

- a single governing register;
- unique document identifiers;
- explicit ownership;
- explicit dependencies;
- lifecycle states;
- change history;
- retained superseded/retired decisions;
- separation of reference material from governing authority.

It does not automatically promote:

- the 139-document inventory;
- F01-F29 or D01-D16 assumptions;
- `Sector Pack` terminology;
- predeclared kernel service boundaries;
- Perspective-specific implementation sequencing;
- module-based pricing assumptions;
- route, registry or provisioning schemas;
- duplicate ADR numbering from the earlier suite.

Those remain evidence subject to later reconciliation.
