CREATE SCHEMA IF NOT EXISTS nublox_platform;

CREATE TABLE IF NOT EXISTS nublox_platform.audit_events
(
    audit_event_id uuid NOT NULL,
    tenant_id uuid NOT NULL,
    principal_id uuid NOT NULL,
    event_type text NOT NULL,
    subject_type text NOT NULL,
    subject_id text NOT NULL,
    recorded_at_utc timestamptz NOT NULL,
    correlation_id text NULL,
    CONSTRAINT pk_audit_events PRIMARY KEY (tenant_id, audit_event_id),
    CONSTRAINT ck_audit_events_event_type_not_blank CHECK (btrim(event_type) <> ''),
    CONSTRAINT ck_audit_events_subject_type_not_blank CHECK (btrim(subject_type) <> ''),
    CONSTRAINT ck_audit_events_subject_id_not_blank CHECK (btrim(subject_id) <> '')
);

CREATE INDEX IF NOT EXISTS ix_audit_events_tenant_recorded_at
    ON nublox_platform.audit_events (tenant_id, recorded_at_utc DESC, audit_event_id);

ALTER TABLE nublox_platform.audit_events ENABLE ROW LEVEL SECURITY;
ALTER TABLE nublox_platform.audit_events FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS audit_events_select_tenant ON nublox_platform.audit_events;
CREATE POLICY audit_events_select_tenant
    ON nublox_platform.audit_events
    FOR SELECT
    USING (
        tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid
    );

DROP POLICY IF EXISTS audit_events_insert_tenant ON nublox_platform.audit_events;
CREATE POLICY audit_events_insert_tenant
    ON nublox_platform.audit_events
    FOR INSERT
    WITH CHECK (
        tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid
    );
