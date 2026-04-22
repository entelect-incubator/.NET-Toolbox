namespace PaymentSaga.Application.Payments.Commands.InitiatePayment;

using Utilities.CQRS;
using Utilities.Results;

public record InitiatePaymentCommand : ICommand<Result<Guid>>
{
    public string PayerId { get; init; } = string.Empty;
    public string PayeeId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
