namespace PaymentSaga.Application.Payments.Queries.GetPaymentStatus;

using Utilities.CQRS;
using Utilities.Results;

public record PaymentStatusDto(
    Guid Id,
    string CorrelationId,
    string Status,
    decimal Amount,
    string Currency,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record GetPaymentStatusQuery(Guid CorrelationId) : IQuery<Result<PaymentStatusDto>>;
