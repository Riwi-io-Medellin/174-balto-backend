
BEGIN;

-- =============================================================
-- walk_sessions: add booking-based lifecycle columns
-- =============================================================

ALTER TABLE walk_sessions
    ADD COLUMN IF NOT EXISTS booking_id             uuid,
    ADD COLUMN IF NOT EXISTS total_distance_meters  double precision,
    ADD COLUMN IF NOT EXISTS total_duration_seconds integer;

-- =============================================================
-- walk_bookings: add walk_session_id reference
-- =============================================================

ALTER TABLE walk_bookings
    ADD COLUMN IF NOT EXISTS walk_session_id uuid;

COMMIT;
