// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.CsdlParser;

/// <summary>
/// Represents a collection of error messages used for CSDL parsing validation.
/// </summary>
public abstract record ErrorMessages
{
    public const string entitySetNameIsRequired = "EntitySet name is required.";
    public const string entityContainerNameIsRequired = "EntityContainer name is required.";
    public const string invalidCsdlFormat = "Invalid CSDL format.";
    public const string entitySetEntityTypeIsRequired = "EntitySet EntityType is required.";
    public const string entityTypeNameIsRequired = "EntityType name is required.";
    public const string propertyTypeIsRequiredForEntityType = "Property type is required for EntityType '{0}'.";
    public const string propertyNameIsRequiredForEntityType = "Property name is required for EntityType '{0}'.";
    public const string complexTypeNameIsRequired = "ComplexType name is required.";
    public const string propertyNameIsRequiredForComplexType = "Property name is required for ComplexType '{0}'.";
    public const string propertyTypeIsRequiredForPropertyInComplexType = "Property type is required for Property '{0}' in ComplexType '{1}'.";
    public const string functionNameIsRequired = "Function name is required.";
    public const string functionReturnTypeIsRequired = "Function return type is required for Function '{0}'.";
    public const string parameterTypeIsRequiredForParameterInFunction = "Parameter type is required for Parameter '{0}' in Function '{1}'.";
    public const string parsingError = "An error occurred while parsing the CSDL: {0}";
}