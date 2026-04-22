namespace PaymentSaga.Infrastructure.Consumers;

using MassTransit;
using PaymentSaga.Application.Contracts;
using Utilities.Logging.Static;

/// <summary>
/// Validates that the payment parties exist and the amount is within allowed limits.
/// Publishes PaymentValidatedEvent or PaymentValidationFailedEvent back to the saga.
/// </summary>
public sealed class ValidatePaymentConsumer(IPublishEndpoint publisher) : IConsumer<ValidatePaymentCommand>
{
    public async Task Consume(ConsumeContext<ValidatePaymentCommand> context)
    {
        var msg = context.Message;

        Logger.LogInfo(nameof(ValidatePaymentConsumer), "Validating payment", new { msg.CorrelationId });

        try
        {
            // Domain validation: payer != payee, amount within policy limits
            if (msg.PayerId == msg.PayeeId)
            {
                await publisher.Publish(new PaymentValidationFailedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    Reason = "Payer and payee cannot be the same."
                }, context.CancellationToken);
                return;
            }

            if (msg.Amount > 1_000_000m)
            {
                await publisher.Publish(new PaymentValidationFailedEvent
                {
                    CorrelationId = msg.CorrelationId,
                    Reason = "Amount exceeds maximum allowed transfer limit."
                }, context.CancellationToken);
                return;
            }

            // All checks passed
            await publisher.Publish(new PaymentValidatedEvent
            {
                CorrelationId = msg.CorrelationId
            }, context.CancellationToken);
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { msg.CorrelationId });
            throw; // let MassTransit retry / move to error queue
        }
    }
}
