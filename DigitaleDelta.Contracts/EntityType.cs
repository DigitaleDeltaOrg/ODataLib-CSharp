// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents a type of entity within the data model, defining its structure.
/// </summary>
public class EntityType
{
    /// <summary>
    /// Gets the name of the entity type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or sets the list of properties for this entity type.
    /// </summary>
    public List<Property> Properties { get; set; } = [];

    /// <summary>
    /// Gets or sets the list of property names that form the primary key for this entity type.
    /// </summary>
    public List<string> Keys { get; set; } = [];
}