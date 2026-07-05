using BackEndPets.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Identity;

public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Walker> Walkers => Set<Walker>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<PetWalkingHistory> PetWalkingHistories => Set<PetWalkingHistory>();
    public DbSet<WalkSession> WalkSessions => Set<WalkSession>();
    public DbSet<WalkRoutePoint> WalkRoutePoints => Set<WalkRoutePoint>();
    public DbSet<WalkSessionMedia> WalkSessionMediaItems => Set<WalkSessionMedia>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<WalkerGallery> WalkerGalleries => Set<WalkerGallery>();
    public DbSet<WalkerDocument> WalkerDocuments => Set<WalkerDocument>();
    public DbSet<BusinessDocument> BusinessDocuments => Set<BusinessDocument>();
    public DbSet<BusinessService> BusinessServices => Set<BusinessService>();
    public DbSet<PetClinicalRecord> PetClinicalRecords => Set<PetClinicalRecord>();
    public DbSet<PetClinicalEvent> PetClinicalEvents => Set<PetClinicalEvent>();
    public DbSet<PetClinicalMedication> PetClinicalMedications => Set<PetClinicalMedication>();
    public DbSet<PetClinicalDocument> PetClinicalDocuments => Set<PetClinicalDocument>();
    public DbSet<PetClinicalTip> PetClinicalTips => Set<PetClinicalTip>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<WalkerAvailability> WalkerAvailabilities => Set<WalkerAvailability>();
    public DbSet<WalkerAvailabilityException> WalkerAvailabilityExceptions => Set<WalkerAvailabilityException>();
    public DbSet<WalkBooking> WalkBookings => Set<WalkBooking>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();

    public DbSet<HomeServiceProvider> HomeServiceProviders => Set<HomeServiceProvider>();
    public DbSet<HomeServiceType> HomeServiceTypes => Set<HomeServiceType>();
    public DbSet<HomeProviderService> HomeProviderServices => Set<HomeProviderService>();
    public DbSet<HomeProviderAvailability> HomeProviderAvailabilities => Set<HomeProviderAvailability>();
    public DbSet<HomeProviderAvailabilityException> HomeProviderAvailabilityExceptions => Set<HomeProviderAvailabilityException>();
    public DbSet<HomeProviderDocument> HomeProviderDocuments => Set<HomeProviderDocument>();
    public DbSet<HomeProviderGallery> HomeProviderGalleries => Set<HomeProviderGallery>();
    public DbSet<HomeProviderCertification> HomeProviderCertifications => Set<HomeProviderCertification>();
    public DbSet<HomeProviderSpecialty> HomeProviderSpecialties => Set<HomeProviderSpecialty>();
    public DbSet<HomeProviderServiceArea> HomeProviderServiceAreas => Set<HomeProviderServiceArea>();
    public DbSet<HomeServiceBooking> HomeServiceBookings => Set<HomeServiceBooking>();
    public DbSet<HomeServiceSession> HomeServiceSessions => Set<HomeServiceSession>();
    public DbSet<HomeServiceSessionEvent> HomeServiceSessionEvents => Set<HomeServiceSessionEvent>();
    public DbSet<FavoriteHomeProvider> FavoriteHomeProviders => Set<FavoriteHomeProvider>();

    public DbSet<BusinessHour> BusinessHours => Set<BusinessHour>();
    public DbSet<BusinessHourException> BusinessHourExceptions => Set<BusinessHourException>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("users");
            
            b.Ignore(u => u.PhoneNumber);
            b.Ignore(u => u.PhoneNumberConfirmed);

            b.Property(u => u.Id).HasColumnName("id");
            b.Property(u => u.FirstName).HasColumnName("first_name");
            b.Property(u => u.LastName).HasColumnName("last_name");
            b.Property(u => u.Email).HasColumnName("email");
            b.Property(u => u.IdNumber).HasColumnName("id_number");
            b.Property(u => u.IdType).HasColumnName("id_type");
            b.Property(u => u.Location).HasColumnName("location");
            b.Property(u => u.Address).HasColumnName("address");
            b.Property(u => u.Latitude).HasColumnName("latitude");
            b.Property(u => u.Longitude).HasColumnName("longitude");
            b.Property(u => u.Phone).HasColumnName("phone").HasColumnType("varchar(30)");
            b.Property(u => u.PhoneExtra).HasColumnName("phone_extra").HasColumnType("varchar(30)");
            b.Property(u => u.PhotoUrl).HasColumnName("photo_url");
            b.Property(u => u.CreatedAt).HasColumnName("created_at");

            // Usamos el email como username, así no se duplica info.
            b.Ignore(u => u.UserName);
            b.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
            b.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
            b.Property(u => u.PasswordHash).HasColumnName("password_hash");
            b.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
            b.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            b.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
            b.Property(u => u.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            b.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
            b.Property(u => u.LockoutEnabled).HasColumnName("lockout_enabled");
            b.Property(u => u.AccessFailedCount).HasColumnName("access_failed_count");

            b.HasIndex(u => u.NormalizedEmail).IsUnique();
            b.HasIndex(u => u.NormalizedUserName).IsUnique();
        });
        
        builder.Entity<IdentityRole<Guid>>().ToTable("identity_roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("identity_user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("identity_user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("identity_user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("identity_user_tokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("identity_role_claims");

        builder.Entity<Walker>(b =>
        {
            b.ToTable("walkers");
            b.HasKey(w => w.Id);
            b.Property(w => w.Id).HasColumnName("id");
            b.Property(w => w.UserId).HasColumnName("user_id");
            b.Property(w => w.Available).HasColumnName("available");
            b.Property(w => w.WorkLocation).HasColumnName("work_location");
            b.Property(w => w.Experience).HasColumnName("experience");
            b.Property(w => w.Description).HasColumnName("description");
            b.Property(w => w.VerificationStatus).HasColumnName("verification_status");
            b.Property(w => w.DocumentName).HasColumnName("document_name");
            b.Property(w => w.DocumentNumber).HasColumnName("document_number");
            b.Property(w => w.Bio).HasColumnName("bio");
            b.Property(w => w.HourlyRate).HasColumnName("hourly_rate").HasColumnType("numeric(10,2)");
            b.Property(w => w.ServiceRadiusKm).HasColumnName("service_radius_km").HasColumnType("numeric(8,2)");
            b.Property(w => w.YearsOfExperience).HasColumnName("years_of_experience");
            b.Property(w => w.IsAcceptingBookings).HasColumnName("is_accepting_bookings");
            b.Property(w => w.WorkLatitude).HasColumnName("work_latitude");
            b.Property(w => w.WorkLongitude).HasColumnName("work_longitude");
            b.Property(w => w.MaxDogs).HasColumnName("max_dogs");
            b.Property(w => w.CreatedAt).HasColumnName("created_at");
            b.Property(w => w.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(w => w.UserId).IsUnique();
            b.Property(w => w.InstagramUrl).HasColumnName("instagram_url");
            b.Property(w => w.FacebookUrl).HasColumnName("facebook_url");
        });
        
        builder.Entity<WalkerGallery>(b =>
        {
            b.ToTable("walker_gallery");
            b.HasKey(g => g.Id);
            b.Property(g => g.Id).HasColumnName("id");
            b.Property(g => g.WalkerId).HasColumnName("walker_id");
            b.Property(g => g.PhotoUrl).HasColumnName("photo_url");
            b.Property(g => g.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<WalkerDocument>(b =>
        {
            b.ToTable("walker_documents");
            b.HasKey(d => d.Id);
            b.Property(d => d.Id).HasColumnName("id");
            b.Property(d => d.WalkerId).HasColumnName("walker_id");
            b.Property(d => d.DocumentType).HasColumnName("document_type");
            b.Property(d => d.FileUrl).HasColumnName("file_url");
            b.Property(d => d.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<Business>(b =>
        {
            b.ToTable("businesses");
            b.HasKey(biz => biz.Id);
            b.Property(biz => biz.Id).HasColumnName("id");
            b.Property(biz => biz.OwnerUserId).HasColumnName("owner_user_id");
            b.Property(biz => biz.Name).HasColumnName("name");
            b.Property(biz => biz.Nit).HasColumnName("nit");
            b.Property(biz => biz.Email).HasColumnName("email");
            b.Property(biz => biz.Location).HasColumnName("location");
            b.Property(biz => biz.Address).HasColumnName("address");
            b.Property(biz => biz.Latitude).HasColumnName("latitude");
            b.Property(biz => biz.Longitude).HasColumnName("longitude");
            b.Property(biz => biz.Phone).HasColumnName("phone");
            b.Property(biz => biz.Type).HasColumnName("type");
            b.Property(biz => biz.VerificationStatus).HasColumnName("verification_status");
            b.Property(biz => biz.CreatedAt).HasColumnName("created_at");
            b.HasIndex(biz => biz.Nit).IsUnique();
            b.HasIndex(biz => biz.Email).IsUnique();
            b.Property(biz => biz.InstagramUrl).HasColumnName("instagram_url");
            b.Property(biz => biz.FacebookUrl).HasColumnName("facebook_url");
        });
        
        builder.Entity<BusinessDocument>(b =>
        {
            b.ToTable("business_documents");
            b.HasKey(d => d.Id);
            b.Property(d => d.Id).HasColumnName("id");
            b.Property(d => d.BusinessId).HasColumnName("business_id");
            b.Property(d => d.DocumentType).HasColumnName("document_type");
            b.Property(d => d.FileUrl).HasColumnName("file_url");
            b.Property(d => d.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<BusinessService>(b =>
        {
            b.ToTable("business_services");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.BusinessId).HasColumnName("business_id");
            b.Property(s => s.ServiceType).HasColumnName("service_type");
            b.Property(s => s.Description).HasColumnName("description");
            b.Property(s => s.Price).HasColumnName("price");
            b.Property(s => s.PhotoUrl).HasColumnName("photo_url");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<BusinessHour>(b =>
        {
            b.ToTable("business_hours");
            b.HasKey(h => h.Id);
            b.Property(h => h.Id).HasColumnName("id");
            b.Property(h => h.BusinessId).HasColumnName("business_id");
            b.Property(h => h.DayOfWeek).HasColumnName("day_of_week");
            b.Property(h => h.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(h => h.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(h => h.IsActive).HasColumnName("is_active");
            b.Property(h => h.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<BusinessHourException>(b =>
        {
            b.ToTable("business_hours_exceptions");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.BusinessId).HasColumnName("business_id");
            b.Property(e => e.Date).HasColumnName("date").HasColumnType("date");
            b.Property(e => e.IsUnavailable).HasColumnName("is_unavailable");
            b.Property(e => e.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(e => e.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.HasIndex(e => new { e.BusinessId, e.Date }).IsUnique();
        });
        
        builder.Entity<Pet>(b =>
        {
            b.ToTable("pets");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasColumnName("id");
            b.Property(p => p.UserId).HasColumnName("user_id");
            b.Property(p => p.Name).HasColumnName("name");
            b.Property(p => p.Species).HasColumnName("species");
            b.Property(p => p.Breed).HasColumnName("breed");
            b.Property(p => p.BirthDate).HasColumnName("birth_date");
            b.Property(p => p.Description).HasColumnName("description");
            b.Property(p => p.PhotoUrl).HasColumnName("photo_url");
            b.Property(p => p.Weight).HasColumnName("weight").HasColumnType("numeric(5,2)");
            b.Property(p => p.Sex).HasColumnName("sex");
            b.Property(p => p.Color).HasColumnName("color");
            b.Property(p => p.IdentificationNumber).HasColumnName("identification_number");
            b.Property(p => p.MicrochipNumber).HasColumnName("microchip_number");
            b.Property(p => p.IsLost).HasColumnName("is_lost");
            b.Property(p => p.LostLatitude).HasColumnName("lost_latitude");
            b.Property(p => p.LostLongitude).HasColumnName("lost_longitude");
            b.Property(p => p.LostAt).HasColumnName("lost_at");
            b.Property(p => p.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<PetClinicalRecord>(b =>
        {
            b.ToTable("pet_clinical_records");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.PetId).HasColumnName("pet_id");
            b.Property(r => r.Allergies).HasColumnName("allergies");
            b.Property(r => r.ChronicConditions).HasColumnName("chronic_conditions");
            b.Property(r => r.DietaryRestrictions).HasColumnName("dietary_restrictions");
            b.Property(r => r.DocumentUrl).HasColumnName("document_url");
            b.Property(r => r.DocumentGeneratedAt).HasColumnName("document_generated_at");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(r => r.PetId).IsUnique();
        });

        builder.Entity<PetClinicalEvent>(b =>
        {
            b.ToTable("pet_clinical_events");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.PetId).HasColumnName("pet_id");
            b.Property(e => e.EventType).HasColumnName("event_type");
            b.Property(e => e.EventDate).HasColumnName("event_date");
            b.Property(e => e.ClinicName).HasColumnName("clinic_name");
            b.Property(e => e.VeterinarianName).HasColumnName("veterinarian_name");
            b.Property(e => e.Reason).HasColumnName("reason");
            b.Property(e => e.ClinicalSigns).HasColumnName("clinical_signs");
            b.Property(e => e.Temperature).HasColumnName("temperature").HasColumnType("numeric(5,2)");
            b.Property(e => e.HeartRate).HasColumnName("heart_rate");
            b.Property(e => e.RespiratoryRate).HasColumnName("respiratory_rate");
            b.Property(e => e.Weight).HasColumnName("weight").HasColumnType("numeric(6,2)");
            b.Property(e => e.BodyCondition).HasColumnName("body_condition");
            b.Property(e => e.Findings).HasColumnName("findings");
            b.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            b.Property(e => e.ExamsPerformed).HasColumnName("exams_performed");
            b.Property(e => e.ExamResults).HasColumnName("exam_results");
            b.Property(e => e.Procedures).HasColumnName("procedures");
            b.Property(e => e.Recommendations).HasColumnName("recommendations");
            b.Property(e => e.Observations).HasColumnName("observations");
            b.Property(e => e.NextControlDate).HasColumnName("next_control_date");
            b.Property(e => e.Source).HasColumnName("source");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.HasIndex(e => e.PetId);
            b.HasMany(e => e.Medications)
                .WithOne()
                .HasForeignKey(m => m.ClinicalEventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PetClinicalMedication>(b =>
        {
            b.ToTable("pet_clinical_medications");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("id");
            b.Property(m => m.ClinicalEventId).HasColumnName("clinical_event_id");
            b.Property(m => m.Name).HasColumnName("name");
            b.Property(m => m.Dose).HasColumnName("dose");
            b.Property(m => m.Frequency).HasColumnName("frequency");
            b.Property(m => m.Duration).HasColumnName("duration");
            b.Property(m => m.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<PetClinicalDocument>(b =>
        {
            b.ToTable("pet_clinical_documents");
            b.HasKey(d => d.Id);
            b.Property(d => d.Id).HasColumnName("id");
            b.Property(d => d.PetId).HasColumnName("pet_id");
            b.Property(d => d.FileUrl).HasColumnName("file_url");
            b.Property(d => d.FileName).HasColumnName("file_name");
            b.Property(d => d.FileType).HasColumnName("file_type");
            b.Property(d => d.Status).HasColumnName("status");
            b.Property(d => d.ExtractedJson).HasColumnName("extracted_json");
            b.Property(d => d.ErrorMessage).HasColumnName("error_message");
            b.Property(d => d.CreatedAt).HasColumnName("created_at");
            b.Property(d => d.ProcessedAt).HasColumnName("processed_at");
            b.HasIndex(d => d.PetId);
        });

        builder.Entity<PetClinicalTip>(b =>
        {
            b.ToTable("pet_clinical_tips");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasColumnName("id");
            b.Property(t => t.PetId).HasColumnName("pet_id");
            b.Property(t => t.Category).HasColumnName("category");
            b.Property(t => t.Message).HasColumnName("message");
            b.Property(t => t.CreatedAt).HasColumnName("created_at");
            b.HasIndex(t => t.PetId);
        });

        builder.Entity<PetWalkingHistory>(b =>
        {
            b.ToTable("pet_walking_history");
            b.HasKey(h => h.Id);
            b.Property(h => h.Id).HasColumnName("id");
            b.Property(h => h.WalkSessionId).HasColumnName("walk_session_id");
            b.Property(h => h.UserId).HasColumnName("user_id");
            b.Property(h => h.PetId).HasColumnName("pet_id");
            b.Property(h => h.WalkerId).HasColumnName("walker_id");
            b.Property(h => h.Cost).HasColumnName("cost");
            b.Property(h => h.StartTime).HasColumnName("start_time");
            b.Property(h => h.EndTime).HasColumnName("end_time");
            b.Property(h => h.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<WalkSession>(b =>
        {
            b.ToTable("walk_sessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.WalkerId).HasColumnName("walker_id");
            b.Property(s => s.BookingId).HasColumnName("booking_id");
            b.Property(s => s.Status).HasColumnName("status");
            b.Property(s => s.StartedAt).HasColumnName("started_at");
            b.Property(s => s.EndedAt).HasColumnName("ended_at");
            b.Property(s => s.TotalDistanceMeters).HasColumnName("total_distance_meters");
            b.Property(s => s.TotalDurationSeconds).HasColumnName("total_duration_seconds");
        });

        builder.Entity<WalkRoutePoint>(b =>
        {
            b.ToTable("walk_route_points");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasColumnName("id");
            b.Property(p => p.WalkSessionId).HasColumnName("walk_session_id");
            b.Property(p => p.Latitude).HasColumnName("latitude");
            b.Property(p => p.Longitude).HasColumnName("longitude");
            b.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<WalkSessionMedia>(b =>
        {
            b.ToTable("walk_session_media");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("id");
            b.Property(m => m.WalkSessionId).HasColumnName("walk_session_id");
            b.Property(m => m.Url).HasColumnName("url");
            b.Property(m => m.Type).HasColumnName("type");
            b.Property(m => m.UploadedAt).HasColumnName("uploaded_at");
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.ToTable("chat_messages");
            b.HasKey(m => m.Id);
            b.Property(m => m.Id).HasColumnName("id");
            b.Property(m => m.WalkSessionId).HasColumnName("walk_session_id");
            b.Property(m => m.SenderUserId).HasColumnName("sender_user_id");
            b.Property(m => m.Text).HasColumnName("text");
            b.Property(m => m.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<Feedback>(b =>
        {
            b.ToTable("feedback");
            b.HasKey(f => f.Id);
            b.Property(f => f.Id).HasColumnName("id");
            b.Property(f => f.UserId).HasColumnName("user_id");
            b.Property(f => f.TargetId).HasColumnName("target_id");
            b.Property(f => f.TargetType).HasColumnName("target_type");
            b.Property(f => f.Comment).HasColumnName("comment");
            b.Property(f => f.Rating).HasColumnName("rating");
            b.Property(f => f.CreatedAt).HasColumnName("created_at");
        });
        
        builder.Entity<Notification>(b =>
        {
            b.ToTable("notifications");
            b.HasKey(n => n.Id);
            b.Property(n => n.Id).HasColumnName("id");
            b.Property(n => n.UserId).HasColumnName("user_id");
            b.Property(n => n.Type).HasColumnName("type");
            b.Property(n => n.Title).HasColumnName("title");
            b.Property(n => n.Body).HasColumnName("body");
            b.Property(n => n.EntityId).HasColumnName("entity_id");
            b.Property(n => n.EntityType).HasColumnName("entity_type");
            b.Property(n => n.Metadata).HasColumnName("metadata");
            b.Property(n => n.IsRead).HasColumnName("is_read");
            b.Property(n => n.ReadAt).HasColumnName("read_at");
            b.Property(n => n.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<WalkerAvailability>(b =>
        {
            b.ToTable("walker_availability");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).HasColumnName("id");
            b.Property(a => a.WalkerId).HasColumnName("walker_id");
            b.Property(a => a.DayOfWeek).HasColumnName("day_of_week");
            b.Property(a => a.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(a => a.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(a => a.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<WalkerAvailabilityException>(b =>
        {
            b.ToTable("walker_availability_exceptions");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.WalkerId).HasColumnName("walker_id");
            b.Property(e => e.Date).HasColumnName("date").HasColumnType("date");
            b.Property(e => e.IsUnavailable).HasColumnName("is_unavailable");
            b.Property(e => e.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(e => e.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.HasIndex(e => new { e.WalkerId, e.Date }).IsUnique();
        });

        builder.Entity<WalkBooking>(b =>
        {
            b.ToTable("walk_bookings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ClientUserId).HasColumnName("client_user_id");
            b.Property(x => x.WalkerId).HasColumnName("walker_id");
            b.Property(x => x.PetId).HasColumnName("pet_id");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.SlotStart).HasColumnName("slot_start");
            b.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            b.Property(x => x.SnapshotHourlyRate).HasColumnName("snapshot_hourly_rate").HasColumnType("numeric(10,2)");
            b.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(10,2)");
            b.Property(x => x.IsExclusive).HasColumnName("is_exclusive");
            b.Property(x => x.SpecialInstructions).HasColumnName("special_instructions");
            b.Property(x => x.WalkSessionId).HasColumnName("walk_session_id");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<PaymentOrder>(b =>
        {
            b.ToTable("payment_orders");
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).HasColumnName("id");
            b.Property(o => o.BookingId).HasColumnName("booking_id");
            b.Property(o => o.ClientUserId).HasColumnName("client_user_id");
            b.Property(o => o.Amount).HasColumnName("amount").HasColumnType("numeric(10,2)");
            b.Property(o => o.Currency).HasColumnName("currency");
            b.Property(o => o.Status).HasColumnName("status");
            b.Property(o => o.ProviderReference).HasColumnName("provider_reference");
            b.Property(o => o.ProviderTransactionId).HasColumnName("provider_transaction_id");
            b.Property(o => o.CreatedAt).HasColumnName("created_at");
            b.Property(o => o.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(o => o.BookingId).IsUnique();
            b.HasIndex(o => o.ProviderReference).IsUnique();
        });

        builder.Entity<HomeServiceProvider>(b =>
        {
            b.ToTable("home_service_providers");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasColumnName("id");
            b.Property(p => p.UserId).HasColumnName("user_id");
            b.Property(p => p.Bio).HasColumnName("bio");
            b.Property(p => p.Description).HasColumnName("description");
            b.Property(p => p.Experience).HasColumnName("experience");
            b.Property(p => p.YearsOfExperience).HasColumnName("years_of_experience");
            b.Property(p => p.IsAcceptingBookings).HasColumnName("is_accepting_bookings");
            b.Property(p => p.MaxConcurrentBookings).HasColumnName("max_concurrent_bookings");
            b.Property(p => p.BaseLocation).HasColumnName("base_location");
            b.Property(p => p.Latitude).HasColumnName("latitude");
            b.Property(p => p.Longitude).HasColumnName("longitude");
            b.Property(p => p.VerificationStatus).HasColumnName("verification_status");
            b.Property(p => p.RejectionReason).HasColumnName("rejection_reason");
            b.Property(p => p.DocumentName).HasColumnName("document_name");
            b.Property(p => p.DocumentNumber).HasColumnName("document_number");
            b.Property(p => p.ApprovedAt).HasColumnName("approved_at");
            b.Property(p => p.ApprovedBy).HasColumnName("approved_by");
            b.Property(p => p.CreatedAt).HasColumnName("created_at");
            b.Property(p => p.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(p => p.UserId).IsUnique();
        });

        builder.Entity<HomeServiceType>(b =>
        {
            b.ToTable("home_service_types");
            b.HasKey(t => t.Id);
            b.Property(t => t.Id).HasColumnName("id");
            b.Property(t => t.Code).HasColumnName("code");
            b.Property(t => t.Name).HasColumnName("name");
            b.Property(t => t.Description).HasColumnName("description");
            b.Property(t => t.IsActive).HasColumnName("is_active");
            b.Property(t => t.CreatedAt).HasColumnName("created_at");
            b.HasIndex(t => t.Code).IsUnique();
        });

        builder.Entity<HomeProviderService>(b =>
        {
            b.ToTable("home_provider_services");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.ProviderId).HasColumnName("provider_id");
            b.Property(s => s.ServiceTypeId).HasColumnName("service_type_id");
            b.Property(s => s.Price).HasColumnName("price").HasColumnType("numeric(10,2)");
            b.Property(s => s.PriceUnit).HasColumnName("price_unit");
            b.Property(s => s.Description).HasColumnName("description");
            b.Property(s => s.IsActive).HasColumnName("is_active");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.HasIndex(s => new { s.ProviderId, s.ServiceTypeId }).IsUnique();
        });

        builder.Entity<HomeProviderAvailability>(b =>
        {
            b.ToTable("home_provider_availability");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).HasColumnName("id");
            b.Property(a => a.ProviderId).HasColumnName("provider_id");
            b.Property(a => a.DayOfWeek).HasColumnName("day_of_week");
            b.Property(a => a.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(a => a.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(a => a.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<HomeProviderAvailabilityException>(b =>
        {
            b.ToTable("home_provider_availability_exceptions");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.ProviderId).HasColumnName("provider_id");
            b.Property(e => e.Date).HasColumnName("date").HasColumnType("date");
            b.Property(e => e.IsUnavailable).HasColumnName("is_unavailable");
            b.Property(e => e.StartTime).HasColumnName("start_time").HasColumnType("time");
            b.Property(e => e.EndTime).HasColumnName("end_time").HasColumnType("time");
            b.Property(e => e.Reason).HasColumnName("reason");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.HasIndex(e => new { e.ProviderId, e.Date }).IsUnique();
        });

        builder.Entity<HomeProviderDocument>(b =>
        {
            b.ToTable("home_provider_documents");
            b.HasKey(d => d.Id);
            b.Property(d => d.Id).HasColumnName("id");
            b.Property(d => d.ProviderId).HasColumnName("provider_id");
            b.Property(d => d.DocumentType).HasColumnName("document_type");
            b.Property(d => d.FileUrl).HasColumnName("file_url");
            b.Property(d => d.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<HomeProviderGallery>(b =>
        {
            b.ToTable("home_provider_gallery");
            b.HasKey(g => g.Id);
            b.Property(g => g.Id).HasColumnName("id");
            b.Property(g => g.ProviderId).HasColumnName("provider_id");
            b.Property(g => g.PhotoUrl).HasColumnName("photo_url");
            b.Property(g => g.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<HomeProviderCertification>(b =>
        {
            b.ToTable("home_provider_certifications");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).HasColumnName("id");
            b.Property(c => c.ProviderId).HasColumnName("provider_id");
            b.Property(c => c.ServiceTypeId).HasColumnName("service_type_id");
            b.Property(c => c.Title).HasColumnName("title");
            b.Property(c => c.IssuingOrganization).HasColumnName("issuing_organization");
            b.Property(c => c.CredentialNumber).HasColumnName("credential_number");
            b.Property(c => c.IssuedDate).HasColumnName("issued_date").HasColumnType("date");
            b.Property(c => c.ExpiryDate).HasColumnName("expiry_date").HasColumnType("date");
            b.Property(c => c.DocumentUrl).HasColumnName("document_url");
            b.Property(c => c.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<HomeProviderSpecialty>(b =>
        {
            b.ToTable("home_provider_specialties");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.ProviderId).HasColumnName("provider_id");
            b.Property(s => s.Specialty).HasColumnName("specialty");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.HasIndex(s => new { s.ProviderId, s.Specialty }).IsUnique();
        });

        builder.Entity<HomeProviderServiceArea>(b =>
        {
            b.ToTable("home_provider_service_areas");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).HasColumnName("id");
            b.Property(a => a.ProviderId).HasColumnName("provider_id");
            b.Property(a => a.Label).HasColumnName("label");
            b.Property(a => a.Latitude).HasColumnName("latitude");
            b.Property(a => a.Longitude).HasColumnName("longitude");
            b.Property(a => a.RadiusKm).HasColumnName("radius_km").HasColumnType("numeric(6,2)");
            b.Property(a => a.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<HomeServiceBooking>(b =>
        {
            b.ToTable("home_service_bookings");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ClientUserId).HasColumnName("client_user_id");
            b.Property(x => x.ProviderId).HasColumnName("provider_id");
            b.Property(x => x.ServiceTypeId).HasColumnName("service_type_id");
            b.Property(x => x.PetId).HasColumnName("pet_id");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.SlotStart).HasColumnName("slot_start");
            b.Property(x => x.DurationMinutes).HasColumnName("duration_minutes");
            b.Property(x => x.SnapshotPrice).HasColumnName("snapshot_price").HasColumnType("numeric(10,2)");
            b.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(10,2)");
            b.Property(x => x.ServiceAddress).HasColumnName("service_address");
            b.Property(x => x.ServiceLatitude).HasColumnName("service_latitude");
            b.Property(x => x.ServiceLongitude).HasColumnName("service_longitude");
            b.Property(x => x.SpecialInstructions).HasColumnName("special_instructions");
            b.Property(x => x.HomeServiceSessionId).HasColumnName("home_service_session_id");
            b.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
            b.Property(x => x.RejectedAt).HasColumnName("rejected_at");
            b.Property(x => x.CancelledAt).HasColumnName("cancelled_at");
            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.CompletedAt).HasColumnName("completed_at");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        builder.Entity<HomeServiceSession>(b =>
        {
            b.ToTable("home_service_sessions");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.ProviderId).HasColumnName("provider_id");
            b.Property(s => s.BookingId).HasColumnName("booking_id");
            b.Property(s => s.Status).HasColumnName("status");
            b.Property(s => s.StartedAt).HasColumnName("started_at");
            b.Property(s => s.EndedAt).HasColumnName("ended_at");
            b.Property(s => s.TotalDistanceMeters).HasColumnName("total_distance_meters");
            b.Property(s => s.TotalDurationSeconds).HasColumnName("total_duration_seconds");
        });

        builder.Entity<HomeServiceSessionEvent>(b =>
        {
            b.ToTable("home_service_session_events");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.SessionId).HasColumnName("session_id");
            b.Property(e => e.EventType).HasColumnName("event_type");
            b.Property(e => e.Description).HasColumnName("description");
            b.Property(e => e.Latitude).HasColumnName("latitude").HasColumnType("numeric(10,8)");
            b.Property(e => e.Longitude).HasColumnName("longitude").HasColumnType("numeric(11,8)");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        builder.Entity<FavoriteHomeProvider>(b =>
        {
            b.ToTable("favorite_home_providers");
            b.HasKey(f => new { f.UserId, f.ProviderId });
            b.Property(f => f.UserId).HasColumnName("user_id");
            b.Property(f => f.ProviderId).HasColumnName("provider_id");
            b.Property(f => f.CreatedAt).HasColumnName("created_at");
        });
    }
}