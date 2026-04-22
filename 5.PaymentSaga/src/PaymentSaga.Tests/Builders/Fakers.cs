namespace PaymentSaga.Tests.Builders;

using Bogus;
using PaymentSaga.Application.Payments.Commands.InitiatePayment;
using PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;
using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.ValueObjects;

/// <summary>
/// Centralised Bogus fakers — single source of truth for test data.
/// </summary>
internal static class Fakers
{
    private static readonly Faker F = new("en");

    internal static Money ValidMoney(decimal? amount = null, string? currency = null)
        => new(amount ?? F.Finance.Amount(1, 50_000), currency ?? F.PickRandom("ZAR", "USD", "EUR", "GBP"));

    internal static Payment ValidPayment(Money? money = null)
        => Payment.Create(
            Guid.NewGuid().ToString(),
            F.Random.AlphaNumeric(10),
            F.Random.AlphaNumeric(11),  // different to payer by construction
            money ?? ValidMoney(),
            F.Lorem.Sentence(5));

    internal static readonly Faker<InitiatePaymentCommand> InitiatePaymentCommand =
        new Faker<InitiatePaymentCommand>()
            .CustomInstantiator(f => new InitiatePaymentCommand
            {
                PayerId = f.Random.AlphaNumeric(10),
                PayeeId = f.Random.AlphaNumeric(11),
                Amount = f.Finance.Amount(1, 50_000),
                Currency = f.PickRandom("ZAR", "USD", "EUR"),
                Description = f.Lorem.Sentence(5)
            });

    internal static readonly Faker<SubmitApprovalDecisionCommand> ApprovalDecisionCommand =
        new Faker<SubmitApprovalDecisionCommand>()
            .CustomInstantiator(f => new SubmitApprovalDecisionCommand
            {
                CorrelationId = Guid.NewGuid(),
                IsApproved = f.Random.Bool(),
                Reason = f.Random.Bool() ? f.Lorem.Sentence(3) : null
            });
}
