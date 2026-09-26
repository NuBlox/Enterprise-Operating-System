CREATE TABLE IF NOT EXISTS work_products.delivery_intents (
    tenant_id uuid NOT NULL,
    delivery_intent_id uuid NOT NULL,
    work_product_id uuid NOT NULL,
    work_product_revision_id uuid NOT NULL,
    issue_evidence_id uuid NOT NULL,
    consequence_type text NOT NULL,
    idempotency_key text NOT NULL,
    created_at timestamptz NOT NULL,
    correlation_id text NULL,
    state text NOT NULL DEFAULT 'PENDING',
    attempt_count integer NOT NULL DEFAULT 0,
    next_attempt_at timestamptz NOT NULL,
    claimed_by text NULL,
    claim_expires_at timestamptz NULL,
    last_failure_code text NULL,
    completed_at timestamptz NULL,
    CONSTRAINT pk_delivery_intents PRIMARY KEY (tenant_id, delivery_intent_id),
    CONSTRAINT uq_delivery_intent_issue_consequence UNIQUE (tenant_id, issue_evidence_id, consequence_type),
    CONSTRAINT uq_delivery_intent_idempotency UNIQUE (tenant_id, idempotency_key),
    CONSTRAINT fk_delivery_intent_issue FOREIGN KEY (tenant_id, issue_evidence_id)
        REFERENCES work_products.issue_evidence (tenant_id, issue_evidence_id) ON DELETE RESTRICT,
    CONSTRAINT fk_delivery_intent_revision FOREIGN KEY (tenant_id, work_product_revision_id, work_product_id)
        REFERENCES work_products.revisions (tenant_id, work_product_revision_id, work_product_id) ON DELETE RESTRICT,
    CONSTRAINT ck_delivery_intent_type CHECK (length(btrim(consequence_type)) BETWEEN 1 AND 120),
    CONSTRAINT ck_delivery_intent_idempotency CHECK (length(btrim(idempotency_key)) BETWEEN 1 AND 240),
    CONSTRAINT ck_delivery_intent_state CHECK (state IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED')),
    CONSTRAINT ck_delivery_intent_attempts CHECK (attempt_count >= 0),
    CONSTRAINT ck_delivery_intent_claim CHECK (
        (state = 'PROCESSING' AND claimed_by IS NOT NULL AND claim_expires_at IS NOT NULL)
        OR
        (state <> 'PROCESSING' AND claimed_by IS NULL AND claim_expires_at IS NULL)
    ),
    CONSTRAINT ck_delivery_intent_completed CHECK (
        (state = 'COMPLETED' AND completed_at IS NOT NULL)
        OR
        (state <> 'COMPLETED' AND completed_at IS NULL)
    ),
    CONSTRAINT ck_delivery_intent_correlation CHECK (
        correlation_id IS NULL OR length(btrim(correlation_id)) BETWEEN 1 AND 128)
);

CREATE INDEX IF NOT EXISTS ix_delivery_intents_claim
    ON work_products.delivery_intents (tenant_id, state, next_attempt_at, created_at)
    WHERE state IN ('PENDING', 'PROCESSING');

CREATE INDEX IF NOT EXISTS ix_delivery_intents_subject
    ON work_products.delivery_intents (tenant_id, work_product_id, created_at DESC);

ALTER TABLE work_products.delivery_intents ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.delivery_intents FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON work_products.delivery_intents;
CREATE POLICY tenant_scope ON work_products.delivery_intents
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
