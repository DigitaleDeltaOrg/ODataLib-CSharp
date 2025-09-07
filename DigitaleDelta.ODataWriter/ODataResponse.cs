// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// OData response
/// </summary>
/// <param name="value">value (response lines)</param>
/// <param name="context">context</param>
public class ODataResponse(List<Dictionary<string, object?>> value, string context)
{
    /// <summary>
    /// OData context URL
    /// </summary>
    [JsonPropertyName("@odata.context")]
    public string Context { get; set; } = context;

    /// <summary>
    /// OData count
    /// </summary>
    [JsonPropertyName("@odata.count")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Count { get; set; }
    
    /// <summary>
    /// OData next link
    /// </summary>
    [JsonPropertyName("@odata.nextLink")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NextLink { get; set; }

    /// <summary>
    /// OData value (entities)
    /// </summary>
    [JsonPropertyName("value")]
    public List<Dictionary<string, object?>> Value { get; set; } = value;
}

