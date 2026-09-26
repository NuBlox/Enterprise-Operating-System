BEGIN;

-- Both operational measures read the authoritative Work table. Reporting
-- inherits the table's existing customer RLS policy and the nublox_app role.
CREATE INDEX IF NOT EXISTS ix_work_created_through
    ON work.work_requests (customer_id, created_at, id);

CREATE INDEX IF NOT EXISTS ix_work_current_open
    ON work.work_requests (customer_id, id)
    WHERE state = 'OPEN';

COMMIT;
