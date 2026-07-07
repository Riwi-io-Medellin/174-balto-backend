-- Widen notifications.type again to allow 'pet_location_shared', added for the
-- NFC pet-tag "share my location with the owner" feature.
DO $$
DECLARE
    existing_constraint text;
BEGIN
    SELECT con.conname INTO existing_constraint
    FROM pg_constraint con
    JOIN pg_class rel ON rel.oid = con.conrelid
    WHERE rel.relname = 'notifications'
      AND con.contype = 'c'
      AND pg_get_constraintdef(con.oid) ILIKE '%type%';

    IF existing_constraint IS NOT NULL THEN
        EXECUTE format('ALTER TABLE notifications DROP CONSTRAINT %I', existing_constraint);
    END IF;
END $$;

ALTER TABLE notifications ADD CONSTRAINT notifications_type_check CHECK (
    type IN (
        'walk_started',
        'walk_finished',
        'walk_cancelled',
        'walk_media_uploaded',
        'chat_message',
        'walker_assigned',
        'walker_approved',
        'walker_rejected',
        'business_approved',
        'business_rejected',
        'lost_pet',
        'pet_tag_scanned',
        'pet_location_shared',
        'system'
    )
);
