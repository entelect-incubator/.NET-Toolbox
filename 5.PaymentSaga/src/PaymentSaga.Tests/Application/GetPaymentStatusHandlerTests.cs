namespace PaymentSaga.Tests.Application;

using FluentAssertions;
using NSubstitute;
using PaymentSaga.Application.Payments.Queries.GetPaymentStatus;
using PaymentSaga.Application.Ports;
using PaymentSaga.Domain.Enums;
using PaymentSaga.Tests.Builders;
using Utilities.Enums;

[TestFixture]
internal sealed class GetPaymentStatusHandlerTests
{
    private IPaymentRepository _repository = null!;
    private GetPaymentStatusHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = Substitute.For<IPaymentRepository>();
        _handler = new GetPaymentStatusHandler(_repository);
    }

    [Test]
    public async Task Handle_WhenPaymentExists_ReturnsMappedDto()
    {
        var payment = Fakers.ValidPayment();
        var correlationId = Guid.Parse(payment.CorrelationId);
        _repository.GetByCorrelationIdAsync(correlationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<global::PaymentSaga.Domain.Entities.Payment?>(payment));

        var result = await _handler.Handle(new GetPaymentStatusQuery(correlationId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.CorrelationId.Should().Be(payment.CorrelationId);
        result.Data.Status.Should().Be(PaymentStatus.Initiated.ToString());
        result.Data.Amount.Should().Be(payment.Amount.Amount);
        result.Data.Currency.Should().Be(payment.Amount.Currency);
    }

    [Test]
    public async Task Handle_WhenPaymentNotFound_ReturnsNotFound()
    {
        var correlationId = Guid.NewGuid();
        _repository.GetByCorrelationIdAsync(correlationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<global::PaymentSaga.Domain.Entities.Payment?>(null));

        var result = await _handler.Handle(new GetPaymentStatusQuery(correlationId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.NotFound);
    }

    [Test]
    public async Task Handle_WhenCorrelationIdIsEmpty_ReturnsFailure()
    {
        var result = await _handler.Handle(new GetPaymentStatusQuery(Guid.Empty), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
        await _repository.DidNotReceive().GetByCorrelationIdAsync(
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenRepositoryThrows_ReturnsFailure()
    {
        var correlationId = Guid.NewGuid();
        _repository.GetByCorrelationIdAsync(correlationId, Arg.Any<CancellationToken>())
            .Returns<Task<global::PaymentSaga.Domain.Entities.Payment?>>(_ => throw new InvalidOperationException("DB down"));

        var result = await _handler.Handle(new GetPaymentStatusQuery(correlationId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
    }
}
