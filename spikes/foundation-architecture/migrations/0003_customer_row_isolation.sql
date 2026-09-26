BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_roles
        WHERE rolname = 'nublox_app'
    ) THEN
        CREATE ROLE nublox_app
            NOLOGIN
            NOSUPERUSER
            NOCREATEDB
            NOCREATEROLE
            NOINHERIT;
    END IF;
END $$;

GRANT USAGE ON SCHEMA subjects, work, decisions, audit TO nublox_app;

GRANT SELECT, INSERT, UPDATE, DELETE
    ON subjects.business_subjects,
       subjects.business_subject_names,
       work.work_requests,
       decisions.approval_decisions,
       audit.events
    TO nublox_app;

ALTER TABLE subjects.business_subjects ENABLE ROW LEVEL SECURITY;
ALTER TABLE subjects.business_subject_names ENABLE ROW LEVEL SECURITY;
ALTER TABLE work.work_requests ENABLE ROW LEVEL SECURITY;
ALTER TABLE decisions.approval_decisions ENABLE ROW LEVEL SECURITY;
ALTER TABLE audit.events ENABLE ROW LEVEL SECURITY;

DROP POLICY IF EXISTS customer_isolation ON subjects.business_subjects;
CREATE POLICY customer_isolation ON subjects.business_subjects
    FOR ALL
    TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

DROP POLICY IF EXISTS customer_isolation ON subjects.business_subject_names;
CREATE POLICY customer_isolation ON subjects.business_subject_names
    FOR ALL
    TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

DROP POLICY IF EXISTS customer_isolation ON work.work_requests;
CREATE POLICY customer_isolation ON work.work_requests
    FOR ALL
    TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

DROP POLICY IF EXISTS customer_isolation ON decisions.approval_decisions;
CREATE POLICY customer_isolation ON decisions.approval_decisions
    FOR ALL
    TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

DROP POLICY IF EXISTS customer_isolation ON audit.events;
CREATE POLICY customer_isolation ON audit.events
    FOR ALL
    TO nublox_app
    USING (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    )
    WITH CHECK (
        customer_id = NULLIF(current_setting('app.customer_id', true), '')::uuid
    );

COMMIT;
