# 06 — Evidence Reconciliation

## Purpose

This document governs how earlier NuBlox work and external evidence are brought into the clean-slate architecture without silently becoming canon.

## Evidence sources

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

Each imported concept should eventually receive one of these states:

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

## Priority evidence waves

### Wave 1 — Enterprise identity and structure

Review Party, Person, Organisation, Organisational Unit, Position, employment/work relationship, occupancy, reporting and authority concepts.

### Wave 2 — Work execution

Review Activities, Methods, Work Items, workflows, handoffs, work products, Decisions and Evidence.

### Wave 3 — Enterprise objects

Review the object inventories implied by ERP/HCM/CRM/PLM/project/CBE research and identify shared versus contextual semantics.

### Wave 4 — Capability structure

Review prior F-series, D-series, L2 and 1,510-Activity research against the new model. No numbering is preserved merely for continuity.

### Wave 5 — Tool evidence

Reconcile the market-tool acronym/activity registers to clean-slate Activities, Methods, execution modes and shared platform capabilities.

### Wave 6 — Industry evidence

Start with Construction & Built Environment because it has the deepest accumulated evidence, but do not allow CBE assumptions to masquerade as universal enterprise semantics.

## Market-tool evidence

Observed activities from market tools should be treated as evidence phrases, not automatically canonical Activities.

An observed phrase may map to:

- one canonical Activity;
- several Activities;
- a Method step;
- a shared platform capability;
- an industry-specific capability;
- a specialist governed action;
- an external orchestrated action;
- or a genuine gap.

## Decision record discipline

When a major concept is promoted, the decision should record:

1. what problem the concept solves;
2. alternatives considered;
3. evidence supporting it;
4. resulting invariants;
5. implications for later software architecture.

## Clean-slate gate

No application implementation should begin until the first reconciliation wave has established, at minimum:

- enterprise identity boundaries;
- organisational/Position semantics;
- work definition versus work execution;
- core object identity/lifecycle principles;
- permission/responsibility/Authority separation;
- evidence/audit principles.

This is the gate that prevents implementation convenience from becoming accidental product architecture.
