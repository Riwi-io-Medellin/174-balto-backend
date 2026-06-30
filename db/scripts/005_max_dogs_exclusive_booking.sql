
BEGIN;

-- =============================================================
-- walkers: add max_dogs capacity field
-- =============================================================

ALTER TABLE walkers
    ADD COLUMN IF NOT EXISTS max_dogs integer;

-- =============================================================
-- walk_bookings: add is_exclusive flag
-- =============================================================

ALTER TABLE walk_bookings
    ADD COLUMN IF NOT EXISTS is_exclusive boolean NOT NULL DEFAULT false;

COMMIT;
