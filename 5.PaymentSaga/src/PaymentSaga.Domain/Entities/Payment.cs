namespace PaymentSaga.Domain.Entities;

using PaymentSaga.Domain.Enums;
using PaymentSaga.Domain.ValueObjects;

public sealed class Payment
{
    private Payment() { }

    public Guid Id { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public string PayerId { get; private set; } = string.Empty;
    public string PayeeId { get; private set; } = string.Empty;
    public Money Amount { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string? FailureReason { get; private set; }

    public static Payment Create(
        string correlationId,
        string payerId,
        string payeeId,
        Money amount,
        string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(payeeId);
        ArgumentNullException.ThrowIfNull(amount);

        return new Payment
        {
            Id = Guid.NewGuid(),
            CorrelationId = correlationId,
            PayerId = payerId,
            PayeeId = payeeId,
            Amount = amount,
            Description = description,
            Status = PaymentStatus.Initiated,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Transition(PaymentStatus newStatus, string? reason = null)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus is PaymentStatus.Failed or PaymentStatus.Rejected or PaymentStatus.Cancelled)
            FailureReason = reason;
    }
}
