# CSDL Parser

A lightweight Common Schema Definition Language (CSDL) parser for OData metadata documents. This parser reads EDMX/CSDL XML files and creates a structured model of the OData service metadata.

The model will contain structured data representing the parsed EDMX/CSDL document, including entity sets, entity types, complex types, and functions.

It can be used for translating OData queries into SQL or other data access layers and is used in the `DigitaleDelta.ODataTranslator` library.

## Features

- Parses EDMX/CSDL XML documents
- Supports core OData metadata elements:
    - EntityContainer and EntitySets
    - EntityTypes with Keys
    - ComplexTypes
    - Functions with Parameters
    - EDM primitive types
- Strong typing for EDM types
- Null-safe parsing with detailed error messages
- No external dependencies beyond .NET standard libraries

## Usage

```csharp
var parser = new CsdlParser();
var valid = parser.TryParse(edmxXmlContent, out var model, out error);
```

## Porting

Porting this library to other programming languages should be fairly easy, as the library interprets XML and maps it to data classes.

## Testing

This project uses XUnit for unit and integration tests. The test suite covers core functionality including OData parsing, SQL translation, and validation logic.

### Running Tests

```bash
dotnet test
```
