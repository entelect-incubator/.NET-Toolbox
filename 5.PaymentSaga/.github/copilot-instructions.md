# GitHub Copilot Instructions — PaymentSaga

## Project overview
Payment orchestration service using a **MassTransit SAGA state machine** on .NET 10 with minimal APIs.
The saga handles long-running, externally-gated payment approval flows that must survive restarts and scale horizontally.

## Architecture (Clean Architecture)
```
PaymentSaga.Domain          ← pure domain: entities, value objects, enums — zero external deps
PaymentSaga.Application     ← CQRS handlers (Utilities.MediatorLite), FluentValidation, MassTransit contracts
PaymentSaga.Infrastructure  ← MassTransit + RabbitMQ, EF Core SQL Server, saga persistence
PaymentSaga.Api             ← .NET 10 minimal API endpoints, Serilog, OpenTelemetry
PaymentSaga.AppHost         ← .NET Aspire orchestrator (RabbitMQ + SQL Server containers)
4.Utilities                 ← shared library: Result<T>, Dispatcher (CQRS), static Logger
```

## Coding conventions

### Result pattern
- All handlers return `Result<T>` or `Result` from `Utilities.Results`.
- Never throw from handlers — catch and return `Result.Failure(ex)`.
- Use `Result<T>.ValidationFailure(errors)` for FluentValidation failures.
- Use `Result<T>.NotFound()` when a resource does not exist.

### CQRS
- Commands implement `ICommand<TResult>`, queries implement `IQuery<TResult>`.
- Handlers are injected via `ICommandHandler<TCommand, TResult>` / `IQueryHandler<TQuery, TResult>`.
- Wire handlers in the `DependencyInjection.cs` of each layer.
- Dispatch from API endpoints using `Dispatcher` (not MediatR).

### Logging
- Use the **static** `Logger` from `Utilities.Logging.Static` everywhere.
  ```csharp
  Logger.LogInfo("CallerName", "message", new { correlationId });
  Logger.LogException(ex, new { correlationId });
  ```
- Never log passwords, tokens, card numbers, PII, or secrets.
- Always pass a `correlationId` as context.

### SAGA state machine rules
- States are declared as `public State <Name> { get; private set; }` properties on the machine.
- Events are declared as `public Event<TMessage> <Name> { get; private set; }`.
- Correlate all events by `CorrelationId` (Guid).
- Consumers publish reply events back to the bus — the state machine never calls consumers directly.
- Terminal states call `.Finalize()` and the machine calls `SetCompletedWhenFinalized()`.
- Saga state is persisted in SQL Server via `MassTransit.EntityFrameworkCore` with **optimistic concurrency**.

### MassTransit consumers
- One responsibility per consumer.
- Always re-throw unexpected exceptions so MassTransit can retry and route to the error queue.
- Publish failure events for expected domain failures (e.g., `PaymentValidationFailedEvent`) instead of throwing.

### Minimal API endpoints
- Grouped under `/api/v1/<resource>`.
- Return typed `IResult` using `TypedResults` — never `Results.Ok(...)`.
- Map `ErrorResults` enum to HTTP status codes at the endpoint layer only.
- Request models are records in the same file as the endpoint group.

### Security
- Never trust user-supplied `CorrelationId` for saga initiation — always generate server-side.
- Validate all inbound requests with FluentValidation before publishing to the bus.
- Use parameterised EF Core queries only — no raw SQL.
- Connection strings and secrets must come from configuration / Aspire secrets, never hardcoded.

### .NET 10 / language features
- Use primary constructors for DI.
- Use `ArgumentException.ThrowIfNullOrWhiteSpace` and `ArgumentNullException.ThrowIfNull` for guard clauses.
- Prefer `record` for immutable DTOs and message contracts.
- Target `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`.

## Adding a new payment step
1. Add the message contract to `Application/Contracts/Messages.cs`.
2. Add a new `State` and `Event` to `PaymentSagaStateMachine`.
3. Create a new `IConsumer<TCommand>` in `Infrastructure/Consumers/`.
4. Register the consumer in `Infrastructure/DependencyInjection.cs`.

## Running locally
```bash
# Requires Docker Desktop
dotnet run --project src/PaymentSaga.AppHost
```
Aspire spins up RabbitMQ (with management UI) and SQL Server automatically.
