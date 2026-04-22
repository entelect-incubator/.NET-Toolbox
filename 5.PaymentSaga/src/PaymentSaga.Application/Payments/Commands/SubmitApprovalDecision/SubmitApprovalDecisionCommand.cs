namespace PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;

using Utilities.CQRS;
using Utilities.Results;

public record SubmitApprovalDecisionCommand : ICommand<Result>
{
    public Guid CorrelationId { get; init; }
    public bool IsApproved { get; init; }
    public string? Reason { get; init; }
}
