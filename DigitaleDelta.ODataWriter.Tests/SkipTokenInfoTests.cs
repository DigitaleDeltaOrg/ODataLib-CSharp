// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace DigitaleDelta.ODataWriter.Tests;

public class SkipTokenInfoTests
{
    [Fact]
    public void CreateSkipToken_WithValidInfo_ReturnsEncodedToken()
    {
        // Arrange
        var skipInfo = new SkipTokenInfo
        {
            FilterClause = "TestFilter",
            OrderByClause = "TestOrderBy",
            LastId = "123456",
            Timestamp = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var token = SkipTokenInfo.CreateSkipToken(skipInfo);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void GetSkipTokenInfoFromToken_WithValidToken_ReturnsCorrectInfo()
    {
        // Arrange
        var originalInfo = new SkipTokenInfo
        {
            FilterClause = "TestFilter",
            OrderByClause = "TestOrderBy",
            LastId = "123456",
            Timestamp = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        };
        var token = SkipTokenInfo.CreateSkipToken(originalInfo);

        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(token);

        // Assert
        Assert.NotNull(retrievedInfo);
        Assert.Equal(originalInfo.FilterClause, retrievedInfo.FilterClause);
        Assert.Equal(originalInfo.OrderByClause, retrievedInfo.OrderByClause);
        Assert.Equal(originalInfo.LastId, retrievedInfo.LastId);
        Assert.Equal(originalInfo.Timestamp, retrievedInfo.Timestamp);
    }

    [Fact]
    public void GetSkipTokenInfoFromToken_WithInvalidToken_ReturnsNull()
    {
        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken("invalid-token");

        // Assert
        Assert.Null(retrievedInfo);
    }

    [Fact]
    public void CreateSkipToken_WithNullInfo_ReturnsNull()
    {
        // Act
        var token = SkipTokenInfo.CreateSkipToken(null);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void CreateSkipToken_WithMissingLastId_ReturnsNull()
    {
        // Arrange
        var skipInfo = new SkipTokenInfo
        {
            FilterClause = "TestFilter",
            OrderByClause = "TestOrderBy",
            LastId = null
        };

        // Act
        var token = SkipTokenInfo.CreateSkipToken(skipInfo);

        // Assert
        Assert.Null(token);
    }

    [Fact]
    public void RoundTrip_PreservesAllValues()
    {
        // Arrange
        var originalInfo = new SkipTokenInfo
        {
            FilterClause = "name eq 'test'",
            OrderByClause = "id desc",
            LastId = Guid.NewGuid().ToString(),
            Timestamp = DateTime.UtcNow
        };

        // Act
        var token = SkipTokenInfo.CreateSkipToken(originalInfo);
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(token);

        // Assert
        Assert.NotNull(retrievedInfo);
        Assert.Equal(retrievedInfo.FilterClause, retrievedInfo.FilterClause);
        Assert.Equal(originalInfo.OrderByClause, retrievedInfo.OrderByClause);
        Assert.Equal(originalInfo.LastId, retrievedInfo.LastId);
        Assert.Equal(originalInfo.Timestamp.ToUniversalTime(), retrievedInfo.Timestamp.ToUniversalTime());
        Assert.Equal(originalInfo.FilterClause, retrievedInfo.FilterClause);
        Assert.Equal(originalInfo.OrderByClause, retrievedInfo.OrderByClause);
        Assert.Equal(originalInfo.LastId, retrievedInfo.LastId);
        Assert.Equal(originalInfo.Timestamp, retrievedInfo.Timestamp);
    }

    [Fact]
    public void SkipTokenInfo_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var originalInfo = new SkipTokenInfo
        {
            FilterClause = "contains(description, '/'))",
            OrderByClause = "name eq 'O''Brien'",
            LastId = "key/with/slashes",
            Timestamp = DateTime.UtcNow
        };

        // Act
        var token = SkipTokenInfo.CreateSkipToken(originalInfo);
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(token);

        // Assert
        Assert.NotNull(retrievedInfo);
        Assert.Equal(originalInfo.FilterClause, retrievedInfo.FilterClause);
        Assert.Equal(originalInfo.OrderByClause, retrievedInfo.OrderByClause);
        Assert.Equal(originalInfo.LastId, retrievedInfo.LastId);
    }
    
    [Fact]
    public void SkipTokenInfo_WithEmptyFilterClause_ReturnsValidInfo()
    {
        // Arrange
        var skipInfo = new SkipTokenInfo
        {
            FilterClause = string.Empty,
            LastId = Guid.NewGuid().ToString()
        };
        var encodedToken = SkipTokenInfo.CreateSkipToken(skipInfo);

        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(encodedToken);

        // Assert
        Assert.NotNull(retrievedInfo);
        Assert.Equal(skipInfo.FilterClause, retrievedInfo?.FilterClause);
        Assert.Equal(skipInfo.LastId, retrievedInfo?.LastId);
    }
    
    [Fact]
    public void GetSkipTokenInfoFromToken_WithNullToken_ReturnsNull()
    {
        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(null);

        // Assert
        Assert.Null(retrievedInfo);
    }

    [Fact]
    public void GetSkipTokenInfoFromToken_WithEmptyToken_ReturnsNull()
    {
        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(string.Empty);

        // Assert
        Assert.Null(retrievedInfo);
    }

    [Fact]
    public void GetSkipTokenInfoFromToken_WithWhitespaceToken_ReturnsNull()
    {
        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken("   ");

        // Assert
        Assert.Null(retrievedInfo);
    }

    [Fact]
    public void GetSkipTokenInfoFromToken_WithMalformedButValidBase64_ReturnsNull()
    {
        // Arrange - create a valid Base64 string that doesn't contain valid token data
        var invalidBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"not\":\"valid\"}"));
    
        // Act
        var retrievedInfo = SkipTokenInfo.GetSkipTokenInfoFromToken(invalidBase64);

        // Assert
        Assert.Null(retrievedInfo);
    }
}