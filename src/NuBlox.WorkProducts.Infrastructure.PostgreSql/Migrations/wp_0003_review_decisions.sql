CREATE TABLE IF NOT EXISTS work_products.review_decisions (
    tenant_id uuid NOT NULL,
    review_decision_id uuid NOT NULL,
    review_request_id uuid NOT NULL,
    work_product_revision_id uuid NOT NULL,
    outcome text NOT NULL,
    decided_by_principal_id uuid NOT NULL,
    decided_at timestamptz NOT NULL,
    rationale text NULL,
    CONSTRAINT pk_review_decisions PRIMARY KEY (tenant_id, review_decision_id),
    CONSTRAINT uq_review_decision_request UNIQUE (tenant_id, review_request_id),
    CONSTRAINT fk_review_decision_request FOREIGN KEY (tenant_id, review_request_id)
        REFERENCES work_products.review_requests (tenant_id, review_request_id) ON DELETE RESTRICT,
    CONSTRAINT fk_review_decision_revision FOREIGN KEY (tenant_id, work_product_revision_id)
        REFERENCES work_products.revisions (tenant_id, work_product_revision_id) ON DELETE RESTRICT,
    CONSTRAINT ck_review_decision_outcome CHECK (outcome IN ('CHANGES_REQUIRED', 'REJECTED', 'APPROVED')),
    CONSTRAINT ck_review_decision_rationale CHECK (
        (outcome = 'APPROVED' AND (rationale IS NULL OR length(rationale) BETWEEN 1 AND 1000))
        OR
        (outcome <> 'APPROVED' AND rationale IS NOT NULL AND length(rationale) BETWEEN 1 AND 1000)
    )
);

CREATE INDEX IF NOT EXISTS ix_review_decisions_revision
    ON work_products.review_decisions (tenant_id, work_product_revision_id, decided_at);

ALTER TABLE work_products.review_decisions ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.review_decisions FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON work_products.review_decisions;
CREATE POLICY tenant_scope ON work_products.review_decisions
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
