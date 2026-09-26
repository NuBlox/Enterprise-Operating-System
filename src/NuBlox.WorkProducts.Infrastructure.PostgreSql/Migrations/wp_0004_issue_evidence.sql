ALTER TABLE work_products.revisions
    ADD COLUMN IF NOT EXISTS issued_by_principal_id uuid NULL,
    ADD COLUMN IF NOT EXISTS issued_at timestamptz NULL;

ALTER TABLE work_products.revisions
    DROP CONSTRAINT IF EXISTS ck_work_product_revision_issue_evidence;
ALTER TABLE work_products.revisions
    ADD CONSTRAINT ck_work_product_revision_issue_evidence CHECK (
        (
            state IN ('ISSUED', 'SUPERSEDED')
            AND issued_by_principal_id IS NOT NULL
            AND issued_at IS NOT NULL
        )
        OR
        (
            state NOT IN ('ISSUED', 'SUPERSEDED')
            AND issued_by_principal_id IS NULL
            AND issued_at IS NULL
        )
    );

ALTER TABLE work_products.revisions
    DROP CONSTRAINT IF EXISTS uq_work_product_revision_issue_subject;
ALTER TABLE work_products.revisions
    ADD CONSTRAINT uq_work_product_revision_issue_subject
        UNIQUE (tenant_id, work_product_revision_id, work_product_id);

ALTER TABLE work_products.review_decisions
    DROP CONSTRAINT IF EXISTS uq_review_decision_subject_outcome;
ALTER TABLE work_products.review_decisions
    ADD CONSTRAINT uq_review_decision_subject_outcome
        UNIQUE (tenant_id, decision_evidence_id, work_product_revision_id, outcome);

CREATE TABLE IF NOT EXISTS work_products.issue_evidence (
    tenant_id uuid NOT NULL,
    issue_evidence_id uuid NOT NULL,
    work_product_id uuid NOT NULL,
    work_product_revision_id uuid NOT NULL,
    approval_decision_evidence_id uuid NOT NULL,
    approval_outcome text NOT NULL DEFAULT 'APPROVED',
    issued_by_principal_id uuid NOT NULL,
    issued_at timestamptz NOT NULL,
    correlation_id text NULL,
    CONSTRAINT pk_issue_evidence PRIMARY KEY (tenant_id, issue_evidence_id),
    CONSTRAINT uq_issue_evidence_revision UNIQUE (tenant_id, work_product_revision_id),
    CONSTRAINT fk_issue_evidence_revision FOREIGN KEY (
        tenant_id, work_product_revision_id, work_product_id)
        REFERENCES work_products.revisions (
            tenant_id, work_product_revision_id, work_product_id) ON DELETE RESTRICT,
    CONSTRAINT fk_issue_evidence_approval FOREIGN KEY (
        tenant_id, approval_decision_evidence_id, work_product_revision_id, approval_outcome)
        REFERENCES work_products.review_decisions (
            tenant_id, decision_evidence_id, work_product_revision_id, outcome) ON DELETE RESTRICT,
    CONSTRAINT ck_issue_evidence_approval CHECK (approval_outcome = 'APPROVED'),
    CONSTRAINT ck_issue_evidence_correlation CHECK (
        correlation_id IS NULL OR length(btrim(correlation_id)) BETWEEN 1 AND 128)
);

CREATE INDEX IF NOT EXISTS ix_issue_evidence_product
    ON work_products.issue_evidence (tenant_id, work_product_id, issued_at DESC);

ALTER TABLE work_products.issue_evidence ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.issue_evidence FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON work_products.issue_evidence;
CREATE POLICY tenant_scope ON work_products.issue_evidence
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
