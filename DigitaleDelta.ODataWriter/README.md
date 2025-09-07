# DigitaleDelta.ODataWriter

A lightweight .NET library for generating OData-compliant JSON responses with support for pagination, property selection, and entity serialization.

## Features

- Fluent builder pattern for constructing OData responses
- Support for pagination with skip tokens
- Property selection ($select)
- Count metadata
- Entity set context handling
- Clean JSON serialization
- Zero external dependencies beyond .NET standard libraries

## Usage

Basic example:
```csharp
var mapper = ODataPropertyMapper.CreateMapper(typeof(WeatherStation), excludeNulls: true);
var entities = new List<WeatherStation> 
{
    new() { Location = "Loc1", StationId = null, Temperature = 20 }, 
    new() { Location = "Loc2", StationId = "Id2", Temperature = 30 }
} .Select(mapper);
var selectedProperties = new List<string> { "stationId", "location" };
var response = entities
    .CreateODataResponse()
    .WithBaseUrl("https://api.example.com")
    .WithEntitySet("weatherstations")
    .WithPagination(new SkipTokenInfo(), top: 10, totalCount: 1230)
    .SelectProperties(selectedProperties)
    .IncludeCount()
    .Build();

Console.WriteLine(response);

public class WeatherStation
{
    public string? StationId { get; set; }
    public string Location { get; set; }
    public double? Temperature { get; set; }
}
```

## Testing

This project uses [testing framework] for unit and integration tests. The test suite covers core functionality including OData parsing, SQL translation, and validation logic.

### Running Tests

```bash
dotnet test