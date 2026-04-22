# GitHub Actions CI/CD Workflows

This repository contains automated CI/CD workflows for .NET projects.

## 🚀 Active Workflows

### 1. Build & Test CI (`build.yml`)
**Triggers:** Push to main/develop, PRs
- **Technology:** .NET 10 / C#
- **Projects:** Utilities, PaymentSaga modules
- **Features:**
  - Multi-project build (sequential dependencies)
  - NUnit test execution with coverage
  - Test results & coverage artifacts
  - Failure summaries on PR

### 2. CodeQL Security Scan (`codeql.yml`)
**Triggers:** Push, PR, weekly Monday 08:00 UTC
- **Technology:** .NET 10 / C#
- **Scope:** Utilities + PaymentSaga
- **Features:**
  - Automated security analysis
  - Dependency scanning
  - Vulnerability detection
  - Build before analysis

## 📁 Project Structure Coverage

```
.NET-Toolbox/
├── 4.Utilities/                  → build.yml, codeql.yml
├── 5.PaymentSaga/                → build.yml, codeql.yml
│   ├── src/PaymentSaga.Domain/
│   ├── src/PaymentSaga.Application/
│   ├── src/PaymentSaga.Infrastructure/
│   ├── src/PaymentSaga.Api/
│   ├── src/PaymentSaga.AppHost/
│   └── src/PaymentSaga.Tests/
└── .github/
    └── workflows/
        ├── build.yml
        ├── codeql.yml
        └── dependabot.yml
```

## 🔧 Workflow Features

### Build & Test
- **Multi-Project:** Utilities builds first, PaymentSaga depends on it
- **Test Coverage:** NUnit with FluentAssertions, NSubstitute, Bogus
- **Architecture Tests:** NetArchTest to enforce Clean Architecture boundaries
- **Artifact Upload:** Test results (TRX) + coverage reports

### Security & Quality
- **CodeQL Analysis:** C# specific queries, OWASP Top 10 checks
- **Automated Scanning:** Runs on every push and PR
- **Weekly Schedules:** Monday 08:00 UTC for baseline detection
- **Build Prerequisites:** Code builds before scanning

### Automation
- **Dependabot:** Weekly NuGet package updates, grouped by ecosystem
- **Branch Protection:** PR gates require passing workflows
- **Notifications:** GitHub PR status checks on failure

## 🎯 Usage Guidelines

### Branch Strategy
- **Main Branch:** Production-ready code, protected
- **Develop Branch:** Integration branch
- **Feature Branches:** PRs trigger full build + CodeQL

### .NET Build Matrix
- **Framework:** net10.0 (current stable)
- **Configuration:** Debug (fast) and Release (optimized)
- **Test Framework:** NUnit 4.x
- **Coverage:** TRX + Cobertura formats

### Workflow Sequencing
1. **Utilities** builds and tests first
2. **PaymentSaga** depends on Utilities project reference
3. **CodeQL** runs after successful build
4. Parallel artifact collection

## 🔄 Pattern Modules

### Utilities (Module 4)
Patterns: Result, CQRS, Static Façade, Provider
- Cross-cutting library used by all modules
- No business logic, only patterns
- `dotnet build 4.Utilities/Utilities.csproj`

### PaymentSaga (Module 5)
Patterns: Saga, Event-Driven, Retry, Repository
- Long-running payment approval workflow
- MassTransit + RabbitMQ + EF Core
- `dotnet run --project 5.PaymentSaga/src/PaymentSaga.AppHost` (requires Docker)

## 📋 Maintenance

### Adding New Pattern Modules
1. Create new module folder (e.g., `6.EventSourcing/`)
2. Add projects: Domain, Application, Infrastructure, API, Tests
3. Add project references to solution (`.slnx`)
4. Update `build.yml` if new dependency chains exist
5. Create `DEV.md` with Mermaid architecture diagrams

### Updating Workflows
1. Edit relevant `.yml` file in `.github/workflows/`
2. Test on feature branch
3. Merge to main via PR
4. Monitor first workflow run
5. Update this README if scope changes

### Dependabot Configuration
- Edit `.github/dependabot.yml`
- Add new NuGet package directories
- Schedule weekly Monday 06:00 Africa/Johannesburg
- Group by ecosystem (MassTransit, EF, OpenTelemetry, etc.)

## 🚦 Status Monitoring

Check workflow status at:
- **Actions Tab:** All workflow runs with logs
- **Pull Requests:** Required status checks on PRs
- **Commit Status:** Individual commit workflow results
- **Artifacts:** Download test results & coverage reports (7-day retention)

---

*Workflows for .NET Toolbox — C# design patterns & architectural reference implementations*
*Last Updated: April 2026*