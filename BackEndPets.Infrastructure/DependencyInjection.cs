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
        services.AddScoped<IPetClinicalRepository, PetClinicalRepository>();
        services.AddScoped<IPetClinicalRecordService, PetClinicalRecordService>();
        services.AddScoped<IPetClinicalExtractionService, OpenAIPetClinicalExtractionService>();
        services.AddScoped<IPetClinicalDocumentGenerationService, QuestPdfClinicalDocumentGenerationService>();
        services.AddScoped<IPetClinicalTipsService, OpenAIPetClinicalTipsService>();
        services.AddScoped<IDocumentVerificationService, OpenAIDocumentVerificationService>();
        services.AddScoped<IVetDocumentAiClient, GeminiVetDocumentAiClient>();
        services.AddScoped<IVetDocumentAiClient, OpenRouterVetDocumentAiClient>();
        services.AddScoped<IVetDocumentAttachmentFetcher, HttpVetDocumentAttachmentFetcher>();
        services.AddScoped<IVetDocumentAnalysisService, VetDocumentAnalysisService>();
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

        services.AddScoped<IHomeServiceProviderRepository, HomeServiceProviderRepository>();
        services.AddScoped<IHomeServiceTypeRepository, HomeServiceTypeRepository>();
        services.AddScoped<IHomeServiceProviderService, HomeServiceProviderService>();
        services.AddScoped<IHomeServiceTypeService, HomeServiceTypeService>();
        services.AddScoped<IHomeProviderDocumentRepository, HomeProviderDocumentRepository>();
        services.AddScoped<IHomeProviderGalleryRepository, HomeProviderGalleryRepository>();
        services.AddScoped<IHomeProviderCertificationRepository, HomeProviderCertificationRepository>();
        services.AddScoped<IHomeProviderSpecialtyRepository, HomeProviderSpecialtyRepository>();
        services.AddScoped<IHomeProviderAssetsService, HomeProviderAssetsService>();
        services.AddScoped<IHomeServiceApplicationService, HomeServiceApplicationService>();
        services.AddScoped<IHomeProviderServiceRepository, HomeProviderServiceRepository>();
        services.AddScoped<IHomeProviderServicesService, HomeProviderServicesService>();
        services.AddScoped<IHomeProviderServiceAreaRepository, HomeProviderServiceAreaRepository>();
        services.AddScoped<IHomeProviderServiceAreaService, HomeProviderServiceAreaService>();
        services.AddScoped<IHomeProviderAvailabilityRepository, HomeProviderAvailabilityRepository>();
        services.AddScoped<IHomeProviderAvailabilityExceptionRepository, HomeProviderAvailabilityExceptionRepository>();
        services.AddScoped<IHomeServiceAvailabilityEngine, HomeServiceAvailabilityEngine>();
        services.AddScoped<IHomeProviderAvailabilityService, HomeProviderAvailabilityService>();
        services.AddScoped<IHomeServiceBookingRepository, HomeServiceBookingRepository>();
        services.AddScoped<IHomeServiceBookingService, HomeServiceBookingService>();
        services.AddScoped<IHomeServiceMarketplaceService, HomeServiceMarketplaceService>();
        services.AddScoped<IHomeServiceSessionRepository, HomeServiceSessionRepository>();
        services.AddScoped<IHomeServiceSessionEventRepository, HomeServiceSessionEventRepository>();
        services.AddScoped<IHomeServiceSessionService, HomeServiceSessionService>();
        services.AddScoped<IHomeServiceSessionAuthorizationService, HomeServiceSessionAuthorizationService>();
        services.AddScoped<IFavoriteHomeProviderRepository, FavoriteHomeProviderRepository>();
        services.AddScoped<IFavoriteHomeProviderService, FavoriteHomeProviderService>();
        services.AddScoped<IBusinessHourRepository, BusinessHourRepository>();
        services.AddScoped<IBusinessHourExceptionRepository, BusinessHourExceptionRepository>();
        services.AddScoped<IBusinessHourService, BusinessHourService>();

        services.AddSingleton(typeof(ICrudRepository<>), typeof(InMemoryCrudRepository<>));

        return services;
    }
}
