# 01 — Enterprise Model

**Status:** CANONICAL CONCEPTUAL BASELINE  
**Detailed reconciliation:** [07 — Wave 1 Enterprise Identity & Structure Reconciliation](07-wave-1-enterprise-identity-reconciliation.md)

## Purpose

The enterprise model describes the real-world structure within which work occurs. It must remain valid independently of Functions, modules, pages, database tables or implementation technology.

Wave 1 evidence reconciliation has established the core enterprise identity and organisational semantics below. Implementation details remain intentionally unresolved.

---

# Core enterprise concepts

## Tenant

A **Tenant** is the NuBlox platform isolation, configuration and commercial context within which a subscribed enterprise customer operates.

A Tenant may contain one or more Organisations. It is not itself a Person, Organisation or Party type.

```text
Tenant != Party != Organisation
```

## Party

A **Party** is an abstract enterprise participant capable of entering relationships, holding contextual roles or being party to transactions, obligations, contracts or work.

The initial Party kinds are:

```text
PARTY
├── PERSON
└── ORGANISATION
```

Employee, client/customer and vendor/supplier are not Party identity types. They are Work Relationships or contextual business relationship roles.

## Person

A **Person** is a stable human enterprise identity.

A Person exists independently of application credentials, employment, Position or current business role. A Person may participate in multiple enterprise relationships over time without being duplicated.

```text
Person != User Account != Employee != Position
```

## Organisation

An **Organisation** is a separately identifiable organisational Party capable of participating in enterprise relationships.

The same Organisation may simultaneously be a client, supplier, partner or employer in different contexts without creating duplicate Organisation identities.

A **Legal Entity** is an Organisation with recognised legal identity in a jurisdiction. Legal-entity status is a governed Organisation classification/profile, not an Organisational Unit.

## Organisational Unit

An **Organisational Unit** is a structural subdivision of an Organisation used to organise accountability, resources, Positions, reporting or control scope.

An Organisational Unit is not automatically a legal entity.

## Position

A **Position** is an enduring organisational seat established for work and accountability to exist whether occupied or vacant.

A Position:

- belongs to organisational structure;
- survives occupant changes;
- may be planned, vacant, filled or abolished over time;
- may define authorised capacity;
- participates in structural reporting;
- is a primary anchor for organisational Responsibility and Authority.

```text
Position != Person != Job Profile != Access Role
```

## Work Relationship

A **Work Relationship** is the effective-dated legal, contractual or equivalent basis under which a Person performs work for or on behalf of an Organisation.

The first universal relationship families are Employment and Contingent Engagement.

Secondment, global assignment, project assignment, acting arrangements and primary/secondary-working preferences are assignment/deployment concepts layered over a Work Relationship; they are not Work Relationship identities merely because they affect how someone works.

## Position Occupancy

A **Position Occupancy** is the effective-dated allocation of a Work Relationship to a Position.

It may carry allocation/FTE, effective dates and occupancy status. Ending an Occupancy does not end the Position and does not necessarily end the Work Relationship.

## Reporting Relationship

A **Reporting Relationship** is an effective-dated structural relationship between Positions.

NuBlox must support matrix structures, so more than one meaningful simultaneous reporting relationship may exist where business semantics require it.

Reporting:

- is Position-to-Position;
- does not transfer record ownership;
- does not itself grant Permission;
- does not itself grant business Authority;
- may contribute to calculated management scope only through explicit access/scope policy.

---

# Enterprise structure and workforce chain

The canonical conceptual chain is:

```text
Tenant
→ Organisation
→ Organisational Unit
→ Position

Person
→ Work Relationship with Organisation
→ Position Occupancy
→ Position

Position
→ Reporting Relationship
→ Position
```

This is deliberately not a database schema.

---

# Business roles are relationships, not identities

The clean-slate model rejects role labels as identity types when they describe how a Party participates in a context.

```text
Employee
→ Person participating through an Employment Work Relationship

Client / Customer
→ contextual relationship role of a Party

Vendor / Supplier
→ contextual relationship role of a Party

Tenant
→ platform context containing participating Organisations and people
```

Changing any of those roles must not create a duplicate Person or Organisation.

---

# Accountability and control distinctions

The enterprise model must keep the following concepts distinct:

- **Responsibility** — what an organisational actor is accountable for;
- **Assignment** — work allocated to an actor;
- **Permission** — what an authenticated principal may technically access or perform;
- **Authority** — which decisions, approvals, commitments or representations may validly be made;
- **Competence** — whether an actor is qualified/capable to perform particular work;
- **Ownership** — business accountability for a particular enterprise object/domain;
- **Reporting** — structural management relationship;
- **Delegation** — governed exercise/transfer of existing Authority.

No single `role` field may represent all of these.

Access Roles may package Permissions for administration. They do not become organisational Roles or Responsibilities merely because they are called roles.

---

# Enterprise identity versus application identity

Enterprise identity and application identity are separate:

```text
Person
!= User Account / Principal
!= Authentication Credential
!= Session
```

A Person can exist without login credentials. Disabling an account does not delete the Person or historical enterprise relationships. Authentication methods may change without changing enterprise identity.

Service identities and machine principals must not be forced into the Person model.

---

# Enterprise participation contexts

People and Organisations may act within multiple simultaneous contexts, including:

- organisational operations;
- projects and programmes;
- contracts and commercial engagements;
- service delivery;
- product development or production;
- site operations;
- asset operation and maintenance;
- regulatory or assurance activity;
- customer or supplier relationships.

A context narrows where work occurs. It must not silently redefine enterprise identity, Work Relationship, structural Position or global organisational truth.

---

# Effective dating

Enterprise relationships are temporal. The model must support:

- planned future changes;
- current effective state;
- historic state;
- superseded relationships;
- temporary assignments/delegations;
- vacancies;
- simultaneous valid relationships where business reality permits them.

Current state must be derivable from effective history rather than produced by overwriting historical truth.

---

# Canonical invariants

1. `Tenant != Party != Organisation`.
2. Party initially resolves to Person or Organisation.
3. Employee, Client and Supplier are relationships/roles, not Party identity types.
4. `Person != User Account != Work Relationship != Position`.
5. `Organisation != Organisational Unit`.
6. Legal Entity is an Organisation concept.
7. Position survives occupant changes and may exist vacant.
8. Position Occupancy is effective-dated and separate from Work Relationship.
9. Work Relationship is separate from assignment/deployment.
10. Reporting is Position-to-Position and may support matrix structures.
11. Reporting does not itself grant Permission, Authority or ownership.
12. Responsibility, Assignment, Permission, Authority, Competence and Ownership are distinct.
13. Business-role changes must not duplicate Person or Organisation identities.
14. Enterprise identity is separate from application identity.

---

# Remaining design questions before persistence

Wave 1 deliberately leaves implementation-dependent questions unresolved:

1. Is Party a persisted root identity or a strict polymorphic abstraction over Person and Organisation?
2. What tenant-local versus cross-tenant identity matching is permitted?
3. Which Organisation classifications beyond Legal Entity are universal?
4. Which reporting semantics are universally required for matrix organisations?
5. What is the universal Responsibility definition/assignment structure?
6. What scope algebra is required for Permission and Authority?
7. How should machine/service principals participate in authorisation?
8. Which workforce terms are universal versus jurisdiction-specific?

These questions must be answered explicitly before persistence design; they must not be decided accidentally by table shape.