using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace SplitBuildingLimits;

public class Program
{
    // Update this to the relevant path
    private static readonly string RootFolderPath = "C:/MyWork/Dev/Repos/AutoDeskTestProj/SplitBuildingLimits/SplitBuildingLimits/";
    
    private static readonly string BuildingLimitsFilePath = RootFolderPath + "samples/SampleBuildingLimits.json";
    private static readonly string SampleHeightPlateausFilePath = RootFolderPath + "samples/SampleHeightPlateaus.json";

    private static string ReadFile(string filePath)
    {
        string text;
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
        {
            text = streamReader.ReadToEnd();
        }
        if (string.IsNullOrEmpty(text))
        {
            throw new Exception("Contents of file are empty");
        }
        return text;
    }

    private static FeatureCollection DeserializeGeojson(string geojson)
    {
        var serializer = GeoJsonSerializer.Create();
        using (var stringReader = new StringReader(geojson))
        using (var jsonReader = new JsonTextReader(stringReader))
        {
            var geometry = serializer.Deserialize<FeatureCollection>(jsonReader) ?? throw new Exception("Geometry is null");
            return geometry;
        }
        throw new Exception("Unable to deserialize json");
    }

    private static List<IFeature> FilterPolygonsFromFeatureCollection(FeatureCollection featureCollection)
    {
        return featureCollection.Where((feature) => feature.Geometry.GeometryType == "Polygon").ToList();
    }
    
    
    public static void Main()
    {
        var sampleBuildingLimits = ReadFile(BuildingLimitsFilePath);
        var sampleHeightPlateaus = ReadFile(SampleHeightPlateausFilePath);

        FeatureCollection buildingLimitsFeatureCollection = DeserializeGeojson(sampleBuildingLimits);
        FeatureCollection heightPlateausFeatureCollection = DeserializeGeojson(sampleHeightPlateaus);

        List<IFeature> buildingLimitPolygons = FilterPolygonsFromFeatureCollection(buildingLimitsFeatureCollection);
        List<IFeature> heightPlateausPolygons = FilterPolygonsFromFeatureCollection(heightPlateausFeatureCollection);

        SplitBuildingLimitsClass<IFeature>.SplitBuildingLimits(buildingLimitPolygons, heightPlateausPolygons);
    }
}