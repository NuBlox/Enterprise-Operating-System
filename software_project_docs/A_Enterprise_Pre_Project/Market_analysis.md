# Market analysis

**Section:** A_Enterprise_Pre_Project  
**Document ID:** NBEOS-A-003  
**Document Type:** Market analysis  
**Version:** 0.1  
**Status:** Draft  
**Author / Owner:** NuBlox Product  
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
**Related Documents:** `Product_vision_statement.md`, `Product_strategy.md`, `Competitor_analysis.md`  
**Supersedes:** None  
**Superseded By:** None  
**Template Used:** `software_project_docs_templates/A_Enterprise_Pre_Project/Market_analysis.md`  
**Storage Location:** `software_project_docs/A_Enterprise_Pre_Project/Market_analysis.md`  
**Access Permissions:** Repository access controls  
**Digital Signature:** Not yet signed  
**Audit Trail:** Git history  

## Purpose

Describe the market context in which NuBlox may compete, identify relevant buyer problems and market structures, and define the evidence still required before market selection and investment decisions are approved.

## Market framing

NuBlox sits at the intersection of several established software markets rather than inside a single category:

- enterprise resource planning (ERP);
- customer relationship management (CRM);
- human capital management (HCM);
- project and professional-services automation;
- workflow and service management;
- construction/project delivery platforms;
- document and information management;
- specialist industry authoring and operational systems.

This overlap is strategically important because the core NuBlox hypothesis concerns the fragmentation between **operating the enterprise** and **performing the delivery work**.

## Current market characteristics

### Broad enterprise suites are converging

Major vendors increasingly position their suites around connected data, workflows, AI-assisted operations and end-to-end business processes.

- Microsoft Dynamics 365 combines ERP and CRM applications across sales, service, finance and supply-chain operations and explicitly positions the products as individually usable or jointly connected.
- Oracle Fusion Cloud Applications spans ERP, procurement, project management, risk/compliance, supply chain, manufacturing, maintenance, HCM, sales and service.
- SAP positions cloud ERP as an integrated enterprise platform across core business processes.
- Workday, IFS and Infor similarly compete around connected enterprise management, industry depth and operational visibility.

Therefore, generic claims such as “one platform”, “connected data”, “workflow”, “real-time insight” or “AI-enabled” should be treated as table stakes rather than assumed differentiation.

### Industry delivery platforms are deep but often coexist with ERP

Construction and project-delivery platforms provide strong execution capabilities but commonly integrate with separate accounting or ERP systems.

Procore, for example, documents ERP integrations with systems including Viewpoint, Sage, NetSuite, Acumatica, MRI, Workday and others. Its UK construction ERP positioning explicitly describes connecting site/project data to an existing accounting or ERP platform.

This supports a market hypothesis that a structural boundary still exists between enterprise back-office operations and industry/project delivery in many customer estates.

### Professional-services/project ERP is already mature

Deltek Vantagepoint is purpose-built for architecture, engineering and consulting firms and connects projects, pipeline, people and financials. Deltek states that a very high proportion of leading A&E firms use its products. Unanet also serves architecture and engineering firms with project-based ERP capabilities.

This means a consultancy-focused NuBlox launch would enter an established category and must provide materially more value than project accounting, CRM, resource planning and project management alone.

## Buyer problem hypotheses

The following problems require validation through primary research:

1. **Operational fragmentation** — multiple systems represent different parts of the same customer, project, contract, employee, supplier or delivery outcome.
2. **Manual handoffs** — email, spreadsheets and re-keying bridge gaps between formal systems.
3. **Professional-output separation** — business systems manage project/commercial data while the actual professional outputs and their review/issue lifecycle live elsewhere.
4. **Weak end-to-end traceability** — the connection between business intent, work performed, decisions, deliverables and financial outcomes is difficult to reconstruct.
5. **Management-information lag** — management reporting requires separate consolidation rather than emerging directly from operating activity.
6. **Implementation burden** — customers may already have many systems but still lack an operating model that joins them coherently.

These are hypotheses, not established universal conditions.

## Candidate initial market

The working beachhead is UK mid-market multidisciplinary built-environment consultancies and project-services firms, approximately 50–500 employees.

Reasons to investigate this segment first include:

- project-based revenue and delivery;
- cross-functional commercial, finance, people and project operations;
- frequent production of governed professional outputs;
- significant external collaboration;
- enough complexity to test an enterprise operating model;
- lower first-release operational breadth than a main contractor, manufacturer or integrated owner/operator.

The segment must be validated before approval.

## Adjacent markets

Potential adjacent segments include:

- specialist contractors;
- main contractors;
- developers and asset owners;
- facilities and asset operators;
- integrated design-build-operate organisations;
- other professional/project-service industries.

Each should be evaluated for common versus genuinely different operating requirements before being added to the product roadmap.

## Market opportunity hypothesis

The potential opportunity is not simply to replace individual incumbent applications. It is to reduce the number of disconnected operating contexts needed to achieve one business outcome.

A commercially meaningful opportunity exists only if NuBlox can demonstrate one or more of the following:

- fewer systems required for a repeatable business outcome;
- less duplicate data entry and reconciliation;
- faster handoffs;
- improved fee/margin/cash control;
- better utilisation and resource visibility;
- stronger governance and evidence;
- faster project/professional delivery;
- lower implementation/configuration cost than a comparable multi-product estate;
- improved user adoption because work is organised around outcomes rather than module boundaries.

## Market barriers

- strong incumbent vendor relationships;
- high migration cost and perceived switching risk;
- broad functionality expected from established enterprise software;
- specialist tools that customers do not want to replace;
- long enterprise buying cycles;
- implementation/change-management burden;
- integration requirements with payroll, banking, tax, design/engineering and client systems;
- security, privacy, resilience and regulatory expectations;
- need for credible references before enterprise buyers will accept platform risk.

## Evidence gaps

The following work remains required:

1. quantitative market sizing for the selected launch segment;
2. primary interviews with buyers and users;
3. current system-estate mapping for representative firms;
4. total cost of current software/process fragmentation;
5. willingness-to-switch evidence;
6. buying-centre and procurement analysis;
7. implementation budget and timescale expectations;
8. geographic/regulatory constraints;
9. partner/channel opportunity;
10. evidence that identified pain is repeatable across multiple firms.

## Preliminary market implication

The market appears sufficiently mature that NuBlox should **not** compete on feature breadth or “all-in-one” language alone. The product should be evaluated on whether it creates a materially better operating model across boundaries that existing suites and specialist tools still require customers to bridge manually or through integration-heavy estates.

## References

Official vendor sources reviewed 2026-09-26:

- Microsoft Dynamics 365: https://www.microsoft.com/en-gb/dynamics-365/what-is-dynamics-365
- Microsoft ERP overview: https://www.microsoft.com/en-gb/dynamics-365/solutions/erp
- Oracle Fusion Applications scope: https://docs.oracle.com/en/cloud/saas/applications-common/scope.html
- Deltek Vantagepoint: https://www.deltek.com/products/erp/vantagepoint/
- Procore ERP integrations: https://support.procore.com/products/online/user-guide/company-level/erp-integrations
- Procore UK construction ERP: https://www.procore.com/en-gb/fc/construction-erp-software

## Change History

| Version | Date | Author | Description |
|---|---|---|---|
| 0.1 | 2026-09-26 | NuBlox Product | Initial controlled draft instantiated from the NuBlox project template |