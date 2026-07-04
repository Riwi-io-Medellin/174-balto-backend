using BackEndPets.Domain.Entities;

namespace BackEndPets.Application.Interfaces;

public interface IPetClinicalTipsService
{
    /// <summary>
    /// Generates personalized tips/recommendations from the pet's current
    /// clinical state. Called only when the clinical history changes.
    /// </summary>
    Task<IReadOnlyCollection<PetClinicalTip>> GenerateAsync(
        Pet pet, PetClinicalRecord record, IReadOnlyCollection<PetClinicalEvent> events);
}
