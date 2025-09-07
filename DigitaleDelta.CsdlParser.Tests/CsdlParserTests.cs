using DigitaleDelta.Contracts;

namespace DigitaleDelta.CsdlParser.Tests
{
  public class CsdlParserTests
  {
    [Fact]
    public void Parse_ValidCsdl_ReturnsValidModel()
    {
      // Arrange
      var csdlContent = GetValidCsdlContent();

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.True(result);
      Assert.Null(error);
      Assert.NotNull(model);
      Assert.Single(model.EntityContainers);
      Assert.Equal("DefaultContainer", model.EntityContainers[0].Name);
      Assert.Single(model.EntityContainers[0].EntitySets);
      Assert.Equal("Products", model.EntityContainers[0].EntitySets[0].Name);

      Assert.Single(model.EntityTypes);
      Assert.NotNull(model.EntityTypes);
      Assert.Equal("Product", model.EntityTypes[0].Name);
      Assert.Single(model.EntityTypes[0].Keys);
      Assert.Equal("ID", model.EntityTypes[0].Keys[0]);
      Assert.Equal(3, model.EntityTypes[0].Properties.Count);
    }

    [Fact]
    public void Parse_InvalidFilePath_ReturnsError()
    {
      // Act
      var result = CsdlParser.TryParse("nonexistent.xml", out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.NotNull(error);
      Assert.Contains("An error occurred while parsing the CSDL", error);
    }

    [Fact]
    public void Parse_MissingEntityContainerName_ReturnsError()
    {
      // Arrange
      var content = GetCsdlWithMissingEntityContainerName();

      // Act
      var result = CsdlParser.TryParse(content, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Equal("EntityContainer name is required.", error);
    }

    [Fact]
    public void Parse_EdmTypesAreMappedCorrectly()
    {
      // Arrange
      var content = GetCsdlWithVariousEdmTypes();

      // Act
      var result = CsdlParser.TryParse(content, out var model, out var error);

      // Assert
      Assert.True(result);
      Assert.Null(error);
      Assert.NotNull(model);
      var properties = model.EntityTypes[0].Properties;

      Assert.Equal(EdmType.EdmString, properties[0].EdmType);
      Assert.Equal(EdmType.EdmInt32, properties[1].EdmType);
      Assert.Equal(EdmType.EdmBoolean, properties[2].EdmType);
      Assert.Equal(EdmType.EdmDateTimeOffset, properties[3].EdmType);
    }

    [Fact]
    public void Parse_ComplexTypesAndFunctions_ParsedCorrectly()
    {
      // Arrange
      var content = GetValidCsdlContent();

      // Act
      var result = CsdlParser.TryParse(content, out var model, out var error);
      Assert.NotNull(model);

      // Assert
      Assert.True(result);
      Assert.Null(error);
      Assert.Single(model.ComplexTypes);
      Assert.Equal("Address", model.ComplexTypes[0].Name);
      Assert.Equal(2, model.ComplexTypes[0].Properties.Count);

      Assert.Single(model.Functions);
      Assert.Equal("GetProducts", model.Functions[0].Name);
      Assert.Equal("Collection(ODataDemo.Product)", model.Functions[0].ReturnType);
      Assert.Single(model.Functions[0].Parameters);
      Assert.Equal("category", model.Functions[0].Parameters[0].Name);
    }

    [Fact]
    public void ComplexType_PropertiesWithAllAttributesInitializedCorrectly()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <ComplexType Name="Address">
                                <Property Name="Street" Type="Edm.String" Nullable="true" DefaultValue="Main St" MaxLength="100" />
                                <Property Name="City" Type="Edm.String" Nullable="false" />
                                <Property Name="ZipCode" Type="Edm.String" MaxLength="10" />
                                <Property Name="Latitude" Type="Edm.Decimal" Precision="10" Scale="6" />
                              </ComplexType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out _);

      // Assert
      Assert.True(result);
      Assert.NotNull(model);
      Assert.Single(model.ComplexTypes);

      var complexType = model.ComplexTypes[0];
      Assert.Equal("Address", complexType.Name);
      Assert.Equal(4, complexType.Properties.Count);

      // Test first property with all attributes
      var streetProp = complexType.Properties[0];
      Assert.Equal("Street", streetProp.Name);
      Assert.Equal("Edm.String", streetProp.Type);
      Assert.Equal(EdmType.EdmString, streetProp.EdmType);
      Assert.True(streetProp.Nullable);
      Assert.Equal("Main St", streetProp.DefaultValue);
      Assert.Equal(100, streetProp.MaxLength);

      // Test second property with Nullable=false
      var cityProp = complexType.Properties[1];
      Assert.Equal("City", cityProp.Name);
      Assert.Equal("Edm.String", cityProp.Type);
      Assert.False(cityProp.Nullable);

      // Test third property with MaxLength
      var zipProp = complexType.Properties[2];
      Assert.Equal("ZipCode", zipProp.Name);

      // Test fourth property with Precision and Scale
      var latProp = complexType.Properties[3];
      Assert.Equal("Latitude", latProp.Name);
      Assert.Equal("Edm.Decimal", latProp.Type);
      Assert.Equal(10, latProp.Precision);
      Assert.Equal(6, latProp.Scale);
    }

    [Fact]
    public void ComplexType_SetPropertiesCollection_PropertiesCorrectlySet()
    {
      // Arrange
      var complexType = new ComplexType
      {
        Name = "TestComplexType"
      };

      // Create a list of properties to set
      var properties = new List<Property>
      {
        new Property
        {
          Name = "PropertyOne",
          Type = "Edm.String",
          Nullable = true
        },
        new Property
        {
          Name = "PropertyTwo",
          Type = "Edm.Int32",
          Nullable = false
        }
      };

      // Act
      complexType.Properties = properties;

      // Assert
      Assert.Equal(2, complexType.Properties.Count);
      Assert.Equal("PropertyOne", complexType.Properties[0].Name);
      Assert.Equal("PropertyTwo", complexType.Properties[1].Name);
      Assert.Equal("Edm.String", complexType.Properties[0].Type);
      Assert.Equal("Edm.Int32", complexType.Properties[1].Type);
    }

        
    [Fact]
    public void TeyParse_EntityType_NameIsRequired()
    {
      // Arrange
      var entityType = new EntityType
      {
        Name = ""
      };

      var properties = new List<Property>
      {
        new Property
        {
          Name = "PropertyOne",
          Type = "Edm.String",
          Nullable = true
        },
        new Property
        {
          Name = "PropertyTwo",
          Type = "Edm.Int32",
          Nullable = false
        }
      };

      // Act
      entityType.Properties = properties;

      // Assert
      Assert.NotNull(entityType);
      Assert.Equal(2, entityType.Properties.Count);
      Assert.Equal("PropertyOne", entityType.Properties[0].Name);
      Assert.Equal("PropertyTwo", entityType.Properties[1].Name);
      Assert.Equal("Edm.String", entityType.Properties[0].Type);
      Assert.Equal("Edm.Int32", entityType.Properties[1].Type);
    }

    
    [Fact]
    public void EntityType_SetPropertiesCollection_PropertiesCorrectlySet()
    {
      // Arrange
      var entityType = new EntityType
      {
        Name = "TestComplexType"
      };

      // Create a list of properties to set
      var properties = new List<Property>
      {
        new Property
        {
          Name = "PropertyOne",
          Type = "Edm.String",
          Nullable = true
        },
        new Property
        {
          Name = "PropertyTwo",
          Type = "Edm.Int32",
          Nullable = false
        }
      };

      // Act
      entityType.Properties = properties;

      // Assert
      Assert.NotNull(entityType);
      Assert.Equal(2, entityType.Properties.Count);
      Assert.Equal("PropertyOne", entityType.Properties[0].Name);
      Assert.Equal("PropertyTwo", entityType.Properties[1].Name);
      Assert.Equal("Edm.String", entityType.Properties[0].Type);
      Assert.Equal("Edm.Int32", entityType.Properties[1].Type);
    }
    
    [Fact]
    public void EntityType_SetKeysCollection_KeysCorrectlySet()
    {
      // Arrange
      var entityType = new EntityType
      {
        Name = "TestComplexType"
      };

      // Create a list of properties to set
      var properties = new List<string> {  "PropertyOne", "PropertyTwo" };
      // Act
      entityType.Keys = properties;

      // Assert
      Assert.Equal(2, entityType.Keys.Count);
      Assert.Equal("PropertyOne", entityType.Keys[0]);
      Assert.Equal("PropertyTwo", entityType.Keys[1]);
    }
    
    [Fact]
    public void EntityContainer_GetEntityType_ReturnsCorrectEntityType()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityContainer Name="DefaultContainer">
                                <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                              </EntityContainer>
                              <EntityType Name="Product">
                                <Key>
                                  <PropertyRef Name="ID" />
                                </Key>
                                <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                                <Property Name="Name" Type="Edm.String" />
                              </EntityType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      var result = CsdlParser.TryParse(csdlContent, out var model, out _);
      Assert.True(result);
      Assert.NotNull(model);
    
      // Act
      var entityType = model.EntityContainers[0].EntitySets[0].EntityType;
    
      // Assert
      Assert.NotNull(entityType);
      Assert.Equal("ODataDemo.Product", entityType);
    }
    
    [Fact]
    public void EntityContainer_GetEntityType_EntityTypeNameIsRequired()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityContainer Name="DefaultContainer">
                                <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                              </EntityContainer>
                              <EntityType>
                              </EntityType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      var result = CsdlParser.TryParse(csdlContent, out _, out var error);
      Assert.False(result);
      Assert.Equal("EntityType name is required.", error);
    }
    
    [Fact]
    private static void SetEntityContainerName_UpdatesNameCorrectly()
    {
      // Arrange
      var entityContainer = new EntityContainer
      {
        Name = "InitialContainer",
        EntitySets = [new EntitySet { Name = "Products", EntityType = "ODataDemo.Product" }]
      };

      // Act
      entityContainer.Name = "UpdatedContainer";

      // Assert
      Assert.Equal("UpdatedContainer", entityContainer.Name);
      Assert.Equal("ODataDemo.Product", entityContainer.EntitySets[0].EntityType);
      Assert.Equal("Products", entityContainer.EntitySets[0].Name);
    }
      
    [Fact]
    private static void SetParameterType_UpdatesParameterTypeCorrectly()
    {
      // Arrange
      var parameter = new Parameter { Type = "Edm.String", Name = "category" };

      // Act
      parameter.Name = "NewParameterName";

      // Assert
      Assert.Equal("Edm.String", parameter.Type);
    }

    [Fact]
    private static void SetCsdlModel_EntityTypesSetCorrectly()
    {
      // Arrange
      var model = new CsdlModel
      {
        EntityTypes =
        [
          new EntityType
          {
            Name = "TestEntity",
            Properties =
            [
              new Property { Name = "ID", Type = "Edm.Int32", Nullable = false },
              new Property { Name = "Name", Type = "Edm.String" }
            ],
            Keys = ["ID"]
          }
        ],
        ComplexTypes =
        [
          new ComplexType
          {
            Name = "TestComplexType", Properties =
            [
              new Property { Name = "Street", Type = "Edm.String" },
              new Property { Name = "City", Type = "Edm.String" }
            ]
          }
        ],
        Functions =
        [
          new Function
          {
            Name = "GetTestEntities",
            ReturnType = "Collection(ODataDemo.TestEntity)",
            Parameters = [new Parameter { Name = "category", Type = "Edm.String" }]
          }
        ],
        EntityContainers =
        [
          new EntityContainer
          {
            Name = "DefaultContainer",
            EntitySets = [new EntitySet { Name = "TestEntities", EntityType = "ODataDemo.TestEntity" }]
          }
        ]
      };

      // Act
      
      // Assert
      Assert.Single(model.EntityTypes);
      Assert.Single(model.ComplexTypes);
      Assert.Single(model.Functions);
      Assert.Single(model.EntityContainers);
    }
    
    [Fact]
    private static void TryParse_InvalidContent_ReturnsFalseAndNoModel()
    {
      // Arrange
      var csdlContent = string.Empty;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Equal("An error occurred while parsing the CSDL: Root element is missing.", error);
      Assert.Null(model);
    }
    
    [Fact]
    public void TryParse_MalformedXml_ReturnsFalseAndError()
    {
      // Arrange
      var malformedXml = """
                         <?xml version="1.0" encoding="utf-8"?>
                         <edmx:Edmx Version="4.0"
                              xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                              xmlns="http://docs.oasis-open.org/odata/ns/edm">
                           <edmx:DataServices>
                             <Schema Namespace="ODataDemo">
                               <EntityContainer Name="DefaultContainer">
                                 <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                               </EntityContainer>
                               <EntityType Name="Product">
                                 <Key>
                                   <PropertyRef Name="ID" />
                                 </Key>
                                 <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                                 <Property Name="Name" Type="Edm.String" />
                               </EntityType>
                             </Schema>
                           </edmx:DataServices>
                           <!-- Missing closing tag for Edmx -->

                         """;

      // Act
      var result = CsdlParser.TryParse(malformedXml, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("An error occurred while parsing the CSDL", error);
    }
    
    [Fact]
    public void TryParse_ValidXmlButInvalidCsdlStructure_ReturnsFalseAndError()
    {
      // Arrange
      var validXmlButInvalidCsdl = """
                                   <?xml version="1.0" encoding="utf-8"?>
                                   <root>
                                     <child>
                                       <grandchild>Some content</grandchild>
                                     </child>
                                   </root>
                                   """;

      // Act
      var result = CsdlParser.TryParse(validXmlButInvalidCsdl, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("Invalid CSDL format.", error);
    }
    
    [Fact]
    public void EntityContainer_EntitySetWithNonExistentEntityType_ParsesCorrectly()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityContainer Name="DefaultContainer">
                                <EntitySet Name="Products" EntityType="ODataDemo.NonExistentProduct" />
                              </EntityContainer>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.True(result); // Should still parse successfully
      Assert.Null(error);
      Assert.NotNull(model);
      Assert.Single(model.EntityContainers);
    
      var entitySet = model.EntityContainers[0].EntitySets[0];
      Assert.Equal("Products", entitySet.Name);
      Assert.Equal("ODataDemo.NonExistentProduct", entitySet.EntityType);
      Assert.Empty(model.EntityTypes);
    }

    [Fact]
    public void EntityContainer_RequiresName()
    {
      // Arrange
      var validXmlButInvalidCsdl = """
                                   <edmx:Edmx Version="4.0"
                                        xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                                        xmlns="http://docs.oasis-open.org/odata/ns/edm">
                                     <edmx:DataServices>
                                       <Schema Namespace="ODataDemo">
                                         <EntityContainer>
                                           <EntitySet Name="Products" EntityType="ODataDemo.NonExistentProduct" />
                                         </EntityContainer>
                                       </Schema>
                                     </edmx:DataServices>
                                   </edmx:Edmx>
                                   """;

      // Act
      var result = CsdlParser.TryParse(validXmlButInvalidCsdl, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("EntityContainer name is required.", error);
    }
    
    [Fact]
    public void EntityContainerEntitySet_RequiresName()
    {
      // Arrange
      var validXmlButInvalidCsdl = """
                                   <edmx:Edmx Version="4.0"
                                        xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                                        xmlns="http://docs.oasis-open.org/odata/ns/edm">
                                     <edmx:DataServices>
                                       <Schema Namespace="ODataDemo">
                                         <EntityContainer Name="DefaultContainer">
                                           <EntitySet EntityType="ODataDemo.NonExistentProduct" />
                                         </EntityContainer>
                                       </Schema>
                                     </edmx:DataServices>
                                   </edmx:Edmx>
                                   """;

      // Act
      var result = CsdlParser.TryParse(validXmlButInvalidCsdl, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("EntitySet name is required.", error);
    }
    
    [Fact]
    public void EntityContainerEntitySet_RequiresType()
    {
      // Arrange
      var validXmlButInvalidCsdl = """
                                   <edmx:Edmx Version="4.0"
                                        xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                                        xmlns="http://docs.oasis-open.org/odata/ns/edm">
                                     <edmx:DataServices>
                                       <Schema Namespace="ODataDemo">
                                         <EntityContainer Name="DefaultContainer">
                                           <EntitySet Name="Bla" />
                                         </EntityContainer>
                                       </Schema>
                                     </edmx:DataServices>
                                   </edmx:Edmx>
                                   """;

      // Act
      var result = CsdlParser.TryParse(validXmlButInvalidCsdl, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("EntitySet EntityType is required.", error);
    }
    
    [Fact]
    public void TryParse_MissingNamespace_ReturnsFalseAndError()
    {
      // Arrange
      var xmlWithMissingNamespace = """
                                    <?xml version="1.0" encoding="utf-8"?>
                                    <Edmx Version="4.0">
                                      <DataServices>
                                        <Schema Namespace="ODataDemo">
                                          <EntityContainer Name="DefaultContainer">
                                            <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                                          </EntityContainer>
                                        </Schema>
                                      </DataServices>
                                    </Edmx>
                                    """;

      // Act
      var result = CsdlParser.TryParse(xmlWithMissingNamespace, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.NotNull(error);
      Assert.Null(model);
      Assert.Contains("Invalid CSDL format.", error);
    }
    
    
    
    [Fact]
    public void TryParse_MultipleSchemas_ParsesCorrectly()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityContainer Name="DefaultContainer">
                                <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                              </EntityContainer>
                              <EntityType Name="Product">
                                <Key>
                                  <PropertyRef Name="ID" />
                                </Key>
                                <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                                <Property Name="Name" Type="Edm.String" />
                              </EntityType>
                            </Schema>
                            <Schema Namespace="ODataDemo.Common">
                              <ComplexType Name="Address">
                                <Property Name="Street" Type="Edm.String" />
                                <Property Name="City" Type="Edm.String" />
                              </ComplexType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.True(result);
      Assert.Null(error);
      Assert.NotNull(model);
      Assert.Single(model.EntityContainers);
      Assert.Single(model.EntityTypes);
      Assert.Single(model.ComplexTypes);
      Assert.Equal("Address", model.ComplexTypes[0].Name);
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
                   </EntityContainer>
                   <EntityType Name="Product">
                     <Key>
                       <PropertyRef Name="ID" />
                     </Key>
                     <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                     <Property Name="Name" Type="Edm.String" />
                     <Property Name="Description" Type="Edm.String" />
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

    private static string GetCsdlWithMissingEntityContainerName()
    {
      return """
             <?xml version="1.0" encoding="utf-8"?>
             <edmx:Edmx Version="4.0"
                  xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                  xmlns="http://docs.oasis-open.org/odata/ns/edm">
               <edmx:DataServices>
                 <Schema Namespace="ODataDemo">
                   <EntityContainer>
                     <EntitySet Name="Products" EntityType="ODataDemo.Product" />
                   </EntityContainer>
                 </Schema>
               </edmx:DataServices>
             </edmx:Edmx>
             """;
    }
    
    [Fact]
    public void TryParse_MissingPropertyTypeInEntityType_ReturnsFalseWithError()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityType Name="Product">
                                <Key>
                                  <PropertyRef Name="ID" />
                                </Key>
                                <Property Name="ID" Nullable="false" />
                              </EntityType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("Property type is required for EntityType 'Product'", error);
    }
    
    [Fact]
    public void TryParse_WithEnumType_ParsesCorrectly()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EnumType Name="ProductStatus">
                                <Member Name="Available" Value="0" />
                                <Member Name="OutOfStock" Value="1" />
                                <Member Name="Discontinued" Value="2" />
                              </EnumType>
                              <EntityType Name="Product">
                                <Key>
                                  <PropertyRef Name="ID" />
                                </Key>
                                <Property Name="ID" Type="Edm.Int32" Nullable="false" />
                                <Property Name="Status" Type="ODataDemo.ProductStatus" />
                              </EntityType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.True(result);
      Assert.Null(error);
      Assert.NotNull(model);
      // Additional assertions would depend on your model implementation
    }

    [Fact]
    private void ValidateEdmType()
    {
      Assert.Equal(EdmType.EdmString, CsdlParser.MapEdmType("Edm.String"));
      Assert.Equal(EdmType.EdmInt32, CsdlParser.MapEdmType("Edm.Int32"));
      Assert.Equal(EdmType.EdmBoolean, CsdlParser.MapEdmType("Edm.Boolean"));
      Assert.Equal(EdmType.EdmDouble, CsdlParser.MapEdmType("Edm.Double"));
      Assert.Equal(EdmType.EdmBinary, CsdlParser.MapEdmType("Edm.Binary"));
      Assert.Equal(EdmType.EdmGeography, CsdlParser.MapEdmType("Edm.Geography"));
      Assert.Equal(EdmType.EdmGeographyPoint, CsdlParser.MapEdmType("Edm.GeographyPoint"));
      Assert.Equal(EdmType.EdmGeographyLineString, CsdlParser.MapEdmType("Edm.GeographyLineString"));
      Assert.Equal(EdmType.EdmGeographyPolygon, CsdlParser.MapEdmType("Edm.GeographyPolygon"));
      Assert.Equal(EdmType.EdmGeographyMultiPoint, CsdlParser.MapEdmType("Edm.GeographyMultiPoint"));
      Assert.Equal(EdmType.EdmGeographyMultiLineString, CsdlParser.MapEdmType("Edm.GeographyMultiLineString"));
      Assert.Equal(EdmType.EdmGeographyMultiPolygon, CsdlParser.MapEdmType("Edm.GeographyMultiPolygon"));
      Assert.Equal(EdmType.EdmGeometry, CsdlParser.MapEdmType("Edm.Geometry"));
      Assert.Equal(EdmType.EdmGeometryPoint, CsdlParser.MapEdmType("Edm.GeometryPoint"));
      Assert.Equal(EdmType.EdmGeometryLineString, CsdlParser.MapEdmType("Edm.GeometryLineString"));
      Assert.Equal(EdmType.EdmGeometryPolygon, CsdlParser.MapEdmType("Edm.GeometryPolygon"));
      Assert.Equal(EdmType.EdmGeometryMultiPoint, CsdlParser.MapEdmType("Edm.GeometryMultiPoint"));
      Assert.Equal(EdmType.EdmGeometryMultiLineString, CsdlParser.MapEdmType("Edm.GeometryMultiLineString"));
      Assert.Equal(EdmType.EdmGeometryMultiPolygon, CsdlParser.MapEdmType("Edm.GeometryMultiPolygon"));
      Assert.Equal(EdmType.EdmUnknown, CsdlParser.MapEdmType("Edm.Unknown"));
      Assert.Equal(EdmType.EdmString, CsdlParser.MapEdmType("Edm.String"));
      Assert.Equal(EdmType.EdmGuid, CsdlParser.MapEdmType("Edm.Guid"));
    }

    [Fact]
    public void TryParse_MissingPropertyNameInEntityType_ReturnsFalseWithError()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <EntityType Name="Product">
                                <Key>
                                  <PropertyRef Name="ID" />
                                </Key>
                                <Property Type="Edm.Int32" Nullable="false" />
                              </EntityType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("Property name is required for EntityType 'Product'", error);
    }
    
    // Test complex type properties
    [Fact]
    private static void TryParse_ComplexType_MustHaveName()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <ComplexType>
                              </ComplexType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("ComplexType name is required.", error);
    }
    
    [Fact]
    private static void TryParse_ComplexTypeProperties_MustHaveName()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <ComplexType Name="TEST">
                                <Property Type="Edm.String" />
                              </ComplexType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("Property name is required for ComplexType", error);
    }
    
    [Fact]
    private static void TryParse_ComplexTypeProperties_MustHaveType()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <ComplexType Name="TEST">
                                <Property Name="bla" />
                              </ComplexType>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("Property type is required for Property 'bla' in ComplexType 'TEST'.", error);
    }
    
    [Fact]
    private static void TryParse_Function_MustHaveName()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <Function>
                              </Function>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains("Function name is required.", error);
    }
    
    [Fact]
    private static void TryParse_Function_MustHaveReturnType()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <Function Name="TEST">
                              </Function>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains($"Function return type is required for Function 'TEST'.", error);
    }
    
    [Fact]
    private static void TryParse_Function_FunctionParameterMustHaveName()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <Function Name="TEST">
                                <Parameter Type="Edm.String" />
                                <ReturnType Type="Edm.String" />
                              </Function>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains($"Parameter name is required for Function 'TEST'.", error);
    }
    
        
    [Fact]
    private static void TryParse_Function_FunctionParameterMustHaveParameterType()
    {
      // Arrange
      var csdlContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <edmx:Edmx Version="4.0"
                             xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                             xmlns="http://docs.oasis-open.org/odata/ns/edm">
                          <edmx:DataServices>
                            <Schema Namespace="ODataDemo">
                              <Function Name="TEST">
                                <Parameter Name="bla" />
                                <ReturnType Type="Edm.String" />
                              </Function>
                            </Schema>
                          </edmx:DataServices>
                        </edmx:Edmx>
                        """;

      // Act
      var result = CsdlParser.TryParse(csdlContent, out var model, out var error);

      // Assert
      Assert.False(result);
      Assert.Null(model);
      Assert.Contains($"Parameter type is required for Parameter 'bla' in Function 'TEST'.", error);
    }
    
    private static string GetCsdlWithVariousEdmTypes()
    {
      return """
             <?xml version="1.0" encoding="utf-8"?>
             <edmx:Edmx Version="4.0" 
                  xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx"
                  xmlns="http://docs.oasis-open.org/odata/ns/edm">
               <edmx:DataServices>
                 <Schema Namespace="ODataDemo">
                   <EntityType Name="Product">
                     <Key>
                       <PropertyRef Name="ID" />
                     </Key>
                     <Property Name="Name" Type="Edm.String" />
                     <Property Name="ID" Type="Edm.Int32" />
                     <Property Name="IsActive" Type="Edm.Boolean" />
                     <Property Name="CreatedAt" Type="Edm.DateTimeOffset" />
                   </EntityType>
                 </Schema>
               </edmx:DataServices>
             </edmx:Edmx>
             """;
    }
  }
}