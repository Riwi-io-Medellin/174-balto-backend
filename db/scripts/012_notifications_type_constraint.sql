-- notifications.type has a CHECK constraint (added outside version control) that
-- only allowed the original set of notification types. Widen it to include the
-- types added since (walk_media_uploaded, chat_message, pet_tag_scanned) plus
-- every type actually emitted by the app today, so future type additions are a
-- single, reviewable migration rather than a silent runtime failure.
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
        'system'
    )
);
