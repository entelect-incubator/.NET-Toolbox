namespace PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;

using MassTransit;
using PaymentSaga.Application.Contracts;
using Utilities.CQRS;
using Utilities.Logging.Static;
using Utilities.Results;

public sealed class SubmitApprovalDecisionHandler(IPublishEndpoint publisher)
    : ICommandHandler<SubmitApprovalDecisionCommand, Result>
{
    public async Task<Result> Handle(SubmitApprovalDecisionCommand command, CancellationToken ct)
    {
        if (command.CorrelationId == Guid.Empty)
            return Result.Failure("CorrelationId is required.");

        try
        {
            await publisher.Publish(new Contracts.SubmitApprovalDecisionCommand
            {
                CorrelationId = command.CorrelationId,
                IsApproved = command.IsApproved,
                Reason = command.Reason
            }, ct);

            Logger.LogInfo(
                nameof(SubmitApprovalDecisionHandler),
                "Approval decision submitted",
                new { command.CorrelationId, command.IsApproved });

            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, new { command.CorrelationId });
            return Result.Failure(ex);
        }
    }
}
