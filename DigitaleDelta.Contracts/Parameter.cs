// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents a parameter for an OData function or action.
/// </summary>
public class Parameter
{
    /// <summary>
    /// Gets or sets the name of the parameter.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the EDM (Entity Data Model) type of the parameter.
    /// </summary>
    public required string Type { get; set; }
}