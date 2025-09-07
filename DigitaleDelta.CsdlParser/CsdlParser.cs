// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Xml.Linq;
using DigitaleDelta.Contracts;

namespace DigitaleDelta.CsdlParser;

/// <summary>
/// The CsdlParser class provides methods to parse Common Schema Definition Language (CSDL) strings
/// into strongly-typed representations, facilitating schema validation and EDM type mapping.
/// </summary>
public static class CsdlParser
{
    
    /// <summary>
    /// Parses a CSDL (Common Schema Definition Language) string into a CsdlModel.
    /// </summary>
    /// <param name="csdl"></param>
    /// <param name="model"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static bool TryParse(string csdl, out CsdlModel? model, out string? error)
    {
        model = new CsdlModel();

        try
        {
            var xdoc = XDocument.Parse(csdl);
            XNamespace edmxNs = "http://docs.oasis-open.org/odata/ns/edmx";
            XNamespace edmNs = "http://docs.oasis-open.org/odata/ns/edm";

            var schemas = xdoc.Root?.Element(edmxNs + "DataServices")?.Elements(edmNs + "Schema");

            if (schemas == null)
            {
                error = ErrorMessages.invalidCsdlFormat;
                model = null;

                return false;
            }

            foreach (var schema in schemas)
            {
                var entityContainerElement = schema.Element(edmNs + "EntityContainer");
                
                if (entityContainerElement != null)
                {
                    var name = entityContainerElement.Attribute("Name")?.Value;
                    
                    if (string.IsNullOrEmpty(name))
                    {
                        error = ErrorMessages.entityContainerNameIsRequired;
                        model = null;

                        return false;
                    }
                    
                    var entityContainer = new EntityContainer { Name = name };

                    foreach (var entitySetElement in entityContainerElement.Elements(edmNs + "EntitySet"))
                    {
                        var entitySetName = entitySetElement.Attribute("Name")?.Value;
                        var entityType = entitySetElement.Attribute("EntityType")?.Value;
                        
                        if (string.IsNullOrEmpty(entitySetName))
                        {
                            error =  ErrorMessages.entitySetNameIsRequired;
                            model = null;

                            return false;
                        }
                        
                        if (string.IsNullOrEmpty(entityType))
                        {
                            error = ErrorMessages.entitySetEntityTypeIsRequired;
                            model = null;

                            return false;
                        }
                        
                        entityContainer.EntitySets.Add(new EntitySet
                        {
                            Name = entitySetName,
                            EntityType = entityType
                        });
                    }

                    model.EntityContainers.Add(entityContainer);
                }

                // Parse EntityTypes
                foreach (var entityTypeElement in schema.Elements(edmNs + "EntityType"))
                {
                    var name = entityTypeElement.Attribute("Name")?.Value;
                    
                    if (string.IsNullOrEmpty(name))
                    {
                       error = ErrorMessages.entityTypeNameIsRequired;
                       model = null;

                       return false;
                    }
                    
                    var entityType = new EntityType { Name = name };
                    var keyElement = entityTypeElement.Element(edmNs + "Key");
                    
                    if (keyElement != null)
                    {
                        foreach (var propertyRef in keyElement.Elements(edmNs + "PropertyRef"))
                        {
                            var value = propertyRef.Attribute("Name")?.Value;
                            
                            if (!string.IsNullOrEmpty(value))
                            {
                                entityType.Keys.Add(value);
                            }
                        }
                    }

                    foreach (var propertyElement in entityTypeElement.Elements(edmNs + "Property"))
                    {
                        var rawType = propertyElement.Attribute("Type")?.Value;
                        
                        if (string.IsNullOrEmpty(rawType))
                        {
                           error = string.Format(ErrorMessages.propertyTypeIsRequiredForEntityType, name);
                           model = null;

                           return false;
                        }
                        
                        var rawName = propertyElement.Attribute("Name")?.Value;
                        
                        if (string.IsNullOrEmpty(rawName))
                        {
                            error = string.Format(ErrorMessages.propertyNameIsRequiredForEntityType, name);
                            model = null;

                            return false;
                        }
                        
                        entityType.Properties.Add(new Property
                        {
                            Name = propertyElement.Attribute("Name")?.Value ?? string.Empty,
                            Type = rawType,
                            Nullable = propertyElement.Attribute("Nullable")?.Value != "false",
                            EdmType = MapEdmType(rawType),
                            DefaultValue = propertyElement.Attribute("DefaultValue")?.Value,
                            MaxLength = int.TryParse(propertyElement.Attribute("MaxLength")?.Value, out var maxLength) ? maxLength : null,
                            Precision = int.TryParse(propertyElement.Attribute("Precision")?.Value, out var precision) ? precision : null,
                            Scale = int.TryParse(propertyElement.Attribute("Scale")?.Value, out var scale) ? scale : null
                        });
                    }

                    model.EntityTypes.Add(entityType);
                }

                // Parse ComplexTypes
                foreach (var complexTypeElement in schema.Elements(edmNs + "ComplexType"))
                {
                    var name = complexTypeElement.Attribute("Name")?.Value;
                    
                    if (string.IsNullOrEmpty(name))
                    {
                       error = ErrorMessages.complexTypeNameIsRequired;
                       model = null;

                       return false;
                    }
                    
                    var complexType = new ComplexType { Name = name };

                    foreach (var propertyElement in complexTypeElement.Elements(edmNs + "Property"))
                    {
                        var propertyName = propertyElement.Attribute("Name")?.Value;
                        
                        if (string.IsNullOrEmpty(propertyName))
                        {
                           error = string.Format(ErrorMessages.propertyNameIsRequiredForComplexType, name);
                           model = null;

                           return false;
                        }
                        
                        var propertyType = propertyElement.Attribute("Type")?.Value;
                        
                        if (string.IsNullOrEmpty(propertyType))
                        {
                           error = string.Format(ErrorMessages.propertyTypeIsRequiredForPropertyInComplexType, propertyName, name);
                           model = null;

                           return false;
                        }

                        complexType.Properties.Add(new Property
                        {
                            Name = propertyName,
                            Type = propertyType,
                            Nullable = propertyElement.Attribute("Nullable")?.Value != "false",
                            EdmType = MapEdmType(propertyType),
                            DefaultValue = propertyElement.Attribute("DefaultValue")?.Value,
                            MaxLength = int.TryParse(propertyElement.Attribute("MaxLength")?.Value, out var maxLength) ? maxLength : null,
                            Precision = int.TryParse(propertyElement.Attribute("Precision")?.Value, out var precision) ? precision : null,
                            Scale = int.TryParse(propertyElement.Attribute("Scale")?.Value, out var scale) ? scale : null
                        });
                    }

                    model.ComplexTypes.Add(complexType);
                }

                // Parse Functions
                foreach (var functionElement in schema.Elements(edmNs + "Function"))
                {
                    var functionName = functionElement.Attribute("Name")?.Value;
                    var returnType = functionElement.Element(edmNs + "ReturnType")?.Attribute("Type")?.Value;
                    
                    if (string.IsNullOrEmpty(functionName))
                    {
                       error = ErrorMessages.functionNameIsRequired;
                       model = null;

                       return false;
                    }
                    
                    if (string.IsNullOrEmpty(returnType))
                    {
                       error = string.Format(ErrorMessages.functionReturnTypeIsRequired, functionName);
                       model = null;

                       return false;
                    }
                    
                    var function = new Function
                    {
                        Name = functionName,
                        ReturnType = returnType,
                        Parameters = []
                    };

                    foreach (var parameterElement in functionElement.Elements(edmNs + "Parameter").Select((el, idx) => new { el, idx }))
                    {
                        var parameterName = parameterElement.el.Attribute("Name")?.Value;
                        var parameterType = parameterElement.el.Attribute("Type")?.Value;
                        
                        if (string.IsNullOrEmpty(parameterName))
                        {
                            parameterName = $"param{parameterElement.idx + 1}";
                        }
                        
                        if (string.IsNullOrEmpty(parameterType))
                        {
                           error = string.Format(ErrorMessages.parameterTypeIsRequiredForParameterInFunction, parameterName, functionName);
                           model = null;

                           return false;
                        }
                        
                        function.Parameters.Add(new Parameter
                        {
                            Name = parameterName,
                            Type = parameterType
                        });
                    }

                    model.Functions.Add(function);
                }
            }
        }
        catch (Exception ex)
        {
           error = string.Format(ErrorMessages.parsingError, ex.Message);
           model = null;

           return false;
        }

        error = null;

        return true;
    }
    
    /// <summary>
    /// Map Edm tyoe string to EdmType enum.
    /// </summary>
    /// <param name="rawType"></param>
    /// <returns></returns>
    internal static EdmType MapEdmType(string rawType)
    {
        return rawType switch
               {
                   "Edm.String" => EdmType.EdmString, 
                   "Edm.Int32" => EdmType.EdmInt32, 
                   "Edm.Boolean" => EdmType.EdmBoolean, 
                   "Edm.DateTimeOffset" => EdmType.EdmDateTimeOffset, 
                   "Edm.Double" => EdmType.EdmDouble, 
                   "Edm.Guid" => EdmType.EdmGuid, 
                   "Edm.Binary" => EdmType.EdmBinary, 
                   "Edm.Geography" => EdmType.EdmGeography, 
                   "Edm.GeographyPoint" => EdmType.EdmGeographyPoint, 
                   "Edm.GeographyLineString" => EdmType.EdmGeographyLineString, 
                   "Edm.GeographyPolygon" => EdmType.EdmGeographyPolygon, 
                   "Edm.GeographyMultiPoint" => EdmType.EdmGeographyMultiPoint, 
                   "Edm.GeographyMultiLineString" => EdmType.EdmGeographyMultiLineString, 
                   "Edm.GeographyMultiPolygon" => EdmType.EdmGeographyMultiPolygon, 
                   "Edm.Geometry" => EdmType.EdmGeometry, 
                   "Edm.GeometryPoint" => EdmType.EdmGeometryPoint, 
                   "Edm.GeometryLineString" => EdmType.EdmGeometryLineString, 
                   "Edm.GeometryPolygon" => EdmType.EdmGeometryPolygon, 
                   "Edm.GeometryMultiPoint" => EdmType.EdmGeometryMultiPoint, 
                   "Edm.GeometryMultiLineString" => EdmType.EdmGeometryMultiLineString, 
                   "Edm.GeometryMultiPolygon" => EdmType.EdmGeometryMultiPolygon, 
                   _ => EdmType.EdmUnknown
               };
    }
}