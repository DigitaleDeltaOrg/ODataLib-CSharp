// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// OData Response Builder
/// </summary>
public class ODataResponseBuilder
{
    private string                                  _baseUrl         = string.Empty;
    private string                                  _entitySetName   = string.Empty;
    private IEnumerable<Dictionary<string, object?>> _entitiesToWrite = [];
    private int?                                    _total;
    private string?                                 _skipTokenToUse;
    private bool                                    _includeCount;
    private List<string>                            _selectedProperties = [];
    private bool _moreData;
    private string _queryString = string.Empty;

    /// <summary>
    /// Pagination
    /// </summary>
    /// <param name="skipToken">Skip token to add to the page</param>
    /// <param name="totalCount">Total count</param>
    /// <returns></returns>
    public ODataResponseBuilder WithPagination(Guid? skipToken, int? totalCount = null)
    {
        _skipTokenToUse = skipToken.ToString();
        _total = totalCount;
        
        return this;
    }

    /// <summary>
    /// Set the base URL for the OData response
    /// </summary>
    /// <param name="url">Base Url</param>
    /// <returns></returns>
    public ODataResponseBuilder WithBaseUrl(string url)
    {
        _baseUrl = url.TrimEnd('/');
        
        return this;
    }

    /// <summary>
    /// Set the entity set name
    /// </summary>
    /// <param name="setName">Entity set name</param>
    /// <returns></returns>
    public ODataResponseBuilder WithEntitySet(string setName)
    {
        _entitySetName = setName;
        
        return this;
    }
    
    /// <summary>
    /// Set the entity set name
    /// </summary>
    /// <param name="query">Total query string passed.</param>
    /// <returns></returns>
    public ODataResponseBuilder WithQuery(string? query)
    {
        _queryString = query ?? string.Empty;
        
        return this;
    }

    /// <summary>
    /// Entities to write
    /// </summary>
    /// <param name="entities">Entities</param>
    /// <returns></returns>
    public ODataResponseBuilder WithEntities(IEnumerable<Dictionary<string, object?>> entities)
    {
        _entitiesToWrite = entities;
        
        return this;
    }
    
    /// <summary>
    /// Include count in the response. Default: true.
    /// </summary>
    /// <param name="include">Include or not.</param>
    /// <returns></returns>
    public ODataResponseBuilder IncludeCount(bool include = true)
    {
        _includeCount = include;
        
        return this;
    }

    /// <summary>
    /// Specify the properties to select in the response
    /// </summary>
    /// <param name="propertyNames">Properties to return</param>
    /// <returns></returns>
    public ODataResponseBuilder WithSelectProperties(IEnumerable<string> propertyNames)
    {
        _selectedProperties = propertyNames.Select(a => a.Trim()).ToList();
        
        return this;
    }
    
    /// <summary>
    /// Specify the properties to select in the response
    /// </summary>
    /// <param name="propertyNames">Properties to return</param>
    /// <returns></returns>
    public ODataResponseBuilder WithSelectProperties(string? propertyNames)
    {
        return string.IsNullOrWhiteSpace(propertyNames) 
            ? this 
            : WithSelectProperties(propertyNames.Split(','));
    }

    /// <summary>
    /// Indicates if there is more data to be retrieved.
    /// </summary>
    /// <param name="hasMoreData">A boolean value indicating if more data is available.</param>
    /// <returns>An instance of the ODataResponseBuilder.</returns>
    public ODataResponseBuilder HasMoreData(bool hasMoreData)
    {
        _moreData = hasMoreData;

        return this;
    }
    
    /// <summary>
    /// Construct the OData response
    /// </summary>
    /// <returns></returns>
    public ODataResponse Build()
    {
        var context = $"{_baseUrl}/$metadata#{_entitySetName}";
        var selectedEntities = _entitiesToWrite.Select(entity => _selectedProperties.Count == 0 
            ? entity 
            : ApplySelect(entity, _selectedProperties)).ToList();
        var response = new ODataResponse(selectedEntities, context);

        if (_includeCount && _total.HasValue)
        {
            response.Count = _total.Value;
        }

        if (_moreData)
        {
            response.NextLink = SetNextLink();   
        }

        return response;
    }

    /// <summary>
    /// Sets the NextLink property with pagination and optional selected properties.
    /// </summary>
    /// <returns>A string representing the generated NextLink with pagination and selected properties applied.</returns>
    private string SetNextLink()
    {
        if (!_moreData)
        {
            return string.Empty;
        }
        
        var parts = _queryString.Replace("?", string.Empty).Split('&').ToList();
        parts.RemoveAll(p => p.StartsWith("$skiptoken=", StringComparison.OrdinalIgnoreCase));
        parts.Add($"$skiptoken={_skipTokenToUse}");
        
        var nextLink = $"{_baseUrl}?" + string.Join("&", parts).Replace("?&", "?").Replace("&&", "&");
        
        return nextLink;
    }

    /// <summary>
    /// Applies the $select query option to a provided entity, filtering its properties based on the specified selection.
    /// Supports nested paths for deep property selection.
    /// </summary>
    /// <param name="entity">The entity to apply the selection on.</param>
    /// <param name="selectProperties">A collection of property names to include in the result. Supports nested property paths separated by '/'.</param>
    /// <returns>A new dictionary containing only the selected properties of the entity.</returns>
    private static Dictionary<string, object?> ApplySelect(IDictionary<string, object?> entity, IEnumerable<string> selectProperties)
    {
        var wanted = new HashSet<string>(selectProperties.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()), StringComparer.OrdinalIgnoreCase);

        return CopySelectedLevel(entity, wanted, prefix: null);
    }

    /// <summary>
    /// Copies the selected levels of a nested dictionary based on the specified selected paths.
    /// </summary>
    /// <param name="current">The current dictionary to process for selection.</param>
    /// <param name="wantedPaths">The set of paths that determine selected keys at various levels.</param>
    /// <param name="prefix">An optional prefix to track the key path during recursion.</param>
    /// <returns>A new dictionary containing only the selected levels and keys.</returns>
    private static Dictionary<string, object?> CopySelectedLevel(IDictionary<string, object?> current, HashSet<string> wantedPaths, string? prefix)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, value) in current)
        {
            var fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}/{key}";
            var parentSelected      = wantedPaths.Contains(key) || wantedPaths.Contains(fullKey);
            var hasNestedSelections = wantedPaths.Any(p => p.StartsWith(fullKey + "/", StringComparison.OrdinalIgnoreCase));

            if (value is IDictionary<string, object?> childDict)
            {
                if (parentSelected)
                {
                    result[key] = CopySelectedLevel(childDict, wantedPaths, fullKey);
                    continue;
                }

                if (hasNestedSelections)
                {
                    var child = CopySelectedLevel(childDict, wantedPaths, fullKey);
                    if (child.Count > 0)
                    {
                        result[key] = child;
                    }
                }

                continue;
            }

            if (parentSelected || wantedPaths.Contains(key) || wantedPaths.Contains(fullKey))
            {
                result[key] = value;
            }
        }

        return result;
    }
}
