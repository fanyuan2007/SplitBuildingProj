using NetTopologySuite.Features;
namespace SplitBuildingLimits;

public static class SplitBuildingLimitsClass<TPolygon> where TPolygon : IFeature
{
    /**
    * Example usage: 
    * GetPolygonMember<double>(polygon, "elevation"); // Returns the elevation property corresponding to the polygon if it exists, else null
    */
    private static T GetPolygonMember<T>(TPolygon polygon, string key)
    {
        return (T) polygon.Attributes.GetOptionalValue(key);
    }

    /**
     * Consumes a list of building limits (polygons) and a list of height plateaus (polygons). Splits up the building
     * limits according to the height plateaus and persists:
     * 1. The original building limits
     * 2. The original height plateaus
     * 3. The split building limits
     * 
     * <param name="buildingLimits"> A list of buildings limits. A building limit is a polygon indicating where building can happen. </param>
     * <param name="heightPlateaus"> A list of height plateaus. A height plateau is a discrete polygon with a constant elevation. </param>
     */
    public static void SplitBuildingLimits(List<IFeature> buildingLimits, List<IFeature> heightPlateaus)
    {
        Console.WriteLine("Splitting building limits according to height plateaus");
        // Step 1: Input validation
        Validate(buildingLimits, heightPlateaus);

        Console.WriteLine("Start splitting...");
        // Step 2: Split building limits processing
        //var mergedBuildingLimits = GeometryOperations<IFeature>.MergePolygonsWithOverlaps(buildingLimits);

        Console.WriteLine("Finished splitting...");
    }

    /// <summary>
    /// Input validation
    /// </summary>
    /// <param name="buildingLimits">The input of the building limits</param>
    /// <param name="heightPlateaus">The input of the height plateaus</param>
    private static void Validate(List<IFeature> buildingLimits, List<IFeature> heightPlateaus)
    {
        ValidatePolygons(buildingLimits, "Building Limits");

        ValidatePolygons(heightPlateaus, "Height Plateaus");
        ValidatePolygonWithoutOverlaps(heightPlateaus, "Height Plateaus");
    }

    /// <summary>
    /// Validate the polygon itself
    /// </summary>
    /// <param name="polygons">The input polygon list</param>
    /// <param name="polygonType">The type of the input polygon, for error message output</param>
    /// <exception cref="ArgumentNullException">If the input polygon list is null</exception>
    /// <exception cref="ArgumentException">If any of the polygon in the list is invalid</exception>
    private static void ValidatePolygons(List<IFeature> polygons, string polygonType)
    {
        if (polygons == null || polygons.Count == 0)
        {
            throw new ArgumentNullException($"Error: The input polygon list {polygonType} cannot be null or empty.");
        }

        foreach (IFeature polygon in polygons)
        {
            if (!GeometryOperations<IFeature>.IsValidPolygon(polygon))
            {
                throw new ArgumentException($"Error: Input polygon list {polygonType} contains invalid polygons.");
            }
        }
    }

    /// <summary>
    /// Validate the polygon list and check if there are overlaps
    /// </summary>
    /// <param name="polygons">The input polygon list</param>
    /// <param name="polygonType">The type of the input polygon, for error message output</param>
    /// <exception cref="ArgumentNullException">If the input polygon list is null</exception>
    /// <exception cref="ArgumentException">If any of the polygon in the list is invalid</exception>
    private static void ValidatePolygonWithoutOverlaps(List<IFeature> polygons, string polygonType)
    {
        if (polygons == null || polygons.Count == 0)
        {
            throw new ArgumentNullException($"Error: The input polygon list {polygonType} cannot be null or empty.");
        }

        if (GeometryOperations<IFeature>.ArePolygonsHaveOverlaps(polygons))
        {
            throw new ArgumentException($"Error: The input polygon list {polygonType} contains overlapped subset which is invalid.");
        }
    }

    private static void BuildingLimitsSplitting(List<IFeature> buildingLimits, List<IFeature> heightPlateaus)
    {

    }
}