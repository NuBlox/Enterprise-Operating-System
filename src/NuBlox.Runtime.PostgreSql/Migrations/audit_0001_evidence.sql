CREATE SCHEMA IF NOT EXISTS audit;

CREATE TABLE IF NOT EXISTS audit.events (
    tenant_id uuid NOT NULL,
    event_id uuid NOT NULL,
    actor_principal_id uuid NOT NULL,
    actor_kind text NOT NULL,
    action_code text NOT NULL,
    subject_type text NOT NULL,
    subject_id text NOT NULL,
    recorded_at timestamptz NOT NULL,
    effective_at timestamptz NULL,
    outcome text NOT NULL,
    source text NOT NULL,
    correlation_id text NULL,
    trace_id text NULL,
    reason_reference text NULL,
    corrects_event_id uuid NULL,
    CONSTRAINT pk_audit_events PRIMARY KEY (tenant_id, event_id),
    CONSTRAINT fk_audit_event_tenant FOREIGN KEY (tenant_id)
        REFERENCES kernel.tenants (tenant_id) ON DELETE RESTRICT,
    CONSTRAINT ck_audit_actor_kind CHECK (actor_kind IN ('HumanPrincipal', 'ServicePrincipal', 'Integration')),
    CONSTRAINT ck_audit_outcome CHECK (outcome IN ('Succeeded', 'Denied', 'Failed', 'Corrected'))
);

CREATE TABLE IF NOT EXISTS audit.evidence_references (
    tenant_id uuid NOT NULL,
    event_id uuid NOT NULL,
    ordinal integer NOT NULL,
    reference_type text NOT NULL,
    reference_id text NOT NULL,
    CONSTRAINT pk_audit_evidence_references PRIMARY KEY (tenant_id, event_id, ordinal),
    CONSTRAINT fk_audit_evidence_event FOREIGN KEY (tenant_id, event_id)
        REFERENCES audit.events (tenant_id, event_id) ON DELETE CASCADE,
    CONSTRAINT ck_audit_evidence_ordinal CHECK (ordinal >= 0)
);

ALTER TABLE audit.events ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.events FORCE ROW LEVEL SECURITY;
ALTER TABLE audit.evidence_references ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.evidence_references FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON audit.events;
CREATE POLICY tenant_scope ON audit.events
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

DROP POLICY IF EXISTS tenant_scope ON audit.evidence_references;
CREATE POLICY tenant_scope ON audit.evidence_references
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

CREATE INDEX IF NOT EXISTS ix_audit_events_subject
    ON audit.events (tenant_id, subject_type, subject_id, recorded_at DESC);

CREATE INDEX IF NOT EXISTS ix_audit_events_correlation
    ON audit.events (tenant_id, correlation_id)
    WHERE correlation_id IS NOT NULL;
