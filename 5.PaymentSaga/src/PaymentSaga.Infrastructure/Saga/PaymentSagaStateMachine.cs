namespace PaymentSaga.Infrastructure.Saga;

using MassTransit;
using PaymentSaga.Application.Contracts;
using Utilities.Logging.Static;

public sealed class PaymentSagaStateMachine : MassTransitStateMachine<PaymentSagaState>
{
    // ── States ───────────────────────────────────────────────────────────────
    public State Validating { get; private set; } = null!;
    public State AwaitingApproval { get; private set; } = null!;
    public State Processing { get; private set; } = null!;
    public State Settling { get; private set; } = null!;
    public State Settled { get; private set; } = null!;
    public State Rejected { get; private set; } = null!;
    public State Failed { get; private set; } = null!;
    public State Cancelled { get; private set; } = null!;

    // ── Events ───────────────────────────────────────────────────────────────
    public Event<InitiatePaymentSagaCommand> PaymentInitiated { get; private set; } = null!;
    public Event<SubmitApprovalDecisionCommand> ApprovalDecisionReceived { get; private set; } = null!;
    public Event<PaymentValidatedEvent> PaymentValidated { get; private set; } = null!;
    public Event<PaymentValidationFailedEvent> PaymentValidationFailed { get; private set; } = null!;
    public Event<PaymentProcessedEvent> PaymentProcessed { get; private set; } = null!;
    public Event<PaymentProcessingFailedEvent> PaymentProcessingFailed { get; private set; } = null!;
    public Event<PaymentSettledEvent> PaymentSettled { get; private set; } = null!;

    public PaymentSagaStateMachine()
    {
        InstanceState(x => x.CurrentState);

        // ── Correlations ─────────────────────────────────────────────────────
        Event(() => PaymentInitiated, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => ApprovalDecisionReceived, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentValidated, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentValidationFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentProcessed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentProcessingFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentSettled, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        // ── Transitions ───────────────────────────────────────────────────────

        // Initial → Validating
        Initially(
            When(PaymentInitiated)
                .Then(ctx =>
                {
                    ctx.Saga.PayerId = ctx.Message.PayerId;
                    ctx.Saga.PayeeId = ctx.Message.PayeeId;
                    ctx.Saga.Amount = ctx.Message.Amount;
                    ctx.Saga.Currency = ctx.Message.Currency;
                    ctx.Saga.Description = ctx.Message.Description;
                    ctx.Saga.CreatedAt = DateTimeOffset.UtcNow;
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;

                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment saga started",
                        new { ctx.Saga.CorrelationId });
                })
                .PublishAsync(ctx => ctx.Init<ValidatePaymentCommand>(new ValidatePaymentCommand
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    PayerId = ctx.Saga.PayerId,
                    PayeeId = ctx.Saga.PayeeId,
                    Amount = ctx.Saga.Amount,
                    Currency = ctx.Saga.Currency
                }))
                .TransitionTo(Validating));

        // Validating → AwaitingApproval | Failed
        During(Validating,
            When(PaymentValidated)
                .Then(ctx =>
                {
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment validated — awaiting approval",
                        new { ctx.Saga.CorrelationId });
                })
                .TransitionTo(AwaitingApproval),

            When(PaymentValidationFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment validation failed",
                        new { ctx.Saga.CorrelationId, ctx.Message.Reason });
                })
                .TransitionTo(Failed)
                .Finalize());

        // AwaitingApproval → Processing | Rejected
        // The saga sits here until an external actor calls the approval endpoint.
        During(AwaitingApproval,
            When(ApprovalDecisionReceived, ctx => ctx.Message.IsApproved)
                .Then(ctx =>
                {
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment approved — processing",
                        new { ctx.Saga.CorrelationId });
                })
                .PublishAsync(ctx => ctx.Init<ProcessPaymentCommand>(new ProcessPaymentCommand
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    PayerId = ctx.Saga.PayerId,
                    PayeeId = ctx.Saga.PayeeId,
                    Amount = ctx.Saga.Amount,
                    Currency = ctx.Saga.Currency
                }))
                .TransitionTo(Processing),

            When(ApprovalDecisionReceived, ctx => !ctx.Message.IsApproved)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason ?? "Rejected by approver.";
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment rejected",
                        new { ctx.Saga.CorrelationId, ctx.Saga.FailureReason });
                })
                .TransitionTo(Rejected)
                .Finalize());

        // Processing → Settling | Failed
        During(Processing,
            When(PaymentProcessed)
                .Then(ctx =>
                {
                    ctx.Saga.ExternalTransactionId = ctx.Message.ExternalTransactionId;
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment processed — settling",
                        new { ctx.Saga.CorrelationId, ctx.Message.ExternalTransactionId });
                })
                .PublishAsync(ctx => ctx.Init<SettlePaymentCommand>(new SettlePaymentCommand
                {
                    CorrelationId = ctx.Saga.CorrelationId,
                    ExternalTransactionId = ctx.Saga.ExternalTransactionId!,
                    Amount = ctx.Saga.Amount,
                    Currency = ctx.Saga.Currency
                }))
                .TransitionTo(Settling),

            When(PaymentProcessingFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment processing failed",
                        new { ctx.Saga.CorrelationId, ctx.Message.Reason });
                })
                .TransitionTo(Failed)
                .Finalize());

        // Settling → Settled
        During(Settling,
            When(PaymentSettled)
                .Then(ctx =>
                {
                    ctx.Saga.UpdatedAt = DateTimeOffset.UtcNow;
                    Logger.LogInfo(nameof(PaymentSagaStateMachine), "Payment settled",
                        new { ctx.Saga.CorrelationId });
                })
                .TransitionTo(Settled)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
