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
    public const string unexpectedTokenFound = "Unexpected token found: '{0}'.";
    public const string failedToParseFilterQuery = "Failed to parse filter query.";
    public const string filterContextIsNull = "Filter context is null.";
    public const string entitySetNotFound = "EntitySet '{0}' not found in the CSDL model.";
    public const string invalidFilterExpression = "Invalid filter expression: {0}";
    public const string invalidFunctionDataType = "Argument {0} of function '{1}' has type '{2}', but expected '{3}'.";
    public const string unknownPropertyType = "Unknown propert type: {0}";
    public const string propertyNotFoundInEntityType = "Property '{0}' not found in EntityType '{1}'";
    public const string unknownProperty = "Unknown property '{0}'.";
    public const string unknownFunction = "Unknown function '{0}'.";
    public const string unknownOperator = "Unknown operator '{0}'.";
    public const string functionParameterCountMismatch = "Function '{0}' expects {1} arguments, but {2} were provided.";
    public const string inClauseTypeMismatch = "Type mismatch in IN clause: '{0}' does not match type of '{1}'";
    public const string mustBeALiteralValue = "'{0}' must be a literal value.";
    public const string topOutOfRange = "Top value must be between 1 and {0}.";
}