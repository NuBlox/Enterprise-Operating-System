BEGIN;

CREATE SCHEMA IF NOT EXISTS subjects;
CREATE SCHEMA IF NOT EXISTS work;
CREATE SCHEMA IF NOT EXISTS decisions;
CREATE SCHEMA IF NOT EXISTS audit;

CREATE TABLE IF NOT EXISTS subjects.business_subjects (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    display_name text NOT NULL CHECK (length(btrim(display_name)) > 0),
    created_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, id)
);

CREATE TABLE IF NOT EXISTS work.work_requests (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    subject_id uuid NOT NULL,
    summary text NOT NULL CHECK (length(btrim(summary)) > 0),
    state text NOT NULL CHECK (state IN ('OPEN', 'COMPLETED')),
    created_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, id),
    CONSTRAINT fk_work_subject
        FOREIGN KEY (customer_id, subject_id)
        REFERENCES subjects.business_subjects (customer_id, id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS decisions.approval_decisions (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    work_id uuid NOT NULL,
    outcome text NOT NULL CHECK (outcome IN ('APPROVED', 'REJECTED')),
    decided_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    PRIMARY KEY (customer_id, id),
    CONSTRAINT fk_decision_work
        FOREIGN KEY (customer_id, work_id)
        REFERENCES work.work_requests (customer_id, id)
        ON DELETE RESTRICT
);

CREATE TABLE IF NOT EXISTS audit.events (
    customer_id uuid NOT NULL,
    id uuid NOT NULL,
    subject_type text NOT NULL CHECK (length(btrim(subject_type)) > 0),
    subject_id uuid NOT NULL,
    event_type text NOT NULL CHECK (length(btrim(event_type)) > 0),
    occurred_at timestamptz NOT NULL DEFAULT clock_timestamp(),
    payload jsonb NOT NULL DEFAULT '{}'::jsonb,
    PRIMARY KEY (customer_id, id)
);

CREATE INDEX IF NOT EXISTS ix_work_subject
    ON work.work_requests (customer_id, subject_id);

CREATE INDEX IF NOT EXISTS ix_decisions_work
    ON decisions.approval_decisions (customer_id, work_id);

CREATE INDEX IF NOT EXISTS ix_audit_subject
    ON audit.events (customer_id, subject_type, subject_id, occurred_at);

COMMIT;
