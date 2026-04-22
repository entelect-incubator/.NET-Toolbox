namespace PaymentSaga.Infrastructure.Consumers;

using MassTransit;
using PaymentSaga.Application.Contracts;
using Utilities.Logging.Static;

/// <summary>
/// Finalises the payment in the ledger / accounting system.
/// Publishes PaymentSettledEvent back to the saga.
/// </summary>
public sealed class SettlePaymentConsumer(IPublishEndpoint publisher) : IConsumer<SettlePaymentCommand>
{
    public async Task Consume(ConsumeContext<SettlePaymentCommand> context)
    {
        var msg = context.Message;

        Logger.LogInfo(nameof(SettlePaymentConsumer), "Settling payment", new { msg.CorrelationId });

        try
        {
            // TODO: inject and call real settlement / ledger service here
            await publisher.Publish(new PaymentSettledEvent
            {
                CorrelationId = msg.CorrelationId
            }, context.CancellationToken);

            Logger.LogInfo(nameof(SettlePaymentConsumer), "Payment settled", new { msg.CorrelationId });
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { msg.CorrelationId });
            throw; // let MassTransit retry / move to error queue
        }
    }
}
