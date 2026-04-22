namespace PaymentSaga.Application.Ports;

using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.Enums;

public interface IPaymentRepository
{
    Task<Payment?> GetByCorrelationIdAsync(Guid correlationId, CancellationToken ct);
    Task AddAsync(Payment payment, CancellationToken ct);
    Task UpdateStatusAsync(Guid correlationId, PaymentStatus status, string? reason, CancellationToken ct);
}
