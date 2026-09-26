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

CREATE INDEX IF NOT EXISTS ix_subject_name_as_of
    ON subjects.business_subject_names (customer_id, subject_id, effective_from DESC);

COMMIT;
