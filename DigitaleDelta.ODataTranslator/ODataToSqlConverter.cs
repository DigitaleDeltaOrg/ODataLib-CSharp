// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DigitaleDelta.ODataTranslator.Helpers;
using DigitaleDelta.ODataTranslator.Models;

namespace DigitaleDelta.ODataTranslator;

/// <summary>
/// OData to SQL converter.
/// </summary>
/// <param name="propertyMaps">Map of OData properties to SQL properties</param>
/// <param name="functionMaps">Map of OData functions to SQL functions</param>
/// <param name="parameterPrefix">Prefix for parameters passed to SQL. Use '@;  for SQL Server, '$' for Postgres and ':' for Oracle</param>
/// <param name="srid">Request srid</param>
public class ODataToSqlConverter(IEnumerable<ODataToSqlMap> propertyMaps, IEnumerable<ODataFunctionMap> functionMaps, char parameterPrefix = '@', int srid = 4258)
{
    private readonly Dictionary<string, object> _parameters = [];
    private int _parameterCount;
    public record SqlResult(string Sql, IReadOnlyDictionary<string, object> Parameters);
    
    /// <summary>
    /// Known operators for OData to SQL conversion.
    /// </summary>
    private static readonly Dictionary<string, string> ODataToSqlOperatorMaps = new()
    {
        { "eq", "=" },
        { "ne", "<>" },
        { "gt", ">" },
        { "ge", ">=" },
        { "lt", "<" },
        { "le", "<=" }
    };

    /// <summary>
    /// Get the property map for a given property name.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    internal (bool Success, string? ErrorMessage, string? Query) TryGetPropertyMap(string propertyName)
    {
        if (string.Compare(propertyName, "null", StringComparison.Ordinal) == 0)
        {
            return (true, null, "NULL");
        }
        
        var map = propertyMaps.FirstOrDefault(m => string.Equals(m.ODataPropertyName, propertyName, StringComparison.OrdinalIgnoreCase));

        if (map == null)
        {
            return (false, string.Format(ErrorMessages.unknownProperty, propertyName), null);
        }

        return (true, null, map.WhereClausePart ?? map.Query);
    }

    /// <summary>
    /// Get the SQL function map for a given OData function name and arguments.
    /// </summary>
    /// <param name="functionName"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    internal (bool Success, string? ErrorMessage, string? SqlFunction) TryGetFunctionMap(string functionName, string?[] arguments)
    {
        var functionMap = functionMaps.FirstOrDefault(f => string.Equals(f.ODataFunctionName, functionName, StringComparison.OrdinalIgnoreCase));

        if (functionMap == null)
        {
            return (false, string.Format(ErrorMessages.unknownFunction, functionName), null);
        }

        if ((arguments.Length == 0) && functionMap.ExpectedArgumentTypes.Count == 0)
        {
            // zero-arg functie; retourneer zoals gedefinieerd
            var zeroArgSql = functionMap.SqlFunctionFormat.Replace("@srid", srid.ToString());
            // Als ReturnType boolean is, maak er een predicate van
            if (!string.IsNullOrWhiteSpace(functionMap.ReturnType) &&
                functionMap.ReturnType.Equals("Edm.Boolean", StringComparison.OrdinalIgnoreCase))
            {
                return (true, null, $"(({zeroArgSql}) = 1)");
            }
            return (true, null, zeroArgSql);
        }

        if (arguments.Length != functionMap.ExpectedArgumentTypes.Count)
        {
            return (false, string.Format(ErrorMessages.functionParameterCountMismatch, functionName, functionMap.ExpectedArgumentTypes.Count, arguments.Length), null);
        }

        if (functionMap.WildCardPosition != null)
        {
            MapWildcard(arguments, functionMap);   
        }

        var sqlFunction = string.Format(functionMap.SqlFunctionFormat.Replace("@srid", srid.ToString()), arguments.Cast<object>().ToArray());

        // Als de functie een boolean ReturnType heeft, vertaal naar predicate door = 1 toe te voegen
        if (!string.IsNullOrWhiteSpace(functionMap.ReturnType) &&
            functionMap.ReturnType.Equals("Edm.Boolean", StringComparison.OrdinalIgnoreCase))
        {
            return (true, null, $"(({sqlFunction}) = 1)");
        }
        
        return (true, null, sqlFunction);
    }


    /// <summary>
    /// Maps wildcard symbols to the specified argument based on the wildcard position defined in the function map.
    /// </summary>
    /// <param name="arguments">An array of arguments to modify with the wildcard symbol.</param>
    /// <param name="functionMap">The function map containing the wildcard position and wildcard symbol.</param>
    internal static void MapWildcard(string?[] arguments, ODataFunctionMap functionMap)
    {
        arguments[1] = functionMap.WildCardPosition switch
        {
            WildCardPosition.Right => $"{arguments[1]}+'{functionMap.WildCardSymbol}'", 
            WildCardPosition.Left => $"'{functionMap.WildCardSymbol}'+{arguments[1]}", 
            WildCardPosition.LeftAndRight => $"'{functionMap.WildCardSymbol}'+{arguments[1]}+'{functionMap.WildCardSymbol}'",
            _ => arguments[1]
        };
    }

    /// <summary>
    /// Get the SQL operator map for a given OData comparison operator.
    /// </summary>
    /// <param name="operatorSymbol"></param>
    /// <returns></returns>
    internal static (bool Success, string? ErrorMessage, string? SqlOperator) TryGetOperatorMap(string operatorSymbol)
    {
        if (!ODataToSqlOperatorMaps.TryGetValue(operatorSymbol, out var sqlOperator))
        {
            return (false, string.Format(ErrorMessages.unknownOperator, operatorSymbol), null);
        }

        return (true, null, sqlOperator);
    }

    /// <summary>
    /// Convert an OData filter expression to a SQL WHERE clause.
    /// </summary>
    /// <param name="filterContext"></param>
    /// <param name="error"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public bool TryConvert(ODataParser.FilterOptionContext? filterContext, out string? error, out SqlResult? result)
    {
        _parameters.Clear();
        _parameterCount = 0;
        
        if (filterContext?.filterExpr() == null || filterContext.filterExpr().IsEmpty || string.IsNullOrEmpty(filterContext.filterExpr().GetText()))
        {
            error = null;
            result = null;
            
            return true;
        }
        
        var (success, errorMessage, sql) = TryConvertFilterExpressionToSql(filterContext.filterExpr());
        
        if (!success)
        {
            error = errorMessage;
            result = null;
            
            return false;
        }

        error = null;
        result = new SqlResult(sql!, new Dictionary<string, object>(_parameters));
        
        return true;
    }

    /// <summary>
    /// Convert an OData filter expression to a SQL WHERE clause.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    private (bool Success, string? ErrorMessage, string? SqlQuery) TryConvertFilterExpressionToSql(ODataParser.FilterExprContext context)
    {
        if (context.IN() != null)
        {
            // Process the left side (property)
            var leftExpr = context.filterExpr(0);
            var leftResult = TryConvertFilterExpressionToSql(leftExpr);

            return HandleInClause(context, leftResult.SqlQuery!);
        }

        if (context.primary() != null)
        {
            var propertyName = context.primary().GetText();

            return propertyName.IsLiteralValue() ? HandleLiteral(propertyName) : TryGetPropertyMap(propertyName);
        }

        if (context.function() != null)
        {
            var function     = context.function();
            var functionName = function.Start.Text;

            // Zoek functionMap eerst om verwachte arg-count te kennen
            var functionMap = functionMaps.FirstOrDefault(f => string.Equals(f.ODataFunctionName, functionName, StringComparison.OrdinalIgnoreCase));
            if (functionMap == null)
            {
                return (false, string.Format(ErrorMessages.unknownFunction, functionName), null);
            }

            var arguments = function.filterExpr()
                .Select(TryConvertFilterExpressionToSql)
                .ToArray();
            var argumentQueries = arguments.Select(arg => arg.SqlQuery).ToArray();

            // Fallback ALLEEN wanneer exact 1 arg verwacht wordt en er 0 geparsed zijn
            if (argumentQueries.Length == 0 && functionMap.ExpectedArgumentTypes.Count == 1)
            {
                var raw = function.GetText(); // bv. distance('POINT(1 1)')
                var lParen = raw.IndexOf('(');
                var rParen = raw.LastIndexOf(')');
                if (lParen >= 0 && rParen > lParen + 1)
                {
                    var inner = raw.Substring(lParen + 1, rParen - lParen - 1).Trim();
                    if (!string.IsNullOrWhiteSpace(inner) && inner.IsLiteralValue())
                    {
                        var handled = HandleLiteral(inner);
                        if (handled.Success && !string.IsNullOrEmpty(handled.SqlQuery))
                        {
                            argumentQueries = new[] { handled.SqlQuery };
                        }
                    }
                }
            }

            return TryGetFunctionMap(functionName, argumentQueries);
        }
        
        if (context.filterExpr().Length == 1 && context.NOT() != null)
        {
            var inner = TryConvertFilterExpressionToSql(context.filterExpr(0));
            return (true, null, $"NOT ({inner.SqlQuery})");
        }

        if (context.filterExpr().Length == 2 && context.comparison() != null)
        {
            var left           = TryConvertFilterExpressionToSql(context.filterExpr(0));
            var right          = TryConvertFilterExpressionToSql(context.filterExpr(1));
            var operatorSymbol = context.comparison().GetText();

            if (left.SqlQuery != null && left.SqlQuery.Contains("distance", StringComparison.OrdinalIgnoreCase))
            {
                var threshold = ExtractParameterValue(right.SqlQuery);
                var unitExpr = context.filterExpr(0).function().filterExpr(1);
                var unitResult = TryConvertFilterExpressionToSql(unitExpr);
                var unitValue = ExtractParameterValue(unitResult.SqlQuery);
                var converted = ConvertDistanceToMetersIfNeeded(threshold, unitValue);

                // If right.SqlQuery is a parameter, update its value
                if (!string.IsNullOrEmpty(right.SqlQuery) && right.SqlQuery[0] == parameterPrefix && _parameters.ContainsKey(right.SqlQuery))
                {
                    _parameters[right.SqlQuery] = converted;
                }
                else
                {
                    right = (right.Success, right.ErrorMessage, CreateParameter(converted));
                }
                
                if (!string.IsNullOrEmpty(unitResult.SqlQuery) && unitResult.SqlQuery[0] == parameterPrefix)
                {
                    _parameters.Remove(unitResult.SqlQuery);
                }
            }
            
            if (!left.Success || !right.Success)
            {
                return (false, left.ErrorMessage ?? right.ErrorMessage, null);
            }

            // Special handling for NULL comparisons
            if (right.SqlQuery?.ToLower() == "'null'")
            {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                return operatorSymbol.ToLowerInvariant() switch
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
                {
                    "eq" => (true, null, $"{left.SqlQuery} IS NULL"),
                    "ne" => (true, null, $"{left.SqlQuery} IS NOT NULL")
                };
            }
            
            var (success, errorMessage, sqlOperator) = TryGetOperatorMap(operatorSymbol);

            return (success, errorMessage, $"{left.SqlQuery} {sqlOperator} {right.SqlQuery}");
        }

        if (context.filterExpr().Length == 2 && (context.AND() != null || context.OR() != null))
        {
            var left            = TryConvertFilterExpressionToSql(context.filterExpr(0));
            var right           = TryConvertFilterExpressionToSql(context.filterExpr(1));
            var logicalOperator = context.AND() != null ? "AND" : "OR";

            // Add parentheses only if the sub-expression contains another logical operator
            var leftQuery = context.filterExpr(0).AND() != null || context.filterExpr(0).OR() != null
                ? $"({left.SqlQuery})"
                : left.SqlQuery;

            var rightQuery = context.filterExpr(1).AND() != null || context.filterExpr(1).OR() != null
                ? $"({right.SqlQuery})"
                : right.SqlQuery;

            return (true, null, $"{leftQuery} {logicalOperator} {rightQuery}");
        }

        var innerExpression = TryConvertFilterExpressionToSql(context.filterExpr(0));

        // Retain explicit parentheses
        return (true, null, $"({innerExpression.SqlQuery})");
    }
    
    /// <summary>
    /// Create a parameter for the SQL query.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private string CreateParameter(object value)
    {
        var name = $"{parameterPrefix}p{++_parameterCount}";
        _parameters[name] = value;
        return name;
    }

    /// <summary>
    /// Handle the IN clause in the filter expression. This can cause multiple values to be passed in a single IN clause.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="leftSql"></param>
    /// <returns></returns>
    private (bool Success, string? ErrorMessage, string? SqlQuery) HandleInClause(ODataParser.FilterExprContext context, string leftSql)
    {
        var parameterNames = new List<string>();
        var foundOpenParen = false;
        var leftExpression = context.filterExpr(0);

        for (var i = 0; i < context.ChildCount; i++)
        {
            var text = context.GetChild(i).GetText().ToLower();

            if (text == "(")
            {
                foundOpenParen = true;
                continue;
            }

            if (!foundOpenParen || text == "in" || text == ")" || text == ",")
            {
                continue;
            }

            // Split the combined string into individual values
            var values = text.Split([","], StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (var value in values)
            {
                if (!value.IsLiteralValue())
                {
                    return (false, string.Format(ErrorMessages.mustBeALiteralValue, value), null);
                }

                if (!value.InferLiteralType().IsTypeCompatibleWith(GetParameterTypeForProperty(leftExpression.GetText())))
                {
                    return (false, string.Format(ErrorMessages.inClauseTypeMismatch, 0, leftExpression.GetText()), null);
                }

                parameterNames.Add(CreateParameter(value));
            }
        }

        return (true, null, $"{leftSql} IN ({string.Join(",", parameterNames)})");
    }
    
    private string GetParameterTypeForProperty(string propertyName)
    {
        var map = propertyMaps.FirstOrDefault(m => string.Equals(m.ODataPropertyName, propertyName, StringComparison.OrdinalIgnoreCase));
        
        return map?.EdmType ?? string.Empty;
    }
    
    /// <summary>
    /// Handle a literal value in the filter expression.
    /// </summary>
    /// <param name="literal"></param>
    /// <returns></returns>
    internal (bool Success, string? ErrorMessage, string? SqlQuery) HandleLiteral(string literal)
    {
        if (LooksLikeWktLiteral(literal))
        {
            try
            {
                literal = UnwrapQuotes(literal);
                
                var r = new NetTopologySuite.IO.WKTReader();
                var g = r.Read(literal);
                
                g.SRID = srid;
                
                var (ok, g4258) = CrsHelper.TransformGeometry(srid, 4258, g);
                var res = ok && g4258 != null ? g4258 : g;
                
                res.SRID = 4258;

                var w = new NetTopologySuite.IO.WKTWriter { MaxCoordinatesPerLine = int.MaxValue };
                
                literal = w.Write(res); // vervang literal
                
                return (true, null, CreateParameter(literal));
            }
            catch
            {
                return (true, null, CreateParameter(literal));
            }
        }

        if (literal.Equals("null", StringComparison.OrdinalIgnoreCase))
        {
            return (true, null, "'null'");
        }
        
        var value = literal.ParseLiteralValue();
        
        return (true, null, CreateParameter(value));
    }
    
    private static bool LooksLikeWktLiteral(string s)
    {
        var t = UnwrapQuotes(s).TrimStart();
        return t.StartsWith("POINT", StringComparison.OrdinalIgnoreCase)
               || t.StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase)
               || t.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase)
               || t.StartsWith("MULTI", StringComparison.OrdinalIgnoreCase)
               || t.StartsWith("GEOMETRYCOLLECTION", StringComparison.OrdinalIgnoreCase);
    }

    private static string UnwrapQuotes(string s)
    {
        s = s.Trim();
        return (s.Length >= 2 && ((s[0] == '\'' && s[^1] == '\'') || (s[0] == '\"' && s[^1] == '\"')))
            ? s.Substring(1, s.Length - 2)
            : s;
    }
    
    private object ExtractParameterValue(string? sqlQuery)
    {
        if (string.IsNullOrEmpty(sqlQuery))
            throw new ArgumentNullException(nameof(sqlQuery));

        // If it's a parameter (e.g., @p1), look it up
        if (sqlQuery[0] == parameterPrefix && _parameters.TryGetValue(sqlQuery, out var value))
            return value;

        // Otherwise, try to parse as a literal
        if (double.TryParse(sqlQuery, out var d))
            return d;

        // Remove quotes if present
        return UnwrapQuotes(sqlQuery);
    }
    
    private static object ConvertDistanceToMetersIfNeeded(object threshold, object unit)
    {
        if (unit is string s)
        {
            if (s.Equals("m", StringComparison.OrdinalIgnoreCase))
            {
                if (threshold is double d)
                    return d / 111320.0;
                if (double.TryParse(threshold.ToString(), out var parsed))
                    return parsed / 111320.0;
            }
            if (s.Equals("d", StringComparison.OrdinalIgnoreCase))
            {
                return threshold;
            }
        }
        return threshold;
    }
}
