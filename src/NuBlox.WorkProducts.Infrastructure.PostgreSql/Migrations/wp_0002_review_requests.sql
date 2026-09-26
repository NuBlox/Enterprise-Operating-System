ALTER TABLE work_products.revisions
    ADD COLUMN IF NOT EXISTS submitted_by_principal_id uuid NULL,
    ADD COLUMN IF NOT EXISTS submitted_at timestamptz NULL;

ALTER TABLE work_products.revisions
    DROP CONSTRAINT IF EXISTS ck_work_product_revision_submission;
ALTER TABLE work_products.revisions
    ADD CONSTRAINT ck_work_product_revision_submission CHECK (
        (state = 'DRAFT' AND submitted_by_principal_id IS NULL AND submitted_at IS NULL)
        OR
        (state <> 'DRAFT' AND submitted_by_principal_id IS NOT NULL AND submitted_at IS NOT NULL)
    );

CREATE TABLE IF NOT EXISTS work_products.review_requests (
    tenant_id uuid NOT NULL,
    review_request_id uuid NOT NULL,
    work_product_revision_id uuid NOT NULL,
    kind text NOT NULL,
    requested_principal_id uuid NOT NULL,
    requested_by_principal_id uuid NOT NULL,
    requested_at timestamptz NOT NULL,
    state text NOT NULL,
    CONSTRAINT pk_review_requests PRIMARY KEY (tenant_id, review_request_id),
    CONSTRAINT fk_review_request_revision FOREIGN KEY (tenant_id, work_product_revision_id)
        REFERENCES work_products.revisions (tenant_id, work_product_revision_id) ON DELETE RESTRICT,
    CONSTRAINT ck_review_request_kind CHECK (kind IN ('REVIEW', 'APPROVAL')),
    CONSTRAINT ck_review_request_state CHECK (state IN ('OPEN', 'COMPLETED', 'CANCELLED'))
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_open_review_request_per_kind_revision
    ON work_products.review_requests (tenant_id, work_product_revision_id, kind)
    WHERE state = 'OPEN';

CREATE INDEX IF NOT EXISTS ix_review_requests_requested_principal
    ON work_products.review_requests (tenant_id, requested_principal_id, state, requested_at);

ALTER TABLE work_products.review_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.review_requests FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON work_products.review_requests;
CREATE POLICY tenant_scope ON work_products.review_requests
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
