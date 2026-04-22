namespace PaymentSaga.Infrastructure;

using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentSaga.Application.Ports;
using PaymentSaga.Infrastructure.Consumers;
using PaymentSaga.Infrastructure.Persistence;
using PaymentSaga.Infrastructure.Saga;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PaymentDb")
            ?? throw new InvalidOperationException("Missing connection string 'PaymentDb'.");

        services.AddDbContext<PaymentDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IPaymentRepository, PaymentRepository>();

        services.AddMassTransit(bus =>
        {
            // Saga persistence via EF Core
            bus.AddSagaStateMachine<PaymentSagaStateMachine, PaymentSagaState>()
                .EntityFrameworkRepository(r =>
                {
                    r.ConcurrencyMode = ConcurrencyMode.Optimistic;
                    r.AddDbContext<DbContext, PaymentDbContext>((sp, db) =>
                        db.UseSqlServer(connectionString));
                });

            bus.AddConsumer<ValidatePaymentConsumer>();
            bus.AddConsumer<ProcessPaymentConsumer>();
            bus.AddConsumer<SettlePaymentConsumer>();

            bus.UsingRabbitMq((ctx, cfg) =>
            {
                var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
                var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(rabbitHost, h =>
                {
                    h.Username(rabbitUser);
                    h.Password(rabbitPass);
                });

                cfg.UseMessageRetry(r => r.Intervals(
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(15)));

                cfg.ConfigureEndpoints(ctx);
            });
        });

        return services;
    }
}
