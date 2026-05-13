# Mandatory First Step

Before making any change, GitHub Copilot must first read this entire instructions file and follow it as binding project guidance.

Copilot must not start editing, refactoring, upgrading packages, changing tests, or modifying build/pipeline configuration before considering these instructions.

For every requested task, Copilot must:

1. Identify the exact scope of the user request.
2. Check whether the requested change is covered by these instructions.
3. Keep the change as small as possible.
4. Avoid unrelated modernization or cleanup.
5. Run the required verification commands before claiming completion.
6. Report exactly which files were changed and why.

If the requested task conflicts with these instructions, Copilot must stop and ask for confirmation before proceeding.
# GitHub Copilot Instructions for Clean Architecture Template
## Project Overview
This is a **Clean Architecture template** for .NET 10 that demonstrates Domain-Driven Design (DDD) patterns. It's a starter template, not a reference application - delete sample code once you understand the patterns.

## Architecture & Project Structure

### C# Conventions
- Use standard Microsoft naming conventions
- Use `PascalCase` for types and methods, `camelCase` for parameters and private fields
- Use `I` prefix for interfaces (e.g., `IRepository`)
- Use `Async` suffix for async methods (e.g., `GetByIdAsync`)
- Prefix private fields with `_` (e.g., `_repository`)
- Always use {} for blocks except single-line exits (e.g. `return`, `throw`)
- Always keep single line blocks on one line (e.g., `if (x) return y;`)
- Prefer primary constructors for required dependencies
- Never use primary constructor parameters directly - always assign to private fields for clarity and testability

### Core Dependencies Flow
- **Core** ← UseCases ← Infrastructure 
- **Core** ← UseCases ← Web
- Never allow Core to depend on outer layers

### Key Projects
- **Core**: Domain entities, aggregates, value objects, specifications, interfaces
- **UseCases**: Commands/queries (CQRS), Mediator handlers, application logic  
- **Infrastructure**: EF Core, external services, email, file access
- **Web**: FastEndpoints API, REPR pattern, validation

## Development Patterns

### API Endpoints (FastEndpoints + REPR)
- One endpoint per file: `Create.cs`, `Update.cs`, `Delete.cs`, `GetById.cs`
- Separate request/response/validator files: `Create.CreateRequest.cs`, `Create.CreateValidator.cs`
- Use `Endpoint<TRequest, TResponse>` base class
- Example: `src/Clean.Architecture.Web/Contributors/Create.cs`

### Domain Model (Core)
- Entities use encapsulation - minimize public setters
- Group related entities into Aggregates
- Use Value Objects (e.g., `ContributorName.From()`)
- Domain Events for cross-aggregate communication
- Repository interfaces defined in Core, implemented in Infrastructure

### Use Cases (CQRS)
- Commands for mutations, Queries for reads
- Queries can bypass repository pattern for performance
- Use Mediator (source generator) for command/query handling
- Chain of responsibility for cross-cutting concerns (logging, validation)

### Validation Strategy
- **API Level**: FluentValidation on request DTOs (FastEndpoints integration)
- **Use Case Level**: Validate commands/queries (defensive coding)
- **Domain Level**: Business invariants throw exceptions, assume pre-validated input

## Essential Commands

### Build & Test
```bash
dotnet build Clean.Architecture.slnx
dotnet test Clean.Architecture.slnx
```

### Entity Framework Migrations
```bash
# From Web project directory
dotnet ef migrations add MigrationName -c AppDbContext -p ../Clean.Architecture.Infrastructure/Clean.Architecture.Infrastructure.csproj -s Clean.Architecture.Web.csproj -o Data/Migrations

dotnet ef database update -c AppDbContext -p ../Clean.Architecture.Infrastructure/Clean.Architecture.Infrastructure.csproj -s Clean.Architecture.Web.csproj
```

### Template Installation & Usage
```bash
dotnet new install Ardalis.CleanArchitecture.Template
dotnet new clean-arch -o Your.ProjectName
```

## Key Dependencies & Patterns

### Primary Libraries
- **FastEndpoints**: API endpoints (replaced Controllers/Minimal APIs)
- **Mediator**: Command/query handling in UseCases
- **EF Core**: Data access (SQLite default, easily changed to SQL Server)
- **Ardalis.Specification**: Repository query specifications
- **Ardalis.Result**: Error handling pattern
- **Serilog**: Structured logging

### Central Package Management
- All package versions in `Directory.Packages.props`
- Use `<PackageReference Include="..." />` without Version attribute

### Test Organization
- **UnitTests**: Core business logic, use cases
- **IntegrationTests**: Database, infrastructure components  
- **FunctionalTests**: API endpoints (subcutaneous testing)
- Use `Microsoft.AspNetCore.Mvc.Testing` for API tests

## File Organization Conventions

### Web Project Structure
```
Contributors/
  Create.cs                    # Endpoint
  Create.CreateRequest.cs      # Request DTO
  Create.CreateResponse.cs     # Response DTO  
  Create.CreateValidator.cs    # FluentValidation
  Update.cs, Delete.cs, etc.
```

### Sample vs Template
- `/sample` folder: Complete working example (NimblePros.SampleToDo)
- `/src` folder: Clean template ready for your project
- Study sample for patterns, use src for new projects

## Common Gotchas

- Don't include hyphens in project names (template limitation)
- Replace `Ardalis.SharedKernel` with your own shared kernel
- Database path in `appsettings.json` for SQLite
- Use absolute paths in EF migration commands
- FastEndpoints uses different validation approach than Controller-based APIs

## VS Code Tasks
Use the predefined tasks: `build`, `publish`, `watch` instead of manual `dotnet` commands when possible.

## Mandatory Pre-Task Checklist

Before starting any task, Copilot must explicitly check:

- What branch is currently active?
- What files are expected to change?
- Which files must not be changed?
- Are package versions centrally managed in `Directory.Packages.props`?
- Are tests expected to run before completion?

Copilot must not perform broad automatic modernization unless explicitly requested.

## Strict Migration Rules for Dependency/Test Updates

When working on dependency updates, test framework migrations, or build/test modernization, follow these strict rules:
...

## Strict Migration Rules for Dependency/Test Updates

When working on dependency updates, test framework migrations, or build/test modernization, follow these strict rules:

### Scope Control
- Do not perform broad refactorings unless explicitly requested.
- Do not change application architecture, project structure, namespaces, or production code unless required to fix compilation after the requested change.
- Do not update unrelated NuGet packages.
- Do not modify CI/CD pipeline files unless explicitly requested.
- Do not change Docker, Testcontainers, database provider configuration, Aspire configuration, or functional test infrastructure unless explicitly requested.

### xUnit v3 Migration Rules
When asked to migrate from `xunit` to `xunit.v3`:
- Only replace the deprecated `xunit` package with `xunit.v3`.
- Keep `Microsoft.NET.Test.Sdk` unless there is a proven build/test failure requiring a change.
- Keep `xunit.runner.visualstudio` unless there is a proven build/test failure requiring a change.
- Do not introduce `Microsoft.Testing.Platform.MSBuild` unless explicitly requested.
- Do not introduce `coverlet.MTP` unless explicitly requested.
- Do not change code coverage configuration unless explicitly requested.
- Do not change test logic unless required by xUnit v3 API compatibility.

### Central Package Management
- Package versions must remain in `Directory.Packages.props`.
- Test projects must use `<PackageReference Include="..." />` without `Version`.
- Do not add package versions directly to `.csproj` files.

### Verification Required Before Claiming Completion
Before saying that the task is complete, run and verify:

```bash
dotnet restore
dotnet build Clean.Architecture.slnx
dotnet test Clean.Architecture.slnx
