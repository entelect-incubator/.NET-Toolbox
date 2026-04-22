# .NET Toolbox

[![Build](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/build.yml)
[![CodeQL](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/codeql.yml/badge.svg)](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

A curated, production-grade collection of .NET patterns, utilities, and reference implementations.
Each module is a self-contained, buildable example that can be lifted into real projects.

---

## Contents

| #   | Module                        | Description                                           | Key tech                                                                                     |
| --- | ----------------------------- | ----------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| 4   | [Utilities](4.Utilities/)     | Shared cross-cutting library used by all modules      | Result pattern, CQRS (MediatorLite), Static Logger, FluentValidation, OpenTelemetry, Serilog |
| 5   | [PaymentSaga](5.PaymentSaga/) | Long-running payment approval flow with external wait | MassTransit SAGA, RabbitMQ, EF Core, .NET Aspire, OpenTelemetry                              |

---

## Philosophy

- **No magic** — every pattern is explicit, readable, and traceable.
- **Clean Architecture** — Domain → Application → Infrastructure → API. Dependencies only flow inward.
- **Result pattern everywhere** — no unhandled exceptions, predictable error contracts.
- **CQRS via MediatorLite** — lightweight, no reflection overhead, fully testable.
- **Observable by default** — Serilog structured logging + OpenTelemetry traces on every module.
- **Security first** — OWASP Top 10 considered, no secrets in code, parameterised queries only.

---

## 4. Utilities

Shared library that all modules reference. Do **not** add business logic here.

```
Utilities/
  Results/          Result<T>, Result — typed success/failure with error codes
  CQRS/             ICommand, IQuery, INotification, Dispatcher
  Logging/Static/   Logger — static façade over Serilog
  Enums/            ErrorResults
  Extensions/       Common extension methods
  Helpers/          API response helpers
  Middleware/        Logging middleware
  Providers/         DateTimeOffset + Guid providers (testable)
```

## 5. PaymentSaga

Reference implementation of a **durable, externally-gated payment approval flow**.

> See [5.PaymentSaga/DEV.md](5.PaymentSaga/DEV.md) for full architecture docs, Mermaid diagrams and system analysis.

**Quick start** (requires Docker Desktop):
```bash
dotnet run --project 5.PaymentSaga/src/PaymentSaga.AppHost
```

---

## Running all tests

```bash
dotnet test --configuration Release --logger "console;verbosity=normal"
```

## Security

- Static analysis via CodeQL on every push and PR.
- NuGet dependencies are kept up to date automatically via Dependabot (weekly, Mondays).
- See [SECURITY.md](SECURITY.md) if you discover a vulnerability.

---

## Contributing

1. Follow the conventions in each module's `.github/copilot-instructions.md`.
2. All new code must have tests.
3. PRs must pass build + CodeQL gates.

