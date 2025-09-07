using DigitaleDelta.ODataTranslator.Helpers;
using DigitaleDelta.ODataTranslator.Models;

namespace DigitaleDelta.ODataTranslator.Tests
{
    public class ODataToSqlConverterTests
    {
        [Fact]
        private static void ODataFunctionMap_AssignedCorrectly()
        {
            // Arrange
            var functionMaps = new List<ODataFunctionMap>
            {
                new()
                {
                    ODataFunctionName = "contains",
                    SqlFunctionFormat = "{0} LIKE {1}",
                    ExpectedArgumentTypes = ["string", "string"],
                    WildCardPosition = WildCardPosition.LeftAndRight,
                    WildCardSymbol = "%",
                    ReturnType = "Edm.Boolean"
                }
            };

            // Assert

            Assert.Equal("Edm.Boolean", functionMaps[0].ReturnType);
            Assert.Equal(WildCardPosition.LeftAndRight, functionMaps[0].WildCardPosition);
            Assert.Equal("%", functionMaps[0].WildCardSymbol);
            Assert.Equal("contains", functionMaps[0].ODataFunctionName);
            Assert.Equal("{0} LIKE {1}", functionMaps[0].SqlFunctionFormat);
            Assert.Equal(2, functionMaps[0].ExpectedArgumentTypes.Count);
        }
        
        [Fact]
        private static void ODataSqlMap_AssignedCorrectly()
        {
            // Arrange
            var map = new ODataToSqlMap { ODataPropertyName = "Name", Query = "ProductName", EdmType = "Edm.String" };

            // Assert

            Assert.Equal("Edm.String", map.EdmType);
            Assert.Equal("Name", map.ODataPropertyName);
            Assert.Equal("ProductName", map.Query);
        }
        
        private static ODataFilterProcessor SetupProcessor()
        {
            CsdlParser.CsdlParser.TryParse(GetValidCsdlContent(), out var model, out _);
            Assert.NotNull(model);
            
            var processor = new ODataFilterProcessor(model, GetFunctionMaps(), GetPropertyMaps());
            
            Assert.NotNull(processor);

            return processor;
        }

        private static string GetValidCsdlContent()
        {
            return """
                   <?xml version="1.0" encoding="utf-8"?>
                   <edmx:Edmx Version="4.0" 
                        xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                        xmlns="http://docs.oasis-open.org/odata/ns/edm">
                     <edmx:DataServices>
                       <Schema Namespace="ODataDemo">
                         <EntityContainer Name="DefaultContainer">
                           <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                           <EntitySet Name="Flubbers" EntityType="ODataDemo.Flubber" />
                         </EntityContainer>
                         <EntityType Name="Flubber">
                           <Key>
                             <PropertyRef Name="ID" />
                           </Key>
                           <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                         </EntityType>
                         <EntityType Name="Product">
                           <Key>
                             <PropertyRef Name="ID" />
                           </Key>
                           <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                           <Property Name="Name" Type="Edm.String" />
                           <Property Name="Description" Type="Edm.String" />
                           <Property Name="Price" Type="Edm.Double" />
                           <Property Name="Address" Type="ODataDemo.Address" />
                         </EntityType>
                         <ComplexType Name="Address">
                           <Property Name="Street" Type="Edm.String" />
                           <Property Name="City" Type="Edm.String" />
                         </ComplexType>
                         <Function Name="GetProducts">
                           <Parameter Name="category" Type="Edm.String" />
                           <ReturnType Type="Collection(ODataDemo.Product)" />
                         </Function>
                       </Schema>
                     </edmx:DataServices>
                   </edmx:Edmx>
                   """;
        }
        
        private static List<ODataFunctionMap> GetFunctionMaps()
        {
            return
            [
                new()
                {
                    ODataFunctionName = "startswith",
                    ExpectedArgumentTypes = ["Edm.String", "Edm.String"],
                    ReturnType = "Edm.String",
                    SqlFunctionFormat = "ILIKE({0}, {1})",
                    WildCardPosition = WildCardPosition.Right,
                    WildCardSymbol = "%"
                },
                new()
                {
                    ODataFunctionName = "endswith",
                    ExpectedArgumentTypes = ["Edm.String", "Edm.String"],
                    ReturnType = "Edm.String",
                    SqlFunctionFormat = "ILIKE({0}, {1})",
                    WildCardPosition = WildCardPosition.Left,
                    WildCardSymbol = "%"
                },
                new()
                {
                    ODataFunctionName = "contains",
                    ExpectedArgumentTypes = ["Edm.String", "Edm.String"],
                    ReturnType = "Edm.String",
                    SqlFunctionFormat = "ILIKE({0}, {1})",
                    WildCardPosition = WildCardPosition.LeftAndRight,
                    WildCardSymbol = "%"
                },
                new()
                {
                    ODataFunctionName = "now",
                    ExpectedArgumentTypes = [],
                    ReturnType = "Edm.DateTimeOffset",
                    SqlFunctionFormat = "NOW()"
                },
                new()
                {
                    ODataFunctionName = "bad",
                    ExpectedArgumentTypes = ["Edm.String", "Edm.String"],
                    ReturnType = "Edm.String",
                    SqlFunctionFormat = "ILIKE({0}, {1})",
                    WildCardSymbol = "%"
                },
            ];
        }

        private static List<ODataToSqlMap> GetPropertyMaps()
        {
            return
            [
                new()
                {
                    ODataPropertyName = "Name",
                    Query = "name",
                    EdmType = "Edm.String"
                },
                new()
                {
                    ODataPropertyName = "Price",
                    Query = "price",
                    EdmType = "Edm.Double"
                },
                new()
                {
                    ODataPropertyName = "ResultOf",
                    Query = "result",
                    EdmType = "Edm.String"
                },
                new()
                {
                    ODataPropertyName = "PhenomenonTime/BeginPosition",
                    Query = "phenomenon_time_start",
                    EdmType = "Edm.DateTimeOffset"
                },
                new()
                {
                    ODataPropertyName = "Address/Street",
                    Query = "street",
                    EdmType = "Edm.String"
                }
            ];
        }
        
        [Fact]
        public void TryConvertFilterToSql_SimpleComparison_ReturnsCorrectSql()
        {
            // Arrange
            var processor = SetupProcessor();

            // Act
            var success = processor.TryProcessFilter("$filter=Name eq '123'", "Products", out var sqlResult, out var error);

            // Assert
            Assert.True(success);
            Assert.NotNull(sqlResult);
            Assert.Null(error);
            Assert.Equal("name = @p1", sqlResult);
        }
                
        [Fact]
        public void TryConvertFilterToSql_IncorrectComparison_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();

            // Act
            var success = processor.TryProcessFilter("$filter=Name bla '123'", "Products", out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
        }
        
        [Fact]
        public void TryConvertFilterToSql_SimpleCorrectComparisonWithBugsInCsdl_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();

            // Act
            var success = processor.TryProcessFilter("$filter=Blurp eq '123'", "Products", out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
        }
        
        [Fact]
        public void TryConvertFilterToSql_WithoutFilter_ReturnsTrue()
        {
            // Arrange
            var processor = SetupProcessor();

            // Act
            var success = processor.TryProcessFilter(string.Empty,"Products", out var sqlResult, out var error);

            // Assert
            Assert.True(success);
            Assert.Null(sqlResult);
            Assert.Null(error);
        }
        
                
        [Fact]
        public void TryConvertFilterToSql_FunctionTest_ReturnsTrue()
        {
            // Arrange
            var processor = SetupProcessor();

            // Act
            var success = processor.TryProcessFilter("$filter=startswith(Name, '123')","Products", out var sqlResult, out var error);

            // Assert
            Assert.True(success);
            Assert.NotNull(sqlResult);
            Assert.Null(error);
        }

        
        [Fact]
        public void ODataToSqlConvertor_WithEmptyFilter_ShouldReturnTrue()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());

            var result = converter.TryConvert(null, out _, out _);
            Assert.True(result);
        }
        
        [Fact]
        public void TryConvertFilterToSql_LogicalOperators_ReturnsCorrectSql()
        {
            // Arrange
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=Name eq 'Product' and Price gt 10", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.True(result);
            Assert.NotNull(sqlResult);
            Assert.Null(error);
            Assert.Equal("name = @p1 AND price > @p2", sqlResult.Sql);
        }

        [Fact]
        public void HandleInClause_WithNonLiteralValues_ReturnsError()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=Price in (Price)", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.Null(sqlResult);
            Assert.Contains("'price' must be a literal", error);
        }

        [Fact]
        public void HandleInClause_NullCheck_ReturnsSuccess()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=(Price eq null)", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.NotNull(sqlResult);
            Assert.Contains("price IS NULL", sqlResult.Sql);
            Assert.Empty(sqlResult.Parameters);
        }
        
        [Fact]
        public void HandleInClause_NeNullCheck_ReturnsSuccess()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=(Price ne null)", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.NotNull(sqlResult);
            Assert.Contains("price IS NOT NULL", sqlResult.Sql);
            Assert.Empty(sqlResult.Parameters);
        }
        [Fact]
        public void ODataFilter_TryParse_FailTest()
        {
            ODataFilter.TryParse("asdkjf lasd", out var context, out var error);

            Assert.Null(context);
            Assert.NotNull(error);
            Assert.Contains("Failed to parse filter query", error);
        }
        
        [Fact]
        public void HandleInClause_WithCorrectValues_ReturnsTrue()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=Price in (123.456, 234.56)", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.True(result);
            Assert.Null(error);
            Assert.NotNull(sqlResult);
            Assert.Contains("price IN (@p1,@p2)", sqlResult.Sql);
        }
        
        [Fact]
        public void HandleInClause_MismatchInType_ReturnsError()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            ODataFilter.TryParse("$filter=Price in ('a', 'b')", out var context, out var error);

            Assert.NotNull(context);
            // Act
            var result = converter.TryConvert(context.Context, out error, out var sqlResult);

            // Assert
            Assert.False(result);
            Assert.NotNull(error);
            Assert.Null(sqlResult);
            Assert.Contains("mismatch", error);
        }
        
        [Fact]
        public void HandleInClause_WithoutParameters_ReturnsError()
        {
            ODataFilter.TryParse("$filter=Price in ()", out var context, out var error);

            Assert.Null(context);
            Assert.NotNull(error);
            Assert.Contains("Failed to parse filter query.", error);
        }
        
        [Fact]
        public void TryProcessFilter_ConverterFailsToConvertToSql_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();
    
            // Use a filter that will parse successfully but fail during SQL conversion
            // For example, using an unsupported function or property
            const string filterString = "$filter=startswith(Bla, 'value')";
    
            // First validate that the filter parses successfully
            Assert.True(ODataFilter.TryParse(filterString, out _, out _));
    
            // Act
            var success = processor.TryProcessFilter(filterString, "Products", out var sqlResult, out var error);
    
            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
        }
        
        [Fact]
        public void InferLiteralType_Tests()
        {
            Assert.Equal("Edm.String", "Hello World".InferLiteralType());
            Assert.Equal("Edm.String", "'Hello'".InferLiteralType());
            Assert.Equal("Edm.Int32", "123".InferLiteralType());
            Assert.Equal("Edm.Double", "123.456".InferLiteralType());
            Assert.Equal("Edm.Boolean", "true".InferLiteralType());
            Assert.Equal("Edm.Boolean", "false".InferLiteralType());
            Assert.Equal("Edm.DateTimeOffset", "2025-01-01T12:00:00Z".InferLiteralType());
        }
        
        [Fact]
        public void HandleLiteral_Tests()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            var literal = converter.HandleLiteral("123");
            Assert.Equal("@p1", literal.SqlQuery);
            Assert.True(literal.Success);
            Assert.Null(literal.ErrorMessage);
        }
        
        [Fact]
        public void HandleNullLiteral_Tests()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            var literal = converter.HandleLiteral("null");
            Assert.Equal("'null'", literal.SqlQuery);
            Assert.True(literal.Success);
            Assert.Null(literal.ErrorMessage);
        }
        
        [Fact]
        public void ParseLiteralValue_Tests()
        {
            Assert.Equal(typeof(string), "\"123\"".ParseLiteralValue().GetType());
            Assert.Equal(typeof(string), "'123'".ParseLiteralValue().GetType());
            Assert.Equal(typeof(int), "123".ParseLiteralValue().GetType());
            Assert.Equal(typeof(double), "123.456".ParseLiteralValue().GetType());
            Assert.Equal(typeof(string), "Blabla".ParseLiteralValue().GetType());
            Assert.Equal(typeof(DateTimeOffset), "2025-01-01T12:00:00Z".ParseLiteralValue().GetType());
            Assert.Equal(typeof(bool), "true".ParseLiteralValue().GetType());
            Assert.Equal(typeof(bool), "false".ParseLiteralValue().GetType());
        }
        
        [Fact]
        public void IsLiteralValue_Tests()
        {
            Assert.True("null".IsLiteralValue());
            Assert.True("\"123\"".IsLiteralValue());
            Assert.True("'123'".IsLiteralValue());
            Assert.True("123".IsLiteralValue());
            Assert.True("123.456".IsLiteralValue());
            Assert.False("Blabla".IsLiteralValue());
            Assert.True("2025-01-01T12:00:00Z".IsLiteralValue());
            Assert.True("true".IsLiteralValue());
            Assert.True("false".IsLiteralValue());
        }

        [Fact]
        public void OperatorMapTests()
        {
            Assert.True(ODataToSqlConverter.TryGetOperatorMap("eq").Success);
            Assert.False(ODataToSqlConverter.TryGetOperatorMap("<>").Success);
        }

        [Fact] 
        public void PropertyNullMapTests()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            Assert.NotNull(converter);
            Assert.True(converter.TryGetPropertyMap("null").Success);
        }

        [Fact]
        public void TryGetFunctionMap_Tests()
        {
            var converter = new ODataToSqlConverter(GetPropertyMaps(), GetFunctionMaps());
            var parameters = new List<string> { "Edm.String", "Edm.String" }.ToArray();
            Assert.False(converter.TryGetFunctionMap("test", parameters).Success);
            Assert.False(converter.TryGetFunctionMap("startswith", []).Success);
            Assert.True(converter.TryGetFunctionMap("now", []).Success);
            Assert.True(converter.TryGetFunctionMap("startswith", ["bla", "bla"]).Success);
            Assert.False(converter.TryGetFunctionMap("startswith", ["bla"]).Success);
            Assert.True(converter.TryGetFunctionMap("endswith", ["bla", "bla"]).Success);
            Assert.True(converter.TryGetFunctionMap("contains", ["bla", "bla"]).Success);
        }
        
        [Fact]
        public void IsTypeCompatible_Tests()
        {
            Assert.True("Edm.String".IsTypeCompatibleWith("Edm.String"));
            Assert.False("Edm.String".IsTypeCompatibleWith("Edm.Int32"));
            Assert.True("Edm.Int32".IsTypeCompatibleWith("Edm.Int32"));
            Assert.False("Edm.Double".IsTypeCompatibleWith("Edm.Boolean"));
            Assert.True("Edm.DateTimeOffset".IsTypeCompatibleWith("Edm.DateTimeOffset"));
            Assert.False("Edm.Boolean".IsTypeCompatibleWith("Edm.String"));
            Assert.True("Edm.Double".IsTypeCompatibleWith("Edm.Int32"));
            Assert.True("Edm.Decimal".IsTypeCompatibleWith("Edm.Int32"));
            Assert.True("Edm.String".IsTypeCompatibleWith("Edm.Guid"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyPoint"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyMultiPoint"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyLineString"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyMultiLineString"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyPolygon"));
            Assert.True("Edm.Geography".IsTypeCompatibleWith("Edm.GeographyMultiPolygon"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryPoint"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryMultiPoint"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryLineString"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryMultiLineString"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryPolygon"));
            Assert.True("Edm.Geometry".IsTypeCompatibleWith("Edm.GeometryMultiPolygon"));
        }
        
        [Fact]
        public void TryValidateFilter_UnknownEntitySet_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();
            var filter = "$filter=Name eq 'Test'";
            var entitySet = "UnknownEntitySet";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
            Assert.Contains("EntitySet 'UnknownEntitySet' not found", error);
        }
        
        [Fact]
        public void TryValidateFilter_UnknownProperty_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();
            var filter = "$filter=Bla.Name eq 'Test'";
            var entitySet = "Products";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
            Assert.Contains("Unexpected token found", error);
        }
        
        [Fact]
        public void TryValidateFilter_UnknownEntityType_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();
            var filter = "$filter=Address.Bla eq 'Test'";
            var entitySet = "Products";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
            Assert.Contains("Unexpected token found", error);
        }
        
        [Fact]
        public void TryValidateFilter_WithComplexTypeButWithoutMap_ReturnsFalse()
        {
            // Arrange
            var processor = SetupProcessor();
            var filter = "$filter=Address/City eq 'Test'";
            var entitySet = "Products";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.False(success);
            Assert.Null(sqlResult);
            Assert.NotNull(error);
            Assert.Contains("Unknown property", error);
        }
        
        [Fact]
        public void TryValidateFilter_WithComplexTypeWithMap_ReturnsTrue()
        {
            // Arrange
            var processor = SetupProcessor();
            var filter = "$filter=Address/Street eq 'Test'";
            var entitySet = "Products";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.True(success);
            Assert.NotNull(sqlResult);
            Assert.Null(error);
            Assert.Contains("street = @p1", sqlResult);
        }
        
        [Fact]
        public void TryValidateFilter_WithComplexTypeWithIncorrectEntityType_ReturnsFalse()
        {
            // Arrange
            CsdlParser.CsdlParser.TryParse(GetValidCsdlContent(), out var model, out var error);
            Assert.NotNull(model);
            
            ODataFilter.TryParse("$filter=Bla/Street eq 'Test'", out var filter, out error);
            Assert.NotNull(filter);
            
            var validator = new ODataFilterValidator(model, GetFunctionMaps());
            var success = validator.TryValidate(filter.Context, "Products", out error);
            
            // Assert
            Assert.False(success);
            Assert.NotNull(error);
        }
        
        [Fact]
        public void TryValidateFilter_CorrectInClause_ReturnsTrue()
        {
            // Arrange
            CsdlParser.CsdlParser.TryParse(GetValidCsdlContent(), out var model, out var error);
            Assert.NotNull(model);
            
            ODataFilter.TryParse("$filter=Price in (1.222)", out var filter, out error);
            Assert.NotNull(filter);
            
            var validator = new ODataFilterValidator(model, GetFunctionMaps());
            var success = validator.TryValidate(filter.Context, "Products", out error);
            
            // Assert
            Assert.True(success);
            Assert.Null(error);
        }
        
        [Fact]
        public void TryValidateFilter_InClauseWithoutValues_ReturnsFalse()
        {
            // Arrange
            CsdlParser.CsdlParser.TryParse(GetValidCsdlContent(), out var model, out var error);
            Assert.NotNull(model);
            
            ODataFilter.TryParse("$filter=Price in ()", out var filter, out error);
            
            var validator = new ODataFilterValidator(model, GetFunctionMaps());
            var success = validator.TryValidate(filter?.Context, "Products", out error);
            
            // Assert
            Assert.False(success);
            Assert.NotNull(error);
        }
        
        [Fact]
        public void TryValidateFilter_WithFunction_ReturnsTrue()
        {
            // Arrange
            CsdlParser.CsdlParser.TryParse(GetValidCsdlContent(), out var model, out var error);
            Assert.NotNull(model);
            
            ODataFilter.TryParse("$filter=startswith(Name, '123')", out var filter, out error);
            
            var validator = new ODataFilterValidator(model, GetFunctionMaps());
            var success = validator.TryValidate(filter?.Context, "Products", out error);
            
            // Assert
            Assert.True(success);
            Assert.Null(error);
        }

        [Fact]
        public void TryProcessFilter_WithComplexTypeNavigationPath_ShouldHitComplexTypeBranch()
        {
            // Arrange
            var processor = SetupProcessor();
    
            // This filter uses a complex path - first navigating to the Address complex type
            // and then to a property within it (Street)
            var filter = "$filter=Address/Street eq 'TestStreet'";
            var entitySet = "Products";

            // Act
            var success = processor.TryProcessFilter(filter, entitySet, out var sqlResult, out var error);

            // Assert
            Assert.True(success);
            Assert.NotNull(sqlResult);
            Assert.Null(error);
            Assert.Equal("street = @p1", sqlResult);
        }
        
        [Fact]
        public void MapWildCardTest()
        {
            var data = new [] {"1", "2"};
            ODataToSqlConverter.MapWildcard(data, new ODataFunctionMap { ODataFunctionName = "", ExpectedArgumentTypes = [], ReturnType = "", SqlFunctionFormat = "" });
            Assert.Equal("1", data[0]);
        }
    }
}