-- ============================================================
-- Home Services module — schema (idempotent, additive only)
-- Parallel to the Walker module. No existing table is renamed,
-- dropped, or has columns removed. See plan for full rationale.
-- ============================================================

CREATE TABLE IF NOT EXISTS public.home_service_types (
    id          uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    code        varchar(50) NOT NULL UNIQUE,
    name        varchar(100) NOT NULL,
    description text,
    is_active   boolean     DEFAULT true NOT NULL,
    created_at  timestamp   DEFAULT now() NOT NULL
);

INSERT INTO public.home_service_types (code, name) VALUES
    ('veterinary', 'Veterinarian'),
    ('grooming', 'Grooming'),
    ('pet_sitting', 'Pet Sitting'),
    ('training', 'Pet Training'),
    ('transportation', 'Transportation'),
    ('medication_administration', 'Medication Administration'),
    ('nail_trimming', 'Nail Trimming'),
    ('other', 'Other')
ON CONFLICT (code) DO NOTHING;

CREATE TABLE IF NOT EXISTS public.home_service_providers (
    id                    uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    user_id               uuid        NOT NULL UNIQUE REFERENCES public.users ON DELETE CASCADE,
    bio                   text,
    description           text,
    experience            text,
    years_of_experience   integer
        CONSTRAINT home_service_providers_years_of_experience_check CHECK (years_of_experience >= 0),
    is_accepting_bookings boolean     DEFAULT true NOT NULL,
    max_concurrent_bookings integer   DEFAULT 1 NOT NULL
        CONSTRAINT home_service_providers_max_concurrent_check CHECK (max_concurrent_bookings >= 1),
    base_location         varchar,
    latitude              double precision,
    longitude             double precision,
    verification_status   varchar(30) DEFAULT 'pending' NOT NULL
        CONSTRAINT home_service_providers_verification_status_check
            CHECK (verification_status IN ('pending','approved','rejected','suspended')),
    rejection_reason      text,
    document_name         text,
    document_number       varchar(50),
    approved_at           timestamp,
    approved_by           uuid REFERENCES public.users ON DELETE SET NULL,
    created_at            timestamp   DEFAULT now() NOT NULL,
    updated_at            timestamp   DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_service_providers_user_id ON public.home_service_providers (user_id);
CREATE INDEX IF NOT EXISTS idx_home_service_providers_status ON public.home_service_providers (verification_status);

CREATE TABLE IF NOT EXISTS public.home_provider_services (
    id              uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id     uuid        NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    service_type_id uuid        NOT NULL REFERENCES public.home_service_types ON DELETE RESTRICT,
    price           numeric(10,2)
        CONSTRAINT home_provider_services_price_check CHECK (price >= 0),
    price_unit      varchar(20) DEFAULT 'flat' NOT NULL
        CONSTRAINT home_provider_services_price_unit_check CHECK (price_unit IN ('flat','hourly','per_visit')),
    description     text,
    is_active       boolean     DEFAULT true NOT NULL,
    created_at      timestamp   DEFAULT now() NOT NULL,
    CONSTRAINT uq_provider_service UNIQUE (provider_id, service_type_id)
);

CREATE INDEX IF NOT EXISTS idx_home_provider_services_provider ON public.home_provider_services (provider_id);
CREATE INDEX IF NOT EXISTS idx_home_provider_services_type ON public.home_provider_services (service_type_id);

CREATE TABLE IF NOT EXISTS public.home_provider_availability (
    id          uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    day_of_week smallint  NOT NULL
        CONSTRAINT home_provider_availability_day_of_week_check CHECK (day_of_week >= 0 AND day_of_week <= 6),
    start_time  time      NOT NULL,
    end_time    time      NOT NULL,
    is_active   boolean   DEFAULT true NOT NULL,
    created_at  timestamp DEFAULT now() NOT NULL,
    CONSTRAINT chk_home_provider_availability_time CHECK (start_time < end_time)
);

CREATE INDEX IF NOT EXISTS idx_home_provider_availability_provider ON public.home_provider_availability (provider_id);
CREATE INDEX IF NOT EXISTS idx_home_provider_availability_day ON public.home_provider_availability (day_of_week);

CREATE TABLE IF NOT EXISTS public.home_provider_availability_exceptions (
    id             uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id    uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    date           date      NOT NULL,
    start_time     time,
    end_time       time,
    is_unavailable boolean   DEFAULT false NOT NULL,
    reason         text,
    created_at     timestamp DEFAULT now() NOT NULL,
    CONSTRAINT chk_home_provider_exception_time
        CHECK ((start_time IS NULL AND end_time IS NULL) OR (start_time < end_time))
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_home_provider_exception_unique
    ON public.home_provider_availability_exceptions (provider_id, date);

CREATE TABLE IF NOT EXISTS public.home_provider_documents (
    id            uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id   uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    document_type varchar   NOT NULL,
    file_url      varchar   NOT NULL,
    created_at    timestamp DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_provider_documents_provider ON public.home_provider_documents (provider_id);

CREATE TABLE IF NOT EXISTS public.home_provider_gallery (
    id          uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    photo_url   varchar   NOT NULL,
    created_at  timestamp DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_provider_gallery_provider ON public.home_provider_gallery (provider_id);

CREATE TABLE IF NOT EXISTS public.home_provider_certifications (
    id                   uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id          uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    service_type_id      uuid      REFERENCES public.home_service_types ON DELETE SET NULL,
    title                varchar(150) NOT NULL,
    issuing_organization varchar(150),
    credential_number    varchar(100),
    issued_date          date,
    expiry_date          date,
    document_url         text,
    created_at           timestamp DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_provider_certifications_provider ON public.home_provider_certifications (provider_id);

CREATE TABLE IF NOT EXISTS public.home_provider_specialties (
    id          uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    specialty   varchar(100) NOT NULL,
    created_at  timestamp DEFAULT now() NOT NULL,
    CONSTRAINT uq_provider_specialty UNIQUE (provider_id, specialty)
);

CREATE INDEX IF NOT EXISTS idx_home_provider_specialties_provider ON public.home_provider_specialties (provider_id);

CREATE TABLE IF NOT EXISTS public.home_provider_service_areas (
    id          uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    provider_id uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    label       varchar(150),
    latitude    double precision NOT NULL,
    longitude   double precision NOT NULL,
    radius_km   numeric(6,2) NOT NULL
        CONSTRAINT home_provider_service_areas_radius_check CHECK (radius_km > 0),
    created_at  timestamp DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_provider_service_areas_provider ON public.home_provider_service_areas (provider_id);

CREATE TABLE IF NOT EXISTS public.home_service_sessions (
    id                     uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    status                 varchar     DEFAULT 'pending' NOT NULL
        CONSTRAINT home_service_sessions_status_check
            CHECK (status IN ('pending','in_progress','paused','completed','cancelled')),
    started_at             timestamp   DEFAULT now() NOT NULL,
    ended_at               timestamp,
    provider_id            uuid REFERENCES public.home_service_providers ON DELETE RESTRICT,
    booking_id             uuid,
    total_distance_meters  numeric(10,2),
    total_duration_seconds integer
);

CREATE INDEX IF NOT EXISTS idx_home_service_sessions_status ON public.home_service_sessions (status);
CREATE INDEX IF NOT EXISTS idx_home_service_sessions_provider ON public.home_service_sessions (provider_id);

CREATE TABLE IF NOT EXISTS public.home_service_bookings (
    id                      uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    client_user_id          uuid        NOT NULL REFERENCES public.users ON DELETE RESTRICT,
    provider_id             uuid        NOT NULL REFERENCES public.home_service_providers ON DELETE RESTRICT,
    service_type_id         uuid        NOT NULL REFERENCES public.home_service_types ON DELETE RESTRICT,
    pet_id                  uuid        NOT NULL REFERENCES public.pets ON DELETE RESTRICT,
    status                  varchar(30) DEFAULT 'pending' NOT NULL,
    slot_start              timestamp   NOT NULL,
    duration_minutes        integer     NOT NULL,
    snapshot_price          numeric(10,2),
    total_price             numeric(10,2)
        CONSTRAINT home_service_bookings_total_price_check CHECK (total_price >= 0),
    service_address         text,
    service_latitude        double precision,
    service_longitude       double precision,
    special_instructions    text,
    home_service_session_id uuid REFERENCES public.home_service_sessions ON DELETE SET NULL,
    accepted_at             timestamp,
    rejected_at             timestamp,
    cancelled_at            timestamp,
    started_at              timestamp,
    completed_at            timestamp,
    created_at              timestamp   DEFAULT now() NOT NULL,
    updated_at              timestamp   DEFAULT now() NOT NULL
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_home_service_sessions_booking'
    ) THEN
        ALTER TABLE public.home_service_sessions
            ADD CONSTRAINT fk_home_service_sessions_booking
                FOREIGN KEY (booking_id) REFERENCES public.home_service_bookings ON DELETE RESTRICT;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS idx_home_service_sessions_booking
    ON public.home_service_sessions (booking_id);

CREATE INDEX IF NOT EXISTS idx_home_service_bookings_client ON public.home_service_bookings (client_user_id);
CREATE INDEX IF NOT EXISTS idx_home_service_bookings_provider ON public.home_service_bookings (provider_id);
CREATE INDEX IF NOT EXISTS idx_home_service_bookings_service_type ON public.home_service_bookings (service_type_id);
CREATE INDEX IF NOT EXISTS idx_home_service_bookings_pet ON public.home_service_bookings (pet_id);
CREATE INDEX IF NOT EXISTS idx_home_service_bookings_status ON public.home_service_bookings (status);
CREATE INDEX IF NOT EXISTS idx_home_service_bookings_schedule ON public.home_service_bookings (slot_start);

CREATE TABLE IF NOT EXISTS public.home_service_session_events (
    id          uuid      DEFAULT gen_random_uuid() PRIMARY KEY,
    session_id  uuid      NOT NULL REFERENCES public.home_service_sessions ON DELETE CASCADE,
    event_type  varchar   NOT NULL,
    description text,
    latitude    numeric(10,8),
    longitude   numeric(11,8),
    created_at  timestamp DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_home_service_session_events_session ON public.home_service_session_events (session_id);
CREATE INDEX IF NOT EXISTS idx_home_service_session_events_time ON public.home_service_session_events (session_id, created_at);

CREATE TABLE IF NOT EXISTS public.favorite_home_providers (
    user_id     uuid      NOT NULL REFERENCES public.users ON DELETE CASCADE,
    provider_id uuid      NOT NULL REFERENCES public.home_service_providers ON DELETE CASCADE,
    created_at  timestamp DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id, provider_id)
);

-- ------------------------------------------------------------
-- Shared-table touch: widen feedback.target_type to allow
-- 'home_service_provider' reviews (reusing the existing
-- polymorphic Feedback mechanism instead of a duplicate table).
-- Additive only — existing rows/constraints for 'walker',
-- 'user', 'business' are untouched.
-- ------------------------------------------------------------
ALTER TABLE public.feedback DROP CONSTRAINT IF EXISTS feedback_target_type_check;
ALTER TABLE public.feedback
    ADD CONSTRAINT feedback_target_type_check
        CHECK ((target_type)::text = ANY (
            (ARRAY['walker','user','business','home_service_provider'])::text[]
        ));
