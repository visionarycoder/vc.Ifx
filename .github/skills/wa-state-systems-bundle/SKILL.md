---
name: wa-state-systems-bundle
title: Washington State Systems Bundle
description: >
  Bundle router for Washington State system integration across SAAM policy, AFRS submission, 
  FEMS fleet operations, CAMS asset accounting, fiscal controls, CGI Advantage, and legacy 
  Trains migration requests.
doc_type: skill
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: low
estimated_tokens: 2118
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - wa-state-fems
  - wa-state-cams
  - wa-state-saam
  - wa-state-afrs
  - wa-state-fiscal-policies
  - appropriation-lifecycle-patterns
  - saam-ontology
  - cgi-advantage-integration
  - legacy-trains-migration
appliesTo: '**/*.{cs,csproj,json,sql,xml,md,csv,txt}'
tags:
  - washington-state
  - afrs
  - fems
  - saam
  - cgi-advantage
  - trains
  - routing
---
# Washington State Systems Bundle

Agent uses this bundle for Washington State financial-system work spanning policy interpretation, AFRS submission, FEMS fleet operations, CAMS asset accounting, fiscal coding, CGI Advantage integration, or Trains migration.

## When to Use

| Prompt Pattern | Use This Bundle |
|---|---|
| Washington State agency integration request | Yes |
| AFRS submission plus statewide reporting alignment | Yes |
| FEMS fleet, maintenance, fuel, or replacement-planning request | Yes |
| CAMS asset registration, depreciation, or reconciliation work | Yes |
| CGI Advantage 4 or Trains 4 data exchange work | Yes |
| SAAM policy plus fiscal coding in one request | Yes |
| Single specialist task with no routing need | No |

## When Not to Use

| Prompt Pattern | Agent Route |
|---|---|
| SAAM interpretation only | `wa-state-saam` |
| AFRS file layout or submission only | `wa-state-afrs` |
| Fleet inventory, maintenance, fuel, or utilization only | `wa-state-fems` |
| CAMS asset registration or depreciation only | `wa-state-cams` |
| CGI Advantage endpoint mapping only | `cgi-advantage-integration` |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Agency system role | Yes | Source ledger, subledger, payroll, grants, or reporting |
| Integration target | Yes | SAAM policy, AFRS, FEMS, CAMS, fiscal codes, CGI Advantage, or Trains migration |
| Data exchange pattern | Yes | Batch file, staged table, API, message, or report extract |
| Fiscal timing | No | Fiscal month, fiscal year close, biennium close, or ad hoc |

## Activation

| Request Signal | Use Bundle | Route Detail |
|---|---|---|
| Policy citation, accounting treatment, statewide control question | Yes | `wa-state-saam` |
| SAAM semantic modeling, ontology queries, RDF validation | Yes | `saam-ontology` |
| Appropriation lifecycle, funds availability, budget control | Yes | `appropriation-lifecycle-patterns` |
| AFRS batch layout, file balancing, interface submission | Yes | `wa-state-afrs` |
| Fleet inventory, maintenance, fuel, utilization, or replacement question | Yes | `wa-state-fems` |
| Capital asset registration, depreciation, inventory, or disposal question | Yes | `wa-state-cams` |
| Appropriation, allotment, fund, object, or fiscal-year coding | Yes | `wa-state-fiscal-policies` |
| CGI Advantage 4 interface, vendor, project, or chart mapping | Yes | `cgi-advantage-integration` |
| Trains extract replacement, coexistence, or cutover sequencing | Yes | `legacy-trains-migration` |

## Decision Matrix: SAAM vs AFRS vs FEMS vs CAMS vs CGI Advantage

| Primary Need | Route | Decision Signal |
|---|---|---|
| Accounting rule, approval evidence, or statewide policy interpretation | `wa-state-saam` | Compliance meaning |
| File submission, batch balancing, edit checks, or statewide ledger intake | `wa-state-afrs` | Interface payload and submission flow |
| Fleet inventory, maintenance, fuel, utilization, replacement, or fleet chargebacks | `wa-state-fems` | Fleet equipment and fleet-cost behavior |
| Asset registration, useful life, disposal, physical inventory, or depreciation | `wa-state-cams` | Capital-asset register and lifecycle accounting |
| ERP master data, transaction mapping, or operational system integration | `cgi-advantage-integration` | Source-to-target system exchange |
| Mixed policy plus batch design | `wa-state-saam` then `wa-state-afrs` | Policy drives file content |
| Mixed fleet operations plus capital asset reporting | `wa-state-fems` then `wa-state-cams` | Fleet history drives asset registration or reporting |
| Mixed acquisition plus capital asset registration | `cgi-advantage-integration` then `wa-state-cams` | Source acquisition data drives the CAMS asset record |
| Mixed acquisition plus fleet-equipment operations | `cgi-advantage-integration` then `wa-state-fems` | Source acquisition and expense data drive the fleet record |
| Mixed ERP plus legacy cutover | `cgi-advantage-integration` then `legacy-trains-migration` | Target model drives migration design |

## Decision Order

| Decision | Agent Action |
|---|---|
| Washington State policy ambiguity appears first | Route to `wa-state-saam` before payload design. |
| File balancing or AFRS edit risk appears | Route to `wa-state-afrs` before schedule finalization. |
| Fleet inventory, maintenance, fuel, utilization, or replacement scope appears | Route to `wa-state-fems` before capital-asset packaging. |
| Asset lifecycle, depreciation, or physical inventory scope appears | Route to `wa-state-cams` before annual-report packaging. |
| Fiscal coding affects payload values | Route to `wa-state-fiscal-policies` before interface freeze. |
| CGI Advantage target model is the destination | Route to `cgi-advantage-integration` before migration mapping. |
| Trains retirement or coexistence is in scope | Route to `legacy-trains-migration` after target mapping selection. |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Classify request | Agent maps the prompt to policy, AFRS, FEMS, CAMS, fiscal, ERP, and migration tracks. | Agent records track list. | Every requirement maps to at least one track. |
| 2. Order routing | Agent places policy and coding decisions before interface design. | Agent records routing order. | Upstream policy drivers appear before downstream payload work. |
| 3. Define flow | Agent selects AFRS batch, Advantage exchange, or coexistence flow. | Agent records one primary flow per integration. | Every integration has one primary flow. |
| 4. Verify fiscal timing | Agent checks month-end, year-end, or biennium timing. | Agent records the close window. | Timing aligns with the relevant fiscal cycle. |
| 5. Verify outputs | Agent checks balancing, mapping, and rejection handling. | Inspect control totals and exception paths. | One deterministic reconciliation path exists. |

## Verification Matrix

| Area | Test | Pass |
|---|---|---|
| SAAM routing | Compare requirements to policy-driven fields and controls. | Policy-dependent fields map to one documented rule path. |
| AFRS readiness | Inspect file layout, batch totals, and rejection path. | Interface totals balance and rejection handling exists. |
| FEMS routing | Inspect fleet-equipment scope, maintenance, fuel, and chargeback needs. | Fleet-operational requirements map to one FEMS path before capital-asset routing. |
| CAMS routing | Inspect capitalization, depreciation, and capital-asset registration needs. | Capital-asset requirements map to one CAMS path after fleet routing when both remain in scope. |
| Fiscal coding | Inspect fiscal year, appropriation, allotment, fund, and object mapping. | Every coded field maps to one valid state value set. |
| CGI Advantage integration | Inspect master and transaction mapping. | Source entities map to one target model without duplicate keys. |
| Migration path | Inspect coexistence, cutover, and reconciliation steps. | Legacy and target outputs reconcile across the cutover path. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Payload design starts before SAAM interpretation | Agent routes first to `wa-state-saam`. |
| AFRS submission omits balancing controls | Agent adds control totals and reject handling through `wa-state-afrs`. |
| Fleet-operational requirements route straight to CAMS | Agent routes first to `wa-state-fems` and then to `wa-state-cams` when capital reporting remains in scope. |
| Fiscal year and biennium logic share one date rule | Agent separates fiscal-year and biennium checkpoints through `wa-state-fiscal-policies`. |
| CGI Advantage mapping copies legacy Trains codes without normalization | Agent routes through `cgi-advantage-integration` before cutover. |
| Migration plan skips dual-run reconciliation | Agent adds coexistence validation through `legacy-trains-migration`. |
