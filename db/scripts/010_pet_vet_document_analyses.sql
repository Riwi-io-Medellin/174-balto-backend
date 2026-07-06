-- ============================================================
-- Vet Document Analysis history (idempotent, additive only)
-- Stores each AI-generated preliminary guidance result so the
-- Coach chatbot can reference it as clinical context.
-- ============================================================

CREATE TABLE IF NOT EXISTS public.pet_vet_document_analyses (
    id             uuid        DEFAULT gen_random_uuid() PRIMARY KEY,
    pet_id         uuid        NOT NULL REFERENCES public.pets ON DELETE CASCADE,
    document_type  varchar(50),
    symptoms       text,
    urgency_level  varchar(30) NOT NULL,
    result_json    text        NOT NULL, -- full VetDocumentAnalysisResponse snapshot
    created_at     timestamp   DEFAULT now() NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_pet_vet_document_analyses_pet_id ON public.pet_vet_document_analyses (pet_id);
