# Process maps

**Section:** F_Requirements_Analysis  
**Document ID:** NBEOS-F-023  
**Document Type:** Process maps  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product / Business Analysis  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Expiry Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** [TBD]  
**Disposal Method:** [TBD]  
**Distribution List:** NuBlox programme contributors  
**Related Documents:** `Use_cases.md`, `User_journeys.md`, `Business_rules.md`, `BPMN_diagrams.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/F_Requirements_Analysis/Process_maps.md`  
**Storage Location:** `software_project_docs/F_Requirements_Analysis/Process_maps.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Record conceptual candidate process maps derived from the current requirements so end-to-end continuity, decisions, handoffs, information and exceptions can be reviewed before formal BPMN and solution design.

These maps are **not approved operating procedures and not formal BPMN 2.0 models**.

## PM-001 — From business trigger to completed work

```mermaid
flowchart LR
    A[Business trigger] --> B[Identify business context]
    B --> C{Prerequisites valid?}
    C -- No --> D[Raise exception / obtain missing input]
    D --> C
    C -- Yes --> E[Initiate work]
    E --> F[Assign / route ownership]
    F --> G[Perform work]
    G --> H[Create or update output]
    H --> I{Review / decision required?}
    I -- Yes --> J[Request review / decision]
    J --> K{Accepted / approved?}
    K -- No --> G
    K -- Yes --> L[Controlled handoff / downstream action]
    I -- No --> L
    L --> M{More work/stage?}
    M -- Yes --> F
    M -- No --> N[Accept / complete outcome]
    N --> O[Retain evidence and outcome measures]
```

**Key requirements:** `BR-001`, `BR-002`, `BR-005`, `BR-007`, `FR-009`–`FR-018`.

## PM-002 — Governed work-product lifecycle

```mermaid
flowchart LR
    A[Need for work product] --> B[Create / register product]
    B --> C[Relate to work and context]
    C --> D[Develop / update]
    D --> E[Submit required revision for review]
    E --> F{Review outcome}
    F -- Changes required --> D
    F -- Accept --> G{Approval required?}
    G -- Yes --> H[Validate authority]
    H --> I{Approved?}
    I -- No --> D
    I -- Yes --> J[Mark approved state]
    G -- No --> J
    J --> K[Issue / distribute to authorised recipients]
    K --> L[Retain issue, revision and decision evidence]
    L --> M{Superseded later?}
    M -- Yes --> D
    M -- No --> N[Retain as current / historical record]
```

**Key requirements:** `FR-019`–`FR-022`, `RULE-003`, `RULE-006`, `BR-014`.

## PM-003 — Controlled business change

```mermaid
flowchart LR
    A[Change identified] --> B[Record subject, reason and impact]
    B --> C[Assess delivery / commercial / control impact]
    C --> D{Decision / approval required?}
    D -- Yes --> E[Determine authority and evidence]
    E --> F{Approved?}
    F -- No --> G[Reject / revise / close]
    F -- Yes --> H[Apply approved change]
    D -- No --> H
    H --> I[Update affected work / commitments / information]
    I --> J{External system handoff required?}
    J -- Yes --> K[Send controlled integration request]
    K --> L{Confirmed?}
    L -- No --> M[Exception / retry / reconcile]
    M --> L
    L -- Yes --> N[Record completion]
    J -- No --> N
    N --> O[Update reporting / evidence]
```

**Key requirements:** `FR-018`, `FR-023`–`FR-025`, `INT-004`–`INT-007`, `AC-007`.

## PM-004 — External collaboration

```mermaid
flowchart LR
    A[External contribution required] --> B[Identify participant and context]
    B --> C[Authorise minimum access]
    C --> D[Invite / establish access]
    D --> E[Present required information/action]
    E --> F[External participant contributes]
    F --> G[Record attributable submission / response]
    G --> H{Internal review / acceptance}
    H -- Rework --> E
    H -- Accepted --> I[Progress internal workflow]
    I --> J[Reduce / revoke access when no longer required]
```

**Key requirements:** `FR-004`, `RULE-008`, `NFR-SEC-001`, `SR-031`, `SR-032`.

## PM-005 — Migration and reconciliation

```mermaid
flowchart LR
    A[Agree source and scope] --> B[Profile source information]
    B --> C[Define semantic mapping]
    C --> D[Transform / validate]
    D --> E[Trial load]
    E --> F[Reconcile counts, values, relationships and history]
    F --> G{Within accepted tolerance?}
    G -- No --> H[Classify exception / correct mapping or source]
    H --> D
    G -- Yes --> I[Customer/data-owner review]
    I --> J{Accepted?}
    J -- No --> H
    J -- Yes --> K[Approved cutover]
    K --> L[Post-cutover reconciliation]
    L --> M[Retain migration evidence / source identifiers]
```

**Key requirements:** `DATA-009`, `DATA-010`, `AC-008`, `BR-013`.

## Process-map review checklist

For each process map, validation must identify:

- actual actor(s) and ownership;
- triggers and completion conditions;
- required information and authoritative sources;
- work products/transactions/records;
- decisions/authority rules;
- handoffs and external parties;
- exceptions/cancellation/recovery;
- timing/deadline obligations;
- reporting/measure consequences;
- applicable NFR/security/privacy constraints;
- customer/discipline variations.

## References

- `Use_cases.md`
- `User_journeys.md`
- `Business_rules.md`

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product / Business Analysis | Initial five conceptual process maps derived from controlled requirements |