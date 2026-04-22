namespace PaymentSaga.Application.Payments.Commands.InitiatePayment;

using MassTransit;
using PaymentSaga.Application.Contracts;
using PaymentSaga.Application.Ports;
using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.ValueObjects;
using Utilities.CQRS;
using Utilities.Logging.Static;
using Utilities.Results;

public sealed class InitiatePaymentHandler(
    IPublishEndpoint publisher,
    IPaymentRepository repository,
    InitiatePaymentValidator validator)
    : ICommandHandler<InitiatePaymentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(InitiatePaymentCommand command, CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(command, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

            return Result<Guid>.ValidationFailure(errors);
        }

        var correlationId = Guid.NewGuid();

        try
        {
            var payment = Payment.Create(
                correlationId.ToString(),
                command.PayerId,
                command.PayeeId,
                new Money(command.Amount, command.Currency),
                command.Description);

            await repository.AddAsync(payment, ct);

            await publisher.Publish(new InitiatePaymentSagaCommand
            {
                CorrelationId = correlationId,
                PayerId = command.PayerId,
                PayeeId = command.PayeeId,
                Amount = command.Amount,
                Currency = command.Currency,
                Description = command.Description
            }, ct);

            Logger.LogInfo(nameof(InitiatePaymentHandler), "Payment saga initiated", new { correlationId });

            return Result<Guid>.Success(correlationId);
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { correlationId });
            return Result<Guid>.Failure(ex);
        }
    }
}
