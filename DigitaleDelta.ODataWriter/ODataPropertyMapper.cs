// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DigitaleDelta.ODataWriter;

/// <summary>
/// OData property mapper
/// </summary>
public static class ODataPropertyMapper
{
    private static readonly ConcurrentDictionary<(Type Type, bool ExcludeNulls), Func<object, Dictionary<string, object?>>> Mappers = new();
    private static readonly DictionaryPool<string, object?> DictionaryPool = new(initialCapacity: 32);

    /// <summary>
    /// create a mapper for the given type
    /// </summary>
    /// <param name="type">Type of object</param>
    /// <param name="excludeNulls">Exclude null values?</param>
    /// <returns></returns>
    public static Func<object, Dictionary<string, object?>> CreateMapper(Type type, bool excludeNulls)
    {
        return Mappers.GetOrAdd((type, excludeNulls), key => CreateMapperInternal(key.Type, key.ExcludeNulls));
    }

    /// <summary>
    /// Create a mapper for the given type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="excludeNulls"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Func<object, Dictionary<string, object?>> CreateMapperInternal(Type type, bool excludeNulls)
    {
        var entityParam = Expression.Parameter(typeof(object), "entity");
        var castEntity = Expression.Convert(entityParam, type);
        var dictVar = Expression.Variable(typeof(Dictionary<string, object?>), "dict");
        var expressions = new List<Expression>
        {
            Expression.Assign(dictVar, Expression.Call(Expression.Constant(DictionaryPool), 
                typeof(DictionaryPool<string, object?>).GetMethod("Get") 
                ?? throw new InvalidOperationException(ErrorMessages.getMethodNotFound)))
        };

        foreach (var property in type.GetProperties())
        {
            var propAccess = Expression.Property(castEntity, property);
            var keyExpr = Expression.Constant(char.ToLowerInvariant(property.Name[0]) + property.Name[1..]);
            var addMethod = typeof(Dictionary<string, object?>).GetMethod("Add") ?? throw new InvalidOperationException(ErrorMessages.addMethodNotFound);
            Expression valueExpr = propAccess;
            var callMethod = Expression.Call(dictVar, addMethod, keyExpr, Expression.Convert(valueExpr, typeof(object)));

            if (excludeNulls && property.PropertyType.IsClass)
            {
                var propIsNull = Expression.Equal(propAccess, Expression.Constant(null));
                
                expressions.Add(Expression.IfThen(Expression.Not(propIsNull), callMethod));
            }
            else
            {
                expressions.Add(callMethod);
            }
        }

        expressions.Add(dictVar);
        
        var block = Expression.Block([dictVar], expressions);
        var lambda = Expression.Lambda<Func<object, Dictionary<string, object?>>>(block, entityParam);

        return lambda.Compile();
    }
}