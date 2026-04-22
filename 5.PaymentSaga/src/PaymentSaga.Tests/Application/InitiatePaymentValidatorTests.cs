namespace PaymentSaga.Tests.Application;

using FluentAssertions;
using PaymentSaga.Application.Payments.Commands.InitiatePayment;

[TestFixture]
internal sealed class InitiatePaymentValidatorTests
{
    private InitiatePaymentValidator _validator = null!;

    [SetUp]
    public void SetUp() => _validator = new InitiatePaymentValidator();

    [Test]
    public async Task Validate_ValidCommand_PassesWithNoErrors()
    {
        var command = new InitiatePaymentCommand
        {
            PayerId = "payer-abc",
            PayeeId = "payee-xyz",
            Amount = 250m,
            Currency = "ZAR",
            Description = "Valid payment"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Test]
    [TestCase("", "payee-1", 100, "USD", nameof(InitiatePaymentCommand.PayerId))]
    [TestCase("payer-1", "", 100, "USD", nameof(InitiatePaymentCommand.PayeeId))]
    [TestCase("payer-1", "payee-1", 0, "USD", nameof(InitiatePaymentCommand.Amount))]
    [TestCase("payer-1", "payee-1", -5, "USD", nameof(InitiatePaymentCommand.Amount))]
    [TestCase("payer-1", "payee-1", 100, "XX", nameof(InitiatePaymentCommand.Currency))]
    [TestCase("payer-1", "payee-1", 100, "", nameof(InitiatePaymentCommand.Currency))]
    public async Task Validate_InvalidField_FailsWithExpectedError(
        string payerId, string payeeId, decimal amount, string currency, string expectedField)
    {
        var command = new InitiatePaymentCommand
        {
            PayerId = payerId,
            PayeeId = payeeId,
            Amount = amount,
            Currency = currency,
            Description = "test"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedField);
    }

    [Test]
    public async Task Validate_WhenPayerEqualsPayee_FailsOnPayeeId()
    {
        var command = new InitiatePaymentCommand
        {
            PayerId = "same",
            PayeeId = "same",
            Amount = 100m,
            Currency = "EUR"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.PayeeId));
    }

    [Test]
    public async Task Validate_WhenDescriptionExceedsMaxLength_Fails()
    {
        var command = new InitiatePaymentCommand
        {
            PayerId = "payer-1",
            PayeeId = "payee-1",
            Amount = 100m,
            Currency = "USD",
            Description = new string('x', 501)
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }
}
