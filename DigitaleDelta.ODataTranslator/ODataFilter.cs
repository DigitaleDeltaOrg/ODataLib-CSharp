// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Antlr4.Runtime;

namespace DigitaleDelta.ODataTranslator;

/// <summary>
/// Represents an OData filter query, enabling parsing and validation functionality.
/// </summary>
public class ODataFilter
{
    /// <summary>
    /// Gets the parsed filter context associated with the OData filter query.
    /// This property provides access to the underlying <see cref="ODataParser.FilterOptionContext"/> object,
    /// which represents the structured representation of the filter query following parsing.
    /// </summary>
    public ODataParser.FilterOptionContext Context { get; }

    /// <summary>
    /// Represents an OData filter that can parse and validate filter queries based on the OData syntax.
    /// </summary>
    private ODataFilter(ODataParser.FilterOptionContext context)
    {
        Context = context;
    }

    /// <summary>
    /// Attempts to parse the given OData filter query string into an <see cref="ODataFilter"/> object.
    /// </summary>
    /// <param name="query">The OData filter query string to parse.</param>
    /// <param name="filter">
    /// When this method returns, contains the <see cref="ODataFilter"/> object resulting from the parsing if successful; otherwise, <c>null</c>.
    /// </param>
    /// <param name="error">
    /// When this method returns, contains the error message if the parsing fails; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the query was successfully parsed into an <see cref="ODataFilter"/> object; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParse(string query, out ODataFilter? filter, out string? error)
    {
        error = null;
        filter = null;

        var inputStream = new AntlrInputStream(query);
        var lexer       = new ODataLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser      = new ODataParser(tokenStream);
        var result = parser.filterOption();

        if (string.IsNullOrEmpty(query))
        {
            filter = new ODataFilter(result);
            
            return true;       
        }
        
        if (parser.CurrentToken.Type != TokenConstants.EOF)
        {
            var offendingToken = parser.CurrentToken;
            error = string.Format(ErrorMessages.unexpectedTokenFound, offendingToken.Text);
            filter = null;
            
            return false;
        }

        if (result == null || parser.NumberOfSyntaxErrors > 0)
        {
            error = ErrorMessages.failedToParseFilterQuery;
            filter = null;
            
            return false;
        }
        
        error = null;
        filter = new ODataFilter(result);
        
        return true;
    }
}