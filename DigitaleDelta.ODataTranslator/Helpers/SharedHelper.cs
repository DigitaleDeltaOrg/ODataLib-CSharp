// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataTranslator.Helpers;

/// <summary>
/// Shared helper for translator
/// </summary>
public static class SharedHelper
{
    /// <summary>
    /// Infer the EDM type of literal value based on its content.
    /// </summary>
    /// <param name="literal"></param>
    /// <returns></returns>
    internal static string InferLiteralType(this string literal)
    {
        if (int.TryParse(literal, out _))
        {
            return DigitaleDeltaEdmType.edmInt32;
        }

        if (double.TryParse(literal, out _))
        {
            return DigitaleDeltaEdmType.edmDouble;
        }

        if (bool.TryParse(literal, out _))
        {
            return DigitaleDeltaEdmType.edmBoolean;
        }

        if (DateTimeOffset.TryParse(literal, out _))
        {
            return DigitaleDeltaEdmType.edmDateTimeOffset;
        }

        return DigitaleDeltaEdmType.edmString;
    }
    

    /// <summary>
    /// Determines is the value is a literal.
    /// </summary>
    /// <param name="segment">segment to parse</param>
    /// <returns></returns>
    public static bool IsLiteralValue(this string segment)
    {
        // Check if the segment is a string literal
        if ((segment.StartsWith('\'') && segment.EndsWith('\'')) || (segment.StartsWith('"') && segment.EndsWith('"')))
        {
            return true;
        }

        if (segment.Equals("null", StringComparison.CurrentCultureIgnoreCase))
        {
            return true;
        }
        
        // Check if the segment is a numeric literal (integer or floating-point)
        if (int.TryParse(segment, out _) || double.TryParse(segment, out _) || bool.TryParse(segment, out _))
        {
            return true;
        }

        // Check if the segment is a date/time literal (ISO 8601 format)
        return DateTime.TryParse(segment, out _);
    }
    
    /// <summary>
    /// Parse a literal value from the OData filter expression.
    /// </summary>
    /// <param name="literal"></param>
    /// <returns></returns>
    public static object ParseLiteralValue(this string literal)
    {
        // Remove quotes for string literals
        if ((literal.StartsWith('\'') && literal.EndsWith('\'')) || 
            (literal.StartsWith('"') && literal.EndsWith('"')))
        {
            return literal[1..^1];
        }

        // Handle numeric literals
        if (bool.TryParse(literal, out var boolValue))
        {
            return boolValue;
        }
        
        // Handle numeric literals
        if (int.TryParse(literal, out var intValue))
        {
            return intValue;
        }
        
        if (double.TryParse(literal, out var doubleValue))
        {
            return doubleValue;
        }
        
        if (DateTimeOffset.TryParse(literal, out var dateTimeOffsetValue))
        {
            return dateTimeOffsetValue;
        }
    
        return literal;
    }

    /// <summary>
    /// Determine if two types are compatible in the context of OData.
    /// </summary>
    /// <param name="leftType"></param>
    /// <param name="rightType"></param>
    /// <returns></returns>
    public static bool IsTypeCompatibleWith(this string leftType, string rightType)
    {
        // Check if the types are the same
        if (leftType.Equals(rightType, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (leftType.Equals(DigitaleDeltaEdmType.edmDouble, StringComparison.OrdinalIgnoreCase) && rightType.Equals(DigitaleDeltaEdmType.edmInt32, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmDecimal, StringComparison.OrdinalIgnoreCase) && rightType.Equals(DigitaleDeltaEdmType.edmInt32, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyPoint, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyMultiPoint, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyPolygon, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyMultiPolygon, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyLineString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeography, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeographyMultiLineString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryPoint, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryMultiPoint, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryPolygon, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryMultiPolygon, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryLineString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmGeometry, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGeometryMultiLineString, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        if (leftType.Equals(DigitaleDeltaEdmType.edmString, StringComparison.CurrentCulture) && rightType.Equals(DigitaleDeltaEdmType.edmGuid, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        return false;
    }
}