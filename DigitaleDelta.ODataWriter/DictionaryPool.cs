// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// Dictionary pool
/// </summary>
/// <param name="initialCapacity"></param>
/// <param name="maxPoolSize"></param>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public sealed class DictionaryPool<TKey, TValue>(int initialCapacity = 32, int maxPoolSize = 100) where TKey : notnull
{
    private readonly ConcurrentBag<Dictionary<TKey, TValue>> _pool        = [];
    private          int                                     _count;

    /// <summary>
    /// Retrieves a dictionary from the pool or creates a new one if the pool is empty.
    /// </summary>
    /// <returns></returns>
    public Dictionary<TKey, TValue> Get()
    {
        return _pool.TryTake(out var dict) ? dict : new Dictionary<TKey, TValue>(initialCapacity);
    }

    /// <summary>
    /// Returns a dictionary to the pool if the pool size is below the maximum size.
    /// </summary>
    public bool Return(Dictionary<TKey, TValue>? dict)
    {
        ArgumentNullException.ThrowIfNull(dict);

        if (Interlocked.Increment(ref _count) <= maxPoolSize)
        {
            dict.Clear();
            _pool.Add(dict);
            return true;
        }
        
        Interlocked.Decrement(ref _count);
        
        return false;
    }

    public int Count => _count;
}