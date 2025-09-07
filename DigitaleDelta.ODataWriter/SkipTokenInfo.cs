// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// Provide skip token information for OData responses and functions that handle skip tokens, i.e. extraction and validation.
/// </summary>
public class SkipTokenInfo
{
    /// <summary>
    /// OData order by clause used to sort the entities.
    /// </summary>
    public string? OrderByClause { get; init; }
    /// <summary>
    /// Last ID used in the skip token, typically the last entity ID from the previous page of results.
    /// </summary>
    public string LastId { get; init; } = string.Empty;
    /// <summary>
    /// OData filter clause used to filter the entities. This is used to ensure that the skip token is valid for the current query.
    /// </summary>
    public string? FilterClause { get; init; }
    /// <summary>
    /// Timestamp when the skip token was created. This is used to determine if the skip token is still valid.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    
    /// <summary>
    /// Creates a base64 encoded skip token from the provided skip token info.
    /// </summary>
    /// <param name="skipInfo"></param>
    /// <returns></returns>
    public static string? CreateSkipToken(SkipTokenInfo? skipInfo)
    {
        if (skipInfo == null)
        {
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(skipInfo.LastId))
        {
            return null;
        }
        
        var json = JsonSerializer.Serialize(skipInfo, Shared.JsonOptions);
        
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    /// <summary>
    /// Retrieves the skip token info from a base64 encoded string.
    /// </summary>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public static SkipTokenInfo? GetSkipTokenInfoFromToken(string? skipToken)
    {
        if (skipToken == null)
        {
            return null;
        }
        
        try
        {
            var json = Convert.FromBase64String(skipToken);
            var skipTokenInfo = JsonSerializer.Deserialize<SkipTokenInfo>(json, Shared.JsonOptions);

            if (skipTokenInfo == null || string.IsNullOrWhiteSpace(skipTokenInfo.LastId))
            {
                return null;
            }
            
            return skipTokenInfo;
        }
        catch (Exception)
        {
            return null;
        }
    }
}