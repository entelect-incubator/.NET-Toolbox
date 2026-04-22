namespace PaymentSaga.Application.Contracts;

// ─── Saga-triggering commands (published by API → MassTransit) ──────────────

public record InitiatePaymentSagaCommand
{
    public Guid CorrelationId { get; init; }
    public string PayerId { get; init; } = string.Empty;
    public string PayeeId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public record SubmitApprovalDecisionCommand
{
    public Guid CorrelationId { get; init; }
    public bool IsApproved { get; init; }
    public string? Reason { get; init; }
}

// ─── Internal saga commands (published by state machine → consumers) ─────────

public record ValidatePaymentCommand
{
    public Guid CorrelationId { get; init; }
    public string PayerId { get; init; } = string.Empty;
    public string PayeeId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record ProcessPaymentCommand
{
    public Guid CorrelationId { get; init; }
    public string PayerId { get; init; } = string.Empty;
    public string PayeeId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record SettlePaymentCommand
{
    public Guid CorrelationId { get; init; }
    public string ExternalTransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

// ─── Internal saga events (published by consumers → state machine) ──────────

public record PaymentValidatedEvent
{
    public Guid CorrelationId { get; init; }
}

public record PaymentValidationFailedEvent
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record PaymentProcessedEvent
{
    public Guid CorrelationId { get; init; }
    public string ExternalTransactionId { get; init; } = string.Empty;
}

public record PaymentProcessingFailedEvent
{
    public Guid CorrelationId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record PaymentSettledEvent
{
    public Guid CorrelationId { get; init; }
}
