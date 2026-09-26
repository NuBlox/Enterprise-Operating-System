# Foundation Architecture Spike

> **Disposable architecture experiment — not the NuBlox production application.**

This spike implements the first bounded experiments defined by `NBEOS-G-013 Technical_spikes.md`.

## Current scope

Initially targets `SPIKE-001` and prepares evidence for `SPIKE-003`:

- .NET 10 LTS / C#;
- PostgreSQL 18;
- explicit typed module boundaries;
- one atomic transaction spanning module-owned writes;
- relational constraints and customer-context integrity;
- append-only-style audit event written in the same transaction;
- minimal HTTP harness;
- console verification harness;
- versioned SQL migration.

It deliberately does **not** define the final NuBlox business taxonomy, frontend, tenancy model, identity model, cloud topology or production architecture.

## Structure

```text
spikes/foundation-architecture/
├── src/
│   ├── NuBlox.FoundationSpike.Shared/
│   ├── NuBlox.FoundationSpike.Subjects/
│   ├── NuBlox.FoundationSpike.Work/
│   ├── NuBlox.FoundationSpike.Decisions/
│   ├── NuBlox.FoundationSpike.Audit/
│   ├── NuBlox.FoundationSpike.Application/
│   ├── NuBlox.FoundationSpike.Infrastructure/
│   └── NuBlox.FoundationSpike.Api/
├── verification/
│   └── NuBlox.FoundationSpike.Verification/
├── migrations/
│   └── 0001_foundation.sql
├── docker-compose.yml
├── Directory.Build.props
└── global.json
```

## Architectural test

A representative transaction performs:

```text
customer context
→ create typed sample business subject
→ create work request linked to subject
→ create decision linked to work
→ append audit event
→ commit all or none
```

The module projects own their SQL writes. The application layer orchestrates the use case through module APIs and a shared transaction abstraction.

The schema uses composite `(customer_id, id)` keys and composite foreign keys so a work record cannot link to a subject belonging to another customer context even if application code is wrong.

## Local prerequisites

- .NET 10 SDK
- Docker Desktop / compatible Docker runtime

Current package evidence when the spike was created:

- `Npgsql` 10.0.3
- PostgreSQL 18.x

Revalidate package versions before promoting any spike code.

## Run on macOS

From the repository root:

```bash
cd spikes/foundation-architecture

docker compose up -d postgres

docker compose exec -T postgres \
  psql -U nublox -d nublox_spike \
  < migrations/0001_foundation.sql

dotnet restore src/NuBlox.FoundationSpike.Api/NuBlox.FoundationSpike.Api.csproj
dotnet build src/NuBlox.FoundationSpike.Api/NuBlox.FoundationSpike.Api.csproj --no-restore

dotnet run --project verification/NuBlox.FoundationSpike.Verification/NuBlox.FoundationSpike.Verification.csproj
```

Run the HTTP harness separately:

```bash
export ConnectionStrings__Database='Host=localhost;Port=55432;Database=nublox_spike;Username=nublox;Password=nublox_spike_dev_only'
dotnet run --project src/NuBlox.FoundationSpike.Api/NuBlox.FoundationSpike.Api.csproj
```

Then POST a spike transaction:

```bash
curl -sS -X POST http://localhost:5080/spike/governed-work \
  -H 'content-type: application/json' \
  -d '{
    "customerId":"11111111-1111-1111-1111-111111111111",
    "subjectName":"Architecture Spike Subject",
    "workSummary":"Prove atomic modular transaction",
    "decisionOutcome":"APPROVED"
  }'
```

## Verification harness

The console verifier is intended to prove:

1. a valid transaction commits all module records;
2. a cross-customer relationship is rejected by the database;
3. an explicitly rolled-back transaction leaves no partial subject/work state.

A successful run is evidence for the spike; it is not proof that the eventual production architecture is approved.

## Current verification status

**UNVERIFIED in the assistant execution environment.** The environment used to create these files does not contain the .NET SDK or Docker. Run the commands above on the Mac or add CI before changing the spike status.

## Promotion rule

No code under `spikes/` may be treated as production application code merely because it works. Promotion requires:

- spike report;
- ADR update;
- code/design review;
- controlled implementation plan;
- normal development/testing/security controls.
