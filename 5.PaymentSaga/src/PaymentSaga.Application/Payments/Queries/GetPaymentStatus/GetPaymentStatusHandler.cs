namespace PaymentSaga.Application.Payments.Queries.GetPaymentStatus;

using PaymentSaga.Application.Ports;
using Utilities.CQRS;
using Utilities.Logging.Static;
using Utilities.Results;

public sealed class GetPaymentStatusHandler(IPaymentRepository repository)
    : IQueryHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>
{
    public async Task<Result<PaymentStatusDto>> Handle(GetPaymentStatusQuery query, CancellationToken ct)
    {
        if (query.CorrelationId == Guid.Empty)
            return Result<PaymentStatusDto>.Failure("CorrelationId is required.");

        try
        {
            var payment = await repository.GetByCorrelationIdAsync(query.CorrelationId, ct);
            if (payment is null)
                return Result<PaymentStatusDto>.NotFound($"Payment {query.CorrelationId} not found.");

            var dto = new PaymentStatusDto(
                payment.Id,
                payment.CorrelationId,
                payment.Status.ToString(),
                payment.Amount.Amount,
                payment.Amount.Currency,
                payment.FailureReason,
                payment.CreatedAt,
                payment.UpdatedAt);

            return Result<PaymentStatusDto>.Success(dto);
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { query.CorrelationId });
            return Result<PaymentStatusDto>.Failure(ex);
        }
    }
}
