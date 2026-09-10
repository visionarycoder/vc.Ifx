---
title: Accounting Ontology Alignment Reference
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Alignment reference that maps SAAM accounting concepts to XBRL GL, FIBO, Schema.org, and W3C PROV patterns for semantic integration, audit lineage, and .NET implementation.
target_audience: ai
tags:
  - ontology
  - xbrl
  - fibo
  - semantic-web
  - rdf
  - accounting
related_docs:
  - ../SKILL.md
  - ../../saam-ontology/SKILL.md
  - ../../knowledge-graph-patterns/SKILL.md
  - ../../semantic-modeling-standards/SKILL.md
source_paths:
  - ../../saam-ontology/ontology/saam-core.ttl
---
# Accounting Ontology Alignment Reference

This reference aligns the SAAM ontology with widely used accounting and financial semantic standards. The alignment supports journal-entry exchange, budget and reporting semantics, web publication, provenance capture, and knowledge-graph construction.

## Purpose and Scope

The SAAM ontology defines Washington State accounting semantics for funds, appropriations, object codes, programs, periods, and financial transactions. Standard accounting ontologies contribute interoperable exchange patterns that extend SAAM beyond one repository or one application boundary.

This reference covers four external standards:

| Standard | Primary Role | SAAM Benefit |
|---|---|---|
| XBRL GL | Journal-level accounting exchange | Preserves debit-credit detail, posting context, and account structure for ledger interoperability. |
| FIBO | Formal financial business semantics | Aligns legal entities, obligations, reporting parties, monetary concepts, and reporting structures. |
| Schema.org | Web-facing business and commerce metadata | Publishes invoices, orders, and payment metadata in JSON-LD for portal and API discovery. |
| W3C PROV | Provenance and audit lineage | Tracks who created, approved, posted, corrected, and consumed each journal entry or balance artifact. |

## Related Skill Crosswalk

| Skill | Relationship to This Reference | Observable Reuse Point |
|---|---|---|
| [Ontology Design Patterns](../SKILL.md) | Supplies class, property, and alignment design patterns. | Alignment tables use exact, broad, and extension-oriented mappings. |
| [SAAM Ontology](../../saam-ontology/SKILL.md) | Supplies the source classes and properties for funds, appropriations, object codes, and transactions. | `saam:Fund`, `saam:Appropriation`, `saam:ObjectCode`, and `saam:FinancialTransaction` anchor the mapping. |
| [Knowledge Graph Patterns](../../knowledge-graph-patterns/SKILL.md) | Supplies graph-store and traversal patterns for multi-hop accounting lineage. | Provenance and cross-system joins project into RDF graphs or hybrid graph stores. |
| [Semantic Modeling Standards](../../semantic-modeling-standards/SKILL.md) | Supplies downstream dimensional projection rules. | XBRL GL and PROV attributes project into fact grain, conformed dimensions, and audit dimensions. |

## Source Concept Baseline

The SAAM ontology exposes the core concepts that drive this mapping set:

| SAAM Concept | Current Ontology Anchor | Semantic Role |
|---|---|---|
| Fund | `saam:Fund` and fund subclasses | Self-balancing accounting entity and fiscal control unit. |
| Appropriation | `saam:Appropriation` | Legislative budget authority linked to a fund and biennium. |
| ObjectCode | `saam:ObjectCode` plus `saam:SubObject` and `saam:SubSubObject` | Classification hierarchy for expenditure and revenue coding. |
| FinancialTransaction | `saam:FinancialTransaction` and subclasses | Accounting event with amount, date, and coding dimensions. |
| Fund relationship | `saam:hasFund` | Connects a transaction to the controlling fund. |
| Appropriation relationship | `saam:hasAppropriation` | Connects a transaction to budget authority. |
| Object code relationship | `saam:hasObjectCode` | Connects a transaction to a classification axis. |
| Document reference | `saam:documentReference` | Connects a transaction to a source record or external document key. |

## External Standard Overview

| Standard Family | Modeling Style | Typical Serialization | Best Fit in SAAM Integration |
|---|---|---|---|
| XBRL GL | Taxonomy and instance facts | XML or XBRL instance documents | Journal import, export, audit package exchange, and posting trace. |
| FIBO | OWL ontologies with enterprise financial vocabulary | RDF, Turtle, OWL, JSON-LD | Entity identity, obligation semantics, monetary concepts, reporting context, and policy alignment. |
| Schema.org | Lightweight web vocabulary | JSON-LD, RDFa, Microdata | Public or partner-facing invoice, order, and payment metadata exposure. |
| PROV-O | OWL/RDF provenance vocabulary | RDF, Turtle, JSON-LD | Audit trail, lineage, agent accountability, derived-balance trace, and correction history. |

## XBRL GL Taxonomy Overview

XBRL GL provides a standardized representation for detailed accounting entries rather than only summary financial statements. The framework focuses on source transactions, journal entries, account references, posting metadata, identifiers, document references, and linkage to reporting taxonomies. XBRL International published the 2015 framework as a proposed recommendation and highlighted the SRCD integration layer for linkage between detailed GL data and reporting taxonomies.

### Core XBRL GL Concepts

| XBRL GL Concept | Role | SAAM Alignment |
|---|---|---|
| Account | Identifies the ledger account, segment, or coded classification used in posting. | Maps from SAAM fund, object code, subobject, sub-subobject, and local chart-of-accounts crosswalks. |
| Entry | Represents one debit or credit line within a journal entry. | Maps from a detailed SAAM transaction line or split allocation line. |
| Transaction | Represents the larger business event or document that groups one or more entries. | Maps from a SAAM financial transaction header, source document, or batch document. |
| Entity context | Identifies the reporting organization or ledger owner. | Maps from agency, statewide ledger, or subsystem origin. |
| Posting date and period | States when the entry posts and which period it affects. | Maps from `saam:transactionDate`, `saam:hasFiscalPeriod`, and biennium context. |
| Document reference | Preserves traceability back to an invoice, voucher, payroll run, or adjustment document. | Maps from `saam:documentReference` and external source identifiers. |

### Journal Entry Representation in XBRL GL

XBRL GL models a journal entry as a document structure with header context plus one or more line entries. A header records document identity, source system, posting date, originating entity, and supporting references. Each line records the account or classification target, amount, debit-credit sign, quantity attributes when present, and links to counterpart lines through shared document or entry grouping identifiers.

The following journal-entry pattern fits SAAM cleanly:

| Journal Layer | XBRL GL Pattern | SAAM Pattern |
|---|---|---|
| Batch or source file | Ledger document or journal set | AFRS or CGI batch, interface file, payroll run, or imported voucher set |
| Transaction header | Document identifier, description, origin, posting context | `saam:FinancialTransaction` plus source document metadata |
| Entry line | One posting line with debit or credit value | Detailed expenditure, revenue, transfer, or accrual line |
| Account reference | Main account plus segmented dimensions | Fund, appropriation, object code, program, organization, project, and optional local account |
| Supporting document | External reference and evidence metadata | Invoice number, purchase order, payroll control number, contract ID, or journal voucher number |

### XBRL GL Field-Level Mapping Pattern

| SAAM Field or Relationship | XBRL GL Target | Mapping Notes |
|---|---|---|
| `saam:FinancialTransaction` | Journal transaction container | One SAAM transaction header aligns to one journal event or one document grouping node. |
| `saam:transactionAmount` | Line amount | Split lines preserve accounting sign rules at the line level. |
| `saam:transactionDate` | Posting date or document date | Posting date and document date stay distinct when source systems record both. |
| `saam:documentReference` | Document reference identifier | External invoice, batch, or voucher identifiers remain explicit. |
| `saam:hasFund` | Account segment or dimension member | Fund acts as a structural accounting segment. |
| `saam:hasAppropriation` | Supplemental budget segment or document context | Appropriation expresses budget authority, not only ledger classification. |
| `saam:hasObjectCode` | Account classification segment | Object code hierarchy supplies reporting and control classification. |
| `saam:hasProgram` | Segment or dimension member | Program retains semantic separation from account and fund. |
| `saam:hasOrganization` | Segment or entity substructure | Organization supports responsibility-center analysis. |
| `saam:hasProject` | Segment or project dimension | Project remains optional and appears only when source data carries a project code. |
| `saam:hasFiscalPeriod` | Period context | Posting period remains queryable in the XBRL context layer. |

### SAAM Journal Entry to XBRL GL Example

The following conceptual transformation illustrates the alignment:

1. A SAAM expenditure transaction references Fund `001`, Appropriation `A123`, Object Code `E010`, Program `07`, Organization `401`, amount `1250.00`, posting date `2026-07-15`, and voucher `V0009876`.
2. The XBRL GL journal document records voucher `V0009876` as the document reference.
3. The document contains one debit line for expenditure classification and one credit line for cash or payable settlement, depending on the posting pattern in the source system.
4. The debit line carries segmented account detail that includes fund `001`, appropriation `A123`, object code `E010`, program `07`, and organization `401`.
5. The posting context records the fiscal period and agency entity.
6. The provenance package links both lines to the same business event identifier and supporting evidence set.

### XBRL GL Alignment Guidance

| Design Question | Alignment Rule | Pass Condition |
|---|---|---|
| One SAAM transaction contains multiple accounting lines | Preserve one transaction header with multiple XBRL GL entries. | Each debit-credit line remains separately queryable. |
| SAAM stores sign only at the header | Derive balanced line entries during transformation. | The XBRL GL instance balances to zero within each journal entry. |
| SAAM object code hierarchy exceeds one flat account code | Model object code, subobject, and sub-subobject as distinct classification segments. | Hierarchy remains reconstructable from the instance data. |
| SAAM appropriation acts as budget authority instead of a pure ledger account | Store appropriation as a dimension or supplemental classification with explicit semantics. | Budget-control queries distinguish appropriation from natural account. |

## FIBO Mapping

FIBO supplies formal semantics for organizations, legal entities, contractual relationships, monetary concepts, reporting structures, and financial instruments. FIBO does not focus on detailed public-sector journal entries in the same way that XBRL GL does. The most effective SAAM alignment uses FIBO for business meaning, party identity, reporting context, obligations, and monetary abstractions, then keeps detailed posting mechanics in SAAM plus XBRL GL.

### Business Entities and Instruments

| FIBO Area | Representative Concept | SAAM Relevance |
|---|---|---|
| Legal and formal organizations | Agency, department, legal entity, formal organization | Maps agencies, issuing bodies, counterparties, and accountable organizations. |
| Contracts and commitments | Agreement, commitment, obligation | Anchors appropriations, encumbrances, grants, and purchase commitments. |
| Financial instruments | Instrument, debt instrument, security, payment obligation | Supports treasury, debt-service, investment, and grant-disbursement extensions. |
| Monetary concepts | Monetary amount, currency, valuation | Normalizes amounts, currency metadata, and measurement semantics. |
| Reporting concepts | Report, reporting period, reporting party | Structures statewide, agency, and program reporting packages. |

### Financial Reporting Concepts

FIBO strengthens the semantic layer around financial reporting by separating reporting party, report artifact, period, measure, and governing policy. That separation fits SAAM reporting processes that combine agency data, statewide reporting, biennium controls, and audit evidence packages.

| Reporting Need | FIBO Pattern | SAAM Use |
|---|---|---|
| Reporting organization identity | Formal organization plus reporting party pattern | Identifies the agency, office, or statewide entity that owns the report. |
| Reporting period | Reporting period or date interval pattern | Represents fiscal month, fiscal year, or biennium. |
| Measured amount | Monetary amount and accounting measure pattern | Represents balances, budget authority, and actuals. |
| Published report artifact | Report document pattern | Represents CAFR schedules, budget status reports, or agency exports. |
| Responsibility and policy | Governing body, agreement, policy reference | Represents statutory authority, budget act linkage, or accounting rule anchor. |

### How SAAM Entities Map to FIBO

| SAAM Entity | FIBO Alignment | Mapping Type | Notes |
|---|---|---|---|
| Fund | Account or fund-like financial container plus reporting classification | Broad match | FIBO supplies monetary and reporting structure semantics; SAAM preserves public-sector fund specialization. |
| Appropriation | Budget authority or commitment pattern anchored to monetary amount and reporting period concepts | Extension-aligned match | A public-sector appropriation requires a local extension over FIBO monetary, commitment, and authority semantics. |
| Agency | Legal entity, formal organization, or organizational unit | Exact-to-broad match | Match level depends on whether the source uses legal or administrative boundaries. |
| FinancialTransaction | Accounting event or business event pattern | Broad match | FIBO frames the event semantics while XBRL GL holds posting detail. |
| ObjectCode | Classification scheme or accounting taxonomy member | Broad match | SAAM retains the detailed code hierarchy and statewide definitions. |
| Revenue or expenditure program | Purpose or reporting classification | Related match | Program semantics usually remain local and align through a classification extension. |
| Contract, grant, or debt reference | Agreement or financial instrument | Exact or broad match | Treasury and grant domains benefit most from direct FIBO reuse. |

### FIBO Extension Strategy for Public-Sector Accounting

SAAM includes concepts that exceed the default scope of commercial or capital-market ontologies. A disciplined extension strategy preserves interoperability:

| SAAM Need | FIBO Anchor | Local Extension Pattern |
|---|---|---|
| Legislative appropriation | Monetary amount, commitment, responsible party, reporting period | Define a `BudgetAuthority` subclass or profile term that references the fund, biennium, act, and limit amount. |
| Biennium | Reporting period or date interval | Define a public-sector fiscal period class with start date, end date, and statutory cycle attributes. |
| Object code hierarchy | Classification scheme | Define a state-specific classification scheme and link members through exact or narrow matches. |
| Encumbrance control | Commitment or obligation | Specialize commitment states for pre-encumbrance, encumbrance, liquidation, and expenditure conversion. |

## Schema.org Financial Extensions

Schema.org provides lightweight web semantics rather than enterprise accounting rigor. The vocabulary fits portal metadata, API payload decoration, public invoice publication, supplier-exchange pages, and linked-data discovery on the web.

### Core Financial Web Concepts

| Schema.org Concept | Role | SAAM or Accounting Link |
|---|---|---|
| `Invoice` | Payment request or billing artifact with due date, customer, and monetary totals | Represents supplier invoice metadata or customer billing artifacts linked to SAAM document references. |
| `Order` | Purchase or service order with ordered items and order date | Represents purchase-order context that precedes or justifies a journal entry. |
| `PaymentMethod` | Payment mechanism accepted or used by a system | Represents payment channel metadata for portals or e-commerce integrations. |

### Integration with Web-Based Systems

Schema.org supports a practical bridge from internal accounting records to external-facing systems:

| Web System Need | Schema.org Pattern | SAAM Connection |
|---|---|---|
| Supplier portal publishes invoice metadata | `Invoice` in JSON-LD | `saam:documentReference` links the journal entry to the published invoice URI or ID. |
| Procurement portal publishes order metadata | `Order` in JSON-LD | Purchase-order identifiers align to encumbrance or expenditure source references. |
| Citizen payment portal advertises accepted payment channels | `PaymentMethod` or `PaymentService` | Payment metadata links to receipt or cash-application transactions. |
| Search engine and crawler visibility | Embedded JSON-LD | Public financial artifacts gain discoverable structured data without exposing ledger internals. |

### Schema.org Alignment Guidance

Schema.org does not replace SAAM, XBRL GL, or FIBO. Schema.org supplies the web envelope, while SAAM and XBRL GL preserve authoritative accounting detail.

| Layer | Primary Vocabulary | Reason |
|---|---|---|
| Public web page or portal payload | Schema.org | Searchability and JSON-LD interoperability |
| Ledger exchange or audit package | XBRL GL | Detailed posting semantics |
| Entity identity and formal reporting semantics | FIBO | Shared enterprise financial vocabulary |
| Audit lineage | PROV-O | Provenance across transformations and approvals |

## W3C PROV for Audit Trail

PROV-O defines a simple and powerful provenance model based on three starting-point classes: `prov:Entity`, `prov:Activity`, and `prov:Agent`. An entity represents a thing with stable aspects. An activity represents a process or event over time. An agent represents the actor responsible for an activity or an entity. Those three classes align directly with accounting evidence chains.

### Activity, Entity, Agent Model

| PROV Class or Property | Accounting Meaning | SAAM Alignment |
|---|---|---|
| `prov:Entity` | Journal voucher, invoice, balance extract, exception file, or posted ledger line | Represents source and derived accounting artifacts. |
| `prov:Activity` | Entry creation, approval, posting, import, validation, correction, or report generation | Represents accounting lifecycle steps. |
| `prov:Agent` | Clerk, approving manager, batch service, API client, or agency | Represents accountable humans and systems. |
| `prov:used` | Activity consumed an input artifact | Posting activity uses a voucher, invoice, and coding crosswalk. |
| `prov:wasGeneratedBy` | Entity came from an activity | Ledger entry entity was generated by a posting activity. |
| `prov:wasDerivedFrom` | Entity traces back to another entity | Corrected journal derives from an original journal. |
| `prov:wasAssociatedWith` | Agent was responsible for an activity | Approver or system service participated in posting. |
| `prov:wasAttributedTo` | Entity responsibility traces to an agent | Report extract attributes to the generating agency or service. |

### Tracking Journal Entry Lineage

An accounting lineage chain often contains the following sequence:

1. A source invoice enters the system as a `prov:Entity`.
2. A coding activity assigns fund, appropriation, object code, and program values.
3. An approval activity reviews the coded record and records the responsible agent.
4. A posting activity generates the authoritative journal-entry entities.
5. A reporting activity derives balance, status, or compliance report entities from the posted entries.
6. A correction activity derives a reversing or adjusting entry from the original posted entry.

That sequence creates a machine-readable audit path from source document to derived report.

### Audit Evidence Representation

| Evidence Need | PROV Pattern | Pass Condition |
|---|---|---|
| Who approved a journal | `prov:Activity` plus `prov:wasAssociatedWith` | Query returns the activity, agent, and timestamp. |
| Which document produced a posted line | `prov:used` and `prov:wasGeneratedBy` | Query walks from posted line to source document without ambiguity. |
| Which report contains balances derived from a transaction | `prov:wasDerivedFrom` chain | Query reaches report artifact and intermediary calculations. |
| Which service imported external data | `prov:Agent` for the service principal | The responsible system identity remains visible. |
| Which correction superseded an original posting | `prov:wasDerivedFrom` plus revision property | The supersession chain remains intact. |

## Unified Alignment Table

The following table captures the primary crosswalk between SAAM and the external standards.

| SAAM Concept | XBRL GL Alignment | FIBO Alignment | Schema.org Alignment | PROV Alignment | Mapping Notes |
|---|---|---|---|---|---|
| SAAM Fund | Account hierarchy segment, ledger dimension, or account member | Fund-like financial container and reporting classification | None in core web vocabulary | `prov:Entity` for governed fund master data | Fund remains a public-sector semantic type in SAAM; external mappings focus on exchange and classification. |
| SAAM Transaction | Journal entry header and line `Entry` set | Accounting or business event | Linked document references through `Invoice` or `Order` when published | `prov:Entity` for the transaction record plus `prov:Activity` for posting | XBRL GL carries the richest line-level representation. |
| SAAM Appropriation | Budget or authority dimension linked to journal context | Budget authority concept anchored to monetary amount, commitment, responsible party, and period | None in core web vocabulary | `prov:Entity` for the budget authorization record | A local FIBO extension preserves public-sector authority semantics. |
| SAAM ObjectCode | Classification taxonomy within account structure | Classification-scheme member | None in core web vocabulary | `prov:Entity` for the classification master | State hierarchy remains authoritative in SAAM. |
| SAAM Agency | Reporting entity or organization context | Legal entity, formal organization, or organizational unit | `Organization` when exposed on the web | `prov:Agent` and `prov:Entity` | Agency identity participates in both business and provenance views. |
| SAAM Source Document | Document reference | Agreement, invoice-related business artifact, or report artifact | `Invoice` or `Order` | `prov:Entity` | Source evidence often bridges all four standards. |

### Required Alignment Rows

| Requested Mapping | Standardized Interpretation | Observable Pass Condition |
|---|---|---|
| SAAM Fund → XBRL GL Account hierarchy | Fund appears as a first-class account segment or dimension member in each exported journal line. | Query on the XBRL GL instance groups entries by fund without string parsing. |
| SAAM Transaction → XBRL GL Entry | Transaction header expands into one or more balanced XBRL GL entries. | Each exported journal entry contains line-level debit-credit detail. |
| SAAM Appropriation → FIBO Budget concept | Appropriation maps to a budget-authority extension anchored to FIBO monetary, commitment, party, and reporting-period semantics. | The graph links authority amount, responsible entity, and fiscal cycle in one pattern. |
| SAAM ObjectCode → Classification taxonomy | Object code maps to a classification scheme with explicit broader-narrower hierarchy. | The hierarchy resolves from object code to subobject and sub-subobject levels. |

## .NET Integration Patterns

### dotNetRDF for RDF and OWL Processing

`dotNetRDF` provides a practical .NET runtime surface for loading Turtle, RDF/XML, JSON-LD, and SPARQL assets. A SAAM integration service typically loads the SAAM ontology, loads instance triples from one or more operational feeds, and executes validation or export queries over one merged triple store.

```csharp
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

var saamGraph = new Graph();
FileLoader.Load(saamGraph, @".github\skills\saam-ontology\ontology\saam-core.ttl");

var instanceGraph = new Graph();
StringParser.Parse(instanceGraph, @"
@prefix saam: <https://example.org/saam#> .
@prefix ex: <https://example.org/txn/> .

ex:txn1 a saam:FinancialTransaction ;
    saam:transactionAmount ""1250.00"" ;
    saam:documentReference ""V0009876"" .
");

var store = new TripleStore();
store.Add(saamGraph);
store.Add(instanceGraph);

var parser = new SparqlQueryParser();
var query = parser.ParseFromString(@"
PREFIX saam: <https://example.org/saam#>
SELECT ?transaction ?fund ?appropriation ?objectCode
WHERE {
  ?transaction a saam:FinancialTransaction ;
               saam:hasFund ?fund ;
               saam:hasAppropriation ?appropriation ;
               saam:hasObjectCode ?objectCode .
}
");

var processor = new LeviathanQueryProcessor(store);
var results = processor.ProcessQuery(query);
```

### SPARQL Query Patterns

| Query Goal | SPARQL Pattern | Result Shape |
|---|---|---|
| Find transactions missing required dimensions | `FILTER NOT EXISTS` over fund, appropriation, or object code relationships | Exception rows for data-quality workflows |
| Trace a posting back to source evidence | Join `saam:documentReference` with `prov:used` and `prov:wasGeneratedBy` | Lineage chain from document to posted entry |
| Group journal lines by fund and object code | Aggregate over aligned XBRL GL or SAAM triples | Control totals and classification summaries |
| Find appropriations that exceed authority | Compare actual transaction aggregates to authority amount nodes | Budget-compliance exception set |
| Enumerate crosswalk members | Query `skos:exactMatch`, `skos:broadMatch`, or equivalent alignment predicates | Mapping inventory for governance review |

### Ontology Validation with SHACL

SHACL shapes add executable constraints over RDF data. A SAAM validation pack benefits from shapes that assert required transaction dimensions, valid object-code hierarchy membership, and provenance completeness for auditable events.

| SHACL Shape | Validation Focus | Pass Condition |
|---|---|---|
| Transaction completeness shape | Every `saam:FinancialTransaction` has fund, appropriation, object code, amount, and date. | Zero focus nodes violate required-property rules. |
| Fund-authority consistency shape | Every appropriation links to exactly one fund and one fiscal cycle. | Zero orphan or multiply linked appropriations remain. |
| Object-code taxonomy shape | Every subobject links to a parent object code. | The classification hierarchy contains no disconnected nodes. |
| Provenance completeness shape | Every posted journal entry links to a posting activity and responsible agent. | Audit lineage exists for every posted entry. |

### Validation Workflow in .NET

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Load SAAM ontology, instance data, and alignment graph into one store. | Parse all graphs. | Zero parser errors occur. |
| 2 | Execute SPARQL prechecks for missing identifiers and duplicate transaction keys. | Run exception query pack. | Exception set size matches expected baseline or drops to zero. |
| 3 | Execute SHACL shapes over transaction and provenance graphs. | Run validation engine. | Validation report contains zero violations for release data. |
| 4 | Export aligned facts to JSON-LD, Turtle, or tabular projection. | Run serializer. | Output contains stable IRIs and expected classification segments. |

## Use Cases

### Semantic Validation of Transactions

SAAM transactions benefit from explicit semantic validation when a pipeline validates required coding dimensions, checks appropriation-to-fund consistency, verifies object-code taxonomy membership, and confirms provenance coverage for posted entries.

### Cross-System Data Integration

An integration layer aligns CGI, AFRS, procurement, payroll, grants, and reporting systems by using SAAM as the domain anchor, XBRL GL as the journal exchange shape, FIBO as the enterprise financial vocabulary, Schema.org as the web metadata envelope, and PROV as the lineage backbone.

### Automated Compliance Checking

Compliance rules evaluate appropriation authority, reporting-period validity, approval presence, segregation-of-duty evidence, and published-report derivation by querying the aligned graph. The graph exposes the control context as data rather than hidden application logic.

### Knowledge Graph Construction

A knowledge graph built from SAAM, XBRL GL mappings, FIBO-aligned party and monetary semantics, and PROV lineage supports multi-hop questions such as:

- Which invoices produced expenditures in one appropriation during a fiscal period?
- Which adjustments reversed a prior entry and who approved the correction?
- Which reports consumed transactions from a given fund and agency?
- Which object-code branches show unusual growth relative to budget authority?

## Recommended Alignment Architecture

| Layer | Primary Asset | Responsibility |
|---|---|---|
| Domain ontology | SAAM ontology | Governs public-sector accounting concepts and state-specific coding. |
| Exchange taxonomy | XBRL GL | Transports detailed journal-entry structure across systems. |
| Enterprise financial semantics | FIBO | Governs party, obligation, monetary, and reporting abstractions. |
| Web metadata layer | Schema.org | Publishes discoverable invoice, order, and payment metadata. |
| Provenance layer | PROV-O | Preserves lineage, responsibility, and derivation chains. |
| Validation layer | SHACL and SPARQL | Enforces semantic completeness and control rules. |

## Verification Matrix

| Check | Test | Pass Criteria |
|---|---|---|
| Front matter validity | Run `npm run frontmatter:validate`. | Validation exits with code `0`. |
| Relative skill links | Resolve links to the four referenced skill files. | Each linked file exists. |
| SAAM anchor coverage | Compare the reference to `saam:Fund`, `saam:Appropriation`, `saam:ObjectCode`, and `saam:FinancialTransaction`. | Each required SAAM concept appears in at least one alignment table. |
| External ontology coverage | Review sections for XBRL GL, FIBO, Schema.org, and PROV-O. | All four standards have dedicated sections and mapping guidance. |
| Provenance coverage | Review the audit-lineage section and tables. | Entity, Activity, and Agent mappings appear explicitly. |
| Integration readiness | Review .NET, SPARQL, and SHACL sections. | The document includes one concrete runtime path for graph loading, querying, and validation. |

## Reference Pointers

| Resource | Use in This Reference |
|---|---|
| XBRL GL 2015 framework publication | Confirms detailed-ledger focus and SRCD linkage to reporting taxonomies. |
| FIBO viewer and ontology structure pages | Confirm the ontology-set structure and enterprise financial scope. |
| Schema.org `Invoice`, `Order`, and `PaymentMethod` pages | Confirm web-facing financial metadata patterns. |
| W3C PROV-O specification | Confirms the `Entity`, `Activity`, and `Agent` starting-point model and lineage properties. |

## Summary

SAAM supplies the authoritative public-sector accounting vocabulary. XBRL GL supplies detailed journal exchange. FIBO supplies formal enterprise financial semantics. Schema.org supplies web publication metadata. PROV-O supplies audit lineage. Together, those standards form a practical semantic stack for accounting integration, validation, compliance checking, and knowledge-graph construction.
