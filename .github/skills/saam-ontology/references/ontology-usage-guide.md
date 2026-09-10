---
title: SAAM Ontology Usage Guide
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
complexity: high
estimated_tokens: 4860
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
  - saam-ontology
related_docs:
  - ../SKILL.md
  - ../ontology/saam-core.ttl
  - ../../../../docs/references/SAAM/Ontology/saam-fabric-semantic-model.json
source_paths:
  - .github/skills/saam-ontology/ontology/saam-core.ttl
  - docs/references/SAAM/Ontology/saam-fabric-semantic-model.json
tags:
  - saam
  - ontology
  - rdf
  - sparql
  - semantic-web
  - washington-state
---
# SAAM Ontology Usage Guide

Agent uses this guide with `saam-ontology` for query authoring, validation design, Fabric semantic-model projection, and .NET graph integration.

## Asset Inventory

| Asset | Role | Location |
|---|---|---|
| Core Turtle ontology | Canonical skill-packaged ontology asset | `../ontology/saam-core.ttl` |
| Source Turtle ontology | Upstream source used for the packaged copy | `../../../docs/references/SAAM/Ontology/saam-core-turtle.txt` |
| OWL serialization | Alternate XML serialization of the same ontology | `../../../docs/references/SAAM/Ontology/saam-core-owl.xml` |
| JSON-LD serialization | Alternate linked-data projection | `../../../docs/references/SAAM/Ontology/saam-core-jsonld.json` |
| Fabric semantic model | Tabular projection aligned to ontology terms | `../../../docs/references/SAAM/Ontology/saam-fabric-semantic-model.json` |

## Namespace Bootstrap

Use this prefix set at the top of SPARQL files and dotNetRDF query strings.

```sparql
PREFIX saam:    <http://ontology.saam.wa.gov/saam#>
PREFIX afrs:    <http://ontology.saam.wa.gov/afrs#>
PREFIX cgi:     <http://ontology.saam.wa.gov/cgi#>
PREFIX dot:     <http://ontology.saam.wa.gov/dot#>
PREFIX err:     <http://ontology.saam.wa.gov/error#>
PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs:    <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl:     <http://www.w3.org/2002/07/owl#>
PREFIX xsd:     <http://www.w3.org/2001/XMLSchema#>
PREFIX skos:    <http://www.w3.org/2004/02/skos/core#>
```

## Query Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent loads the ontology graph and confirms prefix coverage. | Run a class count query. | Query returns SAAM, AFRS, CGI, DOT, or error classes. |
| 2 | Agent decides whether the graph is schema-only or schema-plus-instance. | Review data source inputs. | Query shape matches the available triples. |
| 3 | Agent writes one narrow SPARQL query per business question. | Review query set. | Each query has one observable result target. |
| 4 | Agent projects exception rows or tabular outputs when validation is the goal. | Review result columns. | Output names expose the failing term and business key. |
| 5 | Agent aligns analytics outputs to the Fabric semantic model when reporting is in scope. | Review table, column, and relationship mappings. | Fabric objects resolve to named ontology anchors. |

## Vocabulary Inventory Queries

### List all ontology classes

```sparql
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl:  <http://www.w3.org/2002/07/owl#>

SELECT ?class ?label
WHERE {
  ?class rdf:type owl:Class .
  OPTIONAL { ?class rdfs:label ?label }
}
ORDER BY ?class
```

### List SAAM object properties

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl:  <http://www.w3.org/2002/07/owl#>

SELECT ?property ?label ?domain ?range
WHERE {
  ?property rdf:type owl:ObjectProperty ;
            rdfs:domain ?domain ;
            rdfs:range ?range .
  FILTER(STRSTARTS(STR(?property), STR(saam:)))
  OPTIONAL { ?property rdfs:label ?label }
}
ORDER BY ?property
```

### List SAAM datatype properties

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl:  <http://www.w3.org/2002/07/owl#>

SELECT ?property ?label ?domain ?range
WHERE {
  ?property rdf:type owl:DatatypeProperty ;
            rdfs:domain ?domain ;
            rdfs:range ?range .
  FILTER(STRSTARTS(STR(?property), STR(saam:)))
  OPTIONAL { ?property rdfs:label ?label }
}
ORDER BY ?property
```

### List subclasses of `saam:Fund`

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?fundClass ?label
WHERE {
  ?fundClass rdfs:subClassOf saam:Fund .
  OPTIONAL { ?fundClass rdfs:label ?label }
}
ORDER BY ?fundClass
```

## Appropriation and Fund Queries

### Resolve appropriation lineage from ontology structure

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?property ?domain ?range
WHERE {
  VALUES ?property { saam:hasBiennium saam:belongsToFund }
  ?property rdfs:domain ?domain ;
            rdfs:range ?range .
}
ORDER BY ?property
```

### Resolve appropriation instances to fund and biennium

This query expects a data graph in addition to the schema graph.

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?appropriation ?appropriationCode ?fund ?fundCode ?biennium ?bienniumCode
WHERE {
  ?appropriation rdf:type saam:Appropriation ;
                 saam:appropriationCode ?appropriationCode ;
                 saam:belongsToFund ?fund ;
                 saam:hasBiennium ?biennium .
  OPTIONAL { ?fund saam:fundCode ?fundCode }
  OPTIONAL { ?biennium saam:bienniumCode ?bienniumCode }
}
ORDER BY ?appropriationCode
```

### Find appropriations missing required lineage

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?appropriation ?appropriationCode
WHERE {
  ?appropriation rdf:type saam:Appropriation ;
                 saam:appropriationCode ?appropriationCode .
  FILTER NOT EXISTS { ?appropriation saam:belongsToFund ?fund }
  UNION
  {
    ?appropriation rdf:type saam:Appropriation ;
                   saam:appropriationCode ?appropriationCode .
    FILTER NOT EXISTS { ?appropriation saam:hasBiennium ?biennium }
  }
}
ORDER BY ?appropriationCode
```

## Transaction Validation Queries

The shipped TTL is a vocabulary. Validation needs instance triples loaded into a second graph or the same merged graph. Agent keeps the schema triples and the transaction triples separate in source control, then merges them in the validation process.

### Example transaction instance shape

```turtle
@prefix saam: <http://ontology.saam.wa.gov/saam#> .
@prefix ex:   <http://example.org/saam/> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .

ex:txn-1001 a saam:Expenditure ;
    saam:transactionAmount "1250.00"^^xsd:decimal ;
    saam:transactionDate "2026-07-15"^^xsd:date ;
    saam:documentReference "JV-1001" ;
    saam:hasAgency ex:agency-4100 ;
    saam:hasFund ex:fund-001 ;
    saam:hasAppropriation ex:app-100 ;
    saam:hasObjectCode ex:obj-04-00 ;
    saam:hasFiscalPeriod ex:fp-2026-01 .
```

### Find financial transactions missing key accounting links

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?transaction ?missingLink
WHERE {
  ?transaction rdf:type saam:FinancialTransaction .
  {
    FILTER NOT EXISTS { ?transaction saam:hasFund ?fund }
    BIND("hasFund" AS ?missingLink)
  }
  UNION
  {
    FILTER NOT EXISTS { ?transaction saam:hasAppropriation ?appropriation }
    BIND("hasAppropriation" AS ?missingLink)
  }
  UNION
  {
    FILTER NOT EXISTS { ?transaction saam:hasObjectCode ?objectCode }
    BIND("hasObjectCode" AS ?missingLink)
  }
  UNION
  {
    FILTER NOT EXISTS { ?transaction saam:hasFiscalPeriod ?period }
    BIND("hasFiscalPeriod" AS ?missingLink)
  }
}
ORDER BY ?transaction ?missingLink
```

### Validate appropriation-to-fund consistency across linked instances

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?transaction ?transactionFund ?appropriation ?appropriationFund
WHERE {
  ?transaction rdf:type saam:FinancialTransaction ;
               saam:hasFund ?transactionFund ;
               saam:hasAppropriation ?appropriation .
  ?appropriation saam:belongsToFund ?appropriationFund .
  FILTER(?transactionFund != ?appropriationFund)
}
ORDER BY ?transaction
```

### Validate transaction presence by ASK query

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

ASK {
  ?transaction rdf:type saam:FinancialTransaction ;
               saam:hasAgency ?agency ;
               saam:hasFund ?fund ;
               saam:hasAppropriation ?appropriation ;
               saam:hasObjectCode ?objectCode ;
               saam:hasFiscalPeriod ?period .
}
```

### Validate payments linked to vendor or employee

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?payment ?issue
WHERE {
  ?payment rdf:type saam:Payment .
  {
    FILTER NOT EXISTS { ?payment saam:hasVendor ?vendor }
    FILTER NOT EXISTS { ?payment saam:hasEmployee ?employee }
    BIND("missing payee link" AS ?issue)
  }
}
ORDER BY ?payment
```

## Object-Code Hierarchy Queries

### List object-code hierarchy definitions from the ontology

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT ?class ?parent ?label
WHERE {
  VALUES ?class { saam:ObjectCode saam:SubObject saam:SubSubObject }
  OPTIONAL { ?class rdfs:subClassOf ?parent }
  OPTIONAL { ?class rdfs:label ?label }
}
ORDER BY ?class
```

### Roll transactions up by object-code class

This query expects instance triples that connect transactions to object-code resources.

```sparql
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX xsd:  <http://www.w3.org/2001/XMLSchema#>

SELECT ?objectCode (SUM(xsd:decimal(?amount)) AS ?totalAmount)
WHERE {
  ?transaction rdf:type saam:FinancialTransaction ;
               saam:hasObjectCode ?objectCode ;
               saam:transactionAmount ?amount .
}
GROUP BY ?objectCode
ORDER BY DESC(?totalAmount)
```

## AFRS and CGI Operational Queries

### Find AFRS error records and codes

```sparql
PREFIX afrs: <http://ontology.saam.wa.gov/afrs#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?errorRecord ?errorCode ?description
WHERE {
  ?errorRecord rdf:type afrs:ErrorRecord ;
               afrs:hasErrorCode ?errorCode .
  OPTIONAL { ?errorRecord afrs:errorDescription ?description }
}
ORDER BY ?errorRecord
```

### Find input records that produced errors

```sparql
PREFIX afrs: <http://ontology.saam.wa.gov/afrs#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?record ?errorRecord
WHERE {
  ?record rdf:type afrs:InputRecord ;
          afrs:producesError ?errorRecord .
}
ORDER BY ?record
```

### Find CGI batch jobs with return codes and shadow-table impact

```sparql
PREFIX cgi: <http://ontology.saam.wa.gov/cgi#>
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?job ?returnCode ?shadowTable
WHERE {
  ?job rdf:type cgi:BatchJob ;
       cgi:hasReturnCode ?returnCode .
  OPTIONAL { ?job cgi:affectsShadowTable ?shadowTable }
}
ORDER BY ?job
```

## Revenue-Source Extension Pattern

The current core TTL defines `saam:Revenue`, `saam:ProgramRevenue`, and `saam:GeneralRevenue`. The current core TTL does not declare a `saam:RevenueSource` class. Agent preserves the packaged TTL and layers a local extension namespace when revenue-source codes need semantic identity.

### Example extension triples

```turtle
@prefix saam:    <http://ontology.saam.wa.gov/saam#> .
@prefix saamext: <http://ontology.saam.wa.gov/extensions/saam#> .
@prefix rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix rdfs:    <http://www.w3.org/2000/01/rdf-schema#> .
@prefix xsd:     <http://www.w3.org/2001/XMLSchema#> .
@prefix owl:     <http://www.w3.org/2002/07/owl#> .

saamext:RevenueSource a owl:Class ;
    rdfs:label "Revenue Source"@en .

saamext:revenueSourceCode a owl:DatatypeProperty ;
    rdfs:domain saamext:RevenueSource ;
    rdfs:range xsd:string ;
    rdfs:label "revenue source code"@en .

saamext:classifiesRevenue a owl:ObjectProperty ;
    rdfs:domain saamext:RevenueSource ;
    rdfs:range saam:Revenue ;
    rdfs:label "classifies revenue"@en .
```

### Query revenue-source extensions

```sparql
PREFIX saam:    <http://ontology.saam.wa.gov/saam#>
PREFIX saamext: <http://ontology.saam.wa.gov/extensions/saam#>
PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?revenueSource ?code ?revenueClass
WHERE {
  ?revenueSource rdf:type saamext:RevenueSource ;
                 saamext:revenueSourceCode ?code ;
                 saamext:classifiesRevenue ?revenueClass .
}
ORDER BY ?code
```

### Find revenue transactions with no revenue-source extension

```sparql
PREFIX saam:    <http://ontology.saam.wa.gov/saam#>
PREFIX saamext: <http://ontology.saam.wa.gov/extensions/saam#>
PREFIX rdf:     <http://www.w3.org/1999/02/22-rdf-syntax-ns#>

SELECT ?transaction
WHERE {
  ?transaction rdf:type saam:Revenue .
  FILTER NOT EXISTS {
    ?transaction saamext:hasRevenueSource ?revenueSource .
  }
}
ORDER BY ?transaction
```

## Fabric Semantic-Model Alignment

The reference semantic model declares ontology alignment in JSON with `ontologyClass` and `ontologyProperty` fields. Agent uses the ontology as the semantic contract and the JSON model as the tabular projection.

### Mapping workflow

| Fabric Artifact | Ontology Evidence | Example |
|---|---|---|
| Table | `ontologyClass` in semantic-model JSON | `Fund` maps to `saam:Fund` |
| Column | `ontologyProperty` in semantic-model JSON | `FundCode` maps to `saam:fundCode` |
| Relationship | Object-property or foreign-key projection | `Appropriation.FundCode` maps to `saam:belongsToFund` |
| Hierarchy | Subclass or code-level hierarchy | `Object Hierarchy` aligns to `ObjectCode`, `SubObject`, `SubSubObject` |
| Measure | Derived analytic expression anchored to ontology-linked columns | `Budget Utilization %` derives from appropriation and transaction facts |

### Query tables that carry ontology-class mappings from JSON-LD-converted metadata

If the Fabric JSON is converted into RDF, this query pattern lists table-to-class mappings.

```sparql
PREFIX ex: <http://example.org/fabric#>

SELECT ?tableName ?ontologyClass
WHERE {
  ?table ex:tableName ?tableName ;
         ex:ontologyClass ?ontologyClass .
}
ORDER BY ?tableName
```

### Direct JSON inspection checklist

| Check | Test | Pass |
|---|---|---|
| Table coverage | Inspect `tables[*].ontologyClass`. | Curated dimensions and facts reference ontology classes. |
| Column coverage | Inspect `columns[*].ontologyProperty`. | Governed key and attribute columns reference ontology properties where available. |
| Relationship fidelity | Inspect relationship names and foreign keys. | Tabular joins preserve ontology semantics. |
| Hierarchy fidelity | Inspect hierarchy level order. | Level order matches ontology hierarchy or code hierarchy. |
| Governance metadata | Inspect workspace guidance and sensitivity labels. | Fabric metadata stays traceable and audit ready. |

## dotNetRDF Integration Patterns

### Load the ontology into a graph

```csharp
using System;
using VDS.RDF;
using VDS.RDF.Parsing;

var graph = new Graph();
FileLoader.Load(graph, @"C:\dev\w\journal-loader\.github\skills\saam-ontology\ontology\saam-core.ttl");

Console.WriteLine($"Triples: {graph.Triples.Count}");
```

### Register namespace prefixes

```csharp
using VDS.RDF;

var graph = new Graph();
graph.NamespaceMap.AddNamespace("saam", UriFactory.Create("http://ontology.saam.wa.gov/saam#"));
graph.NamespaceMap.AddNamespace("afrs", UriFactory.Create("http://ontology.saam.wa.gov/afrs#"));
graph.NamespaceMap.AddNamespace("cgi", UriFactory.Create("http://ontology.saam.wa.gov/cgi#"));
graph.NamespaceMap.AddNamespace("dot", UriFactory.Create("http://ontology.saam.wa.gov/dot#"));
graph.NamespaceMap.AddNamespace("err", UriFactory.Create("http://ontology.saam.wa.gov/error#"));
```

### Run a SPARQL SELECT query in .NET

```csharp
using System;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

var graph = new Graph();
FileLoader.Load(graph, @"C:\dev\w\journal-loader\.github\skills\saam-ontology\ontology\saam-core.ttl");

var queryText = @"
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX owl:  <http://www.w3.org/2002/07/owl#>
SELECT ?class
WHERE {
  ?class rdf:type owl:Class .
  FILTER(STRSTARTS(STR(?class), STR(saam:)))
}
ORDER BY ?class";

var parser = new SparqlQueryParser();
var query = parser.ParseFromString(queryText);
var processor = new LeviathanQueryProcessor(graph);
var result = processor.ProcessQuery(query);

if (result is SparqlResultSet rows)
{
    foreach (var row in rows)
    {
        Console.WriteLine(row["class"]);
    }
}
```

### Merge schema and instance graphs

```csharp
using VDS.RDF;
using VDS.RDF.Parsing;

var schemaGraph = new Graph();
var dataGraph = new Graph();

FileLoader.Load(schemaGraph, @"C:\dev\w\journal-loader\.github\skills\saam-ontology\ontology\saam-core.ttl");
FileLoader.Load(dataGraph, @"C:\dev\w\journal-loader\data\saam-instance-data.ttl");

var store = new TripleStore();
store.Add(schemaGraph, true);
store.Add(dataGraph, true);
```

### Run validation and emit exception rows

```csharp
using System;
using System.Collections.Generic;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

var graph = new Graph();
FileLoader.Load(graph, @"C:\dev\w\journal-loader\.github\skills\saam-ontology\ontology\saam-core.ttl");
FileLoader.Load(graph, @"C:\dev\w\journal-loader\data\saam-instance-data.ttl");

var queryText = @"
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT ?transaction ?missingLink
WHERE {
  ?transaction rdf:type saam:FinancialTransaction .
  {
    FILTER NOT EXISTS { ?transaction saam:hasFund ?fund }
    BIND(""hasFund"" AS ?missingLink)
  }
  UNION
  {
    FILTER NOT EXISTS { ?transaction saam:hasAppropriation ?appropriation }
    BIND(""hasAppropriation"" AS ?missingLink)
  }
}";

var query = new SparqlQueryParser().ParseFromString(queryText);
var processor = new LeviathanQueryProcessor(graph);
var result = processor.ProcessQuery(query) as SparqlResultSet;
var exceptions = new List<(string Transaction, string MissingLink)>();

if (result is not null)
{
    foreach (var row in result)
    {
        exceptions.Add((
            row["transaction"].ToString(),
            row["missingLink"].ToString()));
    }
}

foreach (var item in exceptions)
{
    Console.WriteLine($"{item.Transaction}|{item.MissingLink}");
}
```

### Project ontology-aligned rows for Fabric dimensions

```csharp
using System.Collections.Generic;
using VDS.RDF.Query;

var queryText = @"
PREFIX saam: <http://ontology.saam.wa.gov/saam#>
PREFIX rdf:  <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
SELECT ?fund ?fundCode ?label
WHERE {
  ?fund rdf:type saam:Fund ;
        saam:fundCode ?fundCode .
  OPTIONAL { ?fund <http://www.w3.org/2000/01/rdf-schema#label> ?label }
}
ORDER BY ?fundCode";

var rows = new List<Dictionary<string, string>>();
var results = (SparqlResultSet)new LeviathanQueryProcessor(graph)
    .ProcessQuery(new SparqlQueryParser().ParseFromString(queryText));

foreach (var row in results)
{
    rows.Add(new Dictionary<string, string>
    {
        ["FundIri"] = row["fund"].ToString(),
        ["FundCode"] = row["fundCode"].ToString(),
        ["FundName"] = row.TryGetValue("label", out var label) ? label.ToString() : string.Empty
    });
}
```

## Common Scenario Query Pack

| Scenario | Query Pattern | Output Shape |
|---|---|---|
| Ontology inventory | Class and property listing queries | One row per ontology term |
| Fund hierarchy audit | Subclass or instance listing for fund types | One row per fund class or fund instance |
| Appropriation control | Appropriation-to-fund and appropriation-to-biennium joins | One row per appropriation lineage path |
| Transaction completeness | Missing-link validation query | One row per transaction violation |
| Payment payee validation | Vendor or employee linkage query | One row per invalid payment |
| Error reconciliation | AFRS error-record query | One row per error record and error code |
| Integration operations | CGI job and event query | One row per job or event link |
| Revenue-source governance | Extension query | One row per revenue-source mapping |
| Fabric curation | Ontology-to-table mapping review | One row per curated table or column |

## Validation Rule Pack

| Rule | SPARQL Form | Pass |
|---|---|---|
| Financial transaction carries core accounting links | `ASK` or missing-link `SELECT` | Each transaction has agency, fund, appropriation, object code, and fiscal period links. |
| Appropriation aligns to one fund and one biennium | `SELECT` exception query | No appropriation misses fund or biennium lineage. |
| Transaction fund matches appropriation fund | `SELECT` mismatch query | Zero mismatched transaction-to-appropriation fund pairs remain. |
| Payment links to payee | `SELECT` exception query | Each payment links to a vendor or employee. |
| Revenue transaction carries governed revenue-source mapping | Extension `SELECT` exception query | Each revenue transaction resolves to one governed revenue-source concept or crosswalk row. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Query expects instance triples when only the schema TTL is loaded | Agent checks graph scope before authoring validation queries. |
| Prefix declarations drift between SPARQL files and .NET code | Agent centralizes the namespace bootstrap and reuses it verbatim. |
| Revenue-source codes land in free-text columns only | Agent publishes a governed extension class or a crosswalk table tied to revenue facts. |
| Fabric tables drop ontology IRIs during extraction | Agent carries ontology class and property identifiers into curation metadata. |
| Validation output hides the failing business key | Agent returns document references, fund codes, appropriation codes, or transaction IRIs in exception rows. |
