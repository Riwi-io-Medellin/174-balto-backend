
BEGIN;

-- =============================================================
-- phone / phone_extra: bigint → varchar(30)
-- =============================================================

ALTER TABLE users
    ALTER COLUMN phone TYPE varchar(30) USING phone::varchar,
    ALTER COLUMN phone_extra TYPE varchar(30) USING phone_extra::varchar;

-- =============================================================
-- walkers.updated_at: auto-update trigger
-- =============================================================

CREATE OR REPLACE FUNCTION set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_walkers_updated_at
    BEFORE UPDATE ON walkers
    FOR EACH ROW EXECUTE FUNCTION set_updated_at();

-- =============================================================
-- business_services.service_type: add CHECK constraint
-- =============================================================

ALTER TABLE business_services
    ADD CONSTRAINT business_services_service_type_check
    CHECK (service_type IN (
        'grooming', 'bathing', 'veterinary', 'consultation',
        'vaccination', 'surgery', 'boarding', 'daycare',
        'training', 'walking', 'other'
    ));

-- =============================================================
-- pet_walking_history.user_id: document denormalization intent
-- =============================================================

COMMENT ON COLUMN pet_walking_history.user_id
    IS 'Denormalized from pets.user_id for query performance. Must match the pet owner at walk creation time.';

COMMIT;
