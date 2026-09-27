CREATE TABLE IF NOT EXISTS enterprise.people (
    tenant_id uuid NOT NULL,
    party_id uuid NOT NULL,
    display_name text NOT NULL,
    CONSTRAINT pk_enterprise_people PRIMARY KEY (tenant_id, party_id),
    CONSTRAINT fk_enterprise_person_party
        FOREIGN KEY (tenant_id, party_id)
        REFERENCES enterprise.parties (tenant_id, party_id)
        ON DELETE RESTRICT,
    CONSTRAINT ck_enterprise_person_display_name
        CHECK (char_length(btrim(display_name)) BETWEEN 1 AND 240)
);

ALTER TABLE enterprise.people ENABLE ROW LEVEL SECURITY;
ALTER TABLE enterprise.people FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON enterprise.people;
CREATE POLICY tenant_scope ON enterprise.people
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);
