# Security classification policy

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-011  
**Document Type:** Security classification policy  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Security  
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
**Related Documents:** `Compliance__regulatory_register.md`, `Data_governance_policy.md`, `Enterprise_risk_register.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Security_classification_policy.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Security_classification_policy.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Define an initial information-classification model for NuBlox programme information and provide principles that can later inform product requirements for customer information classification and handling.

## Policy principles

- Information should be classified according to the impact of unauthorised disclosure, alteration, destruction or unavailability.
- Access should follow least-privilege and need-to-know principles.
- Classification should be understandable enough to use consistently.
- Controls should apply throughout the information lifecycle: creation, use, sharing, storage, backup, export, retention and disposal.
- Customer-controlled information classification must remain distinct from NuBlox internal programme classification.

## Initial NuBlox programme classifications

### PUBLIC

Information approved for unrestricted public disclosure.

Examples:

- approved public website content;
- published product documentation;
- approved marketing material;
- public policies and announcements.

Minimum handling:

- publication approval where required;
- integrity/version control for authoritative public material.

### INTERNAL

Information intended for normal NuBlox internal/programme use where unauthorised disclosure would create limited harm.

Examples:

- working product documents;
- general engineering guidance;
- internal meeting notes;
- non-sensitive project plans.

Minimum handling:

- authenticated access;
- repository/workspace access controls;
- normal backup and change history.

### CONFIDENTIAL

Information where unauthorised disclosure could materially harm NuBlox, a customer, employee, supplier or partner.

Examples:

- customer discovery notes containing identifiable/commercial detail;
- pricing models not yet public;
- contracts and commercial negotiations;
- non-public security architecture;
- personnel information;
- detailed financial information;
- customer data extracts used for migration/testing.

Minimum handling:

- explicit authorised groups;
- encryption in transit and at rest where stored electronically;
- controlled external sharing;
- logging of material access/actions where appropriate;
- defined retention and disposal;
- prohibition on uncontrolled public repositories or personal sharing channels.

### RESTRICTED

Information where unauthorised disclosure or alteration could cause severe security, legal, financial or customer harm.

Examples:

- production credentials/secrets;
- private cryptographic material;
- highly sensitive customer/security incident data;
- vulnerability information before remediation where disclosure materially increases attack risk;
- privileged recovery credentials;
- certain regulated/special-category datasets where risk assessment requires the highest controls.

Minimum handling:

- strict named/role-based access;
- strong authentication and privileged-access controls;
- secrets management rather than documents/code repositories for credentials;
- encryption;
- detailed audit logging;
- controlled export and sharing;
- explicit retention/disposal rules;
- incident escalation for suspected exposure.

## Classification decisions

Information owners should consider:

- confidentiality impact;
- integrity impact;
- availability impact;
- personal-data sensitivity;
- contractual/customer obligations;
- legal/regulatory requirements;
- commercial sensitivity;
- security consequence if disclosed;
- aggregation risk where several low-sensitivity items together become sensitive.

When uncertain, classify at the more protective level until an owner reviews the decision.

## Source code and development artifacts

Unless explicitly approved for public release:

- proprietary source code is at least `INTERNAL`;
- sensitive security design/vulnerability information is `CONFIDENTIAL` or `RESTRICTED` depending on exploitability;
- credentials, tokens and private keys must never be treated as ordinary documents and should be stored only in approved secret-management mechanisms;
- production data must not be copied into development/test environments without controlled justification and appropriate protection.

## Customer information

The product will need a customer-facing information-classification capability only if requirements show that customers need to label/control information at record, document, project, workspace or other scopes.

No customer classification taxonomy is approved by this internal policy. Customer classifications may differ by industry, contract or organisation and must be modelled deliberately.

## Handling lifecycle

Classification controls should address:

1. creation and labelling;
2. storage;
3. access;
4. sharing/collaboration;
5. export/download;
6. backup and recovery;
7. retention;
8. legal hold where required;
9. archival;
10. secure disposal.

## Exceptions

Exceptions to classification controls require documented approval by the accountable information/security owner and must record:

- information affected;
- reason;
- duration;
- compensating controls;
- residual risk;
- approver.

## References

- `software_project_docs/A_Enterprise_Pre_Project/Compliance__regulatory_register.md`
- NCSC secure development guidance: https://www.ncsc.gov.uk/collection/developers-collection/principles
- NCSC Software Security Code implementation guidance: https://www.ncsc.gov.uk/collection/software-security-code-of-practice-implementation-guidance/theme-1-secure-design-development

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Security | Initial programme information-classification baseline |