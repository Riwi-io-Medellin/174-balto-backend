using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using BackEndPets.Infrastructure.Repositories;
using BackEndPets.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace BackEndPets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=pets_db;Username=postgres_pets;Password=TU_PASSWORD";

        services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton(typeof(ICrudRepository<>), typeof(InMemoryCrudRepository<>));

        return services;
    }
}
