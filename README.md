# JLA (Justice League of Agents)

Prototype of the JLA orchestration platform: a .NET 10 modular monolith with PostgreSQL.
The authoritative design is the *JLA Minimum Technical Design Proposal* (a Claude Doc, linked by the project owner)
and the *JLA Architecture Charter (Revised)*. This repository implements that design in increments.

## Status

Increment 0 only: solution layout, contracts v1, generated schemas, architecture tests, CI. No orchestration behaviour exists yet.

## Layout

Seven source projects and three test projects.

| Project | Role |
| --- | --- |
| `src/Jla.Contracts` | Records, enums, boundary interfaces. References nothing |
| `src/Jla.Core` | Orchestration, registry, execution, governance, LLM gateway (namespaces, not projects) |
| `src/Jla.Persistence` | PostgreSQL store (only project that references EF Core or Npgsql) |
| `src/Jla.Llm.Claude` | Claude adapter (only project that references an HTTP or Anthropic client) |
| `src/Jla.Agents` | Seed agents. References only Contracts |
| `src/Jla.Simulation` | Simulated operation handlers and fixtures. References only Contracts |
| `src/Jla.Host` | API, run loop host, composition root. The `jla` CLI will be a command mode of this project |
| `tests/Jla.Tests.Unit` | Unit tests, including the schema drift test |
| `tests/Jla.Tests.Architecture` | Dependency and namespace rules |
| `tests/Jla.Tests.Acceptance` | Scenarios and security tests. Integration tests need Docker |

## Build and test

```
dotnet build Jla.sln
dotnet test Jla.sln --filter "Category!=Integration"
dotnet test tests/Jla.Tests.Acceptance --filter "Category=Integration"   # needs Docker
```

Regenerate the committed schemas after changing a contract:

```
JLA_UPDATE_SCHEMAS=1 dotnet test tests/Jla.Tests.Unit --filter ContractSchemaDriftTests
```

(PowerShell: `$env:JLA_UPDATE_SCHEMAS=1; dotnet test tests/Jla.Tests.Unit --filter ContractSchemaDriftTests`)

## Conventions

- Tenant isolation: every tenant-owned store method will take a `TenantScope` and filter on `tenant_id` explicitly. Only `Jla.Core` can construct one.
- Contracts that are not in the design doc are marked with a comment in the source (`RunUsage`, `ProposedCriterion`, `OperationOutcome`, `OutcomeLookup`).
