# SPIKE-001 — Cohesive modular core + relational transactions

This directory contains the disposable architecture experiment authorised by `software_project_docs/G_Architecture_Design/Technical_spikes.md`.

## Purpose

Prove or falsify the hypothesis that NuBlox can begin as a cohesive modular application with:

- explicit internal module ownership;
- typed business concepts;
- relational transactional persistence;
- database-enforced invariants;
- auditable state transitions;
- a simple reproducible developer workflow.

This is **not production NuBlox application code** and does not establish the final enterprise taxonomy, UI, routes or persistence model.

## Technology under evaluation

- .NET 10 LTS / C#
- ASP.NET Core minimal API as a test harness
- PostgreSQL 18
- SQL-first migrations for the spike
- xUnit for architecture/domain tests

## Sample scenario

```text
Customer context
→ Business Subject
→ Work Request
→ Decision
→ State Transition
→ Audit Evidence
```

The terminology is intentionally generic and typed. It exists only to exercise transaction, boundary and history behaviour.

## Projects

- `src/NuBlox.Spikes.Core` — sample domain/modules and invariants
- `src/NuBlox.Spikes.Api` — minimal HTTP harness
- `tests/NuBlox.Spikes.Tests` — architecture/domain tests
- `database/migrations` — spike-only PostgreSQL schema

## Local prerequisites

- .NET SDK 10.x
- Docker Desktop or compatible container runtime

## Local commands

```bash
cd spikes/SPIKE-001

docker compose up -d postgres

dotnet restore NuBlox.Spikes.slnx
dotnet build NuBlox.Spikes.slnx --no-restore
dotnet test NuBlox.Spikes.slnx --no-build
```

Database migration execution will be added as part of the spike harness; SQL migration files are versioned from the first commit.

## Promotion rule

Nothing in this directory is promoted into the production application unless a subsequent ADR explicitly accepts the relevant design and the code is re-reviewed against production requirements.
