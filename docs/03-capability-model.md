# 03 — Capability Model

## Purpose

The capability model describes what the enterprise must be capable of doing, independently of organisation charts, application modules and incumbent software categories.

## Capability layers

A candidate hierarchy is:

```text
Enterprise Capability
→ Capability Domain
→ Activity
→ Method
→ Tool / Service
```

The exact hierarchy remains subject to evidence. Earlier NuBlox Function/Sub-function/Activity structures are inputs to this review, not pre-approved structure.

## Function

A Function may prove useful as a governed organisational capability boundary, but the clean-slate architecture does not assume a fixed Function catalogue or numbering scheme.

If retained, a Function must have a precise semantic definition. It should describe an enduring enterprise capability, not simply a menu heading or software module.

Potential responsibilities of a Function include:

- governance ownership;
- activity ownership;
- method ownership;
- capability assurance;
- competence requirements;
- performance measures;
- controlled tool composition;
- cross-capability handoffs.

## Activity

An Activity describes work the enterprise performs. Activities must be grounded in business evidence and mapped to outcomes, Methods, objects and controls.

## Method

A Method is the controlled specification for performing an Activity. Methods are versioned, effective-dated and governed.

Method should answer:

```text
This is the work → Activity
This is how we perform it → Method
This is an instance to perform now → Work Item
```

## Tool

A Tool is a capability used in performing work. It can be native software, shared platform capability, specialist environment or external service.

Tool classification must not become product architecture. Instead each Tool should declare its relationship to Activities, Methods, objects and controls.

## Execution modes

For every Activity/Method step, NuBlox should deliberately choose one of:

### EXECUTE

NuBlox performs the work natively and owns the execution state.

### GOVERN

A specialist environment may perform deep authoring or execution while NuBlox governs the authoritative enterprise object, lifecycle, review, decision, acceptance, evidence and record.

### ORCHESTRATE

NuBlox coordinates an external system, counterparty or authority and retains the enterprise state/evidence required to manage the interaction.

These modes are not product tiers. They are architecture decisions.

## Shared capability versus business capability

The architecture should distinguish reusable platform capabilities such as:

- identity/authentication;
- workflow/state machine;
- rules;
- notifications;
- document/information controls;
- search;
- audit/evidence;
- integration/eventing;
- metadata/configuration;
- reporting/analytics;

from business capabilities such as recruiting, purchasing, contract administration, cost management, design review or maintenance.

A shared capability can support many business Activities without becoming a business Function itself.

## Industry capability

Industry-specific capability should extend the enterprise model only where genuine business semantics differ. Industry terminology alone is insufficient justification for a new object or capability boundary.

Industry evidence must determine:

- unique Activities;
- unique Methods;
- unique object types;
- unique controls/regulation;
- specialist Tool requirements;
- industry-specific Job/competence requirements.

## Capability-to-organisation relationship

A capability is not the same thing as an organisational team.

The architecture must allow:

- one organisational unit to perform multiple capabilities;
- one capability to be performed across multiple organisational units;
- Positions to participate in multiple capabilities;
- temporary/contextual participation without structural reorganisation.

## Capability evidence requirements

A proposed capability should be supported by one or more of:

- observed enterprise work;
- recognised occupational/job responsibility;
- legal/regulatory obligation;
- industry standard/process;
- repeated market-tool capability;
- customer/user requirement;
- enterprise governance need.

## Open questions

1. Does the Function concept survive the clean-slate analysis, and at what level?
2. What distinguishes Function, capability, domain, process and service?
3. What is the correct canonical Activity granularity?
4. Which Methods are universal, industry-specific or tenant-specific?
5. Which Tools are shared versus capability-specific?
6. Where should NuBlox EXECUTE versus GOVERN versus ORCHESTRATE?
7. How are capability ownership and organisational accountability connected without conflating them?
