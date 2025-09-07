// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NetTopologySuite.IO.Converters;

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// Shared features for OData Writer
/// </summary>
public static class Shared
{
    /// <summary>
    /// General JSON serialization options for OData responses. Includes handling GeoJSON types.
    /// </summary>
    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new GeoJsonConverterFactory() },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}