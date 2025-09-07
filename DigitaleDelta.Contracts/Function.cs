// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// Represents an OData function.
/// </summary>
public class Function
{
    /// <summary>
    /// Gets or sets the name of the function.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Gets or sets the list of parameters for the function.
    /// </summary>
    public required List<Parameter> Parameters { get; set; } = [];
    /// <summary>
    /// Gets or sets the return type of the function.
    /// </summary>
    public required string ReturnType { get; set; }
}