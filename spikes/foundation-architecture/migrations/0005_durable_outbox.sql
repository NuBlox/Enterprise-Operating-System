BEGIN;

CREATE SCHEMA IF NOT EXISTS integration;

CREATE TABLE IF NOT EXISTS integration.outbox (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    idempotency_key text NOT NULL CHECK (length(btrim(idempotency_key)) > 0),
    operation text NOT NULL CHECK (length(btrim(operation)) > 0),
    subject_type text NOT NULL CHECK (length(btrim(subject_type)) > 0),
    subject_id uuid NOT NULL,
    payload jsonb NOT NULL,
    state text NOT NULL CHECK (
        state IN (
            'PENDING',
            'PROCESSING',
            'RETRY_WAIT',
            'SUCCEEDED',
            'FAILED_ACTION_REQUIRED',
            'CANCELLED'
        )
    ),
    attempt_count integer NOT NULL DEFAULT 0 CHECK (attempt_count >= 0),
    available_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    locked_at timestamptz NULL,
    locked_by text NULL,
    last_error text NULL,
    provider_request_id text NULL,
    created_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    completed_at timestamptz NULL,
    PRIMARY KEY (customer_id, id),
    CONSTRAINT uq_outbox_idempotency UNIQUE (customer_id, idempotency_key),
    CONSTRAINT ck_outbox_lock_state CHECK (
        (state = 'PROCESSING' AND locked_at IS NOT NULL AND locked_by IS NOT NULL)
        OR
        (state <> 'PROCESSING')
    ),
    CONSTRAINT ck_outbox_completed_state CHECK (
        (state = 'SUCCEEDED' AND completed_at IS NOT NULL)
        OR
        (state <> 'SUCCEEDED')
    )
);

CREATE INDEX IF NOT EXISTS ix_outbox_ready
    ON integration.outbox (customer_id, state, available_at, created_at);

CREATE INDEX IF NOT EXISTS ix_outbox_processing
    ON integration.outbox (customer_id, locked_at)
    WHERE state = 'PROCESSING';

GRANT USAGE ON SCHEMA integration TO nublox_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON integration.outbox TO nublox_app;

ALTER TABLE integration.outbox ENABLE ROW LEVEL SECURITY;
ALTER TABLE integration.outbox FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS customer_scope ON integration.outbox;
CREATE POLICY customer_scope ON integration.outbox
    FOR ALL TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

COMMIT;
