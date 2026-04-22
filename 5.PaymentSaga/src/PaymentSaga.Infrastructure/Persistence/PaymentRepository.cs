namespace PaymentSaga.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using PaymentSaga.Application.Ports;
using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.Enums;

public sealed class PaymentRepository(PaymentDbContext db) : IPaymentRepository
{
    public async Task<Payment?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken ct)
        => await db.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.CorrelationId == correlationId.ToString(), ct);

    public async Task AddAsync(Payment payment, CancellationToken ct)
    {
        await db.Payments.AddAsync(payment, ct);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateStatusAsync(Guid correlationId, PaymentStatus status, string? reason, CancellationToken ct)
    {
        var payment = await db.Payments
            .FirstOrDefaultAsync(p => p.CorrelationId == correlationId.ToString(), ct);

        if (payment is null) return;

        payment.Transition(status, reason);
        await db.SaveChangesAsync(ct);
    }
}
