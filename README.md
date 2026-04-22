# .NET Toolbox

[![Build](https://github.com/entelect-incubator/.NET-Toolbox/actions/workflows/build.yml/badge.svg)](https://github.com/entelect-incubator/.NET-Toolbox/actions/workflows/build.yml)
[![CodeQL](https://github.com/entelect-incubator/.NET-Toolbox/actions/workflows/codeql.yml/badge.svg)](https://github.com/entelect-incubator/.NET-Toolbox/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

A curated, production-grade collection of C# design patterns, architectural patterns, and .NET reference implementations.
Each module demonstrates one or more patterns in a self-contained, buildable, test-covered example—ready to lift into real projects.

---

## Contents

| #   | Module                        | Patterns & Concepts                               | Key tech                                                                              |
| --- | ----------------------------- | ------------------------------------------------- | ------------------------------------------------------------------------------------- |
| 4   | [Utilities](4.Utilities/)     | Result, CQRS, Static Façade, Provider Pattern     | Result pattern, MediatorLite, Static Logger, FluentValidation, OpenTelemetry, Serilog |
| 5   | [PaymentSaga](5.PaymentSaga/) | Saga, Eventual Consistency, Event Sourcing, Retry | MassTransit SAGA, RabbitMQ, EF Core, .NET Aspire, OpenTelemetry                       |

---

## Philosophy

- **Result Pattern** — Typed success/failure returns, no exceptions for control flow.
- **Clean Architecture** — Domain → Application → Infrastructure → API. Dependencies only flow inward.
- **CQRS** — Commands and Queries separated via lightweight MediatorLite dispatcher, fully testable.
- **Eventual Consistency** — Saga pattern for long-running workflows across service boundaries.
- **Observable by Default** — Serilog structured logging + OpenTelemetry on every module.
- **Provider Pattern** — Abstract DateTime and Guid creation for deterministic tests.
- **Explicit Over Implicit** — No magic, every pattern is readable, traceable, and refactorable.

---

## 4. Utilities

Shared library demonstrating **Result Pattern**, **CQRS**, and **Provider Pattern**.

```
Utilities/
  Results/          Result<T>, Result — typed success/failure with ErrorResults enum
  CQRS/             ICommand, IQuery, Dispatcher — command/query segregation
  Logging/Static/   Logger — static façade over Serilog for cross-cutting concerns
  Enums/            ErrorResults — centralized domain error codes
  Extensions/       Common extension methods and operators
  Helpers/          API response mappers (Result → HTTP status)
  Middleware/       Logging middleware for request/response tracing
  Providers/        DateTimeOffset + Guid — injectable, mockable factories
```

## 5. PaymentSaga

Reference implementation of the **Saga Pattern** for long-running, eventually-consistent workflows.

**Patterns demonstrated:**
- **Saga State Machine** — Multi-step process with external gates (approval, payment, settlement)
- **Event-Driven Architecture** — MassTransit pub/sub + RabbitMQ message transport
- **Retry & DLQ** — Resilient consumer implementations with exponential backoff
- **Repository Pattern** — EF Core data access with concurrent saga state via RowVersion
- **Validation Rules** — FluentValidation at entry point, domain guards in entities
- **CQRS + Domain Events** — Commands trigger sagas; queries read via DTO projection



## Running all tests

```bash
dotnet test --configuration Release --logger "console;verbosity=normal"
```

> See [5.PaymentSaga/DEV.md](5.PaymentSaga/DEV.md) for full architecture docs, Mermaid diagrams, and system analysis.

**Quick start** (requires Docker Desktop):
```bash
dotnet run --project 5.PaymentSaga/src/PaymentSaga.AppHost
```

---

## Testing

Every pattern module includes:
- **Unit tests** — NUnit with NSubstitute mocks and Bogus fake data generators
- **Architecture tests** — NetArchTest to enforce Clean Architecture layer boundaries
- **Builders** — Faker patterns for consistent test data generation

Example test directory:
```
PaymentSaga.Tests/
  Builders/         Bogus fakers for domain entities and commands
  Domain/           Domain entity & value object tests
  Application/      Handler, validator, CQRS tests with mocked I/O
  Architecture/     Layer dependency rules and naming conventions
```

---

## Security

- Static analysis via CodeQL on every push and PR.
- NuGet dependencies are kept up to date automatically via Dependabot (weekly, Mondays).
- See [SECURITY.md](SECURITY.md) if you discover a vulnerability.

---

## Contributing

Each module must demonstrate **one or more C# design patterns** clearly and completely.

1. **Architecture:** Follow Clean Architecture (Domain → Application → Infrastructure → API)
2. **Testing:** 100% coverage of patterns; use builders, mocks (NSubstitute), and architecture tests
3. **Documentation:** Include a `DEV.md` with Mermaid diagrams (C4, state machine, sequence, etc.)
4. **Code Style:** Follow conventions in each module's `.github/copilot-instructions.md`
5. **Gates:** All PRs must pass `build.yml` + `codeql.yml`

