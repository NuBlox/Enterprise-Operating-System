# 07 — Wave 1 Enterprise Identity & Structure Reconciliation

**Status:** ACCEPTED CLEAN-SLATE BASELINE  
**Wave:** Evidence Reconciliation Wave 1  
**Date:** 26 September 2026

## Purpose

This document establishes the first canonical clean-slate baseline for enterprise identity, organisational structure and authority.

It reconciles useful V3 evidence without inheriting the V3 database schema, Party-Type model, HCM implementation, routes or Function architecture.

The purpose of this wave is to answer a more fundamental question than “what tables should exist?”:

> What enterprise identities and relationships must NuBlox understand so that work, accountability, access, authority and organisational history can be represented truthfully?

## Evidence reviewed

Primary V3 evidence reviewed at frozen V3 commit `10c942f62afab11ab39f6f28c86201da76a0f836`:

- `packages/persistence/migrations/0001_kernel_identity.sql`
- `packages/persistence/migrations/0003_kernel_access.sql`
- `packages/persistence/migrations/0047_hcm_authority_spine.sql`
- `packages/persistence/migrations/0053_party_types.sql`
- `packages/persistence/migrations/0068_hcm_work_relationships.sql`
- `packages/persistence/migrations/0069_hcm_position_establishment.sql`
- `docs/architecture/08-architecture-invariants.md`
- `docs/architecture/01-product-architecture.md`
- `docs/reference/hcm-world-class-capability-benchmark.md`

The evidence shows several durable concepts, but also several implementation shortcuts that must not survive as enterprise semantics.

---

# 1. Canonical enterprise identity model

## 1.1 Tenant

A **Tenant** is the NuBlox platform boundary within which a subscribed enterprise customer operates.

A Tenant defines at least:

- data isolation;
- configuration ownership;
- commercial/subscription context;
- security and policy boundary;
- tenant-local identifiers and definitions;
- the set of Organisations and people participating in that customer environment.

A Tenant is **not itself a Party type, Person or Organisation**.

A Tenant may contain one Organisation or a group of related Organisations and legal entities. The customer Organisation and the software Tenant must therefore remain separate identities.

### Invariant

`Tenant != Organisation != Party`.

---

## 1.2 Party

A **Party** is an abstract enterprise participant capable of entering relationships, holding roles or being party to transactions, obligations, contracts or work.

The initial canonical Party kinds are:

```text
PARTY
├── PERSON
└── ORGANISATION
```

Party is an identity abstraction, not a catch-all business-role catalogue.

Business roles such as employee, client, supplier, partner, contractor or regulator are represented by relationships or contextual roles attached to a Party; they do not create a new Party identity.

### Invariant

A change in business role must not create a duplicate Person or Organisation identity.

---

## 1.3 Person

A **Person** is a human enterprise identity.

A Person:

- exists independently of application credentials;
- may participate in multiple Work Relationships over time;
- may occupy zero, one or multiple Positions where valid;
- may act for multiple Organisations or contexts;
- retains one stable identity across changes of name, job, Position, assignment or access account;
- may exist before having NuBlox login credentials;
- may cease working for an Organisation without ceasing to exist as a Person record where retention obligations require history.

### Invariant

`Person != User Account != Employee != Position`.

---

## 1.4 Organisation

An **Organisation** is a separately identifiable organisational Party capable of participating in enterprise relationships.

An Organisation may be a company, public body, partnership, charity, institution or other separately recognised body.

The same Organisation may simultaneously be, for example:

- a client in one relationship;
- a supplier in another;
- a joint-venture participant elsewhere;
- an employer of its own people.

Those roles do not create duplicate Organisation identities.

### Legal entity

A **Legal Entity** is an Organisation with a recognised legal identity capable of holding legal rights or obligations in a jurisdiction.

Legal-entity status is a governed classification/profile of Organisation, not an Organisational Unit.

Legal entity details may include jurisdiction, registration identifiers, tax registrations and legal names, but those attributes are not defined in this Wave.

### Invariant

`Organisation != Organisational Unit`.

---

## 1.5 Organisational Unit

An **Organisational Unit** is a structural subdivision used to organise accountability, resources, Positions, cost/control scope or reporting.

Examples may include division, business unit, department, function, region, branch or team where the enterprise chooses to structure them as organisational units.

An Organisational Unit:

- belongs to an Organisation;
- may be hierarchical;
- does not automatically possess separate legal identity;
- can own Positions and organisational responsibilities;
- can change structure without recreating the underlying Organisation.

Unit classification and hierarchy semantics remain configurable, but the distinction between Organisation and Organisational Unit is canonical.

---

# 2. Workforce relationship model

## 2.1 Work Relationship

A **Work Relationship** is the effective-dated legal, contractual or equivalent basis under which a Person performs work for or on behalf of an Organisation.

Initial universal relationship families are:

- Employment;
- Contingent Engagement.

Other legally meaningful relationship families may be introduced only when evidence shows they possess distinct lifecycle or obligations.

The Work Relationship may carry facts such as:

- employing/engaging Organisation;
- worker identifier;
- relationship start/end;
- contractual worker category;
- terms or employment classification;
- active/suspended/ended lifecycle;
- applicable jurisdiction where required.

### Important correction from V3

The following are **not Work Relationship identity types merely because they describe a workforce situation**:

- primary versus secondary work;
- secondment;
- global assignment;
- project assignment;
- acting arrangement;
- temporary deployment.

Those concepts describe assignment, priority or deployment over an underlying Work Relationship and must be modelled separately when introduced.

### Invariant

`Work Relationship != Position Occupancy != Assignment`.

---

## 2.2 Position

A **Position** is an enduring organisational seat established by an Organisation for work and accountability to exist, whether currently occupied or vacant.

A Position:

- belongs to an organisational structure;
- exists independently of the Person who occupies it;
- may be planned before occupancy;
- may be vacant;
- may be abolished while retaining history;
- may define authorised capacity/FTE;
- may be classified by a Job Profile or other workforce architecture without becoming that Job Profile;
- participates in structural reporting relationships;
- is a primary anchor for organisational responsibility and authority.

V3 evidence for planned/approved/frozen/abolished states and authorised FTE is retained as a useful lifecycle hypothesis, but the exact lifecycle enumeration is not yet canonical.

### Invariant

`Position != Person != Job Profile != Access Role`.

---

## 2.3 Position Occupancy

A **Position Occupancy** is the effective-dated allocation of a Work Relationship to a Position.

It records the fact that a Person, under a specific Work Relationship, occupies all or part of a Position for a period.

Occupancy may carry:

- effective start/end;
- allocation/FTE;
- primary-working-position indicator where needed for operational defaults;
- occupancy lifecycle/status;
- provenance and reason for change.

The Person is resolved through the Work Relationship; implementations should not duplicate Person identity on Occupancy unless a justified denormalised projection is introduced later.

### Invariant

Ending an Occupancy does not end the Position and does not necessarily end the Work Relationship.

---

# 3. Organisational reporting and management scope

## 3.1 Reporting Relationship

A **Reporting Relationship** is an effective-dated structural relationship between Positions.

The initial model must support matrix organisations; therefore a Position may have more than one simultaneous reporting relationship where semantics permit it.

Candidate relationship meanings include:

- line management;
- functional management;
- supervisory/operational management.

The V3 term `DOTTED_LINE` is treated as presentation language rather than canonical business semantics. Any relationship type must state what management meaning it conveys.

### Invariants

- reporting is Position-to-Position, not Person-to-Person;
- reporting does not transfer object ownership;
- reporting does not itself grant business Authority;
- reporting does not itself grant unrestricted Permission;
- reporting may contribute to calculated management visibility only through explicit access/scope policy.

## 3.2 Management scope

Management scope is a **derived scope**, not a new ownership model.

A manager may be entitled to see or act over subordinate work where:

1. a valid Reporting Relationship exists;
2. applicable Permission permits the action;
3. information/security classification permits access;
4. the relevant work/object scope permits managerial visibility;
5. any required Authority exists separately.

This preserves the useful V3 behaviour of hierarchy-based roll-up without making reporting hierarchy equivalent to access control.

---

# 4. Business roles and Party relationships

## 4.1 Employee

**Employee is not a Party type.**

Employee describes a Person participating in an Employment Work Relationship with an Organisation.

A Person can have multiple Work Relationships over time, and potentially more than one simultaneous employment where lawful and supported.

## 4.2 Client / Customer

**Client/Customer is not a Party type.**

It is a contextual commercial/service relationship role held by a Party in relation to another Party, Contract, Account, Opportunity, Service or other business context.

## 4.3 Vendor / Supplier

**Vendor/Supplier is not a Party type.**

It is a contextual sourcing/commercial relationship role held by a Party.

The same Organisation can be both Client and Supplier without duplication.

## 4.4 Tenant

**Tenant is not a Party type.**

Tenant is a NuBlox platform context. One or more Organisations participate inside it.

---

# 5. Responsibility, Permission and Authority

These three concepts are promoted as separate canonical concepts.

## 5.1 Responsibility

**Responsibility** states what an organisational actor is accountable for.

Responsibility may be attached to a Position or Organisational Unit and may be narrowed by enterprise/work context.

Responsibility answers:

> What is this Position or organisational actor accountable for ensuring is done or governed?

Responsibility does not automatically provide technical access or decision power.

## 5.2 Permission

**Permission** states what an authenticated principal may technically access or perform in NuBlox.

Permission is part of application authorisation.

Permissions may be packaged into reusable access roles for administration, but **Access Role is an authorisation convenience, not organisational identity or business responsibility**.

## 5.3 Authority

**Authority** states which decisions, commitments, approvals or representations may validly be made on behalf of an Organisation or within an enterprise context.

An Authority Grant must be capable of expressing:

- authority subject/type;
- holder;
- organisational/work scope;
- financial or other limit where relevant;
- effective period;
- source/provenance;
- conditions;
- delegation rules.

V3's separation of Authority Definition, Authority Grant and Delegation is promoted conceptually.

The exact set of Authority categories (`FINANCIAL`, `CONTRACTUAL`, `TECHNICAL`, `OPERATIONAL`, `GOVERNANCE` in V3) remains evidence rather than a frozen enumeration.

### Invariant

Permission to perform an `approve` action is not proof that the actor has business Authority to approve the subject.

---

# 6. Delegation

A **Delegation** is a governed, effective-dated transfer or exercise arrangement for an existing Authority.

Delegation must record:

- source Authority;
- delegator/authority holder;
- delegate;
- effective period;
- scope and any reduced limits;
- reason;
- provenance;
- status/revocation;
- evidence of the delegation.

A Delegation may never create greater Authority than the source holder possesses.

Whether delegation targets a Position, Person or both is an implementation-independent business rule to resolve in the Authority design wave; V3's Person-to-Person implementation is not inherited automatically.

---

# 7. Enterprise identity versus application identity

NuBlox must maintain a hard boundary between enterprise identity and application identity.

```text
Person
  !=
User Account / Login Principal
  !=
Authentication Credential
  !=
Session
```

A Person can exist without an account. An account can be disabled while the Person and historical enterprise relationships remain. Authentication methods can change without changing enterprise identity.

External users, service identities and machine principals may require application identities that are not Employees and may not map one-to-one to a Person; those cases must be explicitly modelled rather than forcing every principal into the workforce model.

---

# 8. Effective dating and history

The following are inherently temporal and must preserve effective history:

- Organisation structural relationships;
- Organisational Unit hierarchy where changed over time;
- Work Relationships;
- Position establishment/lifecycle;
- Position Occupancies;
- Reporting Relationships;
- Responsibilities;
- Authority Grants;
- Delegations;
- Access-role/Permission assignments where time-bounded.

Current state must be derivable from effective history; history must not be overwritten merely to simplify UI queries.

---

# 9. Reconciliation register

| V3 concept | V4 state | Clean-slate disposition |
|---|---|---|
| Tenant | PROMOTE_WITH_CHANGE | Platform isolation/commercial context; not Party type |
| Party | PROMOTE_WITH_CHANGE | Abstract enterprise participant identity |
| Party kind PERSON | PROMOTE | Canonical Party kind / Person identity |
| Party kind ORGANISATION | PROMOTE | Canonical Party kind / Organisation identity |
| Party Type TENANT | REJECT | Replaced by Tenant platform context |
| Party Type EMPLOYEE | REJECT | Replaced by Employment Work Relationship semantics |
| Party Type CLIENT | PROMOTE_WITH_CHANGE | Contextual client/customer relationship role |
| Party Type VENDOR_SUPPLIER | PROMOTE_WITH_CHANGE | Contextual supplier relationship role |
| Person | PROMOTE | Stable human enterprise identity |
| Organisation | PROMOTE | Stable organisational Party identity |
| Organisational Unit | PROMOTE | Structural subdivision; not legal identity by default |
| Legal Entity | PROMOTE_WITH_CHANGE | Governed Organisation classification/profile |
| Job Profile | UNRESOLVED | Adjacent workforce-classification concept; reconcile later |
| Position | PROMOTE | Enduring organisational seat |
| Position establishment lifecycle | PROMOTE_WITH_CHANGE | Concept valid; exact state catalogue unresolved |
| Employment table | PROMOTE_WITH_CHANGE | Generalised to Work Relationship |
| PRIMARY/SECONDARY employment as relationship types | REJECT | Priority is not relationship identity |
| Secondment/global assignment as relationship types | REJECT | Belong in Assignment/Deployment layer |
| Contingent engagement | PROMOTE | Distinct Work Relationship family |
| Position Occupancy | PROMOTE | Effective-dated Work Relationship-to-Position allocation |
| Occupancy FTE | PROMOTE | Allocation/capacity characteristic |
| Position reporting lines | PROMOTE_WITH_CHANGE | Position-to-Position structural relationships; semantic types to refine |
| DOTTED_LINE reporting type | REJECT | Presentation term; replace with explicit business semantics |
| Responsibility | PROMOTE | Separate organisational accountability concept |
| Permission Definition | PROMOTE | Technical authorisation concept |
| Access Role | PROMOTE_WITH_CHANGE | Permission packaging only; not business role |
| Authority Definition | PROMOTE | Explicit decision/commitment authority definition |
| Authority Grant | PROMOTE_WITH_CHANGE | Explicit holder/scope/limit/effective-period grant |
| Delegation | PROMOTE_WITH_CHANGE | Governed transfer/exercise of existing Authority |
| Manager hierarchy automatically owns subordinate records | REJECT | Reporting may derive scope but does not transfer ownership |
| Canonical object/relationship generic tables | REFERENCE_ONLY | Implementation evidence for later object/runtime design |
| Audit entries | PROMOTE | Attributable enterprise/control history required |

---

# 10. Canonical Wave 1 relationship graph

```text
Tenant
│
├── contains → Organisation
│                │
│                ├── may carry → Legal Entity profile
│                ├── contains → Organisational Unit
│                │               └── establishes → Position
│                │                                ├── reports to → Position
│                │                                ├── holds → Responsibility
│                │                                └── may hold → Authority Grant
│                │
│                └── engages/employs ← Work Relationship → Person
│                                         │
│                                         └── allocated by → Position Occupancy → Position
│
└── authorises application principals through → Permission / Access Role

Party
├── Person
└── Organisation

Organisation ↔ contextual commercial relationship ↔ Organisation/Person
           client / supplier / partner / other role
```

---

# 11. Wave 1 invariants

The following are now clean-slate architecture invariants:

1. `Tenant != Party != Organisation`.
2. Party identity initially resolves to `Person` or `Organisation`.
3. Employee, Client and Supplier are roles/relationships, not Party identity types.
4. `Person != User Account != Work Relationship != Position`.
5. `Organisation != Organisational Unit`.
6. Legal Entity is an Organisation concept, never merely an Organisational Unit.
7. A Position may exist vacant and survives occupant changes.
8. Position Occupancy is effective-dated and does not redefine Person or Position identity.
9. Work Relationship defines the legal/contractual basis for work; assignment/deployment is separate.
10. Reporting is Position-to-Position and may support matrix structures.
11. Reporting does not itself grant Permission, Authority or record ownership.
12. Responsibility, Permission, Authority, Assignment, Competence and Ownership are distinct concepts.
13. Access Roles package Permissions; they are not organisational business roles.
14. Authority must be explicit, scoped, effective-dated and independently evidenced.
15. Delegation cannot exceed the source Authority.
16. Enterprise relationships preserve effective history rather than overwriting past truth.
17. Application authentication identity is separate from enterprise Person identity.
18. Business-role changes must not duplicate Person or Organisation identities.

---

# 12. Remaining Wave 1 design questions

These do not block the conceptual baseline but must be resolved before persistence design:

1. Is Party represented as a persisted root identity or as a strict polymorphic abstraction over Person and Organisation?
2. What tenant-local versus cross-tenant identity matching is permitted without violating isolation/privacy?
3. What are the canonical Organisation classifications beyond Legal Entity?
4. What exact reporting semantics are universally required for matrix organisations?
5. What is the universal Responsibility definition/assignment structure?
6. Should Authority normally attach to Position, with Person grants reserved for exceptional regulated cases?
7. What scope algebra is required for Permission and Authority across Organisation, Unit, Project, Contract, Site, Asset and other contexts?
8. How should service/machine principals relate to enterprise identity?
9. Which workforce terms are global platform definitions versus jurisdiction-specific configuration?

These questions move forward as explicit design work. They must not be answered accidentally by table shape.

---

# 13. Gate outcome

**Wave 1 passes the clean-slate conceptual gate.**

The enterprise identity boundary, organisational/Position semantics, Work Relationship/Occupancy separation, reporting semantics, and Responsibility/Permission/Authority separation are now sufficiently defined to proceed to the next evidence wave.

This does **not** authorise database schema or application implementation yet.

The next evidence wave is **Wave 2 — Work Execution**: Activity, Method, Work Item, Assignment, workflow, handoff, work product, Decision and Evidence.