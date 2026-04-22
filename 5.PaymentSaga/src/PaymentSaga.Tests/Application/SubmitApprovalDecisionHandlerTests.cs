namespace PaymentSaga.Tests.Application;

using FluentAssertions;
using MassTransit;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using PaymentSaga.Application.Contracts;
using PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;
using PaymentSaga.Tests.Builders;
using Utilities.Enums;

[TestFixture]
internal sealed class SubmitApprovalDecisionHandlerTests
{
    private IPublishEndpoint _publisher = null!;
    private SubmitApprovalDecisionHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _publisher = Substitute.For<IPublishEndpoint>();
        _handler = new SubmitApprovalDecisionHandler(_publisher);
    }

    [Test]
    public async Task Handle_ApprovedDecision_ReturnsSuccess()
    {
        var command = Fakers.ApprovalDecisionCommand.Generate() with { IsApproved = true };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Handle_RejectedDecision_ReturnsSuccess()
    {
        var command = Fakers.ApprovalDecisionCommand.Generate() with
        {
            IsApproved = false,
            Reason = "Fraud risk too high."
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Handle_ApprovedDecision_PublishesCorrectMessage()
    {
        var command = Fakers.ApprovalDecisionCommand.Generate() with { IsApproved = true };

        await _handler.Handle(command, CancellationToken.None);

        await _publisher.Received(1).Publish(
            Arg.Is<global::PaymentSaga.Application.Contracts.SubmitApprovalDecisionCommand>(m =>
                m.CorrelationId == command.CorrelationId &&
                m.IsApproved == true),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenCorrelationIdIsEmpty_ReturnsFailure()
    {
        var command = Fakers.ApprovalDecisionCommand.Generate() with { CorrelationId = Guid.Empty };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
        await _publisher.DidNotReceive().Publish(
            Arg.Any<global::PaymentSaga.Application.Contracts.SubmitApprovalDecisionCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Handle_WhenPublisherThrows_ReturnsFailure()
    {
        var command = Fakers.ApprovalDecisionCommand.Generate();
        _publisher.Publish(Arg.Any<global::PaymentSaga.Application.Contracts.SubmitApprovalDecisionCommand>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Bus offline"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorResult.Should().Be(ErrorResults.GeneralError);
    }
}
