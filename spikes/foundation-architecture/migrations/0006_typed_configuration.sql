BEGIN;

CREATE SCHEMA IF NOT EXISTS configuration;

CREATE TABLE IF NOT EXISTS configuration.work_policy_versions (
    customer_id uuid NOT NULL,
    version_id uuid NOT NULL,
    effective_from timestamptz NOT NULL,
    effective_to timestamptz NULL,
    approval_required boolean NOT NULL,
    max_open_work integer NOT NULL CHECK (max_open_work BETWEEN 1 AND 10000),
    external_collaboration_enabled boolean NOT NULL,
    audit_required boolean NOT NULL DEFAULT true CHECK (audit_required),
    change_reason text NOT NULL CHECK (length(btrim(change_reason)) > 0),
    recorded_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, version_id),
    CONSTRAINT ck_work_policy_period
        CHECK (effective_to IS NULL OR effective_to > effective_from),
    CONSTRAINT ex_work_policy_no_overlap
        EXCLUDE USING gist (
            customer_id WITH =,
            tstzrange(
                effective_from,
                COALESCE(effective_to, 'infinity'::timestamptz),
                '[)'
            ) WITH &&
        )
);

CREATE INDEX IF NOT EXISTS ix_work_policy_as_of
    ON configuration.work_policy_versions
        (customer_id, effective_from DESC);

GRANT USAGE ON SCHEMA configuration TO nublox_app;
GRANT SELECT, INSERT, UPDATE, DELETE ON configuration.work_policy_versions TO nublox_app;

ALTER TABLE configuration.work_policy_versions ENABLE ROW LEVEL SECURITY;
ALTER TABLE configuration.work_policy_versions FORCE ROW LEVEL SECURITY;
DROP POLICY IF EXISTS customer_scope ON configuration.work_policy_versions;
CREATE POLICY customer_scope ON configuration.work_policy_versions
    FOR ALL TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

COMMIT;
