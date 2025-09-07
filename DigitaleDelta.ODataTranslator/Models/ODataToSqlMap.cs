// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Data.Common;

namespace DigitaleDelta.ODataTranslator.Models;

/// <summary>
/// Binds OData query parameters and their database equivalents.
/// </summary>
public record ODataToSqlMap
{
    /// <summary>
    /// Property name in OData, e.g. "Result", "PhenomenonTime/BeginPosition", "Foi/Code", "Parameter/Quantity", etc.
    /// </summary>
    public required string ODataPropertyName { get; init; }
    
    /// <summary>
    /// Name of the SQL column, e.g. "result", "phenomenon_time_begin_position", "foi_code", "parameter_quantity", etc.
    /// </summary>
    public required string Query { get; init; }
    
    /// <summary>
    /// Type of the OData property, e.g. "Edm.String", "Edm.Int32", "Edm.DateTimeOffset", etc.
    /// </summary>
    public required string EdmType { get; init; }
    /// <summary>
    /// Overrides the Query when the select part is not the samen as the where part, e.g. "SELECT result as bla FROM foo WHERE result = 'bar'".
    /// </summary>
    public string? WhereClausePart { get; init; }

    /// <summary>
    /// A delegate function used to retrieve a value from a database reader based on the given ordinal.
    /// It serves as a custom accessor for specific fields within a database result set.
    /// The function takes a <see cref="DbDataReader"/> and an integer representing the column ordinal, and returns an object or null.
    /// </summary>
    public Func<DbDataReader, int, object?>? Getter { set; get; }
}