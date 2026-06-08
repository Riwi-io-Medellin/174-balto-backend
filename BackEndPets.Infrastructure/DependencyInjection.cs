using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BackEndPets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddSingleton(typeof(ICrudRepository<>), typeof(InMemoryCrudRepository<>));

        return services;
    }
}
