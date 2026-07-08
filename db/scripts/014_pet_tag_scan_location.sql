-- Persist the finder's shared location for an NFC tag scan on the pet itself,
-- mirroring the existing lost_latitude/lost_longitude/lost_at columns, so the
-- owner can view it on a map the same way lost-pet reports are shown.
ALTER TABLE pets ADD COLUMN IF NOT EXISTS tag_scan_latitude double precision;
ALTER TABLE pets ADD COLUMN IF NOT EXISTS tag_scan_longitude double precision;
ALTER TABLE pets ADD COLUMN IF NOT EXISTS tag_scan_at timestamp;
