// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DigitaleDelta.Contracts;
using DigitaleDelta.ODataTranslator.Helpers;
using DigitaleDelta.ODataTranslator.Models;

namespace DigitaleDelta.ODataTranslator;

/// <summary>
/// ODataFilterValidator validates OData filter expressions against a CSDL model.
/// </summary>
/// <param name="csdlModel"></param>
/// <param name="functionMaps"></param>
public class ODataFilterValidator(CsdlModel csdlModel, IEnumerable<ODataFunctionMap> functionMaps)
{
    /// <summary>
    /// Parse the CSDL model and function maps.
    /// </summary>
    /// <param name="filterContext"></param>
    /// <param name="entitySetName"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public bool TryValidate(ODataParser.FilterOptionContext? filterContext, string entitySetName, out string? error)
    {
        if (filterContext == null)
        {
            error = ErrorMessages.filterContextIsNull;
            
            return false;
        }
        
        var entitySet = csdlModel.EntityContainers.SelectMany(c => c.EntitySets).FirstOrDefault(es => es.Name == entitySetName);
        if (entitySet == null)
        {
            error = string.Format(ErrorMessages.entitySetNotFound, entitySetName);
            
            return false;
        }

        var entityType = csdlModel.EntityTypes.First(et => et.Name == entitySet.EntityType.Split('.').Last());
        var (isValid, errorPart) = ValidateFilterExpression(filterContext.filterExpr(), entityType);
        error = isValid ? null : string.Format(ErrorMessages.invalidFilterExpression, errorPart);
        
        return isValid;
    }

    /// <summary>
    /// Validate the filter expression against the entity type.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    private (bool Valid, string? Error) ValidateFilterExpression(ODataParser.FilterExprContext context, EntityType entityType)
    {
        if (context.primary() != null)
        {
            var propertyPath     = context.primary().GetText();
            var propertySegments = propertyPath.Split('/');
            var currentType      = (object)entityType;

            foreach (var segment in propertySegments)
            {
                if (segment.IsLiteralValue())
                {
                    continue;
                }

                ProcessSegment(segment, currentType, out var found, out var nextType, out var error);
                if (!found)
                {
                    return (false, error);
                }

                currentType = nextType;
            }

            return (true, null);
        }

        if (context.function() != null)
        {
            var functionContext = context.function();
            var functionName    = functionContext.Start.Text.ToLower();
            var functionMap = functionMaps.First(f => f.ODataFunctionName == functionName);
            var arguments = functionContext.filterExpr();
            
            // Fallback ALLEEN wanneer exact 1 arg verwacht wordt en er 0 geparsed zijn
            if (arguments.Length == 0 && functionMap.ExpectedArgumentTypes.Count == 1)
            {
                var raw = functionContext.GetText();
                var lParen = raw.IndexOf('(');
                var rParen = raw.LastIndexOf(')');
                if (lParen >= 0 && rParen > lParen + 1)
                {
                    var inner = raw.Substring(lParen + 1, rParen - lParen - 1).Trim();
                    if (!string.IsNullOrWhiteSpace(inner) && IsQuoted(inner))
                    {
                        if (!string.Equals(functionMap.ExpectedArgumentTypes[0], "string", StringComparison.OrdinalIgnoreCase))
                        {
                            return (false, string.Format(ErrorMessages.invalidFunctionDataType, 1, functionName, "Edm.String", functionMap.ExpectedArgumentTypes[0]));
                        }

                        return (true, null); // valide
                    }
                }

                return (false, string.Format(ErrorMessages.functionParameterCountMismatch, functionName, functionMap.ExpectedArgumentTypes.Count, 0));
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                var argument     = arguments[i];
                var expectedType = functionMap.ExpectedArgumentTypes[i];
                var actualType   = GetExpressionEdmType(argument, entityType);
                
                if (functionName == "distance" && i == 1)
                {
                    // Example: Only allow "km" or "miles" as the second argument
                    var value = argument.GetText().Trim('\'', '"');
                    if (value != "m" && value != "d")
                    {
                        return (false, $"Invalid unit '{value}' for distance. Allowed: 'm' (meters), 'y' (yards), 'd' (degrees).");
                    }
                }

                if (!string.Equals(expectedType, MapEdmToExpected(actualType), StringComparison.OrdinalIgnoreCase))
                {
                    return (false, string.Format(ErrorMessages.invalidFunctionDataType, i + 1, functionName, actualType, expectedType));
                }
            }

            return (true, null);
        }

        // NOT: valideer de innerlijke expressie (unary not)
        if (context.filterExpr().Length == 1 && context.NOT() != null)
        {
            var inner = ValidateFilterExpression(context.filterExpr(0), entityType);
            return (inner.Valid, inner.Error);
        }

        if (context.LPAREN() != null && context.RPAREN() != null)
        {
            return ValidateFilterExpression(context.filterExpr(0), entityType);
        }

        var leftValid  = ValidateFilterExpression(context.filterExpr(0), entityType);
        var rightValid = ValidateFilterExpression(context.filterExpr(1), entityType);

        return (leftValid.Valid && rightValid.Valid, leftValid.Error ?? rightValid.Error);

    }

    /// <summary>
    /// Get the EDM type of the expression in the filter context.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="entityType"></param>
    /// <returns></returns>
    private string GetExpressionEdmType(ODataParser.FilterExprContext context, EntityType entityType)
  {
      if (context.function() != null)
      {
          var functionName = context.function().Start.Text;
          var fnMap = functionMaps.FirstOrDefault(f => string.Equals(f.ODataFunctionName, functionName, StringComparison.OrdinalIgnoreCase));

          return !string.IsNullOrEmpty(fnMap?.ReturnType) ? fnMap.ReturnType : "Edm.Unknown";
      }

      // Unary NOT resulteert in boolean
      if (context.NOT() != null)
      {
          return "Edm.Boolean";
      }
      
      if (context.primary() != null)
      {
          var primaryText = context.primary().GetText();
          var literalEdm = InferEdmTypeFromLiteral(primaryText);

          if (literalEdm != null)
          {
              return literalEdm;
          }

          var propertyType = GetPropertyPathType(primaryText, entityType);
          if (!string.IsNullOrWhiteSpace(propertyType) && !string.Equals(propertyType, "Edm.Unknown", StringComparison.OrdinalIgnoreCase))
          {
              return propertyType;
          }
      }

      var text = context.GetText();

      {
          var literalEdm = InferEdmTypeFromLiteral(text);
          if (literalEdm != null)
          {
              return literalEdm;
          }
      }

      if (!string.IsNullOrEmpty(text) && !IsQuoted(text))
      {
          var propertyType = GetPropertyPathType(text, entityType);
          if (!string.IsNullOrWhiteSpace(propertyType) && !string.Equals(propertyType, "Edm.Unknown", StringComparison.OrdinalIgnoreCase))
          {
              return propertyType;
          }
      }

      if (context.comparison() != null || context.AND() != null || context.OR() != null)
      {
          return "Edm.Boolean";
      }

      if (context.filterExpr().Length == 1)
      {
          return GetExpressionEdmType(context.filterExpr(0), entityType);
      }

      return "Edm.Unknown";
  }

    /// <summary>
    /// Determines if the specified string is enclosed in single or double quotes.
    /// </summary>
    /// <param name="s">The string to check for enclosing quotes.</param>
    /// <returns>True if the string is enclosed in single or double quotes; otherwise, false.</returns>
    private static bool IsQuoted(string s)
    {
        return s.Length >= 2 && ((s[0] == '\'' && s[^1] == '\'') || (s[0] == '"' && s[^1] == '"'));
    }

    /// <summary>
    /// Infers the EDM type from a given literal value.
    /// </summary>
    /// <param name="text">The literal value to analyze and infer its corresponding EDM type.</param>
    /// <returns>
    /// The inferred EDM type as a string if the literal matches a known type; otherwise, null.
    /// </returns>
    private static string? InferEdmTypeFromLiteral(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // null literal
        if (string.Equals(text, "null", StringComparison.OrdinalIgnoreCase))
        {
            return "Edm.Null";
        }

        // boolean literal
        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
        {
            return "Edm.Boolean";
        }

        // string literal: '...'
        if (text is ['\'', _, ..] && text[^1] == '\'')
        {
            return "Edm.String";
        }

        // eenvoudige numerieke detectie
        if (int.TryParse(text, out _))
        {
            return "Edm.Int32";
        }

        if (long.TryParse(text, out _))
        {
            return "Edm.Int64";
        }

        if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out _))
        {
            return "Edm.Double";
        }

        return null;
    }


    /// <summary>
    /// Determines the EDM's (Entity Data Model) property type.
    /// </summary>
    /// <param name="propertyPath">The string representing the property path, which may contain one or more segmented fields separated by '/'.</param>
    /// <param name="entityType">The entity type in which the property path is evaluated.</param>
    /// <returns>A string representing the EDM type of the property path, or "Edm.Unknown" if the type cannot be determined.</returns>
    private string GetPropertyPathType(string propertyPath, EntityType entityType)
    {
        if (string.IsNullOrWhiteSpace(propertyPath))
        {
            return "Edm.Unknown";
        }

        var segments = propertyPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
        {
            return "Edm.Unknown";
        }

        object currentType = entityType;

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            var isLastSegment = i == segments.Length - 1;

            if (currentType is EntityType et)
            {
                var prop = et.Properties.FirstOrDefault(p =>
                    string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    return "Edm.Unknown";
                }

                if (isLastSegment)
                {
                    return string.IsNullOrWhiteSpace(prop.Type) ? "Edm.Unknown" : prop.Type;
                }

                var complex = GetComplexType(prop);
                currentType = complex != null ? complex : prop;
                continue;
            }

            if (currentType is ComplexType ct)
            {
                var prop = ct.Properties.FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    return "Edm.Unknown";
                }

                if (isLastSegment)
                {
                    return string.IsNullOrWhiteSpace(prop.Type) ? "Edm.Unknown" : prop.Type;
                }

                var complex = GetComplexType(prop);
                currentType = complex != null ? complex : prop;
                continue;
            }

            if (currentType is Property propCurrent)
            {
                if (!string.IsNullOrWhiteSpace(propCurrent.Type) && propCurrent.Type.StartsWith("Edm.", StringComparison.OrdinalIgnoreCase))
                {
                    return "Edm.Unknown";
                }

                var complex = GetComplexType(propCurrent);
                var prop = complex?.Properties.FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));

                if (prop == null)
                {
                    return "Edm.Unknown";
                }

                if (isLastSegment)
                {
                    return string.IsNullOrWhiteSpace(prop.Type) ? "Edm.Unknown" : prop.Type;
                }

                var deeper = GetComplexType(prop);
                
                currentType = deeper != null ? deeper : prop;
                continue;
            }

            return "Edm.Unknown";
        }

        return "Edm.Unknown";
    }

    /// <summary>
    /// Get the complex type for a property.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private ComplexType? GetComplexType(Property property)
    {
        if (string.IsNullOrWhiteSpace(property.Type))
        {
            return null;
        }

        var typeName = property.Type.Trim();

        if (typeName.StartsWith("Edm.", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var shortName = typeName.Contains('.')
            ? typeName.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Last()
            : typeName;

        // Case-insensitive match op ComplexType.Name
        return csdlModel.ComplexTypes.FirstOrDefault(ct =>
            string.Equals(ct.Name, shortName, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ct.Name, typeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Process a segment of the property path.
    /// </summary>
    /// <param name="segment"></param>
    /// <param name="currentType"></param>
    /// <param name="found"></param>
    /// <param name="nextType"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    private void ProcessSegment(string segment, object? currentType, out bool found, out object? nextType, out string? error)
    {
        found = false;
        nextType = null;
        error = null;
        
        switch (currentType)
        {
            case EntityType currentEntityType:
            {
                var property = currentEntityType.Properties.FirstOrDefault(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    found = false;
                    nextType = null;
                    error = string.Format(ErrorMessages.propertyNotFoundInEntityType, segment, currentEntityType.Name);

                    return;
                }

                // Look for a complex type first
                var complexType = GetComplexType(property);
                if (complexType != null)
                {
                    found = true;
                    nextType = complexType;
                    error = null;

                    return;
                }

                // For non-complex properties, return the property type information
                found = true;
                nextType = currentEntityType;
                error = null;

                return;
            }
            case ComplexType currentComplexType:
            {
                var property = currentComplexType.Properties.First(p => string.Equals(p.Name, segment, StringComparison.OrdinalIgnoreCase));
                var propertyTypeName = property.Type.Split('.').Last();
                var complexType = csdlModel.ComplexTypes.FirstOrDefault(ct => string.Equals(ct.Name, propertyTypeName, StringComparison.OrdinalIgnoreCase));

                found = true;
                nextType = complexType ?? currentComplexType;
                error = null;

                return;
            }
        }
    }

    /// <summary>
    /// Converts an EDM type name to a corresponding expected type name used for validation.
    /// </summary>
    /// <param name="edmType">The EDM type name to be converted. This is expected to be a string prefixed with "Edm." or a standard EDM type name.</param>
    /// <returns>Returns the corresponding expected type name as a string (e.g., "string", "integer", "decimal", etc.). If the input is null or whitespace, it returns "unknown".</returns>
    private static string MapEdmToExpected(string? edmType)
    {
        if (string.IsNullOrWhiteSpace(edmType))
        {
            return "unknown";
        }

        var t = edmType.StartsWith("Edm.", StringComparison.OrdinalIgnoreCase)
            ? edmType[4..]
            : edmType;

        return t.ToLowerInvariant() switch
        {
            "string" or "guid" or "binary" => "string",
            "boolean" => "boolean",
            "byte" or "sbyte" or "int16" or "int32" or "int64" => "integer",
            "decimal" or "double" or "single" or "float" => "decimal",
            "datetime" or "datetimeoffset" or "date" or "timeofday" or "duration" => "datetime",
            "geography" or "geographycollection" or "geographypoint" or "geographylinestring" or "geographypolygon" or "geographymultipoint" or "geographymultilinestring" or "geographymultipolygon" => "geography",
            _ => t.ToLowerInvariant()
        };
    }

}