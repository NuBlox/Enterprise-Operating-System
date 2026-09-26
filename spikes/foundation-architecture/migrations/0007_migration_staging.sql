BEGIN;

CREATE SCHEMA IF NOT EXISTS migration;

CREATE TABLE IF NOT EXISTS migration.batches (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    source_name text NOT NULL CHECK (length(btrim(source_name)) > 0),
    state text NOT NULL CHECK (state IN ('STAGING', 'PROCESSED', 'ACCEPTED')),
    created_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, id)
);

CREATE TABLE IF NOT EXISTS migration.staged_subjects (
    customer_id uuid NOT NULL,
    batch_id uuid NOT NULL,
    source_id text NOT NULL CHECK (length(btrim(source_id)) > 0),
    display_name text NULL,
    effective_from timestamptz NOT NULL,
    source_payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    staged_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, batch_id, source_id),
    CONSTRAINT fk_staged_batch
        FOREIGN KEY (customer_id, batch_id)
        REFERENCES migration.batches (customer_id, id)
        ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS migration.loaded_subjects (
    customer_id uuid NOT NULL,
    batch_id uuid NOT NULL,
    source_id text NOT NULL,
    subject_id uuid NOT NULL,
    loaded_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, batch_id, source_id),
    CONSTRAINT uq_loaded_subject UNIQUE (customer_id, subject_id),
    CONSTRAINT fk_loaded_stage
        FOREIGN KEY (customer_id, batch_id, source_id)
        REFERENCES migration.staged_subjects (customer_id, batch_id, source_id)
        ON DELETE RESTRICT,
    CONSTRAINT fk_loaded_subject
        FOREIGN KEY (customer_id, subject_id)
        REFERENCES subjects.business_subjects (customer_id, id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS migration.exceptions (
    customer_id uuid NOT NULL,
    batch_id uuid NOT NULL,
    source_id text NOT NULL,
    code text NOT NULL CHECK (length(btrim(code)) > 0),
    message text NOT NULL CHECK (length(btrim(message)) > 0),
    disposition text NOT NULL CHECK (disposition IN ('UNRESOLVED', 'ACCEPTED_EXCEPTION', 'SOURCE_CORRECTED', 'EXCLUDED')),
    recorded_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, batch_id, source_id, code),
    CONSTRAINT fk_exception_stage
        FOREIGN KEY (customer_id, batch_id, source_id)
        REFERENCES migration.staged_subjects (customer_id, batch_id, source_id)
        ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS migration.reconciliations (
    customer_id uuid NOT NULL,
    batch_id uuid NOT NULL,
    staged_count integer NOT NULL CHECK (staged_count >= 0),
    loaded_count integer NOT NULL CHECK (loaded_count >= 0),
    exception_count integer NOT NULL CHECK (exception_count >= 0),
    unresolved_count integer NOT NULL CHECK (unresolved_count >= 0),
    reconciled_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, batch_id),
    CONSTRAINT fk_reconciliation_batch
        FOREIGN KEY (customer_id, batch_id)
        REFERENCES migration.batches (customer_id, id)
        ON DELETE CASCADE,
    CONSTRAINT ck_reconciliation_accounted
        CHECK (loaded_count + exception_count = staged_count)
);

CREATE INDEX IF NOT EXISTS ix_migration_stage_batch
    ON migration.staged_subjects (customer_id, batch_id, staged_at);

GRANT USAGE ON SCHEMA migration TO nublox_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA migration TO nublox_app;

ALTER TABLE migration.batches ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.batches FORCE ROW LEVEL SECURITY;
ALTER TABLE migration.staged_subjects ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.staged_subjects FORCE ROW LEVEL SECURITY;
ALTER TABLE migration.loaded_subjects ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.loaded_subjects FORCE ROW LEVEL SECURITY;
ALTER TABLE migration.exceptions ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.exceptions FORCE ROW LEVEL SECURITY;
ALTER TABLE migration.reconciliations ENABLE ROW LEVEL SECURITY;
ALTER TABLE migration.reconciliations FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS customer_scope ON migration.batches;
CREATE POLICY customer_scope ON migration.batches FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_scope ON migration.staged_subjects;
CREATE POLICY customer_scope ON migration.staged_subjects FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_scope ON migration.loaded_subjects;
CREATE POLICY customer_scope ON migration.loaded_subjects FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_scope ON migration.exceptions;
CREATE POLICY customer_scope ON migration.exceptions FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_scope ON migration.reconciliations;
CREATE POLICY customer_scope ON migration.reconciliations FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

COMMIT;
