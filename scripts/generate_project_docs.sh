#!/usr/bin/env bash
# generate_project_docs.sh
# Generates placeholder documents for a software development project.
# Usage: ./generate_project_docs.sh [base_directory]
# Default base directory: software_project_docs

set -euo pipefail

BASE_DIR="${1:-software_project_docs}"
TEMPLATE_DIR="$BASE_DIR/00_Templates"
mkdir -p "$TEMPLATE_DIR"

# ------------------------------------------------------------------
# Helper: sanitize a string for use as a filename
# ------------------------------------------------------------------
sanitize() {
    echo "$1" | tr ' ' '_' | tr -cd '[:alnum:]_-' | cut -c1-200
}

# ------------------------------------------------------------------
# Helper: create a placeholder document
# ------------------------------------------------------------------
create_placeholder() {
    local dir="$1"
    local doc="$2"
    local section_dir="$BASE_DIR/$dir"
    mkdir -p "$section_dir"
    local filename
    filename=$(sanitize "$doc")
    local filepath="$section_dir/${filename}.md"
    if [ -f "$filepath" ]; then
        echo "Skipping existing: $filepath"
        return
    fi
    cat > "$filepath" <<EOF
# $doc

**Section:** $dir  
**Document ID:** DOC-$(date +%s)-$RANDOM  
**Version:** 0.1  
**Status:** Draft  
**Author:** [TBD]  
**Reviewer:** [TBD]  
**Approver:** [TBD]  
**Approval Date:** [TBD]  
**Effective Date:** [TBD]  
**Review Date:** [TBD]  
**Classification:** Internal  
**Retention Period:** [TBD]  
**Distribution:** [TBD]  

## Purpose
[Describe the purpose of this document]

## Content
[Placeholder for content]

## References
- [Related documents]

## Change History
| Version | Date | Author | Description |
|---------|------|--------|-------------|
| 0.1     |      |        | Initial draft |
EOF
    echo "Created: $filepath"
}

# ------------------------------------------------------------------
# Create the document control metadata template
# ------------------------------------------------------------------
cat > "$TEMPLATE_DIR/Document_Control_Metadata_Template.md" <<'EOF'
# Document Control Metadata Template

Apply these fields to every document:

- Document ID
- Document title
- Document type
- Version number
- Status: draft / in review / approved / retired
- Author / owner
- Reviewer
- Approver
- Approval date
- Effective date
- Review date
- Expiry date
- Classification: public / internal / confidential / restricted
- Retention period
- Disposal method
- Distribution list
- Related documents
- Supersedes
- Superseded by
- Change history
- Template used
- Storage location
- Access permissions
- Digital signature
- Audit trail
EOF
echo "Created: $TEMPLATE_DIR/Document_Control_Metadata_Template.md"

# ------------------------------------------------------------------
# Master document list (section|document name)
# ------------------------------------------------------------------
while IFS='|' read -r dir doc; do
    [[ -z "$dir" || "$dir" =~ ^# ]] && continue
    create_placeholder "$dir" "$doc"
done <<'DOC_LIST'
A_Enterprise_Pre_Project|Enterprise architecture principles
A_Enterprise_Pre_Project|IT strategy / digital strategy
A_Enterprise_Pre_Project|Digital transformation roadmap
A_Enterprise_Pre_Project|Product vision statement
A_Enterprise_Pre_Project|Product strategy
A_Enterprise_Pre_Project|Business model canvas / lean canvas
A_Enterprise_Pre_Project|Market analysis
A_Enterprise_Pre_Project|Competitor analysis
A_Enterprise_Pre_Project|Customer research summary
A_Enterprise_Pre_Project|Business case
A_Enterprise_Pre_Project|Feasibility study
A_Enterprise_Pre_Project|Cost-benefit analysis
A_Enterprise_Pre_Project|ROI / NPV / IRR / payback analysis
A_Enterprise_Pre_Project|Total cost of ownership (TCO) model
A_Enterprise_Pre_Project|Funding approval / investment memo
A_Enterprise_Pre_Project|Portfolio charter
A_Enterprise_Pre_Project|Programme charter
A_Enterprise_Pre_Project|Portfolio roadmap
A_Enterprise_Pre_Project|Benefits map / benefits dependency network
A_Enterprise_Pre_Project|Benefits realisation plan
A_Enterprise_Pre_Project|Enterprise risk register
A_Enterprise_Pre_Project|Organisational change management strategy
A_Enterprise_Pre_Project|Stakeholder map
A_Enterprise_Pre_Project|Communication strategy
A_Enterprise_Pre_Project|Procurement strategy
A_Enterprise_Pre_Project|Sourcing strategy
A_Enterprise_Pre_Project|Make-vs-buy analysis
A_Enterprise_Pre_Project|Vendor risk assessment
A_Enterprise_Pre_Project|Compliance / regulatory register
A_Enterprise_Pre_Project|Data governance policy
A_Enterprise_Pre_Project|Security classification policy
A_Enterprise_Pre_Project|Enterprise data model
A_Enterprise_Pre_Project|Reference architecture
A_Enterprise_Pre_Project|Technology radar / standards catalogue
A_Enterprise_Pre_Project|Audit and regulatory requirements matrix
A_Enterprise_Pre_Project|Insurance and bonding requirements
A_Enterprise_Pre_Project|Budget / funding model
A_Enterprise_Pre_Project|Chargeback / showback model
A_Enterprise_Pre_Project|Capacity and demand plan
A_Enterprise_Pre_Project|Resource pool plan
A_Enterprise_Pre_Project|Skills matrix
A_Enterprise_Pre_Project|Training and capability plan
A_Enterprise_Pre_Project|Exit / divestment / decommissioning strategy
B_Legal_Commercial_Procurement|Non-disclosure agreement (NDA)
B_Legal_Commercial_Procurement|Memorandum of understanding (MOU)
B_Legal_Commercial_Procurement|Master services agreement (MSA)
B_Legal_Commercial_Procurement|Statement of work (SOW)
B_Legal_Commercial_Procurement|Contract / purchase agreement
B_Legal_Commercial_Procurement|Software licence agreement
B_Legal_Commercial_Procurement|End-user licence agreement (EULA)
B_Legal_Commercial_Procurement|Terms of service
B_Legal_Commercial_Procurement|Privacy policy
B_Legal_Commercial_Procurement|Cookie policy
B_Legal_Commercial_Procurement|Acceptable use policy
B_Legal_Commercial_Procurement|Data processing agreement (DPA)
B_Legal_Commercial_Procurement|Data transfer agreement
B_Legal_Commercial_Procurement|Business associate agreement (BAA) — HIPAA
B_Legal_Commercial_Procurement|Service level agreement (SLA)
B_Legal_Commercial_Procurement|Operational level agreement (OLA)
B_Legal_Commercial_Procurement|Support and maintenance agreement
B_Legal_Commercial_Procurement|Escrow agreement
B_Legal_Commercial_Procurement|IP assignment agreement
B_Legal_Commercial_Procurement|Contractor agreement
B_Legal_Commercial_Procurement|Subcontractor agreement
B_Legal_Commercial_Procurement|Consultancy agreement
B_Legal_Commercial_Procurement|Request for information (RFI)
B_Legal_Commercial_Procurement|Request for proposal (RFP)
B_Legal_Commercial_Procurement|Request for quotation (RFQ)
B_Legal_Commercial_Procurement|Vendor evaluation matrix
B_Legal_Commercial_Procurement|Vendor scorecard
B_Legal_Commercial_Procurement|Procurement plan
B_Legal_Commercial_Procurement|Purchase order (PO)
B_Legal_Commercial_Procurement|Invoice
B_Legal_Commercial_Procurement|Timesheet
B_Legal_Commercial_Procurement|Expense report
B_Legal_Commercial_Procurement|Insurance certificate
B_Legal_Commercial_Procurement|Performance bond
B_Legal_Commercial_Procurement|Export control assessment
B_Legal_Commercial_Procurement|Sanctions screening record
B_Legal_Commercial_Procurement|Anti-bribery / anti-corruption declaration
B_Legal_Commercial_Procurement|Conflict of interest declaration
B_Legal_Commercial_Procurement|Contract change request
B_Legal_Commercial_Procurement|Contract closure document
B_Legal_Commercial_Procurement|Legal review sign-off
C_Product_Market_UX|Product vision board
C_Product_Market_UX|Product roadmap
C_Product_Market_UX|Release roadmap
C_Product_Market_UX|Market requirements document (MRD)
C_Product_Market_UX|Product requirements document (PRD)
C_Product_Market_UX|Product backlog
C_Product_Market_UX|Product goal / OKRs
C_Product_Market_UX|Go-to-market plan
C_Product_Market_UX|Pricing strategy
C_Product_Market_UX|Packaging strategy
C_Product_Market_UX|Sales enablement material
C_Product_Market_UX|Customer support plan
C_Product_Market_UX|Customer journey map
C_Product_Market_UX|User journey map
C_Product_Market_UX|Empathy map
C_Product_Market_UX|Personas
C_Product_Market_UX|User research plan
C_Product_Market_UX|User research findings
C_Product_Market_UX|Usability test plan
C_Product_Market_UX|Usability test report
C_Product_Market_UX|A/B test plan
C_Product_Market_UX|A/B test report
C_Product_Market_UX|Analytics plan
C_Product_Market_UX|Metrics / KPI framework
C_Product_Market_UX|Information architecture
C_Product_Market_UX|Sitemap
C_Product_Market_UX|Wireframes
C_Product_Market_UX|Mockups
C_Product_Market_UX|High-fidelity designs
C_Product_Market_UX|Interactive prototypes
C_Product_Market_UX|Design system
C_Product_Market_UX|Style guide
C_Product_Market_UX|Component library
C_Product_Market_UX|Accessibility design specification
C_Product_Market_UX|Responsive design specification
C_Product_Market_UX|Localisation / internationalisation design specification
C_Product_Market_UX|Content strategy
C_Product_Market_UX|UX writing / microcopy guide
C_Product_Market_UX|Design review records
C_Product_Market_UX|Design sign-off
D_Project_Initiation|Project charter
D_Project_Initiation|Project brief
D_Project_Initiation|Project initiation document (PID)
D_Project_Initiation|High-level scope statement
D_Project_Initiation|Objectives and success criteria
D_Project_Initiation|Stakeholder register
D_Project_Initiation|Governance structure
D_Project_Initiation|Steering committee terms of reference
D_Project_Initiation|RACI chart
D_Project_Initiation|Team charter
D_Project_Initiation|Roles and responsibilities matrix
D_Project_Initiation|Kick-off presentation
D_Project_Initiation|Kick-off meeting minutes
D_Project_Initiation|Assumptions log
D_Project_Initiation|Constraints log
D_Project_Initiation|Initial risk register
D_Project_Initiation|Initial issue log
D_Project_Initiation|Initial dependency log
D_Project_Initiation|High-level schedule
D_Project_Initiation|Milestone list
D_Project_Initiation|High-level budget
D_Project_Initiation|High-level resource plan
D_Project_Initiation|Procurement approach
D_Project_Initiation|Compliance / regulatory register
D_Project_Initiation|Initial communications plan
D_Project_Initiation|Change control approach
D_Project_Initiation|Quality approach
D_Project_Initiation|Project methodology / tailoring document
D_Project_Initiation|Lessons learned from similar projects
D_Project_Initiation|Benefits realisation plan
D_Project_Initiation|Exit / closure criteria
D_Project_Initiation|Project organisation chart
D_Project_Initiation|Escalation matrix
D_Project_Initiation|Approval matrix
E_Project_Planning_Governance|Project management plan — master
E_Project_Planning_Governance|Scope management plan
E_Project_Planning_Governance|Schedule management plan
E_Project_Planning_Governance|Cost management plan
E_Project_Planning_Governance|Quality management plan
E_Project_Planning_Governance|Resource management plan
E_Project_Planning_Governance|Communications management plan
E_Project_Planning_Governance|Risk management plan
E_Project_Planning_Governance|Procurement management plan
E_Project_Planning_Governance|Stakeholder engagement plan
E_Project_Planning_Governance|Change management plan
E_Project_Planning_Governance|Configuration management plan
E_Project_Planning_Governance|Data management plan
E_Project_Planning_Governance|Security management plan
E_Project_Planning_Governance|Compliance management plan
E_Project_Planning_Governance|Benefits management plan
E_Project_Planning_Governance|Transition management plan
E_Project_Planning_Governance|Training management plan
E_Project_Planning_Governance|Knowledge management plan
E_Project_Planning_Governance|Detailed scope statement
E_Project_Planning_Governance|Work breakdown structure (WBS)
E_Project_Planning_Governance|WBS dictionary
E_Project_Planning_Governance|Schedule baseline
E_Project_Planning_Governance|Gantt chart
E_Project_Planning_Governance|Critical path analysis
E_Project_Planning_Governance|Milestone plan
E_Project_Planning_Governance|Cost baseline
E_Project_Planning_Governance|Budget
E_Project_Planning_Governance|Funding drawdown plan
E_Project_Planning_Governance|Cash flow forecast
E_Project_Planning_Governance|Resource plan
E_Project_Planning_Governance|Resource levelling plan
E_Project_Planning_Governance|RACI matrix
E_Project_Planning_Governance|Responsibility assignment matrix
E_Project_Planning_Governance|Risk register
E_Project_Planning_Governance|Issue log
E_Project_Planning_Governance|Assumptions log
E_Project_Planning_Governance|Dependency log
E_Project_Planning_Governance|Decision log
E_Project_Planning_Governance|Change log
E_Project_Planning_Governance|Quality metrics
E_Project_Planning_Governance|Quality checklist
E_Project_Planning_Governance|Communication matrix
E_Project_Planning_Governance|Meeting cadence
E_Project_Planning_Governance|Status report template
E_Project_Planning_Governance|Dashboard template
E_Project_Planning_Governance|Escalation matrix
E_Project_Planning_Governance|Approval matrix
E_Project_Planning_Governance|Procurement documents
E_Project_Planning_Governance|Contract management plan
E_Project_Planning_Governance|Vendor management plan
E_Project_Planning_Governance|Training plan
E_Project_Planning_Governance|Transition plan
E_Project_Planning_Governance|Documentation plan
E_Project_Planning_Governance|Document control plan
E_Project_Planning_Governance|Retention and disposal policy
E_Project_Planning_Governance|Configuration management plan
E_Project_Planning_Governance|CMDB entries
E_Project_Planning_Governance|Environment plan
E_Project_Planning_Governance|Access control plan
E_Project_Planning_Governance|Security plan
E_Project_Planning_Governance|Privacy plan
E_Project_Planning_Governance|Compliance plan
E_Project_Planning_Governance|Audit plan
E_Project_Planning_Governance|Stakeholder engagement plan
E_Project_Planning_Governance|Organisational change management plan
E_Project_Planning_Governance|Benefits realisation plan
E_Project_Planning_Governance|Knowledge management plan
E_Project_Planning_Governance|Tooling plan
E_Project_Planning_Governance|Licensing plan
E_Project_Planning_Governance|Support model
E_Project_Planning_Governance|SLA draft
E_Project_Planning_Governance|Operational readiness plan
E_Project_Planning_Governance|Disaster recovery plan draft
E_Project_Planning_Governance|Business continuity plan draft
E_Project_Planning_Governance|Data migration plan
E_Project_Planning_Governance|Cutover plan draft
E_Project_Planning_Governance|Release plan draft
E_Project_Planning_Governance|Test strategy draft
E_Project_Planning_Governance|Quality assurance plan
E_Project_Planning_Governance|Definition of Ready
E_Project_Planning_Governance|Definition of Done
E_Project_Planning_Governance|Tailoring matrix
E_Project_Planning_Governance|Governance calendar
E_Project_Planning_Governance|Decision rights matrix
F_Requirements_Analysis|Business requirements document (BRD)
F_Requirements_Analysis|Stakeholder requirements specification
F_Requirements_Analysis|Software requirements specification (SRS)
F_Requirements_Analysis|Functional requirements specification
F_Requirements_Analysis|Non-functional requirements specification
F_Requirements_Analysis|User stories
F_Requirements_Analysis|Epics
F_Requirements_Analysis|Themes
F_Requirements_Analysis|Use cases
F_Requirements_Analysis|Use case diagrams
F_Requirements_Analysis|User journeys
F_Requirements_Analysis|Customer journey maps
F_Requirements_Analysis|Personas
F_Requirements_Analysis|Process maps
F_Requirements_Analysis|BPMN diagrams
F_Requirements_Analysis|Value stream maps
F_Requirements_Analysis|Business rules
F_Requirements_Analysis|Data requirements
F_Requirements_Analysis|Data dictionary
F_Requirements_Analysis|Data classification
F_Requirements_Analysis|Data lineage
F_Requirements_Analysis|Data quality requirements
F_Requirements_Analysis|Interface requirements
F_Requirements_Analysis|Integration requirements
F_Requirements_Analysis|API requirements
F_Requirements_Analysis|Reporting requirements
F_Requirements_Analysis|Analytics requirements
F_Requirements_Analysis|Localisation / i18n requirements
F_Requirements_Analysis|Accessibility requirements
F_Requirements_Analysis|Performance requirements
F_Requirements_Analysis|Scalability requirements
F_Requirements_Analysis|Availability requirements
F_Requirements_Analysis|Reliability requirements
F_Requirements_Analysis|Maintainability requirements
F_Requirements_Analysis|Portability requirements
F_Requirements_Analysis|Security requirements
F_Requirements_Analysis|Privacy requirements
F_Requirements_Analysis|Compliance requirements
F_Requirements_Analysis|Regulatory requirements
F_Requirements_Analysis|Acceptance criteria
F_Requirements_Analysis|Definition of Ready
F_Requirements_Analysis|Definition of Done
F_Requirements_Analysis|Requirements traceability matrix (RTM)
F_Requirements_Analysis|Requirements prioritisation (MoSCoW, WSJF, etc.)
F_Requirements_Analysis|Backlog
F_Requirements_Analysis|Product roadmap
F_Requirements_Analysis|Release roadmap
F_Requirements_Analysis|Glossary
F_Requirements_Analysis|Acronym list
F_Requirements_Analysis|Requirements sign-off
F_Requirements_Analysis|Impact analysis
F_Requirements_Analysis|Gap analysis
F_Requirements_Analysis|Feasibility analysis
F_Requirements_Analysis|Prototype feedback
F_Requirements_Analysis|Workshop notes
F_Requirements_Analysis|Elicitation records
F_Requirements_Analysis|Business process re-engineering documents
F_Requirements_Analysis|Domain model
F_Requirements_Analysis|Conceptual model
F_Requirements_Analysis|Logical model
F_Requirements_Analysis|Business glossary
F_Requirements_Analysis|Requirements management plan
G_Architecture_Design|Solution architecture document
G_Architecture_Design|Enterprise architecture alignment
G_Architecture_Design|Architecture vision
G_Architecture_Design|Architecture definition
G_Architecture_Design|Architecture roadmap
G_Architecture_Design|Transition architectures
G_Architecture_Design|Architecture repository
G_Architecture_Design|High-level design (HLD)
G_Architecture_Design|Low-level design (LLD)
G_Architecture_Design|System context diagram
G_Architecture_Design|Container diagram
G_Architecture_Design|Component diagram
G_Architecture_Design|Class diagram
G_Architecture_Design|Object diagram
G_Architecture_Design|Package diagram
G_Architecture_Design|Sequence diagram
G_Architecture_Design|Activity diagram
G_Architecture_Design|State diagram
G_Architecture_Design|Collaboration diagram
G_Architecture_Design|Deployment diagram
G_Architecture_Design|Infrastructure architecture
G_Architecture_Design|Network diagram
G_Architecture_Design|Cloud architecture
G_Architecture_Design|On-prem / hybrid architecture
G_Architecture_Design|Multi-cloud architecture
G_Architecture_Design|Data architecture
G_Architecture_Design|Data model / ERD
G_Architecture_Design|Database design
G_Architecture_Design|Data warehouse / lake design
G_Architecture_Design|ETL / ELT design
G_Architecture_Design|Data migration design
G_Architecture_Design|API specification (OpenAPI / Swagger)
G_Architecture_Design|Integration design
G_Architecture_Design|Middleware design
G_Architecture_Design|Microservices design
G_Architecture_Design|Event-driven design
G_Architecture_Design|Serverless design
G_Architecture_Design|Security architecture
G_Architecture_Design|Threat model
G_Architecture_Design|Privacy by design assessment
G_Architecture_Design|Identity and access management design
G_Architecture_Design|Encryption / key management design
G_Architecture_Design|Logging / monitoring design
G_Architecture_Design|Observability design
G_Architecture_Design|Resilience design
G_Architecture_Design|Disaster recovery design
G_Architecture_Design|Backup design
G_Architecture_Design|Performance design
G_Architecture_Design|Scalability design
G_Architecture_Design|UI/UX design specification
G_Architecture_Design|Information architecture
G_Architecture_Design|Wireframes
G_Architecture_Design|Mockups
G_Architecture_Design|High-fidelity designs
G_Architecture_Design|Interactive prototypes
G_Architecture_Design|Design system
G_Architecture_Design|Style guide
G_Architecture_Design|Component library
G_Architecture_Design|Accessibility design specification
G_Architecture_Design|Responsive design specification
G_Architecture_Design|Localisation design specification
G_Architecture_Design|Architecture decision records (ADRs)
G_Architecture_Design|Technical spikes
G_Architecture_Design|Proof of concept report
G_Architecture_Design|Evaluation matrix for technology selection
G_Architecture_Design|Configuration specification
G_Architecture_Design|Environment design
G_Architecture_Design|CI/CD design
G_Architecture_Design|Release design
G_Architecture_Design|Rollback design
G_Architecture_Design|Technical debt register
G_Architecture_Design|Coding standards
G_Architecture_Design|Branching strategy
G_Architecture_Design|Versioning strategy
G_Architecture_Design|API versioning strategy
G_Architecture_Design|Error handling strategy
G_Architecture_Design|Logging standards
G_Architecture_Design|Naming conventions
G_Architecture_Design|Repository structure
G_Architecture_Design|Infrastructure as Code (IaC) documentation
G_Architecture_Design|Containerisation strategy
G_Architecture_Design|Orchestration strategy
G_Architecture_Design|Service mesh design
G_Architecture_Design|API gateway design
G_Architecture_Design|Message queue design
G_Architecture_Design|Cache design
G_Architecture_Design|Search design
G_Architecture_Design|Reporting design
G_Architecture_Design|Analytics design
G_Architecture_Design|Machine learning design
G_Architecture_Design|AI model architecture
G_Architecture_Design|Data pipeline design
G_Architecture_Design|Feature store design
G_Architecture_Design|Model serving design
G_Architecture_Design|MLOps design
H_Development_Implementation|Development plan
H_Development_Implementation|Iteration / sprint plan
H_Development_Implementation|Product backlog
H_Development_Implementation|Sprint backlog
H_Development_Implementation|Task board
H_Development_Implementation|Code repository README
H_Development_Implementation|CONTRIBUTING guide
H_Development_Implementation|Code of conduct
H_Development_Implementation|Licence file
H_Development_Implementation|Code review checklist
H_Development_Implementation|Pull request template
H_Development_Implementation|Commit message guidelines
H_Development_Implementation|Branching policy
H_Development_Implementation|Environment setup guide
H_Development_Implementation|Build instructions
H_Development_Implementation|Dependency management document
H_Development_Implementation|Software bill of materials (SBOM)
H_Development_Implementation|Open-source licence inventory
H_Development_Implementation|Third-party notices
H_Development_Implementation|API documentation
H_Development_Implementation|Developer documentation
H_Development_Implementation|Technical design documents
H_Development_Implementation|Data migration scripts documentation
H_Development_Implementation|Unit test plan
H_Development_Implementation|Unit test cases
H_Development_Implementation|Unit test reports
H_Development_Implementation|Code coverage reports
H_Development_Implementation|Static analysis reports
H_Development_Implementation|Linting reports
H_Development_Implementation|Peer review records
H_Development_Implementation|Pair programming notes
H_Development_Implementation|Refactoring log
H_Development_Implementation|Technical debt log
H_Development_Implementation|Defect reports — development
H_Development_Implementation|Changelog
H_Development_Implementation|Release notes draft
H_Development_Implementation|Build artefacts manifest
H_Development_Implementation|Container images manifest
H_Development_Implementation|Configuration files documentation
H_Development_Implementation|Secrets management documentation
H_Development_Implementation|Feature flags documentation
H_Development_Implementation|Database migration scripts
H_Development_Implementation|Seed data documentation
H_Development_Implementation|Mock data documentation
H_Development_Implementation|Developer onboarding guide
H_Development_Implementation|Knowledge transfer documents
H_Development_Implementation|Sprint review notes
H_Development_Implementation|Sprint retrospective notes
H_Development_Implementation|Daily stand-up notes
H_Development_Implementation|Burndown chart
H_Development_Implementation|Burnup chart
H_Development_Implementation|Velocity chart
H_Development_Implementation|Cumulative flow diagram
H_Development_Implementation|Cycle time / lead time reports
H_Development_Implementation|Code quality metrics
H_Development_Implementation|SonarQube / quality gate reports
H_Development_Implementation|Dependency vulnerability reports
H_Development_Implementation|Licence compliance reports
H_Development_Implementation|Infrastructure provisioning scripts
H_Development_Implementation|IaC modules documentation
H_Development_Implementation|CI/CD pipeline documentation
H_Development_Implementation|Build pipeline logs
H_Development_Implementation|Deployment pipeline logs
H_Development_Implementation|Environment configuration records
H_Development_Implementation|Secret rotation records
H_Development_Implementation|Access provisioning records
H_Development_Implementation|Development environment inventory
H_Development_Implementation|Toolchain documentation
H_Development_Implementation|IDE configuration guide
H_Development_Implementation|Localisation files
H_Development_Implementation|Translation memory
H_Development_Implementation|String catalogue
H_Development_Implementation|Asset inventory
H_Development_Implementation|Media assets
H_Development_Implementation|Font licences
H_Development_Implementation|Icon licences
H_Development_Implementation|Image licences
I_Testing_QA|Test strategy
I_Testing_QA|Test plan
I_Testing_QA|Test approach
I_Testing_QA|Test cases
I_Testing_QA|Test scripts
I_Testing_QA|Test data
I_Testing_QA|Test data management plan
I_Testing_QA|Test environment plan
I_Testing_QA|Test environment configuration
I_Testing_QA|Test execution report
I_Testing_QA|Test summary report
I_Testing_QA|Defect reports
I_Testing_QA|Defect log
I_Testing_QA|Defect triage notes
I_Testing_QA|Root cause analysis
I_Testing_QA|Regression test suite
I_Testing_QA|Smoke test suite
I_Testing_QA|Sanity test suite
I_Testing_QA|Integration test plan
I_Testing_QA|Integration test report
I_Testing_QA|System test plan
I_Testing_QA|System test report
I_Testing_QA|UAT plan
I_Testing_QA|UAT scripts
I_Testing_QA|UAT scenarios
I_Testing_QA|UAT sign-off
I_Testing_QA|Performance test plan
I_Testing_QA|Performance test scripts
I_Testing_QA|Performance test report
I_Testing_QA|Load test report
I_Testing_QA|Stress test report
I_Testing_QA|Scalability test report
I_Testing_QA|Endurance test report
I_Testing_QA|Security test plan
I_Testing_QA|Security test report
I_Testing_QA|Vulnerability assessment report
I_Testing_QA|Penetration test report
I_Testing_QA|SAST report
I_Testing_QA|DAST report
I_Testing_QA|IAST report
I_Testing_QA|SCA report
I_Testing_QA|Secrets scanning report
I_Testing_QA|Container scanning report
I_Testing_QA|Infrastructure scanning report
I_Testing_QA|Accessibility test plan
I_Testing_QA|Accessibility test report
I_Testing_QA|Compatibility test report
I_Testing_QA|Localisation test report
I_Testing_QA|Usability test plan
I_Testing_QA|Usability test report
I_Testing_QA|User feedback
I_Testing_QA|Alpha test report
I_Testing_QA|Beta test report
I_Testing_QA|Acceptance test report
I_Testing_QA|Quality audit report
I_Testing_QA|Test closure report
I_Testing_QA|Updated RTM
I_Testing_QA|Test metrics
I_Testing_QA|Coverage report
I_Testing_QA|Bug bounty report
I_Testing_QA|Crowdtesting report
I_Testing_QA|Exploratory testing charter
I_Testing_QA|Session-based test management notes
I_Testing_QA|Risk-based testing analysis
I_Testing_QA|Quality risk register
I_Testing_QA|Quality control records
I_Testing_QA|Quality assurance records
I_Testing_QA|Calibration records
I_Testing_QA|Peer test review records
I_Testing_QA|Test automation framework documentation
I_Testing_QA|Test automation scripts
I_Testing_QA|Test automation reports
I_Testing_QA|CI test reports
I_Testing_QA|Nightly build reports
I_Testing_QA|Release candidate validation report
I_Testing_QA|Production verification test report
J_Deployment_Release|Release plan
J_Deployment_Release|Release calendar
J_Deployment_Release|Deployment plan
J_Deployment_Release|Deployment runbook
J_Deployment_Release|Cutover plan
J_Deployment_Release|Cutover checklist
J_Deployment_Release|Rollback plan
J_Deployment_Release|Rollback runbook
J_Deployment_Release|Backout plan
J_Deployment_Release|Go-live checklist
J_Deployment_Release|Go / no-go decision record
J_Deployment_Release|Release notes
J_Deployment_Release|Release approval
J_Deployment_Release|Change request
J_Deployment_Release|CAB submission
J_Deployment_Release|CAB minutes
J_Deployment_Release|Deployment checklist
J_Deployment_Release|Environment configuration document
J_Deployment_Release|Infrastructure provisioning document
J_Deployment_Release|DNS / SSL / certificate document
J_Deployment_Release|Firewall rules document
J_Deployment_Release|Load balancer configuration
J_Deployment_Release|Database deployment document
J_Deployment_Release|Data migration runbook
J_Deployment_Release|Data validation report
J_Deployment_Release|Post-deployment verification plan
J_Deployment_Release|Post-deployment verification report
J_Deployment_Release|Smoke test results
J_Deployment_Release|Operational readiness review
J_Deployment_Release|Service transition plan
J_Deployment_Release|Support handover document
J_Deployment_Release|Knowledge transfer session notes
J_Deployment_Release|Training materials
J_Deployment_Release|User manual
J_Deployment_Release|Admin guide
J_Deployment_Release|Quick reference guide
J_Deployment_Release|FAQ
J_Deployment_Release|Release communications
J_Deployment_Release|Announcement
J_Deployment_Release|Downtime notice
J_Deployment_Release|Maintenance window document
J_Deployment_Release|Hypercare plan
J_Deployment_Release|Hypercare report
J_Deployment_Release|War room notes
J_Deployment_Release|Incident log during release
J_Deployment_Release|Release retrospective
J_Deployment_Release|Deployment metrics
J_Deployment_Release|Change success rate
J_Deployment_Release|Failed change report
J_Deployment_Release|Emergency change record
J_Deployment_Release|Standard change record
J_Deployment_Release|Normal change record
J_Deployment_Release|Release closure report
K_Operations_Support_Maintenance|Service description
K_Operations_Support_Maintenance|Service catalogue entry
K_Operations_Support_Maintenance|SLA
K_Operations_Support_Maintenance|SLO
K_Operations_Support_Maintenance|SLI
K_Operations_Support_Maintenance|OLA
K_Operations_Support_Maintenance|Support model
K_Operations_Support_Maintenance|Support matrix
K_Operations_Support_Maintenance|Escalation matrix
K_Operations_Support_Maintenance|Incident management procedure
K_Operations_Support_Maintenance|Major incident procedure
K_Operations_Support_Maintenance|Problem management procedure
K_Operations_Support_Maintenance|Change management procedure
K_Operations_Support_Maintenance|Release management procedure
K_Operations_Support_Maintenance|Configuration management procedure
K_Operations_Support_Maintenance|Asset management procedure
K_Operations_Support_Maintenance|CMDB records
K_Operations_Support_Maintenance|Monitoring documentation
K_Operations_Support_Maintenance|Alerting documentation
K_Operations_Support_Maintenance|Dashboards
K_Operations_Support_Maintenance|Logging documentation
K_Operations_Support_Maintenance|Tracing documentation
K_Operations_Support_Maintenance|Observability runbook
K_Operations_Support_Maintenance|On-call handbook
K_Operations_Support_Maintenance|Runbooks
K_Operations_Support_Maintenance|Playbooks
K_Operations_Support_Maintenance|Standard operating procedures (SOPs)
K_Operations_Support_Maintenance|Backup procedure
K_Operations_Support_Maintenance|Restore procedure
K_Operations_Support_Maintenance|Backup test report
K_Operations_Support_Maintenance|Disaster recovery plan
K_Operations_Support_Maintenance|DR test report
K_Operations_Support_Maintenance|Business continuity plan
K_Operations_Support_Maintenance|BCP test report
K_Operations_Support_Maintenance|Capacity plan
K_Operations_Support_Maintenance|Performance monitoring report
K_Operations_Support_Maintenance|Availability report
K_Operations_Support_Maintenance|Maintenance plan
K_Operations_Support_Maintenance|Patch management procedure
K_Operations_Support_Maintenance|Vulnerability management procedure
K_Operations_Support_Maintenance|Security operations runbook
K_Operations_Support_Maintenance|Access control matrix
K_Operations_Support_Maintenance|User access review
K_Operations_Support_Maintenance|Audit logs
K_Operations_Support_Maintenance|Compliance reports
K_Operations_Support_Maintenance|Service reports
K_Operations_Support_Maintenance|Monthly service review
K_Operations_Support_Maintenance|Quarterly business review
K_Operations_Support_Maintenance|Continuous improvement log
K_Operations_Support_Maintenance|CSI register
K_Operations_Support_Maintenance|Known error database
K_Operations_Support_Maintenance|Workarounds
K_Operations_Support_Maintenance|Deprecation plan
K_Operations_Support_Maintenance|End-of-life plan
K_Operations_Support_Maintenance|Decommissioning plan
K_Operations_Support_Maintenance|Data retention records
K_Operations_Support_Maintenance|Data disposal records
K_Operations_Support_Maintenance|Licence management
K_Operations_Support_Maintenance|FinOps reports
K_Operations_Support_Maintenance|Cost optimisation report
K_Operations_Support_Maintenance|Green IT / sustainability report
K_Operations_Support_Maintenance|Operational risk register
K_Operations_Support_Maintenance|Service continuity plan
K_Operations_Support_Maintenance|Supplier management records
K_Operations_Support_Maintenance|Escalation records
K_Operations_Support_Maintenance|Incident post-mortem
K_Operations_Support_Maintenance|Problem post-mortem
K_Operations_Support_Maintenance|Change post-implementation review
K_Operations_Support_Maintenance|Release post-implementation review
K_Operations_Support_Maintenance|Service improvement plan
K_Operations_Support_Maintenance|Automation inventory
K_Operations_Support_Maintenance|Toil reduction plan
K_Operations_Support_Maintenance|Error budget policy
K_Operations_Support_Maintenance|SRE runbooks
K_Operations_Support_Maintenance|Chaos engineering plan
K_Operations_Support_Maintenance|Chaos engineering report
K_Operations_Support_Maintenance|Resilience test report
K_Operations_Support_Maintenance|Failover test report
K_Operations_Support_Maintenance|Recovery test report
K_Operations_Support_Maintenance|Security operations report
K_Operations_Support_Maintenance|Threat intelligence report
K_Operations_Support_Maintenance|SIEM use cases
K_Operations_Support_Maintenance|SOC procedures
K_Operations_Support_Maintenance|Incident response plan
K_Operations_Support_Maintenance|Incident response report
K_Operations_Support_Maintenance|Forensic report
K_Operations_Support_Maintenance|Breach notification record
K_Operations_Support_Maintenance|Regulatory notification record
L_Project_Closure|Acceptance certificate
L_Project_Closure|Final acceptance sign-off
L_Project_Closure|Final project report
L_Project_Closure|Final financial report
L_Project_Closure|Budget closure
L_Project_Closure|Contract closure
L_Project_Closure|Procurement closure
L_Project_Closure|Resource release
L_Project_Closure|Team performance review
L_Project_Closure|Lessons learned
L_Project_Closure|Post-implementation review
L_Project_Closure|Benefits realisation report
L_Project_Closure|KPI report
L_Project_Closure|Handover document
L_Project_Closure|Archive index
L_Project_Closure|Document retention records
L_Project_Closure|Closure checklist
L_Project_Closure|Sign-off records
L_Project_Closure|Outstanding issues log
L_Project_Closure|Warranty / support transition
L_Project_Closure|Final risk report
L_Project_Closure|Final quality report
L_Project_Closure|Final communications
L_Project_Closure|Celebration / recognition record
L_Project_Closure|Project closure approval
L_Project_Closure|Archive and disposal certificate
M_Cross_Cutting_Registers_Logs_Reports|Meeting minutes
M_Cross_Cutting_Registers_Logs_Reports|Action items log
M_Cross_Cutting_Registers_Logs_Reports|Status reports
M_Cross_Cutting_Registers_Logs_Reports|Weekly / monthly reports
M_Cross_Cutting_Registers_Logs_Reports|Dashboard
M_Cross_Cutting_Registers_Logs_Reports|RAID log
M_Cross_Cutting_Registers_Logs_Reports|Risk register
M_Cross_Cutting_Registers_Logs_Reports|Issue log
M_Cross_Cutting_Registers_Logs_Reports|Assumptions log
M_Cross_Cutting_Registers_Logs_Reports|Dependencies log
M_Cross_Cutting_Registers_Logs_Reports|Decision log
M_Cross_Cutting_Registers_Logs_Reports|Change requests
M_Cross_Cutting_Registers_Logs_Reports|Change log
M_Cross_Cutting_Registers_Logs_Reports|Approval records
M_Cross_Cutting_Registers_Logs_Reports|Document control register
M_Cross_Cutting_Registers_Logs_Reports|Version history
M_Cross_Cutting_Registers_Logs_Reports|Document review records
M_Cross_Cutting_Registers_Logs_Reports|Compliance certificates
M_Cross_Cutting_Registers_Logs_Reports|Audit reports
M_Cross_Cutting_Registers_Logs_Reports|Risk assessments
M_Cross_Cutting_Registers_Logs_Reports|DPIA
M_Cross_Cutting_Registers_Logs_Reports|Security assessment
M_Cross_Cutting_Registers_Logs_Reports|Privacy assessment
M_Cross_Cutting_Registers_Logs_Reports|Business impact analysis
M_Cross_Cutting_Registers_Logs_Reports|Vendor management documents
M_Cross_Cutting_Registers_Logs_Reports|Communication records
M_Cross_Cutting_Registers_Logs_Reports|Training records
M_Cross_Cutting_Registers_Logs_Reports|Sign-off records
M_Cross_Cutting_Registers_Logs_Reports|Escalation records
M_Cross_Cutting_Registers_Logs_Reports|Knowledge base articles
M_Cross_Cutting_Registers_Logs_Reports|Wiki pages
M_Cross_Cutting_Registers_Logs_Reports|Glossary
M_Cross_Cutting_Registers_Logs_Reports|Acronym list
M_Cross_Cutting_Registers_Logs_Reports|Style guide
M_Cross_Cutting_Registers_Logs_Reports|Template library
M_Cross_Cutting_Registers_Logs_Reports|Naming conventions
M_Cross_Cutting_Registers_Logs_Reports|Retention policy
M_Cross_Cutting_Registers_Logs_Reports|Access control policy
M_Cross_Cutting_Registers_Logs_Reports|Data classification policy
M_Cross_Cutting_Registers_Logs_Reports|Records management policy
M_Cross_Cutting_Registers_Logs_Reports|Information governance policy
M_Cross_Cutting_Registers_Logs_Reports|AI/ML model documentation
M_Cross_Cutting_Registers_Logs_Reports|Model card
M_Cross_Cutting_Registers_Logs_Reports|Datasheet
M_Cross_Cutting_Registers_Logs_Reports|Ethics assessment
M_Cross_Cutting_Registers_Logs_Reports|Bias audit
M_Cross_Cutting_Registers_Logs_Reports|Explainability report
M_Cross_Cutting_Registers_Logs_Reports|Model monitoring plan
M_Cross_Cutting_Registers_Logs_Reports|Retraining plan
M_Cross_Cutting_Registers_Logs_Reports|Model risk management document
M_Cross_Cutting_Registers_Logs_Reports|Regulatory submissions
M_Cross_Cutting_Registers_Logs_Reports|Industry-specific compliance documents
M_Cross_Cutting_Registers_Logs_Reports|Insurance certificates
M_Cross_Cutting_Registers_Logs_Reports|Bonding documents
M_Cross_Cutting_Registers_Logs_Reports|Tax documents
M_Cross_Cutting_Registers_Logs_Reports|Invoices
M_Cross_Cutting_Registers_Logs_Reports|Purchase orders
M_Cross_Cutting_Registers_Logs_Reports|Timesheets
M_Cross_Cutting_Registers_Logs_Reports|Expense reports
M_Cross_Cutting_Registers_Logs_Reports|Resource utilisation reports
N_Data_Analytics_AI_ML|Data governance plan
N_Data_Analytics_AI_ML|Data management plan
N_Data_Analytics_AI_ML|Data quality plan
N_Data_Analytics_AI_ML|Data quality report
N_Data_Analytics_AI_ML|Data dictionary
N_Data_Analytics_AI_ML|Business glossary
N_Data_Analytics_AI_ML|Metadata repository
N_Data_Analytics_AI_ML|Data lineage document
N_Data_Analytics_AI_ML|Data provenance record
N_Data_Analytics_AI_ML|Master data management plan
N_Data_Analytics_AI_ML|Reference data management plan
N_Data_Analytics_AI_ML|Data integration plan
N_Data_Analytics_AI_ML|Data migration plan
N_Data_Analytics_AI_ML|Data migration report
N_Data_Analytics_AI_ML|Data validation report
N_Data_Analytics_AI_ML|Data reconciliation report
N_Data_Analytics_AI_ML|Data archival plan
N_Data_Analytics_AI_ML|Data retention schedule
N_Data_Analytics_AI_ML|Data disposal certificate
N_Data_Analytics_AI_ML|Data privacy impact assessment
N_Data_Analytics_AI_ML|Records of processing activities
N_Data_Analytics_AI_ML|Data subject request procedure
N_Data_Analytics_AI_ML|Consent management record
N_Data_Analytics_AI_ML|Data sharing agreement
N_Data_Analytics_AI_ML|Data processing agreement
N_Data_Analytics_AI_ML|Cross-border transfer mechanism
N_Data_Analytics_AI_ML|Analytics requirements
N_Data_Analytics_AI_ML|Reporting requirements
N_Data_Analytics_AI_ML|Report specifications
N_Data_Analytics_AI_ML|Dashboard specifications
N_Data_Analytics_AI_ML|KPI definitions
N_Data_Analytics_AI_ML|Metric definitions
N_Data_Analytics_AI_ML|Data visualisation standards
N_Data_Analytics_AI_ML|Self-service analytics guide
N_Data_Analytics_AI_ML|Data science project charter
N_Data_Analytics_AI_ML|Experiment plan
N_Data_Analytics_AI_ML|Experiment results
N_Data_Analytics_AI_ML|Feature engineering documentation
N_Data_Analytics_AI_ML|Feature store documentation
N_Data_Analytics_AI_ML|Training data documentation
N_Data_Analytics_AI_ML|Validation data documentation
N_Data_Analytics_AI_ML|Test data documentation
N_Data_Analytics_AI_ML|Model card
N_Data_Analytics_AI_ML|Datasheet for datasets
N_Data_Analytics_AI_ML|Model evaluation report
N_Data_Analytics_AI_ML|Model bias report
N_Data_Analytics_AI_ML|Model explainability report
N_Data_Analytics_AI_ML|Model monitoring plan
N_Data_Analytics_AI_ML|Model retraining plan
N_Data_Analytics_AI_ML|Model deployment plan
N_Data_Analytics_AI_ML|Model serving documentation
N_Data_Analytics_AI_ML|MLOps pipeline documentation
N_Data_Analytics_AI_ML|Model risk assessment
N_Data_Analytics_AI_ML|AI ethics assessment
N_Data_Analytics_AI_ML|AI governance record
N_Data_Analytics_AI_ML|AI incident log
N_Data_Analytics_AI_ML|AI audit report
O_Compliance_Security_Privacy|Information security policy
O_Compliance_Security_Privacy|Acceptable use policy
O_Compliance_Security_Privacy|Access control policy
O_Compliance_Security_Privacy|Password policy
O_Compliance_Security_Privacy|Encryption policy
O_Compliance_Security_Privacy|Key management policy
O_Compliance_Security_Privacy|Network security policy
O_Compliance_Security_Privacy|Endpoint security policy
O_Compliance_Security_Privacy|Cloud security policy
O_Compliance_Security_Privacy|Mobile device policy
O_Compliance_Security_Privacy|Remote access policy
O_Compliance_Security_Privacy|Incident response plan
O_Compliance_Security_Privacy|Incident response report
O_Compliance_Security_Privacy|Disaster recovery plan
O_Compliance_Security_Privacy|Business continuity plan
O_Compliance_Security_Privacy|Security awareness training record
O_Compliance_Security_Privacy|Security risk assessment
O_Compliance_Security_Privacy|Threat model
O_Compliance_Security_Privacy|Vulnerability management plan
O_Compliance_Security_Privacy|Vulnerability report
O_Compliance_Security_Privacy|Patch management record
O_Compliance_Security_Privacy|Penetration test report
O_Compliance_Security_Privacy|Red team report
O_Compliance_Security_Privacy|Blue team report
O_Compliance_Security_Privacy|Purple team report
O_Compliance_Security_Privacy|Security architecture review
O_Compliance_Security_Privacy|Security control matrix
O_Compliance_Security_Privacy|SOC 2 report
O_Compliance_Security_Privacy|ISO 27001 certificate
O_Compliance_Security_Privacy|PCI DSS attestation
O_Compliance_Security_Privacy|HIPAA assessment
O_Compliance_Security_Privacy|GDPR compliance record
O_Compliance_Security_Privacy|CCPA / CPRA compliance record
O_Compliance_Security_Privacy|SOX compliance record
O_Compliance_Security_Privacy|FDA submission
O_Compliance_Security_Privacy|CE marking documentation
O_Compliance_Security_Privacy|MDR / IVDR documentation
O_Compliance_Security_Privacy|FedRAMP documentation
O_Compliance_Security_Privacy|FISMA documentation
O_Compliance_Security_Privacy|NIST assessment
O_Compliance_Security_Privacy|Cyber Essentials certificate
O_Compliance_Security_Privacy|Data protection impact assessment
O_Compliance_Security_Privacy|Privacy notice
O_Compliance_Security_Privacy|Privacy policy
O_Compliance_Security_Privacy|Cookie policy
O_Compliance_Security_Privacy|Consent record
O_Compliance_Security_Privacy|Data subject request log
O_Compliance_Security_Privacy|Breach notification record
O_Compliance_Security_Privacy|Regulatory correspondence
O_Compliance_Security_Privacy|Audit findings
O_Compliance_Security_Privacy|Corrective action plan
O_Compliance_Security_Privacy|Remediation report
O_Compliance_Security_Privacy|Compliance sign-off
O_Compliance_Security_Privacy|Security sign-off
O_Compliance_Security_Privacy|Privacy sign-off
O_Compliance_Security_Privacy|Legal sign-off
DOC_LIST

echo "Done. Generated documents in $BASE_DIR"