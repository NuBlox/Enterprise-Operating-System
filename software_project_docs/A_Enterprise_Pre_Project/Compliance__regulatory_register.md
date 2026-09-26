# Compliance / regulatory register

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-010  
**Document Type:** Compliance / regulatory register  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Compliance  
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
**Related Documents:** `Security_classification_policy.md`, `Data_governance_policy.md`, `Enterprise_risk_register.md`, `Feasibility_study.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Compliance__regulatory_register.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Compliance__regulatory_register.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Maintain the initial legal, regulatory and assurance obligations that may apply to NuBlox as a UK-developed enterprise SaaS/software product and to the customer information it may process.

This register is a programme control document, not legal advice. Applicability must be confirmed for the final product, deployment model, customer contract and jurisdiction.

## Current geographic assumption

The first market hypothesis is the United Kingdom. Expansion to other jurisdictions will require a separate applicability assessment.

## Regulatory baseline

| ID | Requirement / source | Why it may apply | Current implication | Status |
|---|---|---|---|---|
| C-001 | UK GDPR | NuBlox is expected to process personal data relating to users, employees, contacts and other individuals | Privacy by design, lawful processing, minimisation, security, retention, rights handling, accountability | Applicable / scope to define |
| C-002 | Data Protection Act 2018 | Supplements/tailors UK GDPR and governs UK personal-data processing | Legal/privacy controls, special-category/criminal-offence handling where relevant | Applicable / scope to define |
| C-003 | Data (Use and Access) Act 2025 | Amends UK data/privacy framework and introduces wider data-access provisions | Privacy/legal guidance must track commenced amendments; policies cannot rely on outdated pre-2025 assumptions | Applicable changes to monitor |
| C-004 | Privacy and Electronic Communications Regulations (PECR) | May apply to marketing communications, cookies and electronic communications | Consent/marketing/cookie controls for public services and communications | Likely applicable to relevant processing |
| C-005 | Companies Act 2006 / statutory record obligations | NuBlox may hold company/accounting/governance records for customers | Retention/configuration requirements may vary by record type and customer use | Customer-use dependent |
| C-006 | UK accounting/tax record requirements | Finance features may retain records subject to HMRC/statutory requirements | Record retention, auditability and export requirements | Capability dependent |
| C-007 | Equality Act 2010 | Product/service accessibility and employment-related processes may have equality implications | Inclusive design and non-discriminatory product processes | Relevant |
| C-008 | UK accessibility expectations / WCAG procurement requirements | Public-sector and enterprise buyers may contractually require accessibility | Accessibility requirements and testing should be controlled from design stage | Market/contract dependent |
| C-009 | Computer Misuse Act 1990 | Security testing/administration must remain authorised | Security operations and penetration testing require clear authority | Relevant |
| C-010 | Copyright, Designs and Patents Act 1988 / IP law | Product source, customer content and third-party materials involve IP rights | Licence controls, content ownership, third-party software inventory | Relevant |
| C-011 | Software/open-source licence obligations | Dependencies may impose attribution, source or redistribution requirements | SBOM and licence inventory required during development | Relevant once dependencies exist |
| C-012 | Contractual confidentiality / client security requirements | Built-environment projects frequently impose project/client information controls | Tenant/project classification, access, export and audit requirements | Customer/contract dependent |
| C-013 | Construction-sector information-management requirements | Customers may operate to ISO 19650 or client-specific CDE/information standards | Information status, revision, approval, suitability and audit requirements may become product requirements | Segment dependent |
| C-014 | Professional/regulatory body requirements | Architects, engineers, surveyors and other professions may have record/competence/ethical requirements | Requirements discovery by profession and service type | Segment dependent |
| C-015 | Cyber-security governance expectations | Enterprise buyers expect secure development, governance and operational resilience | Secure SDLC, risk ownership, incident response, supplier security and evidence | Relevant |

## Data-protection principles

The current UK data-protection baseline should reflect the seven UK GDPR principles identified by the ICO:

- lawfulness, fairness and transparency;
- purpose limitation;
- data minimisation;
- accuracy;
- storage limitation;
- integrity and confidentiality (security);
- accountability.

The ICO notes that its guidance is being updated following the Data (Use and Access) Act 2025, so implementation decisions must use current guidance at the time they are made.

## Compliance-by-design requirements

The programme should plan for:

- data inventory and processing records;
- controller/processor role analysis;
- lawful-basis assessment;
- privacy notices and transparency;
- configurable retention and disposal;
- subject-rights support where applicable;
- data export and portability capabilities where required;
- consent/preferences where required;
- audit trails;
- security by design/default;
- DPIAs for high-risk processing;
- supplier/subprocessor governance;
- international-transfer controls if data leaves the UK;
- breach/incident procedures;
- contractual data-processing terms;
- evidence that configuration does not silently defeat compliance controls.

## Security governance references

UK NCSC guidance should inform the secure-development programme. NCSC secure-development principles emphasise security throughout delivery, protection of repositories and pipelines, continual testing and planning for security flaws. The 2025 Cyber Governance Code of Practice also provides board-level governance guidance for managing cyber risk.

## Industry-specific discovery

Before industry requirements are baselined, NuBlox must identify which obligations arise from:

- law/regulation;
- professional rules;
- standards adopted contractually;
- client/project information requirements;
- organisational policy;
- good practice rather than mandatory obligation.

These categories must not be conflated because configuration, evidence and liability implications differ.

## Immediate compliance actions

1. appoint privacy/security/compliance ownership;
2. define anticipated controller/processor roles;
3. create initial data inventory during requirements work;
4. create DPIA trigger criteria;
5. define secure-development lifecycle requirements;
6. establish third-party/subprocessor register when services are selected;
7. establish open-source/SBOM controls when code dependencies begin;
8. perform sector-specific obligations research for the selected launch workflows;
9. establish retention policy before production data is stored;
10. review this register before entering new jurisdictions.

## References

Official sources reviewed 2026-09-26:

- ICO data-protection principles: https://ico.org.uk/for-organisations/uk-gdpr-guidance-and-resources/data-protection-principles/a-guide-to-the-data-protection-principles/
- Data Protection Act 2018: https://www.legislation.gov.uk/ukpga/2018/12/contents
- Data (Use and Access) Act 2025: https://www.legislation.gov.uk/ukpga/2025/18/contents
- UK Government DUAA collection: https://www.gov.uk/government/collections/data-use-and-access-act-2025
- NCSC secure-development guidance: https://www.ncsc.gov.uk/collection/developers-collection/principles
- Cyber Governance Code of Practice: https://www.gov.uk/government/publications/cyber-governance-code-of-practice

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Compliance | Initial UK pre-project compliance baseline |