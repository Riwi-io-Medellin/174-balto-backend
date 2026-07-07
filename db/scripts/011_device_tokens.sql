-- ============================================================
-- Device tokens for push notifications (idempotent, additive only)
-- ============================================================

CREATE TABLE IF NOT EXISTS public.device_tokens (
    id            uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id       uuid        NOT NULL REFERENCES public.users ON DELETE CASCADE,
    token         text        NOT NULL,
    platform      varchar(20) NOT NULL, -- android | ios
    created_at    timestamp   DEFAULT now() NOT NULL,
    last_seen_at  timestamp   DEFAULT now() NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_device_tokens_token ON public.device_tokens (token);
CREATE INDEX IF NOT EXISTS ix_device_tokens_user_id ON public.device_tokens (user_id);
