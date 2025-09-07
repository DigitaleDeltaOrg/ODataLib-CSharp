// Copyright (c) 2025 - EcoSys
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Reflection;
using DigitaleDelta.CsdlParser;
using DigitaleDelta.ODataTranslator;
using DigitaleDelta.ODataTranslator.Models;

var assembly = Assembly.GetExecutingAssembly();
var assemblyPath = Path.GetDirectoryName(assembly.Location)!;
var filePath = Path.Combine(assemblyPath, "DigitaleDelta3.xml");
var xmlContent = await File.ReadAllTextAsync(filePath);
var functionMap = GetFunctionMaps();
var propertyMap = GetPropertyMaps();


// Try to parse the CSDL XML content, producing a CsdlModel. Typically, this would be done once at the application startup.
if (!CsdlParser.TryParse(xmlContent, out var csdlModel, out var error) || csdlModel == null)
{
    Console.Error.WriteLine(error);
    
    return;
}

// Create an instance of ODataFilterProcessor with the parsed CsdlModel, function map, and property map. Typically, this is used as a singleton service in an application.
var processor = new ODataFilterProcessor(csdlModel, functionMap, propertyMap);

// Process the filter.
var query = "$filter=startswith(parameter/source, 'Wmr') and ResultOf eq 'aggregation' and (PhenomenonTime/BeginPosition ge now() and PhenomenonTime/BeginPosition le now()) OR parameter/source IN ('Wmr', 'Wmr2')";
if (!processor.TryProcessFilter(query, "observations", out var sql, out error))
{
    Console.WriteLine(error);
    
    return;
}

Console.WriteLine(sql);

// Construct DD API V3 results using the classes.
// Use ODataWriter to write the results.

return;

List<ODataFunctionMap> GetFunctionMaps()
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
            ODataFunctionName = "now",
            ExpectedArgumentTypes = [],
            ReturnType = "Edm.DateTimeOffset",
            SqlFunctionFormat = "NOW()"
        }
    ];
}

List<ODataToSqlMap> GetPropertyMaps()
{
    return
    [
        new()
        {
            ODataPropertyName = "parameter/source",
            Query = "source",
            EdmType = "Edm.String"
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
        }
    ];
}