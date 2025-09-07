// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataTranslator.Models;

/// <summary>
/// Bridge between OData functions and their SQL equivalents.
/// </summary>
public record ODataFunctionMap
{
    /// <summary>
    /// Represents the name of the OData function.
    /// </summary>
    /// <remarks>
    /// This property is used to map the name of an OData function
    /// to its corresponding SQL function equivalent. It acts as
    /// the identifier for the specific OData function being referenced,
    /// facilitating translation from OData expressions to SQL-compatible syntax.
    /// </remarks>
    public required string ODataFunctionName { get; init; }

    /// <summary>
    /// Represents the format string for the SQL function equivalent of an OData function.
    /// </summary>
    /// <remarks>
    /// This property defines how to structure the SQL function call when translating
    /// from an OData function. Placeholders like "{0}", "{1}", etc., are used to represent
    /// arguments passed to the function, enabling dynamic substitution of actual values
    /// during the translation process.
    /// </remarks>
    public required string SqlFunctionFormat { get; init; } // Use placeholders like "{0}" for arguments

    /// <summary>
    /// Defines the expected types for the arguments of the OData function.
    /// </summary>
    /// <remarks>
    /// This property specifies the data types required for each argument of the OData function,
    /// ensuring compatibility during function mapping and validation processes. It is used
    /// to enforce type correctness when translating or executing the function, enabling
    /// validation of argument types and providing reliable conversions to associated SQL functions.
    /// </remarks>
    public required List<string> ExpectedArgumentTypes { get; init; } // The return type of the function

    /// <summary>
    /// Defines the return type of an OData function when mapped to its SQL equivalent.
    /// </summary>
    /// <remarks>
    /// This property specifies the data type that the SQL function is expected to return
    /// after executing its corresponding translation of the OData function. It ensures
    /// type consistency between the OData function definition and the resulting SQL expression.
    /// </remarks>
    public required string ReturnType { get; init; }

    /// <summary>
    /// Represents the position of a wildcard in a string processing operation for SQL translation.
    /// </summary>
    /// <remarks>
    /// This property specifies whether the wildcard should be applied at the beginning, end,
    /// or both ends of the string during the translation of an OData function to its SQL equivalent.
    /// It facilitates accurate mapping of OData string functions involving wildcard usage.
    /// </remarks>
    public WildCardPosition? WildCardPosition { get; init; }

    /// <summary>
    /// Represents the symbol used for wildcard operations in SQL translations.
    /// </summary>
    /// <remarks>
    /// This property is used in conjunction with the WildCardPosition to define the specific
    /// character or string that acts as a wildcard in SQL patterns. It enables accurate
    /// transformation of OData queries involving string operations with wildcards into
    /// their SQL equivalents.
    /// </remarks>
    public string? WildCardSymbol { get; init; }
}