CREATE SCHEMA IF NOT EXISTS work_products;

CREATE TABLE IF NOT EXISTS work_products.work_products (
    tenant_id uuid NOT NULL,
    work_product_id uuid NOT NULL,
    title text NOT NULL,
    product_type text NOT NULL,
    owner_principal_id uuid NOT NULL,
    created_by_principal_id uuid NOT NULL,
    created_at timestamptz NOT NULL,
    lifecycle text NOT NULL,
    current_revision_number integer NOT NULL,
    CONSTRAINT pk_work_products PRIMARY KEY (tenant_id, work_product_id),
    CONSTRAINT fk_work_products_tenant FOREIGN KEY (tenant_id)
        REFERENCES kernel.tenants (tenant_id) ON DELETE RESTRICT,
    CONSTRAINT ck_work_products_title CHECK (length(btrim(title)) BETWEEN 1 AND 240),
    CONSTRAINT ck_work_products_type CHECK (length(btrim(product_type)) BETWEEN 1 AND 80),
    CONSTRAINT ck_work_products_lifecycle CHECK (lifecycle IN ('ACTIVE', 'SUPERSEDED', 'WITHDRAWN')),
    CONSTRAINT ck_work_products_current_revision CHECK (current_revision_number > 0)
);

CREATE TABLE IF NOT EXISTS work_products.revisions (
    tenant_id uuid NOT NULL,
    work_product_revision_id uuid NOT NULL,
    work_product_id uuid NOT NULL,
    revision_number integer NOT NULL,
    title_snapshot text NOT NULL,
    state text NOT NULL,
    created_by_principal_id uuid NOT NULL,
    created_at timestamptz NOT NULL,
    CONSTRAINT pk_work_product_revisions PRIMARY KEY (tenant_id, work_product_revision_id),
    CONSTRAINT uq_work_product_revision_number UNIQUE (tenant_id, work_product_id, revision_number),
    CONSTRAINT fk_work_product_revision_product FOREIGN KEY (tenant_id, work_product_id)
        REFERENCES work_products.work_products (tenant_id, work_product_id) ON DELETE RESTRICT,
    CONSTRAINT ck_work_product_revision_number CHECK (revision_number > 0),
    CONSTRAINT ck_work_product_revision_title CHECK (length(btrim(title_snapshot)) BETWEEN 1 AND 240),
    CONSTRAINT ck_work_product_revision_state CHECK (
        state IN ('DRAFT', 'IN_REVIEW', 'CHANGES_REQUIRED', 'REJECTED', 'APPROVED', 'ISSUED', 'SUPERSEDED')
    )
);

ALTER TABLE work_products.work_products ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.work_products FORCE ROW LEVEL SECURITY;
ALTER TABLE work_products.revisions ENABLE ROW LEVEL SECURITY;
ALTER TABLE work_products.revisions FORCE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS tenant_scope ON work_products.work_products;
CREATE POLICY tenant_scope ON work_products.work_products
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

DROP POLICY IF EXISTS tenant_scope ON work_products.revisions;
CREATE POLICY tenant_scope ON work_products.revisions
    USING (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid)
    WITH CHECK (tenant_id = NULLIF(current_setting('nublox.tenant_id', true), '')::uuid);

CREATE INDEX IF NOT EXISTS ix_work_products_owner
    ON work_products.work_products (tenant_id, owner_principal_id, lifecycle);

CREATE INDEX IF NOT EXISTS ix_work_product_revisions_product
    ON work_products.revisions (tenant_id, work_product_id, revision_number DESC);
