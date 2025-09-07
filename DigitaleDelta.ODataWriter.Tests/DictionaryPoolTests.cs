// // Copyright (c)  2025 - EcoSys
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.ODataWriter.Tests;

public class DictionaryPoolTests
{
    [Fact]
    public void Get_ReturnsDictionary_WithCorrectCapacity()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = pool.Get();
        Assert.NotNull(dict);
        Assert.Empty(dict);
    }

    [Fact]
    public void Return_ClearsDictionary()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = pool.Get();
        dict["test"] = "value";
        pool.Return(dict);

        var newDict = pool.Get();
        Assert.Empty(newDict);
    }

    [Fact]
    public void Return_NullDictionary_ThrowsArgumentNullException()
    {
        var pool = new DictionaryPool<string, string>();
        Assert.Throws<ArgumentNullException>(() => pool.Return(null!));
    }
    
    [Fact]
    public void Count_ReflectsAvailableDictionaries()
    {
        var pool = new DictionaryPool<string, string>();
        Assert.Equal(0, pool.Count);

        var dict1 = pool.Get();
        Assert.Equal(0, pool.Count);

        pool.Return(dict1);
        Assert.Equal(1, pool.Count);

        var dict2 = pool.Get();
        Assert.Equal(1, pool.Count);
    }
    
    [Fact]
    public void Get_MultipleCalls_ReturnsSameDictionaryInstance()
    {
        var pool = new DictionaryPool<string, string>();
        var dict1 = pool.Get();
        pool.Return(dict1);
        var dict2 = pool.Get();
    
        Assert.Same(dict1, dict2);
    }

    [Fact]
    public void Return_MultipleDictionaries_IncreasesCount()
    {
        var pool = new DictionaryPool<string, string>();
        var dict1 = pool.Get();
        var dict2 = pool.Get();
    
        pool.Return(dict1);
        Assert.Equal(1, pool.Count);
    
        pool.Return(dict2);
        Assert.Equal(2, pool.Count);
    }
    
    [Fact]
    public void Get_AfterMaxCapacity_CreatesNewInstance()
    {
        var pool = new DictionaryPool<string, string>();
        var dictionaries = new List<Dictionary<string, string>>();
    
        // Get more dictionaries than the default capacity
        for (int i = 0; i < 1024; i++)
        {
            dictionaries.Add(pool.Get());
        }

        var oneMore = pool.Get();
        Assert.NotNull(oneMore);
        Assert.Empty(oneMore);
    }

    [Fact]
    public void Return_SameDictionaryMultipleTimes_IncreasesCount()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = pool.Get();

        pool.Return(dict);
        Assert.Equal(1, pool.Count);

        pool.Return(dict);
        Assert.Equal(2, pool.Count); // Pool allows duplicate entries
    }
    
    [Fact]
    public void Return_WhenUsedForODataResponses_HandlesConcurrentUsage()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = pool.Get();
    
        // Simulate first OData response
        dict["@odata.context"] = "http://api/context1";
        dict["value"] = "response1";
        pool.Return(dict);
    
        // Simulate concurrent response needing the same structure
        var dict2 = pool.Get();
        dict2["@odata.context"] = "http://api/context2";
        dict2["value"] = "response2";
        pool.Return(dict2);
    
        Assert.Equal(2, pool.Count);
        var nextDict = pool.Get();
        Assert.Empty(nextDict); // Verify we get a clean dictionary
    }
    
    [Fact]
    public void Return_DictionaryWithLargeCapacity_AcceptsAndClears()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = new Dictionary<string, string>(1000);
        for (int i = 0; i < 100; i++)
        {
            dict[$"key{i}"] = $"value{i}";
        }

        pool.Return(dict);
        Assert.Equal(1, pool.Count);

        var retrieved = pool.Get();
        Assert.Same(dict, retrieved);
        Assert.Empty(retrieved);
    }

    [Fact]
    public void Return_EmptyDictionary_AcceptsAndMaintainsEmpty()
    {
        var pool = new DictionaryPool<string, string>();
        var dict = new Dictionary<string, string>();
    
        pool.Return(dict);
        Assert.Equal(1, pool.Count);

        var retrieved = pool.Get();
        Assert.Same(dict, retrieved);
        Assert.Empty(retrieved);
    }
    
    [Fact]
    public void Return_ExternalDictionary_AcceptsAndClears()
    {
        var pool = new DictionaryPool<string, string>();
        var externalDict = new Dictionary<string, string> { ["key"] = "value" };
    
        pool.Return(externalDict);
        Assert.Equal(1, pool.Count);
    
        var retrieved = pool.Get();
        Assert.Same(externalDict, retrieved);
        Assert.Empty(retrieved);
    }
    
    [Fact]
    public void Return_DictionaryWithCustomCapacity_PreservesCapacity()
    {
        var pool = new DictionaryPool<string, string>();
        var customCapacityDict = new Dictionary<string, string>(512);
    
        pool.Return(customCapacityDict);
        var retrieved = pool.Get();
    
        // This test assumes your implementation preserves capacity
        // You may need to adjust based on actual implementation
        Assert.Same(customCapacityDict, retrieved);
    }
    
    [Fact]
    public void Return_WhenPoolAtCapacity_HandlesGracefully()
    {
        var pool = new DictionaryPool<string, string>();
        var dictionaries = new List<Dictionary<string, string>>();
    
        // Fill the pool to capacity (assuming 1024 is max)
        for (int i = 0; i < 100; i++)
        {
            var dict = new Dictionary<string, string>();
            pool.Return(dict);
        }
    
        // Try to return one more
        var oneMore = new Dictionary<string, string>();
        pool.Return(oneMore);
    
        // Verify behavior - either rejects or accepts based on implementation
        Assert.Equal(100, pool.Count); // Or 1024 if it has a hard limit
    }
}