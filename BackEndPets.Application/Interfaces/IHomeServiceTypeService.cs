using BackEndPets.Application.DTOs.HomeServices;

namespace BackEndPets.Application.Interfaces;

public interface IHomeServiceTypeService
{
    Task<IReadOnlyCollection<HomeServiceTypeResponse>> GetAllAsync();
}
