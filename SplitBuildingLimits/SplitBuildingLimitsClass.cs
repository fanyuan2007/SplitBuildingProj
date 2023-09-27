using NetTopologySuite.Features;
namespace SplitBuildingLimits;

public static class SplitBuildingLimitsClass<TPolygon> where TPolygon : IFeature
{
    private static readonly double _defaultElevation = 9999.0;
    private static readonly string _elevationKey = "elevation";

    private static readonly object _lock = new object();
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
    public static List<IFeature> SplitBuildingLimits(List<IFeature> buildingLimits, List<IFeature> heightPlateaus)
    {
        lock (_lock)
        {
            Console.WriteLine("Splitting building limits according to height plateaus");
            // Step 1: Input validation
            Validate(buildingLimits, heightPlateaus);

            // Step 2: Split building limits processing
            Console.WriteLine("Start splitting...");
            var mergedBuildingLimits = GeometryOperations<IFeature>.MergePolygonsWithOverlaps(buildingLimits);
            var results = BuildingLimitsSplitting(mergedBuildingLimits, heightPlateaus);
            Console.WriteLine("Finished splitting...");

            return results;
        }
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
        ValidateElevationAttribute(heightPlateaus, "Height Plateaus", _elevationKey);
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

    /// <summary>
    /// Validate the existance of the attribute in each polygon
    /// </summary>
    /// <param name="polygons">The input polygon list</param>
    /// <param name="polygonType">The type of the input polygon, for error message output</param>
    /// <param name="attrbuteKey">The attribute name to be checked</param>
    /// <exception cref="ArgumentNullException">If the input polygon list is null</exception>
    /// <exception cref="ArgumentException">If any of the polygon in the list is invalid</exception>
    private static void ValidateElevationAttribute(List<IFeature> polygons, string polygonType, string attrbuteKey)
    {
        if (polygons == null || polygons.Count == 0)
        {
            throw new ArgumentNullException($"Error: The input polygon list {polygonType} cannot be null or empty.");
        }

        foreach(IFeature polygon in polygons)
        {
            if (polygon.Attributes == null || 
                polygon.Attributes.Count == 0 || 
                !polygon.Attributes.Exists(attrbuteKey))
            {
                throw new ArgumentException($"Error: The input polygon list {polygonType} contains polygon with missing attribute {attrbuteKey}");
            }
        }
    }

    /// <summary>
    /// Splitting the building limits with the height plateaus list
    /// </summary>
    /// <param name="buildingLimits">The input merged building limits list</param>
    /// <param name="heightPlateaus">The input height plateaus list</param>
    /// <returns>A list of updated building limits with elevation attribute</returns>
    private static List<IFeature> BuildingLimitsSplitting(List<IFeature> buildingLimits, List<IFeature> heightPlateaus)
    {
        var resultBLs = new List<IFeature>();
        for (int i = 0; i < buildingLimits.Count; i++)
        {
            var currentBuildingLimit = buildingLimits[i];

            // Check the current Building Limit against the height plateaus list to find overlaps
            // 1) If overlaps found, add a new building list with the attribute set to the overlapped height plateaus elevation value and update the current building limit geometry
            // 2) If no overlap found, just assign a default elevation value to the building limit and add to the result list
            var remainingBuildingLimitGeom = currentBuildingLimit.Geometry;
            for (int j = 0; j < heightPlateaus.Count; j++)
            {
                var currentHP = heightPlateaus[j];
                if (currentBuildingLimit.Geometry.Overlaps(currentHP.Geometry))
                {
                    var intersectGeom = remainingBuildingLimitGeom.Intersection(currentHP.Geometry);
                    var newAttributes = new AttributesTable
                    {
                        { _elevationKey, currentHP.Attributes[_elevationKey] }
                    };
                    var newFeature = new Feature(intersectGeom, newAttributes);
                    resultBLs.Add(newFeature);

                    remainingBuildingLimitGeom = remainingBuildingLimitGeom.Difference(intersectGeom);
                }
            }

            if (remainingBuildingLimitGeom != null)
            {
                if (!currentBuildingLimit.Attributes.Exists(_elevationKey))
                {
                    currentBuildingLimit.Attributes.Add(_elevationKey, _defaultElevation);
                }
                currentBuildingLimit.Attributes[_elevationKey] = _defaultElevation;
                var newFeature = new Feature(remainingBuildingLimitGeom, currentBuildingLimit.Attributes);
                resultBLs.Add(newFeature);
            }
        }

        return resultBLs;
    }
}