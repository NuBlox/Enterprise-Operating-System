# 06 — Evidence Reconciliation

## Purpose

This document governs how earlier NuBlox work and external evidence are brought into the clean-slate architecture without silently becoming canon.

## Evidence is not authority

A concept does not become canonical because:

- it existed in V3;
- a competitor uses it;
- an industry standard names it;
- one customer uses it;
- a database table already exists;
- a UI page was already implemented.

Canonical status requires semantic fit with the clean-slate enterprise, work, capability, object and control models.

## Reconciliation states

Each imported concept receives one of these states:

- **PROMOTE** — fits the clean-slate architecture and becomes canonical;
- **PROMOTE_WITH_CHANGE** — underlying concept is valid but naming/structure/semantics require revision;
- **REFERENCE_ONLY** — useful evidence but not a product concept;
- **DUPLICATE** — already represented by another canonical concept;
- **CONTEXTUAL** — valid only for an industry, jurisdiction, tenant or workflow context;
- **REJECT** — conflicts with the clean-slate model or has insufficient business meaning;
- **UNRESOLVED** — requires more evidence.

## Required reconciliation fields

A reconciliation register should capture at least:

```text
source
source reference
source concept
source definition
proposed clean-slate concept
enterprise/work/capability/object/control layer
reconciliation state
rationale
evidence
open questions
review owner
review date
```

---

# Evidence waves

## Wave 1 — Enterprise identity and structure — ACCEPTED

**Decision record:** [07 — Wave 1 Enterprise Identity & Structure Reconciliation](07-wave-1-enterprise-identity-reconciliation.md)

Wave 1 established the clean-slate baseline for:

- Tenant boundary;
- Party abstraction;
- Person;
- Organisation;
- Legal Entity semantics;
- Organisational Unit;
- Position;
- Work Relationship;
- Position Occupancy;
- Reporting Relationship;
- Responsibility;
- Permission;
- Authority;
- Delegation;
- enterprise identity versus application identity;
- effective-dated organisational history.

Wave 1 explicitly rejected Employee, Client, Supplier and Tenant as Party identity types and separated Work Relationship from assignment/deployment.

Conceptual gate outcome: **PASS**.

This does not authorise persistence or application implementation.

## Wave 2 — Work execution — ACTIVE NEXT

Reconcile:

- Activity;
- Method;
- Work Item;
- Assignment;
- work queue / My Work projection;
- workflow;
- lifecycle;
- handoff;
- work product;
- Deliverable;
- Decision;
- Evidence;
- acceptance;
- escalation;
- execution mode (`EXECUTE`, `GOVERN`, `ORCHESTRATE`) as evidence rather than assumed canon.

The key Wave 2 questions are:

1. What is the difference between a business Activity and an executable Work Item?
2. Does Method deserve first-class identity, versioning and effective dating?
3. Which work relationships are structural versus runtime assignments?
4. How do workflow and object lifecycle differ?
5. How are work products distinguished from evidence and formal records?
6. When does a Decision require its own business identity?
7. How do reviews, approvals and acceptances relate without becoming one generic status transition?
8. How are handoffs represented across organisational and external boundaries?
9. Which execution modes are universal semantics versus implementation strategy?
10. How does completed work remain traceable to the Method/version and authority under which it was performed?

## Wave 3 — Enterprise objects

Review the object inventories implied by ERP/HCM/CRM/PLM/project/CBE research and identify shared versus contextual semantics.

## Wave 4 — Capability structure

Review prior F-series, D-series, L2 and 1,510-Activity research against the new model. No numbering is preserved merely for continuity.

## Wave 5 — Tool evidence

Reconcile the market-tool acronym/activity registers to clean-slate Activities, Methods, execution modes and shared platform capabilities.

## Wave 6 — Industry evidence

Start with Construction & Built Environment because it has the deepest accumulated evidence, but do not allow CBE assumptions to masquerade as universal enterprise semantics.

---

# Evidence sources

Evidence may include:

- prior NuBlox repositories;
- enterprise Function/activity research;
- market-tool capability research;
- occupational/job frameworks;
- customer/user workflows;
- industry standards;
- legislation/regulation;
- product documentation from incumbent tools;
- implementation experiments;
- migration/schema lessons;
- UI/UX findings;
- commercial/operating-model research.

## Market-tool evidence

Observed activities from market tools are evidence phrases, not automatically canonical Activities.

An observed phrase may map to:

- one canonical Activity;
- several Activities;
- a Method step;
- a shared platform capability;
- an industry-specific capability;
- a specialist governed action;
- an external orchestrated action;
- or a genuine gap.

## V3 evidence discipline

V3 is frozen reference evidence. The following are specifically **not inherited automatically**:

- F01–F29;
- D01–D16;
- the 1,510 Activity register;
- current Party-Type assignments;
- current schema/migrations;
- current Tool Registry;
- current provisioning model;
- current routes/navigation;
- current application implementation.

Where a V3 concept is promoted, the clean-slate record must explain why its business semantics survive independently of the old implementation.

---

# Decision record discipline

When a major concept is promoted, the decision records:

1. what problem the concept solves;
2. alternatives considered;
3. evidence supporting it;
4. resulting invariants;
5. implications for later software architecture.

## Clean-slate implementation gate

Application implementation remains blocked until at least Waves 1 and 2 establish:

- enterprise identity boundaries;
- organisational/Position semantics;
- work definition versus work execution;
- core object identity/lifecycle principles;
- Permission/Responsibility/Authority separation;
- Method/execution semantics;
- Decision/Evidence/audit principles.

Only after those foundations are accepted should NuBlox define software architecture, persistence, routes or UI.