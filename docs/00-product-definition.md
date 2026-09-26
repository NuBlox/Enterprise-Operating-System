# 00 — Product Definition

## Purpose

NuBlox is an enterprise operating system: a governed environment in which an organisation can both **operate the business** and **deliver the work it exists to perform**.

The product is not defined by the category labels of incumbent ERP, HCM, CRM, PLM, CDE, PMIS, CAFM, QMS or other software. Those categories are evidence of market capability, not NuBlox's architecture.

## Product outcome

A person working in NuBlox should be able to understand:

- who they are acting for;
- which organisational position and responsibilities they hold;
- what work they are authorised and expected to perform;
- which method governs that work;
- which tools and enterprise objects are relevant;
- which decisions, reviews and approvals are required;
- what evidence must be retained;
- who receives the resulting work;
- how their work contributes to wider enterprise outcomes.

## Two inseparable concerns

NuBlox must support:

1. **How the organisation operates** — structure, governance, people, finance, commercial management, information, risk, assurance, assets, suppliers, customers and the other capabilities required to run the enterprise.
2. **How the organisation delivers** — the actual professional, operational, project, service, product, asset and industry work through which the enterprise creates value.

These are not separate products. They share one enterprise model, one authority model, one evidence model and one object graph.

## Product principles

### Enterprise reality before software taxonomy

The model starts with real organisations, people, work, objects, relationships, decisions and evidence. Software categories are mapped later.

### One authoritative enterprise graph

The same Person, Organisation, Position, Contract, Project, Asset, Document or other enterprise object must not be duplicated merely because different capabilities use it.

### Work must be executable

A capability is not complete because a dashboard or register exists. NuBlox must support the actions, transactions, work products, decisions and handoffs needed to perform the work.

### Governance is explicit

Responsibility, permission, authority, competence, assignment, approval and record ownership are distinct concepts and must not be collapsed into a single role field.

### History matters

Enterprise state changes over time. Effective dates, versions, lifecycle, changes, supersession and evidence are part of the model rather than implementation afterthoughts.

### Industry depth without enterprise fragmentation

Industry-specific work may extend the enterprise model, but should reuse shared enterprise objects and controls whenever the business meaning is actually shared.

### External tools are a deliberate architectural choice

For any activity NuBlox may:

- **EXECUTE** — perform the work natively;
- **GOVERN** — govern work or work products created using a specialist authoring/execution environment;
- **ORCHESTRATE** — coordinate an external system or authority.

The mode must be explicit and evidence-based.

## Initial non-goals

Until the enterprise model is accepted, this repository will not define:

- application framework;
- database technology;
- deployment topology;
- page navigation;
- canonical Function numbering;
- industry-domain numbering;
- final activity catalogue;
- final tool catalogue;
- pricing model;
- API surface.

Those follow the product model rather than dictate it.

## Success criterion for this phase

The architecture phase succeeds when the enterprise, work, capability, object and control models are sufficiently precise that software architecture can be derived from them without inventing missing business semantics during implementation.
