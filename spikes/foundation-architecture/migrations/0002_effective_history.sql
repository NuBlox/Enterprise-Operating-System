BEGIN;

CREATE EXTENSION IF NOT EXISTS btree_gist;

CREATE TABLE IF NOT EXISTS subjects.business_subject_names (
    customer_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    effective_from timestamptz NOT NULL,
    effective_to timestamptz NULL,
    display_name text NOT NULL CHECK (length(btrim(display_name)) > 0),
    change_reason text NOT NULL CHECK (length(btrim(change_reason)) > 0),
    recorded_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, subject_id, effective_from),
    CONSTRAINT fk_subject_name_subject
        FOREIGN KEY (customer_id, subject_id)
        REFERENCES subjects.business_subjects (customer_id, id)
        ON DELETE RESTRICT,
    CONSTRAINT ck_subject_name_effective_period
        CHECK (effective_to IS NULL OR effective_to > effective_from),
    CONSTRAINT ex_subject_name_no_overlap
        EXCLUDE USING gist (
            customer_id WITH =,
            subject_id WITH =,
            tstzrange(
                effective_from,
                COALESCE(effective_to, 'infinity'::timestamptz),
                '[)'
            ) WITH &&
        )
);

-- Migration 0001 carried the initial display name on the base subject row.
-- Move that business fact into the effective-dated history table once, then
-- remove the duplicate authoritative representation from the base identity.
DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'subjects'
          AND table_name = 'business_subjects'
          AND column_name = 'display_name'
    ) THEN
        INSERT INTO subjects.business_subject_names (
            customer_id,
            subject_id,
            effective_from,
            effective_to,
            display_name,
            change_reason,
            recorded_at
        )
        SELECT
            customer_id,
            id,
            created_at,
            NULL,
            display_name,
            'migration-0002-initial-name',
            created_at
        FROM subjects.business_subjects
        ON CONFLICT DO NOTHING;

        ALTER TABLE subjects.business_subjects
            DROP COLUMN display_name;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_subject_name_as_of
    ON subjects.business_subject_names
        (customer_id, subject_id, effective_from DESC);

CREATE INDEX IF NOT EXISTS ix_subject_name_recorded
    ON subjects.business_subject_names
        (customer_id, subject_id, recorded_at DESC);

COMMIT;
