// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents a property of an OData entity.
/// </summary>
public class Property
{
    /// <summary>
    /// Gets the name of the property.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Gets the CLR type of the property as a string.
    /// </summary>
    public required string Type { get; init; }
    /// <summary>
    /// Gets or sets a value indicating whether the property can be null.
    /// </summary>
    public bool Nullable { get; set; }
    /// <summary>
    /// Gets or sets the EDM (Entity Data Model) type of the property.
    /// </summary>
    public EdmType EdmType { get; set; }
    /// <summary>
    /// Gets or sets the default value of the property.
    /// </summary>
    public string? DefaultValue { get; set; } // Example of a nullable property
    /// <summary>
    /// Gets or sets the maximum length for string or binary properties.
    /// </summary>
    public int? MaxLength { get; set; }
    /// <summary>
    /// Gets or sets the precision for numeric or decimal properties.
    /// </summary>
    public int? Precision { get; set; }
    /// <summary>
    /// Gets or sets the scale for numeric or decimal properties.
    /// </summary>
    public int? Scale { get; set; }
}
