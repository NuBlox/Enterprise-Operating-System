# 05 — Control Model

## Purpose

The control model defines how NuBlox governs access, responsibility, authority, decisions, change, assurance, evidence and record integrity.

The central rule is that concepts with different business meanings must not be collapsed merely because one software field could represent them.

## Distinct control concepts

### Identity

Who is acting.

### Authentication

How the system establishes confidence in that identity.

### Permission

What an authenticated actor may technically access or perform.

### Responsibility

What an actor or Position is accountable for.

### Assignment

Which work has been allocated to an actor.

### Authority

Which decisions, approvals, commitments or representations an actor may validly make.

### Delegation

A governed temporary or scoped transfer of Authority. Delegation must have provenance, scope and effective dates.

### Competence

Evidence that an actor is qualified, skilled or otherwise eligible to perform particular work.

### Ownership

Business accountability for an enterprise object or domain.

### Approval

A controlled Decision that permits, accepts or authorises a state transition or outcome.

### Evidence

Information retained to demonstrate that work, Decision, control or obligation occurred as required.

### Audit

An attributable history of significant actions and state changes.

## Authority model

Authority should be explicit enough to answer:

- Who may approve this?
- On whose behalf?
- Within what monetary/contractual/technical scope?
- In which organisational or work context?
- During what effective period?
- Under which delegation, if any?
- What evidence proves the decision?

A permission to click an `Approve` button is not proof of business Authority.

## Decision model

A Decision should be treated as a business object where its outcome matters beyond the UI interaction that created it.

A Decision may record:

- subject;
- decision type;
- decision maker / Authority;
- decision date/effective date;
- outcome;
- rationale;
- conditions;
- supporting evidence;
- resulting state changes;
- supersession or appeal/reconsideration where relevant.

## Workflow and lifecycle

Workflow coordinates work. Lifecycle describes the valid states of a business object. They are related but not identical.

The architecture must allow:

- state transitions without a long-running workflow where appropriate;
- workflows involving multiple objects;
- parallel review;
- conditional paths;
- escalation;
- timers/deadlines;
- human and automated steps;
- external orchestration;
- cancellation and recovery;
- complete evidence of significant transitions.

## Change control

Changes to governed definitions or controlled objects may require:

- change request;
- impact assessment;
- review;
- Authority-based approval;
- effective date;
- version/revision;
- communication;
- implementation evidence;
- supersession/withdrawal of prior state.

Different object types will need different change-control rigor.

## Assurance

The platform must support preventative, detective and corrective controls where justified.

Examples include:

- separation of duties;
- mandatory review;
- competence prerequisite;
- threshold approval;
- duplicate/conflict detection;
- required evidence;
- exception handling;
- periodic attestation;
- audit sampling;
- control testing.

## Evidence and provenance

Evidence should be attributable and traceable to the work or decision it supports.

Where appropriate, evidence should capture:

- source;
- creator/actor;
- creation time;
- applicable object/work context;
- version/revision;
- integrity metadata;
- retention classification;
- access classification;
- relationship to Decision/control/obligation.

## Records and retention

Operational data and formal records are related but not always identical. The architecture must determine:

- when operational content becomes a record;
- retention period/rule;
- legal hold;
- disposal approval;
- supersession;
- preservation of immutable evidence where required.

## Open questions

1. What is the universal Authority model?
2. Which Decision types require first-class objects?
3. How are financial, contractual, technical and governance Authority scopes represented?
4. How should separation-of-duties rules be expressed?
5. What evidence integrity guarantees are required?
6. How should regulatory retention rules coexist with tenant policy?
7. Which control definitions are global, industry-specific or tenant-defined?
