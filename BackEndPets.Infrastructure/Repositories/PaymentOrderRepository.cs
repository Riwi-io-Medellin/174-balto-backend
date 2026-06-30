using BackEndPets.Domain.Entities;
using BackEndPets.Domain.Interfaces;
using BackEndPets.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace BackEndPets.Infrastructure.Repositories;

public sealed class PaymentOrderRepository(AppIdentityDbContext dbContext) : IPaymentOrderRepository
{
    public Task<PaymentOrder?> GetByIdAsync(Guid id) =>
        dbContext.PaymentOrders.FirstOrDefaultAsync(o => o.Id == id);

    public Task<PaymentOrder?> GetByBookingIdAsync(Guid bookingId) =>
        dbContext.PaymentOrders.FirstOrDefaultAsync(o => o.BookingId == bookingId);

    public Task<PaymentOrder?> GetByProviderReferenceAsync(string providerReference) =>
        dbContext.PaymentOrders.FirstOrDefaultAsync(o => o.ProviderReference == providerReference);

    public async Task<PaymentOrder> CreateAsync(PaymentOrder order)
    {
        order.Id        = Guid.NewGuid();
        order.CreatedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;
        dbContext.PaymentOrders.Add(order);
        await dbContext.SaveChangesAsync();
        return order;
    }

    public async Task<PaymentOrder> UpdateAsync(PaymentOrder order)
    {
        order.UpdatedAt = DateTime.UtcNow;
        dbContext.PaymentOrders.Update(order);
        await dbContext.SaveChangesAsync();
        return order;
    }
}
