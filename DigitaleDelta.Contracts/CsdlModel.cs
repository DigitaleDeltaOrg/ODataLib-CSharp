// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents a CSDL (Common Schema Definition Language) model, which defines the data model for an OData service.
/// </summary>
public class CsdlModel
{
    /// <summary>
    /// Gets or sets the list of entity types defined in the model.
    /// </summary>
    public List<EntityType> EntityTypes { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of complex types defined in the model.
    /// </summary>
    public List<ComplexType> ComplexTypes { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of functions defined in the model.
    /// </summary>
    public List<Function> Functions { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of entity containers defined in the model.
    /// </summary>
    public List<EntityContainer> EntityContainers { get; set; } = [];
}