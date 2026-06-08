using BackEndPets.Application.Interfaces;
using BackEndPets.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BackEndPets.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
