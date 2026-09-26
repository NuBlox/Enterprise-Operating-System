# 04 — Object Model

## Purpose

The object model defines the things that exist in the enterprise because work, governance and relationships require them.

NuBlox should not begin from database tables or UI forms. It should begin from business identity, meaning and lifecycle.

## Object categories

Candidate categories include:

### Identity and organisation

Examples: Person, Organisation, Organisational Unit, Position, Work Relationship.

### Work and context

Examples: Work Item, Project, Programme, Contract, Service, Site, Asset, Product, Case, Work Package.

### Commercial and transactional

Examples: Opportunity, Order, Requisition, Purchase Order, Invoice, Payment, Budget, Forecast, Cost, Commitment.

### Information and work product

Examples: Document, Drawing, Model, Report, Specification, Schedule, Cost Plan, Record, Submission, Deliverable.

### Governance and control

Examples: Policy, Requirement, Risk, Issue, Change, Decision, Approval, Authority, Delegation, Control, Audit Finding.

### Evidence

Examples: Evidence Item, Inspection Result, Review Record, Sign-off, Acceptance Record, Attestation, Event Record.

These are examples for investigation, not a final canonical catalogue.

## Identity versus classification

An object must have a stable identity independent of its current classification, state or context.

For example, changing the status of a Contract must not create a new Contract identity. Reclassifying an Organisation as a customer and supplier should not create duplicate Organisations if the business entity is the same.

## Thing / field / value / relationship

Earlier NuBlox work proposed a metadata-driven pattern:

```text
Thing
→ Thing Fields
→ Thing Values
→ Thing Relationships
```

This is retained as an **evidence hypothesis** to test. The clean-slate model must determine which concepts deserve first-class semantics and which can be safely metadata-driven.

A completely generic `Thing` model is not automatically desirable if it destroys domain invariants, referential meaning or performance. Conversely, hard-coding every object type is not acceptable if the business requires governed extensibility.

## Relationships are first-class

Many enterprise truths are relationships rather than attributes.

Examples:

- Person occupies Position;
- Position reports to Position;
- Organisation is party to Contract;
- Work Item implements Activity;
- Method governs Activity execution;
- Deliverable satisfies Requirement;
- Decision approves Change;
- Asset is located at Site;
- Supplier fulfils Purchase Order;
- Document revises Document;
- Evidence supports Decision.

Relationships may themselves require lifecycle, effective dates, provenance and authority.

## Lifecycle

Objects should declare lifecycle semantics explicitly.

Lifecycle may include:

- draft/planned;
- active/effective;
- under review;
- approved/accepted;
- superseded;
- closed/completed;
- cancelled/withdrawn;
- archived/retained.

No universal status enum should be assumed. Lifecycle is object-specific but should use shared lifecycle primitives where semantics genuinely align.

## Version, revision and history

The architecture must distinguish:

- identity of the business object;
- mutable state/history;
- revision/version identity;
- effective dates;
- baseline/configuration membership;
- event/audit history.

For some objects, revision is business-significant (for example controlled drawings or specifications). For others, event history is sufficient.

## Ownership and custody

The model must distinguish where relevant:

- business owner;
- accountable Position;
- current assignee;
- custodian;
- record owner;
- data steward;
- approving authority.

## Object definition test

Before creating a first-class object type, answer:

1. Does it have an independent business identity?
2. Does it have lifecycle or history?
3. Is it related to multiple other enterprise concepts?
4. Does it carry permissions, authority or ownership?
5. Is it independently searchable/reportable?
6. Can it exist independently of a single UI/process execution?
7. Does legislation, contract, standard or enterprise practice give it distinct meaning?

If not, it may instead be an attribute, value object, relationship, event or work-product subtype.

## Open questions

1. Which objects form the minimum enterprise kernel?
2. Which relationships need first-class identities?
3. What is the extensibility boundary between typed objects and metadata-defined objects?
4. How should effective dating, revision and baselines interact?
5. Which objects are tenant-owned versus globally defined?
6. How are records/retention separated from operational object state?
7. What are the universal provenance requirements?
