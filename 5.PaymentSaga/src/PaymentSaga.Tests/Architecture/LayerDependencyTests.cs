namespace PaymentSaga.Tests.Architecture;

using FluentAssertions;
using NetArchTest.Rules;

/// <summary>
/// Enforces Clean Architecture dependency rules:
/// Domain → no deps on Application / Infrastructure / API
/// Application → no dep on Infrastructure / API
/// Infrastructure → no dep on API
/// </summary>
[TestFixture]
internal sealed class LayerDependencyTests
{
    private const string DomainNs = "PaymentSaga.Domain";
    private const string ApplicationNs = "PaymentSaga.Application";
    private const string InfrastructureNs = "PaymentSaga.Infrastructure";
    private const string ApiNs = "PaymentSaga.Api";

    private static Types AllTypes() =>
        Types.InAssemblies(
        [
            typeof(global::PaymentSaga.Domain.Entities.Payment).Assembly,
            typeof(global::PaymentSaga.Application.DependencyInjection).Assembly,
            typeof(global::PaymentSaga.Infrastructure.DependencyInjection).Assembly
        ]);

    // ── Domain must not depend on anything above it ───────────────────────────

    [Test]
    public void Domain_Should_NotReference_Application()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Domain.Entities.Payment).Assembly)
            .ShouldNot().HaveDependencyOn(ApplicationNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on Application");
    }

    [Test]
    public void Domain_Should_NotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Domain.Entities.Payment).Assembly)
            .ShouldNot().HaveDependencyOn(InfrastructureNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on Infrastructure");
    }

    [Test]
    public void Domain_Should_NotReference_Api()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Domain.Entities.Payment).Assembly)
            .ShouldNot().HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain must not depend on the API layer");
    }

    // ── Application must not depend on Infrastructure or API ─────────────────

    [Test]
    public void Application_Should_NotReference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Application.DependencyInjection).Assembly)
            .ShouldNot().HaveDependencyOn(InfrastructureNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application must not depend on Infrastructure");
    }

    [Test]
    public void Application_Should_NotReference_Api()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Application.DependencyInjection).Assembly)
            .ShouldNot().HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application must not depend on the API layer");
    }

    // ── Infrastructure must not depend on API ─────────────────────────────────

    [Test]
    public void Infrastructure_Should_NotReference_Api()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Infrastructure.DependencyInjection).Assembly)
            .ShouldNot().HaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure must not depend on the API layer");
    }

    // ── Naming conventions ────────────────────────────────────────────────────

    [Test]
    public void Handlers_Should_ResideInApplicationNamespace()
    {
        var result = AllTypes()
            .That().HaveNameEndingWith("Handler")
            .Should().ResideInNamespaceStartingWith(ApplicationNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "all handlers belong in the Application layer");
    }

    [Test]
    public void Consumers_Should_ResideInInfrastructureNamespace()
    {
        var result = AllTypes()
            .That().HaveNameEndingWith("Consumer")
            .Should().ResideInNamespaceStartingWith(InfrastructureNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "MassTransit consumers belong in the Infrastructure layer");
    }

    [Test]
    public void DomainEntities_Should_ResideInDomainNamespace()
    {
        var result = Types.InAssembly(typeof(global::PaymentSaga.Domain.Entities.Payment).Assembly)
            .That().ResideInNamespace($"{DomainNs}.Entities")
            .Should().ResideInNamespaceStartingWith(DomainNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "domain entities must stay within the Domain assembly");
    }

    [Test]
    public void Validators_Should_ResideInApplicationNamespace()
    {
        var result = AllTypes()
            .That().HaveNameEndingWith("Validator")
            .Should().ResideInNamespaceStartingWith(ApplicationNs)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "FluentValidation validators belong in the Application layer");
    }
}
