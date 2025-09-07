// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DigitaleDelta.Contracts;
using DigitaleDelta.ODataTranslator.Helpers;
using DigitaleDelta.ODataTranslator.Models;

namespace DigitaleDelta.ODataTranslator;

/// <summary>
/// Processes OData filter queries by parsing, validating, and converting them into SQL.
/// </summary>
public class ODataFilterProcessor(
    CsdlModel csdlModel,
    List<ODataFunctionMap> functionMap,
    List<ODataToSqlMap> propertyMap)
{
    /// <summary>
    /// Attempts to process an OData filter query by parsing, validating, and converting it into an equivalent SQL representation.
    /// </summary>
    /// <param name="query">The OData filter query as a string.</param>
    /// <param name="entitySet">The name of the entity set that the query targets.</param>
    /// <param name="sql">The resulting SQL query if the process is successful; otherwise, it will be null.</param>
    /// <param name="error">The error message if the process fails; otherwise, it will be null.</param>
    /// <returns>True if the filter query was successfully processed into a SQL query; otherwise, false.</returns>
    public bool TryProcessFilter(string query, string entitySet, out string? sql, out string? error)
    {
        var normalizer = new ODataFunctionNormalizer();
        query = normalizer.Normalize(query);
        // Initialise output parameters
        sql = null;
        error = null;

        if (string.IsNullOrEmpty(query))
        {
            return true;
        }
        
        // Stage 1: Parse the OData filter query
        if (!ODataFilter.TryParse(query, out var filter, out error) || filter == null)
        {
            return false;
        }

        // Stage 2: Validate the filter against the CSDL model
        var validator = new ODataFilterValidator(csdlModel, functionMap);
        
        if (!validator.TryValidate(filter.Context, entitySet, out error))
        {
            return false;
        }
            
        // Stage 3: Convert the validated filter to SQL
        var converter = new ODataToSqlConverter(propertyMap, functionMap);
        
        if (!converter.TryConvert(filter.Context, out error, out var sqlResult))
        {
            return false;
        }

        // Stage 4: Retrieve the SQL query from the conversion result
        sql = sqlResult?.Sql;
        
        return true;
    }
}