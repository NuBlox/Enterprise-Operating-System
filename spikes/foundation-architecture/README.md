# Foundation Architecture Spike

> **Disposable architecture experiment — not the NuBlox production application.**

This spike implements bounded experiments defined by `NBEOS-G-013 Technical_spikes.md`.

## Current scope

Implemented verification currently covers:

- `SPIKE-001` — cohesive modular core + relational transactions;
- `SPIKE-002` — historical/effective-dated data;
- an early database-integrity check relevant to `SPIKE-003` — customer/isolation context.

Technology used for the experiment:

- .NET 10 LTS / C#;
- PostgreSQL 18;
- Npgsql 10.0.3;
- explicit typed module boundaries;
- one atomic transaction spanning module-owned writes;
- customer-scoped relational constraints;
- business-effective subject-name history distinct from technical record time;
- audit event written in the same transaction;
- minimal HTTP harness;
- console verification harness;
- versioned SQL migrations;
- path-scoped GitHub Actions verification.

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
│   ├── 0001_foundation.sql
│   └── 0002_effective_history.sql
├── docker-compose.yml
├── Directory.Build.props
└── global.json
```

## SPIKE-001 architectural test

A representative transaction performs:

```text
customer context
→ create typed sample business subject
→ create work request linked to subject
→ create decision linked to work
→ append audit event
→ commit all or none
```

The module projects own their writes. The application layer orchestrates the use case through module APIs and a shared provider-neutral transaction abstraction. Only Infrastructure references Npgsql.

The schema uses composite `(customer_id, id)` keys and composite foreign keys so a work record cannot link to a subject belonging to another customer context even if application code is wrong.

## SPIKE-002 historical/effective-time test

A subject identity is kept separate from effective-dated subject-name facts.

The history table records:

```text
business effective-from / effective-to
+
technical recorded-at
+
change reason
```

PostgreSQL exclusion constraints reject overlapping effective periods for the same customer/subject.

The verifier checks that:

- a historical as-of query returns the earlier name;
- a later as-of query returns the changed name;
- a change can be recorded technically after its historical effective date;
- overlapping versions are rejected by the database.

This is a spike pattern, not an approved final temporal model for every NuBlox business concept.

## Local prerequisites

- .NET 10 SDK
- Docker Desktop / compatible Docker runtime

Revalidate package/runtime versions before promoting any spike code.

## Run on macOS

From the repository root:

```bash
cd spikes/foundation-architecture

docker compose up -d postgres

for migration in migrations/*.sql; do
  echo "Applying ${migration}"
  docker compose exec -T postgres \
    psql -U nublox -d nublox_spike -v ON_ERROR_STOP=1 \
    < "${migration}"
done

dotnet restore src/NuBlox.FoundationSpike.Api/NuBlox.FoundationSpike.Api.csproj
dotnet build src/NuBlox.FoundationSpike.Api/NuBlox.FoundationSpike.Api.csproj --no-restore

dotnet restore verification/NuBlox.FoundationSpike.Verification/NuBlox.FoundationSpike.Verification.csproj
dotnet build verification/NuBlox.FoundationSpike.Verification/NuBlox.FoundationSpike.Verification.csproj --no-restore

dotnet run --project verification/NuBlox.FoundationSpike.Verification/NuBlox.FoundationSpike.Verification.csproj --no-build
```

For a clean rerun:

```bash
docker compose down -v
docker compose up -d postgres
```

then apply the migrations again.

## HTTP harness

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

The console verifier tests:

1. a valid transaction commits subject, work, decision and audit records;
2. a cross-customer relationship is rejected by the database;
3. an explicitly rolled-back transaction leaves no partial subject/work state;
4. effective-dated history returns the correct business state for different as-of dates;
5. technical recorded time remains distinguishable from business-effective time;
6. overlapping effective periods are rejected by the database.

## Current verification status

`SPIKE-001` has passed GitHub Actions verification. `SPIKE-002` is executed by the same canonical `.github/workflows/foundation-spike.yml` pipeline and must pass at the current head before its result is treated as evidence.

The local assistant execution container does not contain .NET or Docker, so GitHub Actions is the authoritative automated verifier for this spike.

## Promotion rule

No code under `spikes/` may be treated as production application code merely because it works. Promotion requires:

- spike report;
- ADR update;
- code/design review;
- controlled implementation plan;
- normal development/testing/security controls.
