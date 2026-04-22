# PaymentSaga — Developer Documentation

[![Build](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/build.yml/badge.svg)](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/build.yml)
[![CodeQL](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/codeql.yml/badge.svg)](https://github.com/YOUR_ORG/NET-Toolbox/actions/workflows/codeql.yml)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![MassTransit](https://img.shields.io/badge/MassTransit-8.4-blue)](https://masstransit.io)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.x-orange)](https://rabbitmq.com)
[![Aspire](https://img.shields.io/badge/.NET%20Aspire-13.x-blueviolet)](https://learn.microsoft.com/en-us/dotnet/aspire/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../../LICENSE)

---

## Table of Contents

1. [What is this?](#1-what-is-this)
2. [Why a SAGA?](#2-why-a-saga)
3. [System Analysis](#3-system-analysis)
4. [Architecture](#4-architecture)
5. [State Machine](#5-state-machine)
6. [Sequence Diagrams](#6-sequence-diagrams)
7. [Data Model](#7-data-model)
8. [Message Contracts](#8-message-contracts)
9. [Security Considerations](#9-security-considerations)
10. [Running Locally](#10-running-locally)
11. [Testing](#11-testing)

---

## 1. What is this?

**PaymentSaga** is a reference implementation of a long-running, externally-gated payment approval workflow.

The core problem it solves:

> A payment is submitted, validated automatically, then **waits for a human (or external system) to approve or reject it** before processing and settling. This wait can last minutes, hours, or days. The service must survive restarts, scale horizontally, and never lose state.

This is implemented as a **MassTransit Saga State Machine** backed by SQL Server for durable state, and RabbitMQ as the message broker. The API is .NET 10 Minimal API, orchestrated by .NET Aspire.

---

## 2. Why a SAGA?

| Concern             | Naive HTTP approach    | SAGA approach                   |
| ------------------- | ---------------------- | ------------------------------- |
| **Durability**      | State lost on restart  | Persisted in SQL Server         |
| **Scale-out**       | Sticky sessions needed | Any node handles any message    |
| **External wait**   | Polling loop in memory | State machine sleeps in DB      |
| **Partial failure** | Manual rollback code   | Compensation steps are explicit |
| **Observability**   | Custom logging         | Distributed traces via OTEL     |
| **Retry**           | Manual                 | MassTransit retry pipeline      |

---

## 3. System Analysis

### Actors

| Actor                       | Role                                                    |
| --------------------------- | ------------------------------------------------------- |
| **Client**                  | Submits payment requests via REST API                   |
| **Payment API**             | Validates input, dispatches CQRS command, reads status  |
| **Saga State Machine**      | Orchestrates the end-to-end flow, persists state        |
| **ValidatePaymentConsumer** | Runs domain validation rules                            |
| **ProcessPaymentConsumer**  | Calls external payment gateway                          |
| **SettlePaymentConsumer**   | Finalises in ledger / accounting                        |
| **Approver**                | External human or system that approves/rejects via REST |
| **RabbitMQ**                | Message broker — decouples all components               |
| **SQL Server**              | Stores payment records and saga state                   |

### Bounded Context

```
┌─────────────────────────────────────────────────────┐
│  Payment Bounded Context                            │
│                                                     │
│  ┌──────────┐    CQRS     ┌────────────────────┐   │
│  │  API     │ ──────────► │  Application Layer │   │
│  └──────────┘             └────────┬───────────┘   │
│                                    │ publishes      │
│                           ┌────────▼───────────┐   │
│                           │  MassTransit Bus   │   │
│                           └────────┬───────────┘   │
│                    ┌───────────────┼───────────┐    │
│                    ▼               ▼           ▼    │
│             ┌──────────┐  ┌──────────────┐  ┌───┐  │
│             │  Saga SM │  │  Consumers   │  │DB │  │
│             └──────────┘  └──────────────┘  └───┘  │
└─────────────────────────────────────────────────────┘
```

### Quality Attributes

| Attribute         | Mechanism                                                                 |
| ----------------- | ------------------------------------------------------------------------- |
| **Durability**    | Saga state in SQL Server with row-version optimistic concurrency          |
| **Scalability**   | Stateless API + stateless consumers; saga state is externalised           |
| **Observability** | OpenTelemetry traces (OTLP), Serilog structured logs, Aspire dashboard    |
| **Resilience**    | MassTransit retry with exponential back-off; error queue for dead-letters |
| **Security**      | Server-side `CorrelationId`, FluentValidation on all inputs, no raw SQL   |
| **Testability**   | Interfaces at every boundary, Bogus for data, NSubstitute for mocks       |

---

## 4. Architecture

### Layers (Clean Architecture)

```mermaid
graph TD
    API["PaymentSaga.Api<br/>.NET 10 Minimal API"]
    APP["PaymentSaga.Application<br/>CQRS Handlers · Validators · Contracts"]
    INFRA["PaymentSaga.Infrastructure<br/>Saga SM · Consumers · EF Core · MassTransit"]
    DOM["PaymentSaga.Domain<br/>Entities · Value Objects · Enums"]
    UTIL["4.Utilities<br/>Result&lt;T&gt; · Dispatcher · Logger"]

    API --> APP
    API --> INFRA
    APP --> DOM
    APP --> UTIL
    INFRA --> APP
    INFRA --> UTIL

    style DOM fill:#2d6a4f,color:#fff
    style APP fill:#1d3557,color:#fff
    style INFRA fill:#457b9d,color:#fff
    style API fill:#e63946,color:#fff
    style UTIL fill:#6c757d,color:#fff
```

### Dependency rule
Dependencies only point **inward**. Domain knows nothing about infrastructure. Application knows nothing about HTTP.

### Component diagram

```mermaid
C4Component
    title PaymentSaga — Component View

    Container_Boundary(api, "PaymentSaga.Api") {
        Component(ep, "PaymentEndpoints", "Minimal API", "REST entry points for payment operations")
        Component(disp, "Dispatcher", "Utilities.CQRS", "Routes commands/queries to handlers")
    }

    Container_Boundary(app, "PaymentSaga.Application") {
        Component(initH, "InitiatePaymentHandler", "ICommandHandler", "Validates + publishes InitiatePaymentSagaCommand")
        Component(approvalH, "SubmitApprovalDecisionHandler", "ICommandHandler", "Publishes SubmitApprovalDecisionCommand to bus")
        Component(statusH, "GetPaymentStatusHandler", "IQueryHandler", "Reads payment status from repository")
    }

    Container_Boundary(infra, "PaymentSaga.Infrastructure") {
        Component(sm, "PaymentSagaStateMachine", "MassTransit SM", "Durable state orchestrator")
        Component(valC, "ValidatePaymentConsumer", "IConsumer", "Runs domain validation rules")
        Component(procC, "ProcessPaymentConsumer", "IConsumer", "Calls payment gateway")
        Component(settC, "SettlePaymentConsumer", "IConsumer", "Finalises in ledger")
        Component(repo, "PaymentRepository", "IPaymentRepository", "EF Core data access")
    }

    ContainerDb(sql, "SQL Server", "Database", "Payments table + saga state table")
    Container(rabbit, "RabbitMQ", "Message Broker", "Fanout exchanges + queues")

    Rel(ep, disp, "uses")
    Rel(disp, initH, "dispatches")
    Rel(disp, approvalH, "dispatches")
    Rel(disp, statusH, "dispatches")
    Rel(initH, rabbit, "publishes InitiatePaymentSagaCommand")
    Rel(approvalH, rabbit, "publishes SubmitApprovalDecisionCommand")
    Rel(sm, valC, "publishes ValidatePaymentCommand")
    Rel(sm, procC, "publishes ProcessPaymentCommand")
    Rel(sm, settC, "publishes SettlePaymentCommand")
    Rel(valC, rabbit, "publishes Validated/ValidationFailed")
    Rel(procC, rabbit, "publishes Processed/ProcessingFailed")
    Rel(settC, rabbit, "publishes Settled")
    Rel(sm, sql, "reads/writes saga state")
    Rel(repo, sql, "reads/writes payments")
```

---

## 5. State Machine

### States

| State              | Meaning                                              |
| ------------------ | ---------------------------------------------------- |
| `Initial`          | Not yet started                                      |
| `Validating`       | Domain validation running asynchronously             |
| `AwaitingApproval` | **Durable wait** — saga is persisted; no thread held |
| `Processing`       | Payment gateway call in-flight                       |
| `Settling`         | Ledger / accounting finalisation in-flight           |
| `Settled`          | Terminal success                                     |
| `Rejected`         | Terminal — declined by approver                      |
| `Failed`           | Terminal — unrecoverable error                       |

### State transition diagram

```mermaid
stateDiagram-v2
    [*] --> Validating : InitiatePaymentSagaCommand
    Validating --> AwaitingApproval : PaymentValidatedEvent
    Validating --> Failed : PaymentValidationFailedEvent

    AwaitingApproval --> Processing : SubmitApprovalDecisionCommand (approved=true)
    AwaitingApproval --> Rejected : SubmitApprovalDecisionCommand (approved=false)

    Processing --> Settling : PaymentProcessedEvent
    Processing --> Failed : PaymentProcessingFailedEvent

    Settling --> Settled : PaymentSettledEvent

    Settled --> [*]
    Rejected --> [*]
    Failed --> [*]
```

---

## 6. Sequence Diagrams

### Happy path — payment approved and settled

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant API as PaymentSaga.Api
    participant Bus as RabbitMQ
    participant Saga as Saga State Machine
    participant Val as ValidatePaymentConsumer
    participant DB as SQL Server
    actor Approver
    participant Proc as ProcessPaymentConsumer
    participant Settle as SettlePaymentConsumer

    Client->>API: POST /api/v1/payments
    API->>DB: INSERT Payment (Initiated)
    API->>Bus: Publish InitiatePaymentSagaCommand
    API-->>Client: 202 Accepted { correlationId }

    Bus->>Saga: InitiatePaymentSagaCommand
    Saga->>DB: INSERT SagaState (Validating)
    Saga->>Bus: Publish ValidatePaymentCommand

    Bus->>Val: ValidatePaymentCommand
    Val->>Bus: Publish PaymentValidatedEvent

    Bus->>Saga: PaymentValidatedEvent
    Saga->>DB: UPDATE SagaState (AwaitingApproval)

    Note over Saga,DB: Saga sleeps. No thread held.<br/>Survives restarts and scale-out.

    Client->>API: GET /api/v1/payments/{id}
    API->>DB: SELECT Payment
    API-->>Client: 200 OK { status: "AwaitingApproval" }

    Approver->>API: POST /api/v1/payments/{id}/decision { isApproved: true }
    API->>Bus: Publish SubmitApprovalDecisionCommand

    Bus->>Saga: SubmitApprovalDecisionCommand (approved)
    Saga->>DB: UPDATE SagaState (Processing)
    Saga->>Bus: Publish ProcessPaymentCommand

    Bus->>Proc: ProcessPaymentCommand
    Proc->>Bus: Publish PaymentProcessedEvent

    Bus->>Saga: PaymentProcessedEvent
    Saga->>DB: UPDATE SagaState (Settling)
    Saga->>Bus: Publish SettlePaymentCommand

    Bus->>Settle: SettlePaymentCommand
    Settle->>Bus: Publish PaymentSettledEvent

    Bus->>Saga: PaymentSettledEvent
    Saga->>DB: UPDATE SagaState (Settled) — Finalised
```

### Rejection path

```mermaid
sequenceDiagram
    autonumber
    actor Approver
    participant API as PaymentSaga.Api
    participant Bus as RabbitMQ
    participant Saga as Saga State Machine
    participant DB as SQL Server

    Note over Saga,DB: Saga is in AwaitingApproval state

    Approver->>API: POST /api/v1/payments/{id}/decision { isApproved: false, reason: "KYC failed" }
    API->>Bus: Publish SubmitApprovalDecisionCommand (approved=false)

    Bus->>Saga: SubmitApprovalDecisionCommand
    Saga->>DB: UPDATE SagaState (Rejected) — Finalised
```

### Validation failure path

```mermaid
sequenceDiagram
    autonumber
    participant Bus as RabbitMQ
    participant Saga as Saga State Machine
    participant Val as ValidatePaymentConsumer
    participant DB as SQL Server

    Bus->>Val: ValidatePaymentCommand
    Note over Val: Amount > 1,000,000 limit exceeded
    Val->>Bus: Publish PaymentValidationFailedEvent

    Bus->>Saga: PaymentValidationFailedEvent
    Saga->>DB: UPDATE SagaState (Failed) — Finalised
```

### Retry / error queue flow

```mermaid
sequenceDiagram
    participant Bus as RabbitMQ
    participant Consumer as Consumer (any)
    participant ErrorQ as Error Queue

    Bus->>Consumer: Deliver message
    Consumer--xConsumer: Exception thrown
    Note over Bus,Consumer: Retry 1 — after 1s
    Bus->>Consumer: Redeliver
    Consumer--xConsumer: Exception thrown
    Note over Bus,Consumer: Retry 2 — after 5s
    Bus->>Consumer: Redeliver
    Consumer--xConsumer: Exception thrown
    Note over Bus,Consumer: Retry 3 — after 15s
    Bus->>Consumer: Redeliver
    Consumer--xConsumer: Exception thrown
    Consumer->>ErrorQ: Move to *_error queue
    Note over ErrorQ: Dead-letter for manual inspection
```

---

## 7. Data Model

### Payments table (domain record)

```mermaid
erDiagram
    PAYMENTS {
        uniqueidentifier Id PK
        nvarchar(36)     CorrelationId UK
        nvarchar(100)    PayerId
        nvarchar(100)    PayeeId
        decimal(18_4)    Amount
        nvarchar(3)      Currency
        nvarchar(500)    Description
        nvarchar(30)     Status
        nvarchar(1000)   FailureReason
        datetimeoffset   CreatedAt
        datetimeoffset   UpdatedAt
    }

    PAYMENT_SAGA_STATES {
        uniqueidentifier CorrelationId PK
        nvarchar(30)     CurrentState
        nvarchar(100)    PayerId
        nvarchar(100)    PayeeId
        decimal(18_4)    Amount
        nvarchar(3)      Currency
        nvarchar(500)    Description
        nvarchar(100)    ExternalTransactionId
        nvarchar(1000)   FailureReason
        datetimeoffset   CreatedAt
        datetimeoffset   UpdatedAt
        rowversion       RowVersion
    }

    PAYMENTS ||--o| PAYMENT_SAGA_STATES : "correlates via CorrelationId"
```

> `RowVersion` on saga state provides optimistic concurrency — prevents split-brain when two nodes process the same saga simultaneously.

---

## 8. Message Contracts

All contracts are immutable `record` types in `PaymentSaga.Application/Contracts/Messages.cs`.

### Flow

```mermaid
flowchart LR
    subgraph API_Commands["API-issued commands"]
        IC[InitiatePaymentSagaCommand]
        AD[SubmitApprovalDecisionCommand]
    end

    subgraph Saga_Commands["Saga-issued commands"]
        VC[ValidatePaymentCommand]
        PC[ProcessPaymentCommand]
        SC[SettlePaymentCommand]
    end

    subgraph Consumer_Events["Consumer-published events"]
        VE[PaymentValidatedEvent]
        VF[PaymentValidationFailedEvent]
        PE[PaymentProcessedEvent]
        PF[PaymentProcessingFailedEvent]
        SE[PaymentSettledEvent]
    end

    IC -->|triggers| VC
    VC -->|reply| VE
    VC -->|reply| VF
    VE -->|triggers| AD
    AD -->|approved| PC
    PC -->|reply| PE
    PC -->|reply| PF
    PE -->|triggers| SC
    SC -->|reply| SE
```

---

## 9. Security Considerations

| Risk                               | Mitigation                                                                        |
| ---------------------------------- | --------------------------------------------------------------------------------- |
| **Mass assignment / over-posting** | Request models are explicit `record` types — no model binding from unknown fields |
| **CorrelationId injection**        | Server always generates `CorrelationId` — client cannot set it                    |
| **SQL injection**                  | EF Core parameterised queries only — no raw SQL                                   |
| **Secrets in code**                | All connection strings / passwords come from configuration / Aspire secrets       |
| **Denial of service**              | FluentValidation rejects malformed or oversized requests before any DB/bus call   |
| **Saga replay attacks**            | MassTransit deduplicates messages by message-id                                   |
| **Dependency vulnerabilities**     | CodeQL + Dependabot weekly scans                                                  |

---

## 10. Running Locally

**Prerequisites:** Docker Desktop, .NET 10 SDK.

```bash
# From repo root
dotnet run --project 5.PaymentSaga/src/PaymentSaga.AppHost
```

Aspire provisions:
- SQL Server on `localhost:1433`
- RabbitMQ on `localhost:5672`, management UI on `localhost:15672` (guest/guest)
- Aspire dashboard on `https://localhost:15888`

### Sample requests

```bash
# Initiate a payment
curl -X POST https://localhost:5001/api/v1/payments \
  -H "Content-Type: application/json" \
  -d '{"payerId":"payer-1","payeeId":"payee-1","amount":500.00,"currency":"ZAR","description":"Invoice 42"}'

# Check status
curl https://localhost:5001/api/v1/payments/{correlationId}

# Submit approval
curl -X POST https://localhost:5001/api/v1/payments/{correlationId}/decision \
  -H "Content-Type: application/json" \
  -d '{"isApproved":true}'
```

---

## 11. Testing

```bash
dotnet test 5.PaymentSaga/PaymentSaga.slnx --configuration Release
```

| Layer            | What's tested                                     | Tools                     |
| ---------------- | ------------------------------------------------- | ------------------------- |
| **Domain**       | Value object invariants, entity state transitions | NUnit, Bogus              |
| **Application**  | Handler success/failure paths, validation         | NUnit, NSubstitute, Bogus |
| **Architecture** | Layer dependency rules (no Domain → Infra, etc.)  | NetArchTest.Rules         |

See [src/PaymentSaga.Tests/](src/PaymentSaga.Tests/) for the full test suite.
