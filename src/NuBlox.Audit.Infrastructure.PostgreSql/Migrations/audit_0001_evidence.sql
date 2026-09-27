CREATE SCHEMA IF NOT EXISTS audit;

CREATE TABLE IF NOT EXISTS audit.evidence (
    tenant_id uuid NOT NULL,
    audit_event_id uuid NOT NULL,
    actor_principal_id uuid NULL,
    actor_kind text NOT NULL,
    action_code text NOT NULL,
    subject_type text NOT NULL,
    subject_id text NOT NULL,
    occurred_at timestamptz NOT NULL,
    outcome text NOT NULL,
    source text NOT NULL,
    correlation_id text NULL,
    trace_id text NULL,
    reason_reference text NULL,
    evidence_references jsonb NOT NULL DEFAULT '[]'::jsonb,
    CONSTRAINT pk_audit_evidence PRIMARY KEY (tenant_id, audit_event_id),
    CONSTRAINT fk_audit_evidence_tenant FOREIGN KEY (tenant_id)
        REFERENCES kernel.tenants (tenant_id) ON DELETE RESTRICT,
    CONSTRAINT ck_audit_actor_kind CHECK (actor_kind IN ('HUMAN_PRINCIPAL', 'SERVICE_PRINCIPAL', 'SYSTEM')),
    CONSTRAINT ck_audit_outcome CHECK (outcome IN ('SUCCEEDED', 'DENIED', 'FAILED')),
    CONSTRAINT ck_audit_action_code CHECK (length(btrim(action_code)) BETWEEN 1 AND 160),
    CONSTRAINT ck_audit_subject_type CHECK (length(btrim(subject_type)) BETWEEN 1 AND 120),
    CONSTRAINT ck_audit_subject_id CHECK (length(btrim(subject_id)) BETWEEN 1 AND 512),
    CONSTRAINT ck_audit_source CHECK (length(btrim(source)) BETWEEN 1 AND 160)
);

ALTER TABLE audit.evidence ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.evidence FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON audit.evidence;
CREATE POLICY tenant_scope ON audit.evidence
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

CREATE INDEX IF NOT EXISTS ix_audit_evidence_subject
    ON audit.evidence (tenant_id, subject_type, subject_id, occurred_at DESC);

CREATE INDEX IF NOT EXISTS ix_audit_evidence_correlation
    ON audit.evidence (tenant_id, correlation_id, occurred_at DESC)
    WHERE correlation_id IS NOT NULL;
