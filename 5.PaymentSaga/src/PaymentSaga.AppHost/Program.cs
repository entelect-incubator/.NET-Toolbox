var builder = DistributedApplication.CreateBuilder(args);

// ── Infrastructure resources ──────────────────────────────────────────────────
var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sqlServer = builder.AddSqlServer("sql", password: sqlPassword)
    .AddDatabase("PaymentDb");

var rabbit = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ── Services ──────────────────────────────────────────────────────────────────
builder.AddProject<Projects.PaymentSaga_Api>("payment-api")
    .WithReference(sqlServer)
    .WithReference(rabbit)
    .WaitFor(sqlServer)
    .WaitFor(rabbit);

builder.Build().Run();
