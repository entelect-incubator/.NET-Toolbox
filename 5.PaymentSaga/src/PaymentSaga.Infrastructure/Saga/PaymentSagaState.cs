namespace PaymentSaga.Infrastructure.Saga;

using MassTransit;

public sealed class PaymentSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public string PayerId { get; set; } = string.Empty;
    public string PayeeId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // EF Core concurrency token
    public byte[]? RowVersion { get; set; }
}
