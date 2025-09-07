// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents an OData EntitySet, which is a named collection of entities of a specific entity type.
/// </summary>
public class EntitySet
{
    /// <summary>
    /// Gets the name of the EntitySet.
    /// </summary>
    public required string Name { get; init; }
    /// <summary>
    /// Gets the fully qualified name of the entity type for the entities in this set.
    /// </summary>
    public required string EntityType { get; init; }
}
