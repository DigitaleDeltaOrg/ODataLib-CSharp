// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataTranslator;

/// <summary>
/// Provides a collection of error message templates used across components
/// of the DigitaleDelta.ODataTranslator namespace. These messages are primarily
/// used for error reporting when working with OData filter parsing and validation logic.
/// </summary>
/// <remarks>
/// This class contains constant string fields which can be formatted with specific
/// context-related data to yield complete error messages.
/// </remarks>
public abstract record ErrorMessages
{
    /// <summary>
    /// Unexpected token
    /// </summary>
    public const string unexpectedTokenFound = "Unexpected token found: '{0}'.";

    /// <summary>
    /// Failed to parse the filter
    /// </summary>
    public const string failedToParseFilterQuery = "Failed to parse filter query.";

    /// <summary>
    /// Filter context is null
    /// </summary>
    public const string filterContextIsNull = "Filter context is null.";

    /// <summary>
    /// Entity set not found
    /// </summary>
    public const string entitySetNotFound = "EntitySet '{0}' not found in the CSDL model.";

    /// <summary>
    /// Invalid filter expression
    /// </summary>
    public const string invalidFilterExpression = "Invalid filter expression: {0}";

    /// <summary>
    /// Unexpected argument type
    /// </summary>
    public const string invalidFunctionDataType = "Argument {0} of function '{1}' has type '{2}', but expected '{3}'.";

    /// <summary>
    /// Missing property
    /// </summary>
    public const string propertyNotFoundInEntityType = "Property '{0}' not found in EntityType '{1}'";

    /// <summary>
    /// Unknown property
    /// </summary>
    public const string unknownProperty = "Unknown property '{0}'.";

    /// <summary>
    /// Unknown function
    /// </summary>
    public const string unknownFunction = "Unknown function '{0}'.";

    /// <summary>
    /// Unknown operator
    /// </summary>
    public const string unknownOperator = "Unknown operator '{0}'.";

    /// <summary>
    /// Argument count mismatch
    /// </summary>
    public const string functionParameterCountMismatch = "Function '{0}' expects {1} arguments, but {2} were provided.";

    /// <summary>
    /// IN type mismatch
    /// </summary>
    public const string inClauseTypeMismatch = "Type mismatch in IN clause: '{0}' does not match type of '{1}'";

    /// <summary>
    /// Missing literal
    /// </summary>
    public const string mustBeALiteralValue = "'{0}' must be a literal value.";

    /// <summary>
    /// Invalid $top
    /// </summary>
    public const string topOutOfRange = "Top value must be between 1 and {0}.";

    /// <summary>
    /// Distance out or range for degree-based coordinate systems.
    /// </summary>
    public const string distanceOutOfRange = "The distance comparer is not valid for Coordinate Reference Systems that use degrees (latitude/longitude, -90 to 90). Change the comparer or select a Coordinate Reference System that uses meters.";
}