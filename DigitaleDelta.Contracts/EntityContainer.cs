// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents an OData entity container, which is a collection of entity sets.
/// </summary>
public class EntityContainer
{
    /// <summary>
    /// Gets or sets the name of the entity container.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the list of entity sets within this container.
    /// </summary>
    public List<EntitySet> EntitySets { get; set; } = [];
}