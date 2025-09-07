// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// OData entity extensions
/// </summary>
public static class ODataEntityExtensions
{
    /// <summary>
    /// Creates an OData response
    /// </summary>
    /// <param name="entities">Entities to write</param>
    /// <returns></returns>
    public static ODataResponseBuilder CreateODataResponse(this IEnumerable<Dictionary<string, object?>> entities) => new ODataResponseBuilder().WithEntities(entities);
}