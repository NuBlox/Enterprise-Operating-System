CREATE SCHEMA IF NOT EXISTS enterprise;

CREATE TABLE IF NOT EXISTS enterprise.parties (
    tenant_id uuid NOT NULL,
    party_id uuid NOT NULL,
    party_kind text NOT NULL,
    created_by_principal_id uuid NOT NULL,
    created_at timestamptz NOT NULL,
    CONSTRAINT pk_enterprise_parties PRIMARY KEY (tenant_id, party_id),
    CONSTRAINT ck_enterprise_party_kind CHECK (party_kind IN ('PERSON', 'ORGANISATION'))
);

CREATE TABLE IF NOT EXISTS enterprise.organisations (
    tenant_id uuid NOT NULL,
    party_id uuid NOT NULL,
    display_name text NOT NULL,
    CONSTRAINT pk_enterprise_organisations PRIMARY KEY (tenant_id, party_id),
    CONSTRAINT fk_enterprise_organisation_party
        FOREIGN KEY (tenant_id, party_id)
        REFERENCES enterprise.parties (tenant_id, party_id)
        ON DELETE RESTRICT,
    CONSTRAINT ck_enterprise_organisation_display_name
        CHECK (char_length(btrim(display_name)) BETWEEN 1 AND 240)
);

ALTER TABLE enterprise.parties ENABLE ROW LEVEL SECURITY;
ALTER TABLE enterprise.parties FORCE ROW LEVEL SECURITY;
ALTER TABLE enterprise.organisations ENABLE ROW LEVEL SECURITY;
ALTER TABLE enterprise.organisations FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON enterprise.parties;
CREATE POLICY tenant_scope ON enterprise.parties
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

DROP POLICY IF EXISTS tenant_scope ON enterprise.organisations;
CREATE POLICY tenant_scope ON enterprise.organisations
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
