namespace PaymentSaga.Api.Endpoints;

using PaymentSaga.Application.Payments.Commands.InitiatePayment;
using PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;
using PaymentSaga.Application.Payments.Queries.GetPaymentStatus;
using Utilities.CQRS;
using Utilities.Enums;
using Utilities.Results;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payments")
            .WithTags("Payments");

        group.MapPost("/", InitiatePayment)
            .WithName("InitiatePayment")
            .WithSummary("Initiate a new payment — starts the approval saga.")
            .Produces<Result<Guid>>(StatusCodes.Status202Accepted)
            .Produces<Result<Guid>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{correlationId:guid}", GetPaymentStatus)
            .WithName("GetPaymentStatus")
            .WithSummary("Query the current state of a payment saga.")
            .Produces<Result<PaymentStatusDto>>(StatusCodes.Status200OK)
            .Produces<Result<PaymentStatusDto>>(StatusCodes.Status404NotFound);

        group.MapPost("/{correlationId:guid}/decision", SubmitApprovalDecision)
            .WithName("SubmitApprovalDecision")
            .WithSummary("Submit an external approval or rejection for a pending payment.")
            .Produces<Result>(StatusCodes.Status200OK)
            .Produces<Result>(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> InitiatePayment(
        InitiatePaymentRequest request,
        Dispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new InitiatePaymentCommand
        {
            PayerId = request.PayerId,
            PayeeId = request.PayeeId,
            Amount = request.Amount,
            Currency = request.Currency,
            Description = request.Description ?? string.Empty
        };

        var result = await dispatcher.Send<InitiatePaymentCommand, Result<Guid>>(command, ct);

        return result.ErrorResult switch
        {
            ErrorResults.None => TypedResults.Accepted($"/api/v1/payments/{result.Data}", result),
            ErrorResults.ValidationError => TypedResults.BadRequest(result),
            _ => TypedResults.BadRequest(result)
        };
    }

    private static async Task<IResult> GetPaymentStatus(
        Guid correlationId,
        Dispatcher dispatcher,
        CancellationToken ct)
    {
        var result = await dispatcher.Query<GetPaymentStatusQuery, Result<PaymentStatusDto>>(
            new GetPaymentStatusQuery(correlationId), ct);

        return result.ErrorResult switch
        {
            ErrorResults.None => TypedResults.Ok(result),
            ErrorResults.NotFound => TypedResults.NotFound(result),
            _ => TypedResults.BadRequest(result)
        };
    }

    private static async Task<IResult> SubmitApprovalDecision(
        Guid correlationId,
        ApprovalDecisionRequest request,
        Dispatcher dispatcher,
        CancellationToken ct)
    {
        var command = new SubmitApprovalDecisionCommand
        {
            CorrelationId = correlationId,
            IsApproved = request.IsApproved,
            Reason = request.Reason
        };

        var result = await dispatcher.Send<SubmitApprovalDecisionCommand, Result>(command, ct);

        return result.ErrorResult switch
        {
            ErrorResults.None => TypedResults.Ok(result),
            _ => TypedResults.BadRequest(result)
        };
    }
}

// ── Request models ────────────────────────────────────────────────────────────

public record InitiatePaymentRequest(
    string PayerId,
    string PayeeId,
    decimal Amount,
    string Currency,
    string? Description);

public record ApprovalDecisionRequest(
    bool IsApproved,
    string? Reason);
