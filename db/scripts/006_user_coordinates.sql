
BEGIN;

-- =============================================================
-- users: add GPS coordinates for owner location
-- =============================================================

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS latitude  double precision,
    ADD COLUMN IF NOT EXISTS longitude double precision;

COMMIT;
