using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using BackEndPets.Infrastructure.Repositories;
using BackEndPets.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IWalkerRepository, WalkerRepository>();
        services.AddScoped<IBusinessRepository, BusinessRepository>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IPetWalkingHistoryRepository, PetWalkingHistoryRepository>();
        services.AddScoped<IWalkSessionRepository, WalkSessionRepository>();
        services.AddScoped<IWalkRoutePointRepository, WalkRoutePointRepository>();
        services.AddScoped<IWalkSessionService, WalkSessionService>();
        services.AddScoped<IWalkSessionAuthorizationService, WalkSessionAuthorizationService>();
        services.AddScoped<IPetRepository, PetRepository>();
        services.AddScoped<IPetService, PetService>();
        services.AddScoped<IWalkingHistoryService, WalkingHistoryService>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IWalkerGalleryRepository, WalkerGalleryRepository>();
        services.AddScoped<IWalkerDocumentRepository, WalkerDocumentRepository>();
        services.AddScoped<IWalkerAssetsService, WalkerAssetsService>();
        services.AddScoped<IBusinessDocumentRepository, BusinessDocumentRepository>();
        services.AddScoped<IBusinessAssetsService, BusinessAssetsService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IBusinessServiceRepository, BusinessServiceRepository>();
        services.AddScoped<IBusinessServicesService, BusinessServicesService>();
        services.AddScoped<IPetHistoryRepository, PetHistoryRepository>();
        services.AddScoped<IPetHistoryService, PetHistoryService>();
        services.AddScoped<IDocumentVerificationService, OpenAIDocumentVerificationService>();
        services.AddScoped<IWalkerApplicationService, WalkerApplicationService>();
        services.AddScoped<IWalkerAvailabilityRepository, WalkerAvailabilityRepository>();
        services.AddScoped<IWalkerAvailabilityExceptionRepository, WalkerAvailabilityExceptionRepository>();
        services.AddScoped<IAvailabilityEngine, AvailabilityEngine>();
        services.AddScoped<IWalkerAvailabilityService, WalkerAvailabilityService>();
        services.AddScoped<IWalkBookingRepository, WalkBookingRepository>();
        services.AddScoped<IWalkBookingService, WalkBookingService>();
        services.AddScoped<IWalkerMarketplaceService, WalkerMarketplaceService>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IAdminVerificationService, AdminVerificationService>();
        
        services.AddSingleton(typeof(ICrudRepository<>), typeof(InMemoryCrudRepository<>));

        return services;
    }
}
