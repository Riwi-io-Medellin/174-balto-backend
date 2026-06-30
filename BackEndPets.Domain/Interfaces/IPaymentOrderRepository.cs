using BackEndPets.Domain.Entities;

namespace BackEndPets.Domain.Interfaces;

public interface IPaymentOrderRepository
{
    Task<PaymentOrder?> GetByIdAsync(Guid id);
    Task<PaymentOrder?> GetByBookingIdAsync(Guid bookingId);
    Task<PaymentOrder?> GetByProviderReferenceAsync(string providerReference);
    Task<PaymentOrder> CreateAsync(PaymentOrder order);
    Task<PaymentOrder> UpdateAsync(PaymentOrder order);
}
