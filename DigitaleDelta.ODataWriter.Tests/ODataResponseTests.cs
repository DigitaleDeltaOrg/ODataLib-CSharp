// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataWriter.Tests;

using System.Text.Json;

public class ODataResponseTests
{
    [Fact]
    public void Constructor_SetsProperties_Correctly()
    {
        // Arrange
        var value = new List<Dictionary<string, object?>>
        {
            new() { ["id"] = 1, ["name"] = "Test1" },
            new() { ["id"] = 2, ["name"] = "Test2" }
        };
        var context = "https://example.org/$metadata#Entity";

        // Act
        var response = new ODataResponse(value, context);

        // Assert
        Assert.Equal(context, response.Context);
        Assert.Same(value, response.Value);
        Assert.Null(response.Count);
        Assert.Null(response.NextLink);
    }

    [Fact]
    public void Properties_CanBeSet_AfterConstruction()
    {
        // Arrange
        var response = new ODataResponse([], "");
        
        // Act
        response.Context = "https://example.org/$metadata#Entity";
        response.Count = 42;
        response.NextLink = "https://example.org/api/entities?$skip=10";
        response.Value = [new() { ["id"] = 5 }];

        // Assert
        Assert.Equal("https://example.org/$metadata#Entity", response.Context);
        Assert.Equal(42, response.Count);
        Assert.Equal("https://example.org/api/entities?$skip=10", response.NextLink);
        Assert.Single(response.Value);
        Assert.Equal(5, response.Value[0]["id"]);
    }

    [Fact]
    public void Serialization_UsesCorrectPropertyNames()
    {
        // Arrange
        var response = new ODataResponse(
            [new() { ["id"] = 1, ["name"] = "Test" }],
            "https://example.org/$metadata#Entity"
        )
        {
            Count = 1,
            NextLink = "https://example.org/api/entities?$skip=10"
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"@odata.context\":\"https://example.org/$metadata#Entity\"", json);
        Assert.Contains("\"@odata.count\":1", json);
        Assert.Contains("\"@odata.nextLink\":\"https://example.org/api/entities?$skip=10\"", json);
        Assert.Contains("\"value\":[{\"id\":1,\"name\":\"Test\"}]", json);
    }

    [Fact]
    public void Serialization_OmitsNullProperties()
    {
        // Arrange
        var response = new ODataResponse(
            [new() { ["id"] = 1 }],
            "https://example.org/$metadata#Entity"
        );
        // Count and NextLink remain null

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        Assert.Contains("\"@odata.context\":\"https://example.org/$metadata#Entity\"", json);
        Assert.DoesNotContain("\"@odata.count\":", json);
        Assert.DoesNotContain("\"@odata.nextLink\":", json);
        Assert.Contains("\"value\":[{\"id\":1}]", json);
    }
}