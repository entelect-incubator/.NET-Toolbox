namespace PaymentSaga.Application;

using Microsoft.Extensions.DependencyInjection;
using PaymentSaga.Application.Payments.Commands.InitiatePayment;
using PaymentSaga.Application.Payments.Commands.SubmitApprovalDecision;
using PaymentSaga.Application.Payments.Queries.GetPaymentStatus;
using Utilities.CQRS;
using Utilities.Results;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<InitiatePaymentValidator>();

        services.AddScoped<ICommandHandler<InitiatePaymentCommand, Result<Guid>>, InitiatePaymentHandler>();
        services.AddScoped<ICommandHandler<SubmitApprovalDecisionCommand, Result>, SubmitApprovalDecisionHandler>();
        services.AddScoped<IQueryHandler<GetPaymentStatusQuery, Result<PaymentStatusDto>>, GetPaymentStatusHandler>();

        services.AddScoped<Dispatcher>();

        return services;
    }
}
