namespace PaymentSaga.Tests.Domain;

using FluentAssertions;
using PaymentSaga.Domain.Entities;
using PaymentSaga.Domain.Enums;
using PaymentSaga.Domain.ValueObjects;
using PaymentSaga.Tests.Builders;

[TestFixture]
internal sealed class MoneyTests
{
    [Test]
    public void Create_WithValidArgs_SetsProperties()
    {
        var money = new Money(500.50m, "zar");

        money.Amount.Should().Be(500.50m);
        money.Currency.Should().Be("ZAR"); // always uppercase
    }

    [Test]
    [TestCase(-1)]
    [TestCase(-0.01)]
    public void Create_WithNegativeAmount_Throws(decimal amount)
    {
        var act = () => new Money(amount, "USD");

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Test]
    [TestCase("")]
    [TestCase("  ")]
    [TestCase("US")]       // too short
    [TestCase("USDD")]     // too long
    public void Create_WithInvalidCurrency_Throws(string currency)
    {
        var act = () => new Money(100m, currency);

        act.Should().Throw<ArgumentException>().WithParameterName("currency");
    }

    [Test]
    public void ToString_ReturnsFormattedString()
    {
        var money = new Money(1234.5m, "USD");

        money.ToString().Should().Be("1234.50 USD");
    }

    [Test]
    public void Equality_TwoEqualMonies_AreEqual()
    {
        var a = new Money(100m, "EUR");
        var b = new Money(100m, "EUR");

        a.Should().Be(b);
    }

    [Test]
    public void Equality_DifferentAmounts_AreNotEqual()
    {
        var a = new Money(100m, "EUR");
        var b = new Money(200m, "EUR");

        a.Should().NotBe(b);
    }
}

[TestFixture]
internal sealed class PaymentTests
{
    [Test]
    public void Create_WithValidArgs_ReturnsInitiatedPayment()
    {
        var payment = Fakers.ValidPayment();

        payment.Id.Should().NotBe(Guid.Empty);
        payment.Status.Should().Be(PaymentStatus.Initiated);
        payment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(2));
        payment.FailureReason.Should().BeNull();
    }

    [Test]
    public void Create_WhenPayerIdIsEmpty_Throws()
    {
        var act = () => Payment.Create(
            Guid.NewGuid().ToString(),
            string.Empty,
            "payee-1",
            Fakers.ValidMoney(),
            "desc");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WhenPayeeIdIsEmpty_Throws()
    {
        var act = () => Payment.Create(
            Guid.NewGuid().ToString(),
            "payer-1",
            string.Empty,
            Fakers.ValidMoney(),
            "desc");

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Create_WhenAmountIsNull_Throws()
    {
        var act = () => Payment.Create(
            Guid.NewGuid().ToString(),
            "payer-1",
            "payee-1",
            null!,
            "desc");

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Transition_ToValidState_UpdatesStatusAndTimestamp()
    {
        var payment = Fakers.ValidPayment();
        var before = payment.UpdatedAt;

        payment.Transition(PaymentStatus.Validating);

        payment.Status.Should().Be(PaymentStatus.Validating);
        payment.UpdatedAt.Should().BeOnOrAfter(before);
    }

    [Test]
    [TestCase(PaymentStatus.Failed)]
    [TestCase(PaymentStatus.Rejected)]
    [TestCase(PaymentStatus.Cancelled)]
    public void Transition_ToTerminalState_SetsFailureReason(PaymentStatus terminal)
    {
        var payment = Fakers.ValidPayment();
        const string reason = "Something went wrong.";

        payment.Transition(terminal, reason);

        payment.FailureReason.Should().Be(reason);
        payment.Status.Should().Be(terminal);
    }

    [Test]
    public void Transition_ToSuccessState_DoesNotSetFailureReason()
    {
        var payment = Fakers.ValidPayment();

        payment.Transition(PaymentStatus.Settled);

        payment.FailureReason.Should().BeNull();
    }
}
