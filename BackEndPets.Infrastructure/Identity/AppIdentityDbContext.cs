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
            b.Property(w => w.CreatedAt).HasColumnName("created_at");
            b.Property(w => w.UpdatedAt).HasColumnName("updated_at");
            b.HasIndex(w => w.UserId).IsUnique();
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
            b.Property(biz => biz.Phone).HasColumnName("phone");
            b.Property(biz => biz.Type).HasColumnName("type");
            b.Property(biz => biz.VerificationStatus).HasColumnName("verification_status");
            b.Property(biz => biz.CreatedAt).HasColumnName("created_at");
            b.HasIndex(biz => biz.Nit).IsUnique();
            b.HasIndex(biz => biz.Email).IsUnique();
        });
    }
}