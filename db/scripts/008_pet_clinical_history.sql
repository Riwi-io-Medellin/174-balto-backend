-- ============================================================
-- Pet Clinical History module (idempotent, additive only)
-- No existing table is renamed, dropped, or has columns removed.
-- ============================================================

-- 1. Extend pets with fields required for a complete clinical profile
ALTER TABLE public.pets ADD COLUMN IF NOT EXISTS sex varchar(20);
ALTER TABLE public.pets ADD COLUMN IF NOT EXISTS color varchar(100);
ALTER TABLE public.pets ADD COLUMN IF NOT EXISTS identification_number varchar(100);
ALTER TABLE public.pets ADD COLUMN IF NOT EXISTS microchip_number varchar(100);

-- 2. Drop old, insufficient PetHistory table (unused in production per confirmation)
DROP TABLE IF EXISTS public.pet_histories;

-- 3. Clinical record: one per pet, cumulative/current-state fields + official document
CREATE TABLE IF NOT EXISTS public.pet_clinical_records (
    id                    uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    pet_id                uuid        NOT NULL UNIQUE REFERENCES public.pets ON DELETE CASCADE,
    allergies             text,
    chronic_conditions    text,
    dietary_restrictions  text,
    document_url          text,
    document_generated_at timestamp,
    created_at            timestamp   DEFAULT now() NOT NULL,
    updated_at            timestamp   DEFAULT now() NOT NULL
);

-- 4. Clinical events: the chronological log (consultation, vaccine, surgery, lab, hospitalization...)
CREATE TABLE IF NOT EXISTS public.pet_clinical_events (
    id                 uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    pet_id             uuid        NOT NULL REFERENCES public.pets ON DELETE CASCADE,
    event_type         varchar(30) NOT NULL, -- consultation | vaccine | deworming | surgery | lab | hospitalization | sterilization | other
    event_date         timestamp   NOT NULL,
    clinic_name        varchar(150),
    veterinarian_name  varchar(150),
    reason             text,
    clinical_signs     text,
    temperature        numeric(5,2),
    heart_rate         integer,
    respiratory_rate   integer,
    weight             numeric(6,2),
    body_condition     varchar(50),
    findings           text,
    diagnosis          text,
    exams_performed    text,
    exam_results       text,
    procedures         text,
    recommendations    text,
    observations       text,
    next_control_date  timestamp,
    source             varchar(20) DEFAULT 'manual' NOT NULL, -- manual | ai_extracted
    created_at         timestamp   DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pet_clinical_events_pet_id ON public.pet_clinical_events (pet_id);
CREATE INDEX IF NOT EXISTS ix_pet_clinical_events_event_date ON public.pet_clinical_events (event_date);

-- 5. Medications: N per event
CREATE TABLE IF NOT EXISTS public.pet_clinical_medications (
    id                 uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    clinical_event_id  uuid        NOT NULL REFERENCES public.pet_clinical_events ON DELETE CASCADE,
    name               varchar(150) NOT NULL,
    dose               varchar(100),
    frequency          varchar(100),
    duration           varchar(100),
    created_at         timestamp   DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pet_clinical_medications_event_id ON public.pet_clinical_medications (clinical_event_id);

-- 6. Uploaded source documents (NOT the official record — only AI input)
CREATE TABLE IF NOT EXISTS public.pet_clinical_documents (
    id             uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    pet_id         uuid        NOT NULL REFERENCES public.pets ON DELETE CASCADE,
    file_url       text        NOT NULL,
    file_name      varchar(255),
    file_type      varchar(20), -- pdf | image | docx | xlsx | other
    status         varchar(20) DEFAULT 'pending' NOT NULL, -- pending | processed | failed
    extracted_json text,       -- raw AI draft snapshot, for audit only
    error_message  text,
    created_at     timestamp   DEFAULT now() NOT NULL,
    processed_at   timestamp
);

CREATE INDEX IF NOT EXISTS ix_pet_clinical_documents_pet_id ON public.pet_clinical_documents (pet_id);

-- 7. AI-generated tips, regenerated only when the clinical history changes
CREATE TABLE IF NOT EXISTS public.pet_clinical_tips (
    id          uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    pet_id      uuid        NOT NULL REFERENCES public.pets ON DELETE CASCADE,
    category    varchar(50) NOT NULL, -- care | feeding | vaccination | alert | general
    message     text        NOT NULL,
    created_at  timestamp   DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pet_clinical_tips_pet_id ON public.pet_clinical_tips (pet_id);
