// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.CsdlParser;

/// <summary>
/// Represents a collection of error messages used for CSDL parsing validation.
/// </summary>
public abstract record ErrorMessages
{
    /// <summary>
    /// Entity set name is required
    /// </summary>
    public const string entitySetNameIsRequired = "EntitySet name is required.";
    /// <summary>
    /// Entity container is required
    /// </summary>
    public const string entityContainerNameIsRequired = "EntityContainer name is required.";
    /// <summary>
    /// Entity container name is required
    /// </summary>
    public const string invalidCsdlFormat = "Invalid CSDL format.";
    /// <summary>
    /// Invalid CSDL format
    /// </summary>
    public const string entitySetEntityTypeIsRequired = "EntitySet EntityType is required.";
    /// <summary>
    /// Entity type name required
    /// </summary>
    public const string entityTypeNameIsRequired = "EntityType name is required.";
    /// <summary>
    /// Property type is required for entity type
    /// </summary>
    public const string propertyTypeIsRequiredForEntityType = "Property type is required for EntityType '{0}'.";
    /// <summary>
    /// Property name is required for entity type
    /// </summary>
    public const string propertyNameIsRequiredForEntityType = "Property name is required for EntityType '{0}'.";
    /// <summary>
    /// Complextype name is required
    /// </summary>
    public const string complexTypeNameIsRequired = "ComplexType name is required.";
    /// <summary>
    /// Property name is required for complex type 
    /// </summary>
    public const string propertyNameIsRequiredForComplexType = "Property name is required for ComplexType '{0}'.";
    /// <summary>
    /// Property type is required for property in complex type
    /// </summary>
    public const string propertyTypeIsRequiredForPropertyInComplexType = "Property type is required for Property '{0}' in ComplexType '{1}'.";
    /// <summary>
    /// Function name is required
    /// </summary>
    public const string functionNameIsRequired = "Function name is required.";
    /// <summary>
    /// Function return type is required
    /// </summary>
    public const string functionReturnTypeIsRequired = "Function return type is required for Function '{0}'.";
    /// <summary>
    /// Parameter type is required for parameters
    /// </summary>
    public const string parameterTypeIsRequiredForParameterInFunction = "Parameter type is required for Parameter '{0}' in Function '{1}'.";
    /// <summary>
    /// Parsing error
    /// </summary>
    public const string parsingError = "An error occurred while parsing the CSDL: {0}";
}