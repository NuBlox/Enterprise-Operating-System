BEGIN;

CREATE SCHEMA IF NOT EXISTS access;
CREATE SCHEMA IF NOT EXISTS authority;

CREATE TABLE IF NOT EXISTS access.operation_permissions (
    customer_id uuid NOT NULL,
    principal_id uuid NOT NULL,
    operation text NOT NULL CHECK (length(btrim(operation)) > 0),
    granted_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    revoked_at timestamptz NULL,
    PRIMARY KEY (customer_id, principal_id, operation, granted_at),
    CONSTRAINT ck_permission_revocation
        CHECK (revoked_at IS NULL OR revoked_at > granted_at)
);

CREATE INDEX IF NOT EXISTS ix_operation_permissions_active
    ON access.operation_permissions (customer_id, principal_id, operation)
    WHERE revoked_at IS NULL;

CREATE TABLE IF NOT EXISTS authority.decision_authorities (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    principal_id uuid NOT NULL,
    decision_type text NOT NULL CHECK (length(btrim(decision_type)) > 0),
    maximum_amount numeric(18,2) NULL CHECK (maximum_amount IS NULL OR maximum_amount >= 0),
    effective_from timestamptz NOT NULL,
    effective_to timestamptz NULL,
    delegated_by uuid NULL,
    grant_reason text NOT NULL CHECK (length(btrim(grant_reason)) > 0),
    recorded_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, id),
    CONSTRAINT ck_authority_effective_period
        CHECK (effective_to IS NULL OR effective_to > effective_from)
);

CREATE INDEX IF NOT EXISTS ix_decision_authority_lookup
    ON authority.decision_authorities
        (customer_id, principal_id, decision_type, effective_from, effective_to);

CREATE TABLE IF NOT EXISTS authority.evaluations (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    principal_id uuid NOT NULL,
    operation text NOT NULL,
    decision_type text NOT NULL,
    requested_amount numeric(18,2) NULL,
    evaluated_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    allowed boolean NOT NULL,
    reason text NOT NULL CHECK (length(btrim(reason)) > 0),
    PRIMARY KEY (customer_id, id)
);

CREATE INDEX IF NOT EXISTS ix_authority_evaluations_principal
    ON authority.evaluations
        (customer_id, principal_id, evaluated_at DESC);

GRANT USAGE ON SCHEMA access, authority TO nublox_app;
GRANT SELECT, INSERT, UPDATE, DELETE
    ON access.operation_permissions,
       authority.decision_authorities,
       authority.evaluations
    TO nublox_app;

ALTER TABLE access.operation_permissions ENABLE ROW LEVEL SECURITY;
ALTER TABLE authority.decision_authorities ENABLE ROW LEVEL SECURITY;
ALTER TABLE authority.evaluations ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS customer_isolation ON access.operation_permissions;
CREATE POLICY customer_isolation ON access.operation_permissions
    FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_isolation ON authority.decision_authorities;
CREATE POLICY customer_isolation ON authority.decision_authorities
    FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

DROP POLICY IF EXISTS customer_isolation ON authority.evaluations;
CREATE POLICY customer_isolation ON authority.evaluations
    FOR ALL TO nublox_app
    USING (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid)
    WITH CHECK (customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid);

COMMIT;
