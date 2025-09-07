// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace DigitaleDelta.Contracts;

/// <summary>
/// EDM types
/// </summary>
public enum EdmType
{
    /// <summary>
    /// String
    /// </summary>
    EdmString,
    /// <summary>
    /// 32-bits integers
    /// </summary>
    EdmInt32,
    /// <summary>
    /// Boolean
    /// </summary>
    EdmBoolean,
    /// <summary>
    /// Date-related
    /// </summary>
    EdmDateTimeOffset,
    /// <summary>
    /// Double
    /// </summary>
    EdmDouble,
    /// <summary>
    /// Guids (uuids)
    /// </summary>
    EdmGuid,
    /// <summary>
    /// Binary data
    /// </summary>
    EdmBinary,
    /// <summary>
    /// Generic geography
    /// </summary>
    EdmGeography,
    /// <summary>
    /// Geography point
    /// </summary>
    EdmGeographyPoint,
    /// <summary>
    /// Geography line
    /// </summary>
    EdmGeographyLineString,
    /// <summary>
    /// Geography polygon
    /// </summary>
    EdmGeographyPolygon,
    /// <summary>
    /// Geography multipoint
    /// </summary>
    EdmGeographyMultiPoint,
    /// <summary>
    /// Geography multiline
    /// </summary>
    EdmGeographyMultiLineString,
    /// <summary>
    /// Geography multipolygon
    /// </summary>
    EdmGeographyMultiPolygon,
    /// <summary>
    /// Generic geometry
    /// </summary>
    EdmGeometry,
    /// <summary>
    /// Geometry point
    /// </summary>
    EdmGeometryPoint,
    /// <summary>
    /// Geometry line
    /// </summary>
    EdmGeometryLineString,
    /// <summary>
    /// Geometry polygon
    /// </summary>
    EdmGeometryPolygon,
    /// <summary>
    /// Geometry multipoint
    /// </summary>
    EdmGeometryMultiPoint,
    /// <summary>
    /// Geometry multiline
    /// </summary>
    EdmGeometryMultiLineString,
    /// <summary>
    /// Geometry multipolygon
    /// </summary>
    EdmGeometryMultiPolygon,
    /// <summary>
    /// Unknown (fallback)
    /// </summary>
    EdmUnknown
}