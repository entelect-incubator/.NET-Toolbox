namespace PaymentSaga.Infrastructure.Consumers;

using MassTransit;
using PaymentSaga.Application.Contracts;
using Utilities.Logging.Static;

/// <summary>
/// Executes the actual payment against the payment gateway.
/// Publishes PaymentProcessedEvent or PaymentProcessingFailedEvent back to the saga.
/// </summary>
public sealed class ProcessPaymentConsumer(IPublishEndpoint publisher) : IConsumer<ProcessPaymentCommand>
{
    public async Task Consume(ConsumeContext<ProcessPaymentCommand> context)
    {
        var msg = context.Message;

        Logger.LogInfo(nameof(ProcessPaymentConsumer), "Processing payment", new { msg.CorrelationId });

        try
        {
            // TODO: inject and call real payment gateway client here
            // Simulated external call:
            var externalTransactionId = $"TXN-{Guid.NewGuid():N}";

            await publisher.Publish(new PaymentProcessedEvent
            {
                CorrelationId = msg.CorrelationId,
                ExternalTransactionId = externalTransactionId
            }, context.CancellationToken);

            Logger.LogInfo(nameof(ProcessPaymentConsumer), "Payment processed",
                new { msg.CorrelationId, externalTransactionId });
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { msg.CorrelationId });

            await publisher.Publish(new PaymentProcessingFailedEvent
            {
                CorrelationId = msg.CorrelationId,
                Reason = ex.Message
            }, context.CancellationToken);
        }
    }
}
