// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents a complex type in the OData model.
/// Complex types are keyless named structured types.
/// </summary>
public class ComplexType
{
    /// <summary>
    /// Gets the name of the complex type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the collection of properties for this complex type.
    /// </summary>
    public List<Property> Properties { get; set; } = [];
}