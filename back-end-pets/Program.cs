using BackEndPets.API.Converters;
using BackEndPets.API.Middleware;
using BackEndPets.API.Endpoints;
using BackEndPets.API.Hubs;
using BackEndPets.Application;
using BackEndPets.Infrastructure;
using BackEndPets.Infrastructure.Identity;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using System.Security.Claims;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "balto";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "balto.api";
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-only-change-this-secret-key-32-chars";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/walk-tracking"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});
builder.Services.AddSignalR();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UtcDateTimeJsonConverter());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [new OpenApiServer { Url = "/", Description = "Current server" }];
        return Task.CompletedTask;
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var cloudName = builder.Configuration["Cloudinary:CloudName"];
if (!string.IsNullOrEmpty(cloudName))
{
    var cloudinaryAccount = new Account(
        cloudName,
        builder.Configuration["Cloudinary:ApiKey"],
        builder.Configuration["Cloudinary:ApiSecret"]);
    builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));
}

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await db.Database.MigrateAsync();

    // walk_sessions: create if missing, then add columns that were added after initial schema.
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS walk_sessions (
            id                      UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
            walker_id               UUID,
            booking_id              UUID,
            status                  VARCHAR(50) NOT NULL DEFAULT 'pending',
            started_at              TIMESTAMP   NOT NULL DEFAULT NOW(),
            ended_at                TIMESTAMP,
            total_distance_meters   DOUBLE PRECISION,
            total_duration_seconds  INTEGER
        );
        ALTER TABLE walk_sessions ADD COLUMN IF NOT EXISTS booking_id             UUID;
        ALTER TABLE walk_sessions ADD COLUMN IF NOT EXISTS total_distance_meters  DOUBLE PRECISION;
        ALTER TABLE walk_sessions ADD COLUMN IF NOT EXISTS total_duration_seconds INTEGER;
        """);

    // walk_bookings was added after initial DB setup with no EF migrations, so create if missing.
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS walk_bookings (
            id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
            client_user_id      UUID         NOT NULL,
            walker_id           UUID         NOT NULL,
            pet_id              UUID         NOT NULL,
            status              VARCHAR(50)  NOT NULL DEFAULT 'pending',
            slot_start          TIMESTAMP    NOT NULL,
            duration_minutes    INTEGER      NOT NULL,
            snapshot_hourly_rate NUMERIC(10,2),
            total_price         NUMERIC(10,2),
            special_instructions TEXT,
            walk_session_id     UUID,
            created_at          TIMESTAMP    NOT NULL DEFAULT NOW(),
            updated_at          TIMESTAMP    NOT NULL DEFAULT NOW()
        );
        ALTER TABLE walk_bookings ADD COLUMN IF NOT EXISTS walk_session_id UUID;
        """);

    // walkers table was created before DocumentNumber/Bio/HourlyRate/IsAcceptingBookings/WorkLatitude/WorkLongitude
    // columns were added to the entity, so add them if missing.
    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS document_name VARCHAR(255);
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS document_number VARCHAR(100);
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS bio TEXT;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS hourly_rate NUMERIC(10,2);
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS service_radius_km NUMERIC(8,2);
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS years_of_experience INTEGER;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS is_accepting_bookings BOOLEAN NOT NULL DEFAULT false;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS work_latitude DOUBLE PRECISION;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS work_longitude DOUBLE PRECISION;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS admin_status VARCHAR(30) NOT NULL DEFAULT 'active';
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS admin_reason TEXT;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS admin_moderated_by_user_id UUID;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS admin_moderated_at TIMESTAMP;
        ALTER TABLE walkers ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        ALTER TABLE users ADD COLUMN IF NOT EXISTS admin_status VARCHAR(30) NOT NULL DEFAULT 'active';
        ALTER TABLE users ADD COLUMN IF NOT EXISTS admin_reason TEXT;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS admin_moderated_by_user_id UUID;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS admin_moderated_at TIMESTAMP;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMP;
        """);

    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS community_alerts (
            id                      UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
            reporter_user_id        UUID        NOT NULL,
            alert_type              VARCHAR(30) NOT NULL,
            pet_name                VARCHAR(160) NOT NULL,
            species                 VARCHAR(80),
            description             TEXT        NOT NULL,
            last_seen_location      TEXT,
            evidence_url            TEXT,
            status                  VARCHAR(30) NOT NULL DEFAULT 'active',
            moderation_reason       TEXT,
            moderated_by_user_id    UUID,
            moderated_at            TIMESTAMP,
            created_at              TIMESTAMP   NOT NULL DEFAULT NOW(),
            updated_at              TIMESTAMP   NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS ix_community_alerts_status ON community_alerts(status);
        CREATE INDEX IF NOT EXISTS ix_community_alerts_type ON community_alerts(alert_type);
        CREATE TABLE IF NOT EXISTS admin_audit_logs (
            id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
            actor_user_id   UUID        NOT NULL,
            action          VARCHAR(120) NOT NULL,
            entity_type     VARCHAR(80) NOT NULL,
            entity_id       UUID        NOT NULL,
            reason          TEXT        NOT NULL,
            snapshot_json   JSONB       NOT NULL DEFAULT '{}'::jsonb,
            created_at      TIMESTAMP   NOT NULL DEFAULT NOW()
        );
        CREATE INDEX IF NOT EXISTS ix_admin_audit_entity ON admin_audit_logs(entity_type, entity_id);
        CREATE INDEX IF NOT EXISTS ix_admin_audit_created ON admin_audit_logs(created_at);
        """);

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    await IdentitySeeder.SeedDemoUserAsync(userManager, roleManager);
}

app.MapGet("/health", () => Results.Ok("OK"));

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(new { code = "INTERNAL_SERVER_ERROR", error = "An unexpected error occurred." });
}));

app.UseForwardedHeaders();
app.UseCors("AllowAll");
app.MapOpenApi();
app.MapSwaggerEndpoints();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<WalkTrackingHub>("/hubs/walk-tracking");

app.MapAuthEndpoints();
app.MapUsersEndpoints();
app.MapMeEndpoints();
app.MapWalkersEndpoints();
app.MapBusinessesEndpoints();
app.MapPetsEndpoints();
app.MapWalkingHistoryEndpoints();
app.MapWalkSessionsEndpoints();
app.MapFeedbackEndpoints();
app.MapWalkerAssetsEndpoints();
app.MapBusinessAssetsEndpoints();
app.MapBusinessServicesEndpoints();
app.MapPetHistoryEndpoints();
app.MapUploadEndpoints();
app.MapNotificationsEndpoints();
app.MapWalkerAvailabilityEndpoints();
app.MapWalkBookingEndpoints();
app.MapChatEndpoints();
app.MapPaymentsEndpoints();
app.MapAdminVerificationEndpoints();
app.MapAdminOperationsEndpoints();
app.UseGlobalExceptionHandler();

app.Run();
