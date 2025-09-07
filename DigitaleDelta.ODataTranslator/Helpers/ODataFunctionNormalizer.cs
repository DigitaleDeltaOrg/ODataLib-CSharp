namespace DigitaleDelta.ODataTranslator.Helpers;

/// <summary>
/// Provides functionality for normalizing OData function names to a consistent format because ANTLR does not support case-insensitive matching.
/// </summary>
/// <remarks>
/// The <see cref="ODataFunctionNormalizer"/> class is designed to handle normalization of known OData function names
/// by converting them to a consistent case, such as lowercase, while preserving string literals.
/// </remarks>
public sealed class ODataFunctionNormalizer
{
    private readonly HashSet<string> _knownFunctions;

    private static readonly string[] DefaultFunctions =
    [
        "contains","startswith","endswith","now","tolower","toupper","length","index","substring","trim",
        "year","month","day","hour","minute","second","date","time","floor","ceil","round","abs",
        "distance","intersects"
    ];

    /// <summary>
    /// Provides functionality to normalize OData function names by converting known function names
    /// to a consistent format, such as lowercase, while preserving string literals.
    /// </summary>
    public ODataFunctionNormalizer(IEnumerable<string>? knownFunctions = null)
    {
        _knownFunctions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in (knownFunctions ?? DefaultFunctions))
        {
            if (!string.IsNullOrWhiteSpace(f))
                _knownFunctions.Add(f.Trim());
        }
    }

    /// <summary>
    /// Normalizes a given input string by converting known function names to lowercase while preserving string literals, because ANTLR does not support case-insensitive matching.
    /// </summary>
    /// <param name="input">The input string to normalize. If null or empty, the original input is returned.</param>
    /// <returns>The normalized string where recognized function names are converted to lowercase.</returns>
    public string Normalize(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new System.Text.StringBuilder(input.Length);
        var inString = false;
        var i = 0;

        while (i < input.Length)
        {
            var c = input[i];

            if (c == '\'')
            {
                if (inString && i + 1 < input.Length && input[i + 1] == '\'')
                {
                    sb.Append("''");
                    i += 2;
                    continue;
                }
                inString = !inString;
                sb.Append(c);
                i++;
                continue;
            }

            if (!inString && (char.IsLetter(c) || c == '_'))
            {
                var start = i;
                i++;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;

                var ident = input[start..i];

                var j = i;
                while (j < input.Length && char.IsWhiteSpace(input[j])) j++;

                var looksLikeFunctionCall = j < input.Length && input[j] == '(';

                if (looksLikeFunctionCall && _knownFunctions.Contains(ident))
                {
                    sb.Append(ident.ToLowerInvariant()); 
                }
                else
                {
                    sb.Append(ident);
                }

                continue;
            }

            sb.Append(c);
            i++;
        }

        return sb.ToString();
    }

    /// <summary>
    /// Creates a new instance of the <see cref="ODataFunctionNormalizer"/> class with the specified collection of function names.
    /// </summary>
    /// <param name="functionNames">A collection of function names to be recognized by the normalizer. If null, default functions will be used.</param>
    /// <returns>A new instance of the <see cref="ODataFunctionNormalizer"/> class initialized with the specified function names.</returns>
    public static ODataFunctionNormalizer FromNames(IEnumerable<string> functionNames) => new(functionNames);
}
