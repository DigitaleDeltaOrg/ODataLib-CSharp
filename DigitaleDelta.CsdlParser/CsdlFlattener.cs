using DigitaleDelta.Contracts;

namespace DigitaleDelta.CsdlParser;

/// <summary>
/// The CsdlFlattener class provides methods to flatten a CSDL model into a flat list of properties.
/// </summary>
// ReSharper disable once InconsistentNaming
public static class CsdlFlattener
{
    /// <summary>
    /// Flattens the properties of a specified entity type within a CSDL model into a collection of property paths and their corresponding EDM types.
    /// </summary>
    /// <param name="model">The CSDL model containing the entity and related types to be flattened.</param>
    /// <param name="entityTypeName">The name of the entity type to be processed.</param>
    /// <param name="filterEdmType">An optional EDM type filter to include only properties of a specific type.</param>
    /// <param name="nameComparison">Specifies the type of string comparison to apply when matching entity type names.</param>
    /// <returns>A collection of tuples where each tuple consists of a flattened property path and its associated EDM type.</returns>
    public static IEnumerable<(string Path, string EdmType)> FlattenEntityProperties(CsdlModel model, string entityTypeName, string? filterEdmType = null, StringComparison nameComparison = StringComparison.OrdinalIgnoreCase)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(entityTypeName);

        var entity = ResolveEntityType(model, entityTypeName, nameComparison);
        
        if (entity == null)
        {
            yield break;
        }

        foreach (var item in WalkEntity(model, entity, prefix: null, filterEdmType, nameComparison))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Recursively iterates through the properties of an entity, producing a flattened list of paths and their associated EDM types.
    /// </summary>
    /// <param name="model">The CSDL model containing the entity and related types.</param>
    /// <param name="entity">The entity type whose properties are to be traversed.</param>
    /// <param name="prefix">The parent path prefix to maintain the hierarchical structure.</param>
    /// <param name="filterEdmType">An optional filter to include only properties of a specific EDM type.</param>
    /// <param name="cmp">The string comparison option for type name matching.</param>
    /// <returns>A collection of tuples containing property paths and their corresponding EDM types.</returns>
    private static IEnumerable<(string Path, string EdmType)> WalkEntity(CsdlModel model, EntityType entity, string? prefix, string? filterEdmType, StringComparison cmp)
    {
        return entity.Properties.SelectMany(prop => WalkProperty(model, prop, prefix, filterEdmType, cmp));
    }

    /// <summary>
    /// Traverses properties of a complex type in the CSDL model, yielding paths and their associated EDM types.
    /// </summary>
    /// <param name="model">The CSDL model containing the complex type definitions.</param>
    /// <param name="complex">The complex type to be traversed.</param>
    /// <param name="prefix">An optional path prefix to prepend to property names.</param>
    /// <param name="filterEdmType">An optional filter to include only properties matching a specific EDM type.</param>
    /// <param name="cmp">Specifies the string comparison rules to use for filtering by EDM type.</param>
    /// <returns>A collection of tuples where each tuple contains a property path and its corresponding EDM type.</returns>
    private static IEnumerable<(string Path, string EdmType)> WalkComplex(CsdlModel model, ComplexType complex, string? prefix, string? filterEdmType, StringComparison cmp)
    {
        return complex.Properties.SelectMany(prop => WalkProperty(model, prop, prefix, filterEdmType, cmp));
    }

    /// <summary>
    /// Traverses the properties of an entity or complex type in a CSDL model and produces a flat list of property paths with their associated EDM types.
    /// </summary>
    /// <param name="model">The CSDL model containing the property definitions.</param>
    /// <param name="prop">The current property to be processed.</param>
    /// <param name="prefix">The path prefix to prepend to the property name.</param>
    /// <param name="filterEdmType">An optional EDM type to filter the properties. Only properties matching this EDM type will be included.</param>
    /// <param name="cmp">The string comparison rule to use for type name matching.</param>
    /// <returns>A collection of tuples where each tuple contains the property path and its corresponding EDM type.</returns>
    private static IEnumerable<(string Path, string EdmType)> WalkProperty(CsdlModel model, Property prop, string? prefix, string? filterEdmType, StringComparison cmp)
    {
        var currentPath = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}/{prop.Name}";
        var typeName = prop.Type.Trim();

        if (string.IsNullOrEmpty(typeName))
        {
            yield break;
        }

        if (IsEdmPrimitive(typeName))
        {
            if (string.IsNullOrEmpty(filterEdmType) || string.Equals(typeName, filterEdmType, cmp))
            {
                yield return (currentPath, typeName);
            }

            yield break;
        }

        var complex = ResolveComplexType(model, typeName, cmp);
        
        if (complex == null)
        {
            yield break;
        }

        foreach (var item in WalkComplex(model, complex, currentPath, filterEdmType, cmp))
        {
            yield return item;
        }
    }

    /// <summary>
    /// Determines whether the provided type name represents an EDM primitive type.
    /// </summary>
    /// <param name="typeName">The type name to evaluate.</param>
    /// <returns>True if the type name represents an EDM primitive type; otherwise, false.</returns>
    private static bool IsEdmPrimitive(string typeName) => typeName.StartsWith("Edm.", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Resolves an entity type from a CSDL model by matching its name.
    /// </summary>
    /// <param name="model">The CSDL model containing the entity types.</param>
    /// <param name="entityTypeName">The fully qualified or short name of the entity type to resolve.</param>
    /// <param name="cmp">The string comparison strategy to use when matching names.</param>
    /// <returns>The resolved <see cref="EntityType"/> if found; otherwise, null.</returns>
    private static EntityType? ResolveEntityType(CsdlModel model, string entityTypeName, StringComparison cmp)
    {
        var shortName = ShortName(entityTypeName);
        
        return model.EntityTypes.FirstOrDefault(et =>
                   string.Equals(et.Name, entityTypeName, cmp) ||
                   string.Equals(et.Name, shortName, cmp));
    }

    /// <summary>
    /// Resolves a complex type from the CSDL model based on the provided type name and comparison method.
    /// </summary>
    /// <param name="model">The CSDL model containing the complex types.</param>
    /// <param name="typeName">The fully qualified or short name of the type to resolve.</param>
    /// <param name="cmp">The string comparison method to use for matching type names.</param>
    /// <returns>A <see cref="ComplexType"/> instance if the type is found; otherwise, null.</returns>
    private static ComplexType? ResolveComplexType(CsdlModel model, string typeName, StringComparison cmp)
    {
        var shortName = ShortName(typeName);
        
        return model.ComplexTypes.FirstOrDefault(ct =>
                   string.Equals(ct.Name, typeName, cmp) ||
                   string.Equals(ct.Name, shortName, cmp));
    }

    /// <summary>
    /// Extracts the short name from a fully qualified name.
    /// </summary>
    /// <param name="qualified">The fully qualified name to extract the short name from.</param>
    /// <returns>The short name extracted from the qualified name.</returns>
    private static string ShortName(string qualified)
    {
        var parts = qualified.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        return parts.Length == 0 ? qualified : parts[^1];
    }
}
