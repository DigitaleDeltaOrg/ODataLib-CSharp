# Introduction

The purpose of this repository is to provide a set of libraries for working with [OData](https://odata.org) in [.NET](https://dotnet.microsoft.com) applications, 
specifically tailored for the [Digitale Delta API V3](https://digitaledelta.org), 
without any dependency on Microsoft OData libraries. 

OData consists of two main parts:

* Query Language (grammar): This defines the syntax for querying and manipulating data using expressions like `$filter`,`$select`, `$expand`, `$orderby`, etc. Please note that this library only deals with `$filter`- and `$orderby`-expressions. 
* Data Model (semantics): This uses the [Entity Data Model]([Entity Data Model](https://docs.oasis-open.org/odata/odata/v4.01/os/part1-protocol/odata-v4.01-os-part1-protocol.html#sec_EntityDataModel)) (EDM) to describe the structure of data exposed by an OData service, including entities, relationships, and properties. This is typically represented using [CSDL](https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html) (Common Schema Definition Language).

The provided libraries attempt to simplify working with OData and to remove dependency of the Microsoft OData libraries.

The libraries include:

* **DigitaleDelta.CsdlParser**: A lightweight Common Schema Definition Language (CSDL) parser for OData metadata documents, which reads EDMX/CSDL XML files and creates a structured model of the OData service metadata.
* **DigitaleDelta.ODataTranslator**: A library that converts OData query expressions to SQL, allowing applications to use an OData-subset syntax for use with the Digitale Delta API V3, while leveraging native SQL performance. The system is database-agnostic, meaning it can work with any SQL database without being tied to a specific database engine or design. The user of the library will supply mappings to properties and functions in simple JSON configuration files, which allows for flexible and extensible SQL generation.
* **DigitaleDelta.ODataWriter**: A library for generating OData-compliant JSON responses with support for pagination, property selection, and entity serialisation.

## DigitaleDelta.CsdlParser

A lightweight Common Schema Definition Language (CSDL) parser for OData metadata documents,
which reads EDMX/CSDL XML files and creates a structured model of the OData service metadata.
This model is used by the ODataTranslator.

## DigitaleDelta.ODataTranslator

A library that converts OData query expressions to `SQL`, 
allowing applications to use an OData-subset syntax for use with the Digitale Delta API V3, 
while leveraging native SQL performance. 
The system is database-agnostic, 
meaning it can work with any SQL database without being tied to a specific database engine or design. 
The user of the library will supply mappings to properties and functions in simple JSON configuration files, 
which allows for flexible and extensible SQL generation.
The core of the ODataTranslator library is based on [`Antlr`](https://www.antlr.org/index.html) version 4, 
which generates lexers, visitors, and parsers for language definitions, such as OData. 
The same `Anltr` definition can be used to generate parsers, lexers, and visitors for other programming languages.

## DigitaleDelta.ODataWriter

A library for generating OData-compliant JSON responses with support for pagination, 
property selection, and entity serialisation.
It contains helpers to deal with the `$skipToken` segment of OData. 
`$skipToken` is used instead of `$skip`.

This allows for a flexible and extensible approach to handling OData queries and responses in .NET applications.

Based on a custom [Antlr OData language definition](DigitaleDelta.ODataTranslator/grammar/OData.g4), a minimal implementation of OData, specifically suited for the Digitale Delta, 
the libraries can be used to build OData filter parsers and SQL translators. 
The translation to SQL is fully database-engine or database-design agnostic, as the `WHERE` clause is generated using mappings to properties and functions defined in simple JSON configuration files.

Antlr can use the same definition to generate classes in the common programming languages.

Included in the repository are also example projects demonstrating how to use these libraries, as well as unit (XUnit) tests to ensure the functionality and correctness of the implementations.

Security is a key part of the libraries. 
Refer to the [SECURITY.md](SECURITY.md) file for detailed information on security considerations, 
including how the libraries handle SQL injection prevention, 
parametrisation of queries, and abstraction layers to limit the attack surface.

## Definitions

Two definitions govern the Digitale Delta libraries:

* [Antlr definition](#antlr-definition)
* [Digitale Delta CSDL](#digitale-delta-csdl)

### Antlr definition

The [Antlr definition](Resources/AntlrOData/Current/OData.g4) is an Antlr grammar file, 
which defines the OData query language syntax. 
It is used to generate the parser and visitor classes for processing OData queries.

### Digitale Delta CSDL

The [Digitale Delta CSDL](Resources/CSDL/Current/DigitaleDelta3.xml) is the [Common Schema Definition Language (CSDL)](https://docs.oasis-open.org/odata/odata-csdl-xml/v4.01/odata-csdl-xml-v4.01.html) file, 
describing the semantics of the OData service metadata for the Digitale Delta API. It defines entities, relationships, and properties used in OData queries.

## Components used

* [Antlr 4](https://www.antlr.org/): For generating parsers and visitors for the OData language definition.
* [XUnit](https://xunit.net/): For unit testing projects.
* [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite): For handling spatial data types, if needed.
* [System.Text.Json](https://learn.microsoft.com/en-us/dotnet/api/system.text.json?view=net-9.0): For JSON serialisation and deserialization, providing a lightweight and efficient way to handle JSON data in .NET applications.

The root also contains the official Digitale Delta CSDL XML file, which is used to define the OData model for the Digitale Delta API. This file is used by the `DigitaleDelta.CsdlParser` library to create a structured model of the OData service metadata.
It also contains a copy of the OData language definition used by Antlr to generate the parser and visitor classes.

## Workflow

The following steps usually are only needed at the start of the program:

- Read a CSDL document. 
- Parse the CSDL document using the `DigitaleDelta.CsdlParser` library to create a structured model of the OData service metadata (CSDL model).
- Read the function and property maps from JSON files or other sources.

The following steps are used to process an OData query:

1. Parse the OData query using the Antlr-generated parser from the `DigitaleDelta.ODataTranslator` library, which uses the OData language definition to parse the query into an abstract syntax tree (AST).
   * The OData query is validated against the CSDL model to ensure it adheres to the OData specification.
   * The validation checks for syntax errors, unsupported features, and compliance with the CSDL model.
2. Convert the parsed query to a SQL WHERE clause using ODataToSqlConverter from the `DigitaleDelta.ODataTranslator` library using the CSDL model, function maps, and property maps.
   * The OData query is parsed using the Antlr-generated parser.
   * The parsed query is validated against the CSDL model to ensure it adheres to the OData specification.
   * The validated query is then converted to a SQL WHERE clause using the provided mappings for properties and functions.
3. Execute the SQL query against the database using a database access library (e.g. Dapper, Entity Framework, ADO.NET).
4. Return the results in a format that is compliant with the OData specification using the `DigitaleDelta.ODataWriter` library.
   * It takes the SQL query results and formats them into an OData-compliant JSON response.
   * Handles $SkipToken for paging, property selection, and entity serialisation to ensure the response adheres to the OData specification.
   * Handles CRS (Coordinate Reference System) transformations if needed, in line with Kennisplatform APIs (Accept-Crs, Content-Crs), ensuring that spatial data is correctly represented in the response.

To simplify the process, the `DigitaleDelta.ODataTranslator` library provides an `ODataFilterProcessor` class that combines the validation and conversion steps into a single method, allowing you to process OData queries more easily.

With the `ODataFilterProcessor`, you can validate and convert OData queries to SQL in a single step, making it easier to integrate OData support into your .NET applications.

### Example usage

``` csharp
using System.Reflection;
using DigitaleDelta.ODataTranslator;
using DigitaleDelta.ODataTranslator.Models;

// The CSDL is located in the same directory as the executing assembly in this example.
// This is typically performed at the start of the program.
var assembly = Assembly.GetExecutingAssembly();
var assemblyPath = Path.GetDirectoryName(assembly.Location)!;
var filePath = Path.Combine(assemblyPath, "DigitaleDelta3.xml");

// Read the CSDL XML content from the file.
var xmlContent = await File.ReadAllTextAsync(filePath);

// Get function and property maps.
var functionMap = GetFunctionMaps();
var propertyMap = GetPropertyMaps();

// Try to parse the CSDL XML content, producing a CsdlModel
if (!DigitaleDelta.CsdlParser.CsdlParser.TryParse(xmlContent, out var csdlModel, out var error) || csdlModel == null)
{
    Console.Error.WriteLine(error);
    return;
}

var processor = new ODataFilterProcessor(csdlModel, functionMap, propertyMap);

// The initialization of the processor is done with the CSDL model, function maps, and property maps.
// processor can be used for the rest of the program to process OData queries by using it as a singleton and inject it where needed.

// Sample query
var query = "$filter=startswith(parameter/source, 'Wmr') and ResultOf eq 'aggregation' and (PhenomenonTime/BeginPosition ge now() and PhenomenonTime/BeginPosition le now()) OR parameter/source IN ('Wmr', 'Wmr2')";

if (!processor.TryProcessFilter(query, "observation", out var sql, out error))
{
    Console.WriteLine(error);
    return;
}

Console.WriteLine(sql);
return;

// Example function and property maps, these would typically be loaded from JSON files or other sources.
List<ODataFunctionMap> GetFunctionMaps()
{
    return
    [
        new()
        {
            ODataFunctionName = "startswith",
            ExpectedArgumentTypes = ["Edm.String", "Edm.String"],
            ReturnType = "Edm.String",
            SqlFunctionFormat = "ILIKE({0}, {1})",
            WildCardPosition = "right",
            WildCard = "%"
        },

        new()
        {
            ODataFunctionName = "now",
            ExpectedArgumentTypes = [],
            ReturnType = "Edm.DateTimeOffset",
            SqlFunctionFormat = "NOW()"
        }
    ];
}

// Example property maps, these would typically be loaded from JSON files or other sources.
List<ODataToSqlMap> GetPropertyMaps()
{
    return
    [
        new()
        {
            ODataPropertyName = "parameter/source",
            Query = "source",
            EdmType = "Edm.String"
        },

        new()
        {
            ODataPropertyName = "ResultOf",
            Query = "result",
            EdmType = "Edm.String"
        },

        new()
        {
            ODataPropertyName = "PhenomenonTime/BeginPosition",
            Query = "phenomenon_time_start",
            EdmType = "Edm.DateTimeOffset"
        }
    ];
}
```
