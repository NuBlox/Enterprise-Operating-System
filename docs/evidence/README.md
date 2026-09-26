# Evidence Intake

This directory contains source material used to test the clean-slate NuBlox architecture.

## Rule

**Evidence is not canon.**

Do not copy a prior taxonomy, schema, route, object model, tool catalogue or workflow into the clean-slate architecture merely because it existed previously.

Every imported source should be accompanied by enough provenance to answer:

- where did this come from?
- what period/version does it represent?
- what business problem does it describe?
- is it universal, industry-specific, jurisdiction-specific or customer-specific?
- which clean-slate concepts does it support or challenge?
- has it been reconciled?

## Suggested structure

```text
docs/evidence/
├── prior-nublox/
├── market-tools/
├── occupations-and-jobs/
├── standards-and-regulation/
├── customer-workflows/
├── industry/
└── reconciliation/
```

Directories should be created only when actual evidence is imported.

## Prohibited shortcut

Do not treat filenames, numbering or existing database identifiers as proof of semantic authority.

For example, a prior `F07`, `D04`, object ID, table name or route may be cited as source evidence, but the clean-slate architecture is free to retain, change, merge, split or reject that concept after review.

## Reconciliation

Use [`../06-evidence-reconciliation.md`](../06-evidence-reconciliation.md) as the governing process for promoting evidence into canonical architecture.
