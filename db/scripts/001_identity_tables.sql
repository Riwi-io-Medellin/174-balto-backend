
BEGIN;

-- =============================================================
-- MIGRATION: Drop old ASP.NET Identity tables (text-based PKs)
-- and replace with identity_* tables using uuid PKs matching
-- the AppIdentityDbContext configuration.
-- =============================================================

-- Drop old tables in FK-safe order (dependent first)
DROP TABLE IF EXISTS "AspNetUserRoles" CASCADE;
DROP TABLE IF EXISTS "AspNetUserLogins" CASCADE;
DROP TABLE IF EXISTS "AspNetUserTokens" CASCADE;
DROP TABLE IF EXISTS "AspNetUserClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetRoleClaims" CASCADE;
DROP TABLE IF EXISTS "AspNetUsers" CASCADE;
DROP TABLE IF EXISTS "AspNetRoles" CASCADE;

-- =============================================================
-- FIXES ON users TABLE
-- =============================================================

ALTER TABLE users ALTER COLUMN lockout_enabled SET DEFAULT false;
ALTER TABLE users ADD COLUMN IF NOT EXISTS phone_number varchar;

-- =============================================================
-- identity_roles — replaces AspNetRoles with uuid PK
-- =============================================================

CREATE TABLE identity_roles (
    "Id" uuid NOT NULL,
    "Name" varchar(256),
    "NormalizedName" varchar(256),
    "ConcurrencyStamp" text,
    CONSTRAINT "PK_identity_roles" PRIMARY KEY ("Id")
);

-- =============================================================
-- identity_user_roles — join table between users and roles
-- =============================================================

CREATE TABLE identity_user_roles (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL,
    CONSTRAINT "PK_identity_user_roles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_identity_user_roles_users_UserId" FOREIGN KEY ("UserId") REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT "FK_identity_user_roles_identity_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES identity_roles("Id") ON DELETE CASCADE
);

-- =============================================================
-- identity_user_claims — claims assigned to users
-- =============================================================

CREATE TABLE identity_user_claims (
    "Id" serial NOT NULL,
    "UserId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_identity_user_claims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_identity_user_claims_users_UserId" FOREIGN KEY ("UserId") REFERENCES users(id) ON DELETE CASCADE
);

-- =============================================================
-- identity_user_logins — external login providers (Google, etc.)
-- =============================================================

CREATE TABLE identity_user_logins (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" uuid NOT NULL,
    CONSTRAINT "PK_identity_user_logins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_identity_user_logins_users_UserId" FOREIGN KEY ("UserId") REFERENCES users(id) ON DELETE CASCADE
);

-- =============================================================
-- identity_user_tokens — tokens for password reset, 2FA, etc.
-- =============================================================

CREATE TABLE identity_user_tokens (
    "UserId" uuid NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text,
    CONSTRAINT "PK_identity_user_tokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_identity_user_tokens_users_UserId" FOREIGN KEY ("UserId") REFERENCES users(id) ON DELETE CASCADE
);

-- =============================================================
-- identity_role_claims — claims assigned to roles
-- =============================================================

CREATE TABLE identity_role_claims (
    "Id" serial NOT NULL,
    "RoleId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text,
    CONSTRAINT "PK_identity_role_claims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_identity_role_claims_identity_roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES identity_roles("Id") ON DELETE CASCADE
);

-- =============================================================
-- INDEXES (matching EF Core conventions)
-- =============================================================

-- Roles: unique index on NormalizedName (Identity requirement)
CREATE UNIQUE INDEX "RoleNameIndex" ON identity_roles ("NormalizedName") WHERE "NormalizedName" IS NOT NULL;

-- FK lookup indexes
CREATE INDEX "IX_identity_user_roles_RoleId" ON identity_user_roles ("RoleId");
CREATE INDEX "IX_identity_user_claims_UserId" ON identity_user_claims ("UserId");
CREATE INDEX "IX_identity_user_logins_UserId" ON identity_user_logins ("UserId");
CREATE INDEX "IX_identity_role_claims_RoleId" ON identity_role_claims ("RoleId");

COMMIT;
