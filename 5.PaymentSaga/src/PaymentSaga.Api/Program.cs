using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PaymentSaga.Api.Endpoints;
using PaymentSaga.Application;
using PaymentSaga.Infrastructure;
using Serilog;
using Utilities.Logging.Static;

// ── Serilog bootstrap logger (catches startup failures) ──────────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Logger.SetLoggerWrapper(new SerilogWrapper());

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // ── OpenTelemetry ────────────────────────────────────────────────────────
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService("PaymentSaga.Api"))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation()
            .AddOtlpExporter());

    // ── Application layers ───────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── API ──────────────────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapPaymentEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

// ── Thin Serilog wrapper for the static Logger ───────────────────────────────
internal sealed class SerilogWrapper : Utilities.Logging.Static.ILoggerWrapper
{
    public void LogInfo(string subject, string message, object? logObject = null)
        => Log.Information("{Subject}|{Message}|{@Data}", subject, message, logObject);

    public void LogDebug(string subject, string message, object? logObject = null)
        => Log.Debug("{Subject}|{Message}|{@Data}", subject, message, logObject);

    public void LogException(string ex, object? logObject = null)
        => Log.Error("{Message}|{@Data}", ex, logObject);

    public void LogException(Exception ex, object? logObject = null)
        => Log.Error(ex, "{@Data}", logObject);
}

