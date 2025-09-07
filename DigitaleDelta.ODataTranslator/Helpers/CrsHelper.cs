using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace DigitaleDelta.ODataTranslator.Helpers;

/// <summary>
/// Utility class for handling Coordinate Reference Systems (CRS) operations,
/// including validation and transformation of geometries between different CRS definitions.
/// </summary>
public static class CrsHelper
{
	private static readonly CoordinateSystemFactory         CsFactory = new();
	private static readonly CoordinateTransformationFactory CtFactory = new();
	private static readonly Dictionary<int, string> SupportedCrs = new()
	{
		{ 4326, """
		        GEOGCS["WGS 84",
		            DATUM["WGS_1984",
		                SPHEROID["WGS 84",6378137,298.257223563,
		                    AUTHORITY["EPSG","7030"]],
		                AUTHORITY["EPSG","6326"]],
		            PRIMEM["Greenwich",0,
		                AUTHORITY["EPSG","8901"]],
		            UNIT["degree",0.0174532925199433,
		                AUTHORITY["EPSG","9122"]],
		            AUTHORITY["EPSG","4326"]]
		        """ },
		{ 4258, """
		        GEOGCS["ETRS89",
		            DATUM["European_Terrestrial_Reference_System_1989",
		                SPHEROID["GRS 1980",6378137,298.257222101,
		                    AUTHORITY["EPSG","7019"]],
		                AUTHORITY["EPSG","6258"]],
		            PRIMEM["Greenwich",0,
		                AUTHORITY["EPSG","8901"]],
		            UNIT["degree",0.0174532925199433,
		                AUTHORITY["EPSG","9122"]],
		            AUTHORITY["EPSG","4258"]]
		        """ },
		{ 28992, """
		         PROJCS["Amersfoort / RD New",
		             GEOGCS["Amersfoort",
		                 DATUM["Amersfoort",
		                     SPHEROID["Bessel 1841",6377397.155,299.1528128],
		                     TOWGS84[565.4171,50.3319,465.5524,1.9342,-1.6677,9.1019,4.0725]],
		                 PRIMEM["Greenwich",0,
		                     AUTHORITY["EPSG","8901"]],
		                 UNIT["degree",0.0174532925199433,
		                     AUTHORITY["EPSG","9122"]],
		                 AUTHORITY["EPSG","4289"]],
		             PROJECTION["Oblique_Stereographic"],
		             PARAMETER["latitude_of_origin",52.1561605555556],
		             PARAMETER["central_meridian",5.38763888888889],
		             PARAMETER["scale_factor",0.9999079],
		             PARAMETER["false_easting",155000],
		             PARAMETER["false_northing",463000],
		             UNIT["metre",1,
		                 AUTHORITY["EPSG","9001"]],
		             AXIS["Easting",EAST],
		             AXIS["Northing",NORTH],
		             AUTHORITY["EPSG","28992"]]
		         """ },
		{ 3857, """
		        PROJCS["WGS 84 / Pseudo-Mercator",
		            GEOGCS["WGS 84",
		                DATUM["WGS_1984",
		                    SPHEROID["WGS 84",6378137,298.257223563,
		                        AUTHORITY["EPSG","7030"]],
		                    AUTHORITY["EPSG","6326"]],
		                PRIMEM["Greenwich",0,
		                    AUTHORITY["EPSG","8901"]],
		                UNIT["degree",0.0174532925199433,
		                    AUTHORITY["EPSG","9122"]],
		                AUTHORITY["EPSG","4326"]],
		            PROJECTION["Mercator_1SP"],
		            PARAMETER["central_meridian",0],
		            PARAMETER["scale_factor",1],
		            PARAMETER["false_easting",0],
		            PARAMETER["false_northing",0],
		            UNIT["metre",1,
		                AUTHORITY["EPSG","9001"]],
		            AXIS["Easting",EAST],
		            AXIS["Northing",NORTH],
		            EXTENSION["PROJ4","+proj=merc +a=6378137 +b=6378137 +lat_ts=0 +lon_0=0 +x_0=0 +y_0=0 +k=1 +units=m +nadgrids=@null +wktext +no_defs"],
		            AUTHORITY["EPSG","3857"]]
		        """ },
        { 25831, """
                 PROJCS["ETRS89 / UTM zone 31N",
                 GEOGCS["ETRS89",
                     DATUM["European_Terrestrial_Reference_System_1989",
                         SPHEROID["GRS 1980",6378137,298.257222101,
                             AUTHORITY["EPSG","7019"]],
                         AUTHORITY["EPSG","6258"]],
                     PRIMEM["Greenwich",0,
                         AUTHORITY["EPSG","8901"]],
                     UNIT["degree",0.0174532925199433,
                         AUTHORITY["EPSG","9122"]],
                     AUTHORITY["EPSG","4258"]],
                 PROJECTION["Transverse_Mercator"],
                 PARAMETER["latitude_of_origin",0],
                 PARAMETER["central_meridian",3],
                 PARAMETER["scale_factor",0.9996],
                 PARAMETER["false_easting",500000],
                 PARAMETER["false_northing",0],
                 UNIT["metre",1,
                     AUTHORITY["EPSG","9001"]],
                 AXIS["Easting",EAST],
                 AXIS["Northing",NORTH],
                 AUTHORITY["EPSG","25831"]]
                 """
            },
            { 25832,
                """
                PROJCS["ETRS89 / UTM zone 32N",
                GEOGCS["ETRS89",
                    DATUM["European_Terrestrial_Reference_System_1989",
                        SPHEROID["GRS 1980",6378137,298.257222101,
                            AUTHORITY["EPSG","7019"]],
                        AUTHORITY["EPSG","6258"]],
                    PRIMEM["Greenwich",0,
                        AUTHORITY["EPSG","8901"]],
                    UNIT["degree",0.0174532925199433,
                        AUTHORITY["EPSG","9122"]],
                    AUTHORITY["EPSG","4258"]],
                PROJECTION["Transverse_Mercator"],
                PARAMETER["latitude_of_origin",0],
                PARAMETER["central_meridian",9],
                PARAMETER["scale_factor",0.9996],
                PARAMETER["false_easting",500000],
                PARAMETER["false_northing",0],
                UNIT["metre",1,
                    AUTHORITY["EPSG","9001"]],
                AXIS["Easting",EAST],
                AXIS["Northing",NORTH],
                AUTHORITY["EPSG","25832"]]
                """
        }
	};

	/// <summary>
	/// Transform geometry
	/// </summary>
	/// <param name="toId">Target SRID</param>
	/// <param name="geometry">Geometry to transform</param>
	/// <returns></returns>
	public static (bool result, Geometry? transformedGeometry) TransformGeometry(int toId, Geometry geometry)
	{
		if (!SupportedCrs.TryGetValue(toId, out var targetWkt))
		{
			return (false, null);
		}

		var fromCoordinateSystem = CsFactory.CreateFromWkt(SupportedCrs[4258]);
		var toCoordinateSystem   = CsFactory.CreateFromWkt(targetWkt);
		var transformation       = CtFactory.CreateFromCoordinateSystems(fromCoordinateSystem, toCoordinateSystem);
		var transformedGeometry  = TransformGeometry(geometry, transformation.MathTransform);

		return (true, transformedGeometry);
	}

	/// <summary>
	/// Transforms geometry
	/// </summary>
	/// <param name="fromId">Source SRID</param>
	/// <param name="toId">TargetSRId</param>
	/// <param name="geometry">Geometry to convert</param>
	/// <returns></returns>
	public static (bool result, Geometry? transformedGeometry) TransformGeometry(int fromId, int toId, Geometry geometry)
	{
		if (!SupportedCrs.TryGetValue(toId, out var targetWkt))
		{
			return (false, null);
		}

		var fromCoordinateSystem = CsFactory.CreateFromWkt(SupportedCrs[fromId]);
		var toCoordinateSystem   = CsFactory.CreateFromWkt(targetWkt);
		var transformation       = CtFactory.CreateFromCoordinateSystems(fromCoordinateSystem, toCoordinateSystem);
		var transformedGeometry  = TransformGeometry(geometry, transformation.MathTransform);

		return (true, transformedGeometry);
	}
	
	private static Geometry TransformGeometry(Geometry geometry, MathTransform transform)
	{
		var transformedCoordinates = geometry.Coordinates.Select(coordinate =>
		                                                         {
			                                                         var transformedCoordinate = transform.Transform(new[] { coordinate.X, coordinate.Y });
			                                                         return new Coordinate(transformedCoordinate[0], transformedCoordinate[1]);
		                                                         }).ToArray();

		return geometry.Factory.CreateGeometry(geometry.CopyWithNewCoordinates(transformedCoordinates, transform));
	}

	private static Geometry CopyWithNewCoordinates(this Geometry geometry, Coordinate[] coordinates, MathTransform transform)
	{
		var geomType = geometry.GetType();

		if (geomType == typeof(Point))
		{
			return new Point(coordinates[0]);
		}

		if (geomType == typeof(LineString))
		{
			return new LineString(coordinates);
		}

		if (geomType == typeof(Polygon))
		{
			var linearRing = new LinearRing(coordinates);
			return new Polygon(linearRing);
		}

		if (geomType == typeof(MultiPolygon))
		{
			var polygons = new Polygon[geometry.NumGeometries];
			for (var i = 0; i < geometry.NumGeometries; i++)
			{
				var polygon = (Polygon)geometry.GetGeometryN(i);
				polygons[i] = (Polygon)polygon.CopyWithNewCoordinates(polygon.Coordinates.Select(coord => 
				                                                                                 {
					                                                                                 var transformedCoordinate = transform.Transform(new[] { coord.X, coord.Y });
					                                                                                 return new Coordinate(transformedCoordinate[0], transformedCoordinate[1]);
				                                                                                 }).ToArray(), transform);
			}
			return new MultiPolygon(polygons);
		}

		throw new NotSupportedException("Geometry type not supported");
	}

	/// <summary>
	/// Validate content CRS
	/// </summary>
	/// <param name="contentCrs">CRS to validate</param>
	/// <returns></returns>
	public static int? ValidateContentCrs(string? contentCrs)
	{
		if (string.IsNullOrEmpty(contentCrs))
		{
			return 4258;
		}

		if (contentCrs == "CRS84")
		{
			return 4326;
		}

		var validNames = SupportedCrs.Select(a => $"EPSG:{a.Key}");
		if (!validNames.Contains(contentCrs))
		{
			return null;
		}

		if (!int.TryParse(contentCrs.Replace("EPSG:", ""), out var id))
		{
			return null;
		}

		return SupportedCrs.ContainsKey(id) ? id : null;
	}
}