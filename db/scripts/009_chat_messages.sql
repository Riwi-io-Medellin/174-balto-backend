-- Chat messages: real-time text chat between owner and walker during a walk.
CREATE TABLE IF NOT EXISTS chat_messages (
    id                UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    walk_session_id   UUID        NOT NULL,
    sender_user_id    UUID        NOT NULL,
    text              TEXT        NOT NULL,
    created_at        TIMESTAMP   NOT NULL DEFAULT NOW()
);
