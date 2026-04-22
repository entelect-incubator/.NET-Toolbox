namespace PaymentSaga.Tests.Application;

using FluentAssertions;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentSaga.Application.Contracts;
using PaymentSaga.Application.Payments.Commands.InitiatePayment;
using PaymentSaga.Application.Ports;
using PaymentSaga.Tests.Builders;
using Utilities.Enums;

[TestFixture]
internal sealed class InitiatePaymentHandlerTests
{
    private IPublishEndpoint _publisher = null!;
    private IPaymentRepository _repository = null!;
    private InitiatePaymentValidator _validator = null!;
    private InitiatePaymentHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _publisher = Substitute.For<IPublishEndpoint>();
        _repository = Substitute.For<IPaymentRepository>();
        _validator = new InitiatePaymentValidator();
        _handler = new InitiatePaymentHandler(_publisher, _repository, _validator);
    }

    [Test]
    public async Task Handle_ValidCommand_ReturnsSuccessWithCorrelationId()
    {
        var command = Fakers.InitiatePaymentCommand.Generate();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
    }

    [Test]
    public async Task Handle_ValidCommand_PersistsPayment()
    {
        var command = Fakers.InitiatePaymentCommand.Generate();

        await _handler.Handle(command, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<global::PaymentSaga.Domain.Entities.Payment>(p =>
                p.PayerId == command.PayerId &&
                p.PayeeId == command.PayeeId),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_ValidCommand_PublishesInitiateCommand()
    {
        var command = Fakers.InitiatePaymentCommand.Generate();

        await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<InitiatePaymentSagaCommand>(m =>
                m.PayerId == command.PayerId &&
                m.Amount == command.Amount),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenPayerIdIsEmpty_ReturnsValidationFailure()
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { PayerId = string.Empty };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.ValidationError);
        result.ValidationErrors.Should().ContainKey(nameof(command.PayerId));
    }

    [Test]
    public async Task Handle_WhenPayerEqualsPayee_ReturnsValidationFailure()
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { PayeeId = "same-id", PayerId = "same-id" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.ValidationError);
    }

    [Test]
    public async Task Handle_WhenAmountIsZero_ReturnsValidationFailure()
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { Amount = 0m };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.ValidationError);
        result.ValidationErrors.Should().ContainKey(nameof(command.Amount));
    }

    [Test]
    public async Task Handle_WhenAmountIsNegative_ReturnsValidationFailure()
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { Amount = -1m };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.ValidationError);
    }

    [Test]
    public async Task Handle_WhenCurrencyIsInvalid_ReturnsValidationFailure()
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { Currency = "XX" };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.ValidationError);
        result.ValidationErrors.Should().ContainKey(nameof(command.Currency));
    }

    [Test]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailureResult()
    {
        var command = Fakers.InitiatePaymentCommand.Generate();
        _repository.AddAsync(Arg.Any<global::PaymentSaga.Domain.Entities.Payment>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("DB unavailable"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
        result.Errors.Should().Contain(e => e.Contains("DB unavailable"));
    }

    [Test]
    public async Task Handle_WhenPublisherThrows_ReturnsFailureResult()
    {
        var command = Fakers.InitiatePaymentCommand.Generate();
        _publisher.Publish(Arg.Any<InitiatePaymentSagaCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Bus unavailable"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
    }

    [Test]
    [TestCase(1)]
    [TestCase(50_000)]
    [TestCase(999_999.99)]
    public async Task Handle_WithVariousValidAmounts_Succeeds(decimal amount)
    {
        var command = Fakers.InitiatePaymentCommand.Generate() with { Amount = amount };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
