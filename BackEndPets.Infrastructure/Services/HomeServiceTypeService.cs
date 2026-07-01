using BackEndPets.Application.DTOs.HomeServices;
using BackEndPets.Application.Interfaces;
using BackEndPets.Domain.Interfaces;

namespace BackEndPets.Infrastructure.Services;

public sealed class HomeServiceTypeService(IHomeServiceTypeRepository typeRepository) : IHomeServiceTypeService
{
    public async Task<IReadOnlyCollection<HomeServiceTypeResponse>> GetAllAsync() =>
        (await typeRepository.GetAllActiveAsync())
            .Select(t => new HomeServiceTypeResponse(t.Id, t.Code, t.Name, t.Description))
            .ToList();
}
