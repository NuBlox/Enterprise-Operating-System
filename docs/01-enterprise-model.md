# 01 — Enterprise Model

## Purpose

The enterprise model describes the real-world structure within which work occurs. It must be valid before Functions, modules, pages or database tables are designed.

## Core enterprise concepts

### Party

A Party is an entity that participates in enterprise relationships or transactions.

Potential Party classifications include organisations and people acting as employer, employee, customer, supplier, partner, authority or other business participant. Earlier NuBlox Party-Type decisions are evidence for later reconciliation, not imported canon.

### Person

A human identity. A Person may participate in multiple relationships with one or more Organisations over time.

### Organisation

A legally, commercially or operationally recognised body. Organisation structure may include legal entities, business units, departments, teams or other governed organisational units, but those concepts must be separated where their semantics differ.

### Organisational Unit

A structural subdivision of an Organisation used to group accountability, work, resources or reporting. It is not automatically a legal entity.

### Position

An enduring organisational seat carrying accountability, responsibility and structural reporting relationships. A Position may exist while vacant and may be occupied by different people over time.

### Work Relationship

The relationship under which a Person performs work for or on behalf of an Organisation. Employment, contingent engagement, secondment and other relationship types must be represented explicitly rather than inferred from user accounts.

### Position Occupancy

The effective-dated relationship between a Person/Work Relationship and a Position. Occupancy history must survive changes in employment or organisational structure.

### Reporting Relationship

A governed relationship between Positions, distinct from ad-hoc collaboration, project responsibility or approval authority.

## Enterprise participation contexts

People and Organisations may act within multiple simultaneous contexts, including:

- organisational operations;
- project/programme work;
- contracts and commercial engagements;
- service delivery;
- product development or production;
- site operations;
- asset operation and maintenance;
- regulatory or assurance activity;
- customer or supplier relationships.

A context narrows where work occurs. It must not silently redefine identity, employment, structural Position or global enterprise authority.

## Enterprise structure chain

The initial conceptual chain is:

```text
Enterprise / Tenant
→ Organisation
→ Organisational Unit
→ Position
→ Work Relationship
→ Position Occupancy
→ Person
```

This is deliberately not yet a database schema.

## Accountability and participation

The model must distinguish at least:

- responsibility — what an organisational actor is accountable for;
- assignment — work allocated to an actor;
- permission — what an actor may technically access or perform;
- authority — which decisions or commitments an actor may make;
- competence — whether an actor is qualified/capable to perform particular work;
- ownership — responsibility for a particular enterprise object or record;
- reporting — structural management relationship;
- delegation — temporary or scoped transfer of authority.

No single `role` field may represent all of these.

## Effective dating

Enterprise relationships are temporal. At minimum, the model must be capable of representing:

- planned future changes;
- current effective state;
- historic state;
- superseded relationships;
- temporary assignments/delegations;
- vacancies;
- simultaneous valid relationships where business reality permits them.

## Questions that remain open

Before implementation, evidence must settle:

1. Which Party classifications are universal?
2. Which organisational structures require first-class objects versus typed relationships?
3. How should matrix organisations be represented?
4. How do contingent workers and external participants differ from employees?
5. How is management scope calculated across structural and contextual work?
6. What constitutes enterprise identity versus application identity?
7. Which legal-entity semantics are jurisdiction-dependent?

These questions should be answered from real enterprise cases, not UI convenience.
