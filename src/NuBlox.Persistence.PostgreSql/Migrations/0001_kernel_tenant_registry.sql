CREATE SCHEMA IF NOT EXISTS kernel;

CREATE TABLE IF NOT EXISTS kernel.tenants (
    tenant_id uuid PRIMARY KEY,
    tenant_slug text NOT NULL UNIQUE,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_tenants_slug_normalized CHECK (
        tenant_slug = lower(tenant_slug)
        AND tenant_slug ~ '^[a-z0-9]+(?:-[a-z0-9]+)*$'
    )
);

ALTER TABLE kernel.tenants ENABLE ROW LEVEL SECURITY;
ALTER TABLE kernel.tenants FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON kernel.tenants;
CREATE POLICY tenant_scope
    ON kernel.tenants
    USING (
        tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid
    )
    WITH CHECK (
        tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid
    );
