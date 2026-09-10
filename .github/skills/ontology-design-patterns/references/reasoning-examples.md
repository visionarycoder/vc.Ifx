---
title: Advanced Ontology Reasoning Examples
doc_type: reference
status: active
last_updated: 2026-08-31
summary: Advanced SWRL, SHACL, OWL, and .NET reasoning patterns for SAAM-aligned compliance, validation, and semantic query scenarios.
target_audience: ai
tags:
  - ontology
  - swrl
  - shacl
  - owl
  - reasoning
  - validation
  - semantic-web
related_docs:
  - ../SKILL.md
  - ../../saam-ontology/SKILL.md
  - ../../knowledge-graph-patterns/SKILL.md
  - ./accounting-ontologies.md
source_paths:
  - ../../saam-ontology/ontology/saam-core.ttl
---
# Advanced Ontology Reasoning Examples

This reference documents advanced semantic reasoning patterns that extend the ontology design workflow with executable rules, declarative constraints, and inferencing examples. The examples use W3C semantic web technologies and align with SAAM fund, appropriation, transaction, organization, and account semantics.

## Related Skill Crosswalk

| Skill | Relationship | Observable Reuse Point |
|---|---|---|
| [Ontology Design Patterns](../SKILL.md) | Supplies class, property, identity, and axiom design guidance. | The examples reuse explicit classes, object properties, data properties, and reasoning boundaries. |
| [SAAM Ontology](../../saam-ontology/SKILL.md) | Supplies accounting classes and relationship anchors. | Rules and shapes reference `saam:Fund`, `saam:Appropriation`, `saam:FinancialTransaction`, and SAAM coding links. |
| [Knowledge Graph Patterns](../../knowledge-graph-patterns/SKILL.md) | Supplies graph traversal and query projection guidance. | Inference examples project into multi-hop compliance, lineage, and hierarchy queries. |
| [Accounting Ontology Alignment Reference](./accounting-ontologies.md) | Supplies accounting semantics, .NET graph processing context, and validation workflow patterns. | Examples reuse SAAM accounting control concepts and dotNetRDF integration patterns. |

## Reasoning Stack Overview

| Technology | Primary Role | Best Fit | Observable Output |
|---|---|---|---|
| OWL 2 | Class axioms, property semantics, equivalence, disjointness, and inferencing | Stable conceptual semantics | Derived class membership and inferred relationships |
| SWRL | Conditional business rules over ontology facts | Compliance checks and derived warning flags | Inferred exceptions, control flags, and derived status triples |
| SHACL | Graph validation with property and node constraints | Data-quality and contract validation | Validation report with focus nodes and violations |
| SPARQL 1.1 | Query, aggregation, and exception extraction | Audit review, export, and operational verification | Exception sets, rollups, and trace outputs |

## Namespace Baseline

```turtle
@prefix saam: <https://example.org/saam#> .
@prefix org:  <http://www.w3.org/ns/org#> .
@prefix owl:  <http://www.w3.org/2002/07/owl#> .
@prefix prov: <http://www.w3.org/ns/prov#> .
@prefix sh:   <http://www.w3.org/ns/shacl#> .
@prefix swrl: <http://www.w3.org/2003/11/swrl#> .
@prefix xsd:  <http://www.w3.org/2001/XMLSchema#> .
@prefix ex:   <https://example.org/resource/> .
```

## Example Integrated Fact Set

The following example facts provide one compact dataset that supports SWRL, SHACL, OWL, and SPARQL examples in this reference.

```turtle
ex:Fund03 a saam:GovernmentalFund ;
    saam:fundCode "003" ;
    saam:restrictionCategory "Governmental" .

ex:Fund88 a saam:Fund ;
    saam:fundCode "088" ;
    saam:restrictionCategory "FederalRestricted" .

ex:App25 a saam:Appropriation ;
    saam:appropriationCode "A2501" ;
    saam:authorizedAmount "5000.00"^^xsd:decimal ;
    saam:remainingAuthorityAmount "450.00"^^xsd:decimal ;
    saam:appropriationLifecycleState "Active" ;
    saam:belongsToFund ex:Fund03 ;
    saam:administeredByAgency ex:Agency10 ;
    saam:hasBiennium ex:Biennium2025_2027 .

ex:Object5110 a saam:SalariesObjectCode .
ex:Object7999 a saam:NonCapitalObjectCode .

ex:Txn100 a saam:FinancialTransaction ;
    saam:transactionAmount "1250.00"^^xsd:decimal ;
    saam:transactionDate "2026-08-15"^^xsd:date ;
    saam:documentReference "JV-90017" ;
    saam:hasFund ex:Fund88 ;
    saam:hasAppropriation ex:App25 ;
    saam:hasObjectCode ex:Object5110 ;
    saam:hasAgency ex:Agency10 ;
    saam:hasBiennium ex:Biennium2025_2027 .

ex:Txn200 a saam:FinancialTransaction ;
    saam:transactionAmount "200.00"^^xsd:decimal ;
    saam:transactionDate "2026-08-16"^^xsd:date ;
    saam:documentReference "JV-90018" ;
    saam:hasFund ex:Fund03 ;
    saam:hasAppropriation ex:App25 ;
    saam:hasObjectCode ex:Object7999 ;
    saam:hasAgency ex:Agency77 ;
    saam:hasBiennium ex:Biennium2027_2029 .
```

### Sample Fact-Set Expectations

| Resource | Expected Outcome | Observable Reason |
|---|---|---|
| `ex:Txn100` | Fund-restriction violation | Salary object code links to a non-governmental fund. |
| `ex:Txn100` | Budget-authority violation | Transaction amount exceeds remaining authority. |
| `ex:Txn100` | High-risk classification | Budget-authority and fund-restriction flags co-occur. |
| `ex:Txn200` | Entity-alignment violation | Agency and biennium facts differ from the linked appropriation context. |
| `ex:App25` | SHACL conformance for core fields | Required code, amount, state, and fund fields exist. |

## SWRL Compliance Rules

SWRL rules express business logic in the form `IF [conditions] THEN [consequences]`. The rules below model compliance detection, derived classifications, and cross-entity control checks.

### SWRL Pattern Table

| Rule Family | Control Question | Consequence Triple |
|---|---|---|
| Budget authority | Does posted spending exceed available authority? | `saam:violatesBudgetAuthority true` |
| Fund restriction | Does the fund permit the coded object or activity? | `saam:violatesFundRestriction true` |
| Appropriation lifecycle | Does the transaction occur during an active appropriation lifecycle state? | `saam:violatesLifecycleState true` |
| Cross-entity validation | Do agency, fund, and appropriation belong to one compatible control context? | `saam:violatesEntityAlignment true` |
| Classification verification | Does the transaction carry a valid account or object-code branch? | `saam:hasClassificationIssue true` |

### Rule 1: Salary Object Code Restricted to Governmental Funds

**IF** a transaction uses a salary object code and the linked fund is not a governmental fund, **THEN** the transaction receives a fund-restriction error flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasObjectCode(?transaction, ?objectCode) ^
saam:SalariesObjectCode(?objectCode) ^
saam:hasFund(?transaction, ?fund) ^
saam:NonGovernmentalFund(?fund)
-> saam:violatesFundRestriction(?transaction, true)
```

### Rule 2: Budget Authority Exhaustion Check

**IF** a transaction amount exceeds the remaining authority on the linked appropriation, **THEN** the transaction receives a budget-authority violation flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasAppropriation(?transaction, ?appropriation) ^
saam:transactionAmount(?transaction, ?amount) ^
saam:remainingAuthorityAmount(?appropriation, ?remaining) ^
swrlb:greaterThan(?amount, ?remaining)
-> saam:violatesBudgetAuthority(?transaction, true)
```

### Rule 3: Closed Appropriation Posting Check

**IF** a transaction links to an appropriation whose lifecycle state equals `Closed`, **THEN** the transaction receives a lifecycle-state violation flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasAppropriation(?transaction, ?appropriation) ^
saam:appropriationLifecycleState(?appropriation, "Closed")
-> saam:violatesLifecycleState(?transaction, true)
```

### Rule 4: Biennium Alignment Check

**IF** a transaction belongs to one biennium and the linked appropriation belongs to a different biennium, **THEN** the transaction receives an entity-alignment violation flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasBiennium(?transaction, ?transactionBiennium) ^
saam:hasAppropriation(?transaction, ?appropriation) ^
saam:hasBiennium(?appropriation, ?appropriationBiennium) ^
swrlb:notEqual(?transactionBiennium, ?appropriationBiennium)
-> saam:violatesEntityAlignment(?transaction, true)
```

### Rule 5: Fund-to-Appropriation Ownership Check

**IF** a transaction references a fund that differs from the fund on the linked appropriation, **THEN** the transaction receives an ownership-alignment error flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasFund(?transaction, ?transactionFund) ^
saam:hasAppropriation(?transaction, ?appropriation) ^
saam:belongsToFund(?appropriation, ?appropriationFund) ^
swrlb:notEqual(?transactionFund, ?appropriationFund)
-> saam:violatesEntityAlignment(?transaction, true)
```

### Rule 6: Restricted Capital Fund Usage Check

**IF** a fund has the restriction category `CapitalProjectsOnly` and the transaction object code falls outside the capital object-code branch, **THEN** the transaction receives a fund-restriction violation flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasFund(?transaction, ?fund) ^
saam:restrictionCategory(?fund, "CapitalProjectsOnly") ^
saam:hasObjectCode(?transaction, ?objectCode) ^
saam:NonCapitalObjectCode(?objectCode)
-> saam:violatesFundRestriction(?transaction, true)
```

### Rule 7: Organizational Control Boundary Check

**IF** a transaction posts under one agency and the linked appropriation belongs to a different agency, **THEN** the transaction receives a cross-entity violation flag.

```text
saam:FinancialTransaction(?transaction) ^
saam:hasAgency(?transaction, ?transactionAgency) ^
saam:hasAppropriation(?transaction, ?appropriation) ^
saam:administeredByAgency(?appropriation, ?appropriationAgency) ^
swrlb:notEqual(?transactionAgency, ?appropriationAgency)
-> saam:violatesEntityAlignment(?transaction, true)
```

### Rule 8: Derived High-Risk Transaction Classification

**IF** a transaction violates budget authority and fund restrictions at the same time, **THEN** the transaction receives a derived high-risk classification.

```text
saam:FinancialTransaction(?transaction) ^
saam:violatesBudgetAuthority(?transaction, true) ^
saam:violatesFundRestriction(?transaction, true)
-> saam:HighRiskTransaction(?transaction)
```

### SWRL Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Rule trigger coverage | Execute each rule against one positive and one negative fact set. | Each positive fact set emits the expected consequence triple and each negative fact set emits zero unexpected consequence triples. |
| Budget control accuracy | Compare inferred budget violations to a baseline exception extract. | Inferred budget violations match the baseline transaction identifiers. |
| Restriction control accuracy | Compare inferred fund-restriction flags to known restricted-fund test data. | Every known restricted transaction receives one restriction flag. |
| Cross-entity control accuracy | Compare agency, biennium, and fund alignment facts to inferred flags. | Every deliberately misaligned sample receives one entity-alignment flag. |

## SHACL Validation Shapes

SHACL shapes validate graph completeness, cardinality, datatype correctness, value ranges, and controlled value sets. The shapes below focus on transaction, appropriation, and control-context validation.

### Transaction Node Shape

This shape enforces required links and core literal fields for each financial transaction.

```turtle
saam:TransactionShape
    a sh:NodeShape ;
    sh:targetClass saam:FinancialTransaction ;
    sh:property [
        sh:path saam:hasFund ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:class saam:Fund ;
        sh:message "Each transaction links to exactly one fund." ;
    ] ;
    sh:property [
        sh:path saam:hasAccount ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:class saam:Account ;
        sh:message "Each transaction links to exactly one account." ;
    ] ;
    sh:property [
        sh:path saam:transactionAmount ;
        sh:minCount 1 ;
        sh:datatype xsd:decimal ;
        sh:minInclusive 0.00 ;
        sh:message "Each transaction carries a non-negative decimal amount." ;
    ] ;
    sh:property [
        sh:path saam:transactionDate ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:datatype xsd:date ;
        sh:message "Each transaction carries one transaction date." ;
    ] .
```

### SAAM Transaction Validation Shape

This shape adds SAAM-specific control requirements for appropriation, object code, fund, and document reference.

```turtle
saam:SaamTransactionValidationShape
    a sh:NodeShape ;
    sh:targetClass saam:FinancialTransaction ;
    sh:property [
        sh:path saam:hasAppropriation ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:class saam:Appropriation ;
        sh:message "Each SAAM transaction links to exactly one appropriation." ;
    ] ;
    sh:property [
        sh:path saam:hasObjectCode ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:class saam:ObjectCode ;
        sh:message "Each SAAM transaction links to exactly one object code." ;
    ] ;
    sh:property [
        sh:path saam:documentReference ;
        sh:minCount 1 ;
        sh:datatype xsd:string ;
        sh:minLength 3 ;
        sh:pattern "^[A-Z0-9\\-]+$" ;
        sh:message "Each SAAM transaction carries a normalized document reference." ;
    ] ;
    sh:sparql [
        sh:message "The transaction fund matches the appropriation fund." ;
        sh:select """
            PREFIX saam: <https://example.org/saam#>
            SELECT $this
            WHERE {
              $this saam:hasFund ?transactionFund ;
                    saam:hasAppropriation ?appropriation .
              ?appropriation saam:belongsToFund ?appropriationFund .
              FILTER (?transactionFund != ?appropriationFund)
            }
        """ ;
    ] .
```

### Appropriation Validation Shape

This shape verifies appropriation identity, authority amount, lifecycle state, and fund assignment.

```turtle
saam:AppropriationShape
    a sh:NodeShape ;
    sh:targetClass saam:Appropriation ;
    sh:property [
        sh:path saam:appropriationCode ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:datatype xsd:string ;
        sh:pattern "^[0-9A-Z]{4,12}$" ;
        sh:message "Each appropriation carries one normalized appropriation code." ;
    ] ;
    sh:property [
        sh:path saam:authorizedAmount ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:datatype xsd:decimal ;
        sh:minInclusive 0.00 ;
        sh:message "Each appropriation carries one non-negative authority amount." ;
    ] ;
    sh:property [
        sh:path saam:appropriationLifecycleState ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:in ( "Draft" "Active" "Suspended" "Closed" ) ;
        sh:message "Each appropriation lifecycle state appears in the approved state set." ;
    ] ;
    sh:property [
        sh:path saam:belongsToFund ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:class saam:Fund ;
        sh:message "Each appropriation links to exactly one controlling fund." ;
    ] .
```

### Fund Restriction Shape

This shape verifies restriction metadata and enforces a controlled value set.

```turtle
saam:FundRestrictionShape
    a sh:NodeShape ;
    sh:targetClass saam:Fund ;
    sh:property [
        sh:path saam:restrictionCategory ;
        sh:maxCount 1 ;
        sh:in ( "Governmental" "Enterprise" "CapitalProjectsOnly" "FederalRestricted" "DebtService" ) ;
        sh:message "Restriction category values stay inside the governed set." ;
    ] ;
    sh:property [
        sh:path saam:fundCode ;
        sh:minCount 1 ;
        sh:maxCount 1 ;
        sh:datatype xsd:string ;
        sh:pattern "^[0-9A-Z]{3,8}$" ;
        sh:message "Each fund code follows the governed lexical pattern." ;
    ] .
```

### SHACL Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Required-property validation | Run shapes against data that omits fund, account, appropriation, and amount fields. | The validation report returns one violation per omitted required field. |
| Cardinality validation | Run shapes against data with duplicate fund or appropriation links. | The validation report returns a cardinality violation for each duplicated one-to-one relationship. |
| Datatype validation | Run shapes against malformed dates and string-based numeric values. | The validation report returns datatype violations for each malformed literal. |
| Controlled-value validation | Run shapes against invalid lifecycle and restriction values. | The validation report returns value-set violations for each invalid controlled term. |
| Cross-node validation | Run the SAAM transaction SPARQL shape against mismatched fund-to-appropriation samples. | Each mismatched sample appears as one focus node in the report. |

## OWL Reasoning Examples

OWL axioms supply reusable semantic behavior for classification, hierarchy traversal, and consistency checking.

### Transitive Organizational Hierarchy

If `A reportsTo B` and `B reportsTo C`, inference derives `A reportsTo C`.

```turtle
saam:reportsTo a owl:TransitiveProperty .

ex:OrgUnitA saam:reportsTo ex:OrgUnitB .
ex:OrgUnitB saam:reportsTo ex:OrgUnitC .
```

**Derived fact**

```turtle
ex:OrgUnitA saam:reportsTo ex:OrgUnitC .
```

### Equivalent Classes for Account Aliases

Equivalent classes unify alternate business vocabularies under one semantic meaning.

```turtle
saam:RevenueAccount a owl:Class .
saam:IncomeAccount a owl:Class .
saam:RevenueAccount owl:equivalentClass saam:IncomeAccount .

ex:Acct4010 a saam:RevenueAccount .
```

**Derived fact**

```turtle
ex:Acct4010 a saam:IncomeAccount .
```

### Disjoint Classes for Mutual Exclusivity

Disjointness blocks logically inconsistent classification.

```turtle
saam:Asset a owl:Class .
saam:Liability a owl:Class .
saam:Asset owl:disjointWith saam:Liability .
```

If one node receives both types, a reasoner reports an inconsistency.

### Property Chain for Fund Ownership Inference

Property chains derive indirect relationships from explicit paths.

```turtle
saam:controlledByFund a owl:ObjectProperty ;
    owl:propertyChainAxiom ( saam:hasAppropriation saam:belongsToFund ) .
```

With these facts:

```turtle
ex:Txn100 saam:hasAppropriation ex:App25 .
ex:App25 saam:belongsToFund ex:Fund03 .
```

the reasoner derives:

```turtle
ex:Txn100 saam:controlledByFund ex:Fund03 .
```

### Inverse Properties for Navigation

Inverse properties support bidirectional navigation without duplicate assertions.

```turtle
saam:hasAppropriation a owl:ObjectProperty .
saam:isAppropriationFor a owl:ObjectProperty .
saam:hasAppropriation owl:inverseOf saam:isAppropriationFor .

ex:Txn200 saam:hasAppropriation ex:App77 .
```

**Derived fact**

```turtle
ex:App77 saam:isAppropriationFor ex:Txn200 .
```

### Classification by Restriction Category

OWL class expressions support automatic class membership.

```turtle
saam:FederalRestrictedFund a owl:Class ;
    owl:equivalentClass [
        a owl:Class ;
        owl:intersectionOf (
            saam:Fund
            [ a owl:Restriction ;
              owl:onProperty saam:restrictionCategory ;
              owl:hasValue "FederalRestricted"
            ]
        )
    ] .

ex:Fund88 a saam:Fund ;
    saam:restrictionCategory "FederalRestricted" .
```

**Derived fact**

```turtle
ex:Fund88 a saam:FederalRestrictedFund .
```

### OWL Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Transitive hierarchy inference | Load a three-level organizational chain into a reasoner. | The inferred graph contains the derived top-level `reportsTo` relationship. |
| Equivalent-class inference | Assert one alias class membership. | The inferred graph contains membership in the equivalent class. |
| Disjointness protection | Assert one node as both `Asset` and `Liability`. | The reasoner reports one logical inconsistency. |
| Property-chain inference | Assert transaction-to-appropriation and appropriation-to-fund links. | The inferred graph contains the derived transaction-to-fund control relationship. |
| Inverse-property inference | Assert only the forward property. | The inferred graph contains the inverse relationship. |

## Inference-Aware SPARQL Query Pack

SPARQL queries over inferred data surface compliance exceptions and hierarchy answers with shorter graph patterns.

### High-Risk Transaction Extraction

```sparql
PREFIX saam: <https://example.org/saam#>

SELECT ?transaction ?appropriation ?fund
WHERE {
  ?transaction a saam:HighRiskTransaction ;
               saam:hasAppropriation ?appropriation ;
               saam:controlledByFund ?fund .
}
ORDER BY ?appropriation ?transaction
```

### Budget Authority Exception Query

```sparql
PREFIX saam: <https://example.org/saam#>

SELECT ?transaction ?appropriation ?amount ?remaining
WHERE {
  ?transaction saam:violatesBudgetAuthority true ;
               saam:hasAppropriation ?appropriation ;
               saam:transactionAmount ?amount .
  ?appropriation saam:remainingAuthorityAmount ?remaining .
}
ORDER BY DESC(?amount)
```

### Organizational Rollup Query

```sparql
PREFIX saam: <https://example.org/saam#>

SELECT ?unit ?supervisor
WHERE {
  ?unit saam:reportsTo ?supervisor .
  FILTER (?unit != ?supervisor)
}
ORDER BY ?unit ?supervisor
```

### Account Alias Resolution Query

```sparql
PREFIX saam: <https://example.org/saam#>

SELECT ?account ?normalizedType
WHERE {
  ?account a ?normalizedType .
  VALUES ?normalizedType { saam:RevenueAccount saam:IncomeAccount }
}
ORDER BY ?account ?normalizedType
```

### Query Verification Matrix

| Check | Test | Pass |
|---|---|---|
| High-risk extraction | Execute the query over one graph that contains inferred `HighRiskTransaction` types. | The result set returns every expected high-risk transaction and zero non-flagged transactions. |
| Budget exception extraction | Compare query output to SWRL-generated budget flags. | Every returned row carries an asserted or inferred budget-authority violation flag. |
| Hierarchy navigation | Execute the rollup query over asserted and inferred `reportsTo` triples. | The result set contains direct and ancestor supervisor relationships. |
| Alias resolution | Execute the query after equivalent-class reasoning. | The result set contains both alias types for equivalent account classes. |

## Reasoning Boundary Guidance

Reasoning assets stay more reliable when each technology handles the layer that fits its semantics.

| Concern | Preferred Technology | Reason |
|---|---|---|
| Stable conceptual truth | OWL | OWL expresses reusable class and property semantics. |
| Data completeness and lexical validation | SHACL | SHACL reports focus-node violations with explicit messages. |
| Operational control logic | SWRL | SWRL captures IF-THEN compliance rules close to the ontology vocabulary. |
| Reporting and export | SPARQL | SPARQL extracts exception rows, lineage chains, and rollups. |

### Boundary Rules

| Rule | Test | Pass |
|---|---|---|
| OWL axiom discipline | Review each OWL axiom for domain truth rather than one-off workflow logic. | Every OWL axiom remains valid outside one batch or one agency workflow. |
| SHACL scope discipline | Review each shape for data-contract and graph-quality semantics. | Each SHACL violation identifies one focus node and one concrete contract failure. |
| SWRL scope discipline | Review each rule for control logic that derives a consequence triple. | Each SWRL rule emits one observable compliance or classification fact. |
| SPARQL scope discipline | Review each query for extraction rather than hidden business logic. | Each query reads asserted or inferred facts and returns stable identifiers. |

## .NET Integration with dotNetRDF

### Graph Loading and Inference

`dotNetRDF` loads RDF, Turtle, RDF/XML, and SPARQL assets into one store for validation and query execution.

```csharp
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

var ontologyGraph = new Graph();
FileLoader.Load(ontologyGraph, @".github\skills\saam-ontology\ontology\saam-core.ttl");

var instanceGraph = new Graph();
FileLoader.Load(instanceGraph, @".sandbox\reasoning-sample-data.ttl");

var store = new TripleStore();
store.Add(ontologyGraph);
store.Add(instanceGraph);

var query = """
PREFIX saam: <https://example.org/saam#>
SELECT ?transaction ?fund
WHERE {
  ?transaction a saam:FinancialTransaction ;
               saam:controlledByFund ?fund .
}
""";

var parser = new SparqlQueryParser();
var processor = new LeviathanQueryProcessor(store);
var results = processor.ProcessQuery(parser.ParseFromString(query));
```

### SHACL Validation with dotNetRDF

The validation workflow loads shapes into a shapes graph and validates a data graph against that graph.

```csharp
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;

var dataGraph = new Graph();
FileLoader.Load(dataGraph, @".sandbox\reasoning-sample-data.ttl");

var shapesGraph = new Graph();
FileLoader.Load(shapesGraph, @".github\skills\ontology-design-patterns\references\reasoning-shapes.ttl");

var validator = new ShapesGraph(shapesGraph);
Report report = validator.Validate(dataGraph);

if (!report.Conforms)
{
    foreach (var result in report.Results)
    {
        Console.WriteLine(result.Message);
    }
}
```

### SPARQL Queries that Leverage Inference

Inferred facts simplify operational queries because the query targets derived relationships rather than repeating the full traversal logic.

```sparql
PREFIX saam: <https://example.org/saam#>

SELECT ?transaction ?fund ?agency
WHERE {
  ?transaction a saam:HighRiskTransaction ;
               saam:controlledByFund ?fund ;
               saam:hasAgency ?agency .
}
ORDER BY ?fund ?transaction
```

### Performance Considerations

| Area | Practice | Observable Benefit |
|---|---|---|
| Rule partitioning | Group SWRL rules by control domain and execute only the needed set. | Rule execution time drops for focused workloads. |
| Shape partitioning | Run transaction shapes, appropriation shapes, and hierarchy shapes as separate validation packs. | Validation results isolate faster and produce smaller reports. |
| Inference materialization | Persist stable inferred relationships such as `controlledByFund` and organizational rollups. | SPARQL query complexity drops for repeated audit queries. |
| Incremental validation | Validate only changed transactions and directly related appropriations during batch updates. | Batch windows shrink and exception review stays localized. |
| Namespace governance | Keep one stable namespace map in code and validation assets. | Parser errors and query drift decrease. |

### .NET Verification Matrix

| Check | Test | Pass |
|---|---|---|
| Graph load reliability | Load ontology, instance, and shape files through dotNetRDF parsers. | All files parse without exceptions. |
| SHACL report accuracy | Validate a graph with known defects. | The report returns the expected defect count and messages. |
| Inference query value | Run one query before and after inferred relationship materialization. | The materialized query returns the same business answer with fewer triple patterns. |
| Batch performance | Measure validation duration on one baseline batch and one incremental batch. | Incremental validation duration stays below the baseline full-batch duration. |

## Use Cases

| Use Case | Reasoning Pattern | Observable Result |
|---|---|---|
| Automated compliance checking against SAAM rules | SWRL compliance rules plus SHACL exception extraction | The pipeline emits flagged transactions with named violation categories. |
| Budget authority validation | SWRL authority checks plus SPARQL aggregate review | The validation report identifies overspending transactions and affected appropriations. |
| Fund restriction enforcement | SHACL controlled-value checks plus SWRL restriction rules | The exception set isolates prohibited object-code and fund combinations. |
| Account classification verification | OWL equivalence and disjointness plus SHACL account shapes | The graph infers alias classes and reports inconsistent classifications. |
| Organizational hierarchy navigation | OWL transitive and inverse properties | Queries return rolled-up supervisors, agencies, and responsibility chains. |

### Use Case Walkthrough: Automated Compliance Checking

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent loads asserted SAAM facts for funds, appropriations, transactions, and agencies. | Inspect load log. | The graph contains all expected sample identifiers. |
| 2 | Agent runs OWL reasoning for derived control relationships and classification aliases. | Inspect inferred triples. | `controlledByFund`, inverse links, and equivalent-class memberships appear. |
| 3 | Agent runs SHACL validation for required fields and controlled values. | Review the SHACL report. | The report lists only intended lexical, cardinality, or value-set defects. |
| 4 | Agent runs SWRL rules for budget, fund, lifecycle, and cross-entity controls. | Review inferred consequence triples. | Each noncompliant transaction carries the expected violation flags. |
| 5 | Agent runs SPARQL exception extraction and publishes results to a review queue or audit table. | Compare output rows to the rule expectations table. | Exception identifiers and violation categories match the inferred results. |

### Use Case Walkthrough: Organizational Hierarchy Navigation

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Agent loads organization units and asserted `reportsTo` links. | Review asserted hierarchy triples. | The graph contains one direct parent link for each non-root unit. |
| 2 | Agent applies OWL transitive and inverse reasoning. | Review inferred hierarchy triples. | Ancestor and reverse-navigation facts appear without manual duplication. |
| 3 | Agent executes hierarchy queries for approval routing or responsibility lookup. | Compare query results to an organization chart baseline. | Query results return the correct chain for each sampled unit. |
| 4 | Agent reuses the inferred hierarchy in compliance queries that group violations by supervisor chain. | Review aggregated report output. | Violations roll up under the expected organizational branch. |

## Recommended Validation Sequence

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1 | Load ontology schema, instance data, and controlled vocabularies. | Parse graphs. | Zero parser errors occur. |
| 2 | Execute OWL reasoning for stable derived relationships and consistency checks. | Inspect inferred graph and reasoner report. | Expected derived facts exist and zero unexpected inconsistencies appear. |
| 3 | Execute SHACL node and property validation. | Inspect validation report. | Required-field, cardinality, datatype, and value-set violations appear with focus nodes. |
| 4 | Execute SWRL rule evaluation for control exceptions and derived risk labels. | Inspect inferred consequence triples. | Expected control flags and derived classifications exist. |
| 5 | Execute SPARQL exception and audit queries over asserted plus inferred data. | Review result sets. | Queries return targeted control outputs with stable identifiers. |

## Summary

Advanced ontology reasoning combines OWL semantics, SHACL constraints, SWRL business rules, and SPARQL verification into one auditable semantic control layer. SAAM-aligned implementations gain explicit compliance logic, stronger classification integrity, reusable hierarchy navigation, and traceable .NET validation workflows when these patterns remain bounded, testable, and observable.
