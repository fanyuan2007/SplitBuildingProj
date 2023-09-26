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
    public static void SplitBuildingLimits(List<TPolygon> buildingLimits, List<TPolygon> heightPlateaus)
    {
        Console.WriteLine("Splitting building limits according to height plateaus");
    }
}