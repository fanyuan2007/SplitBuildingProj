using NetTopologySuite.Features;
using NetTopologySuite.Operation.Union;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplitBuildingLimits
{
    public static class GeometryOperations<TPolygon> where TPolygon : IFeature
    {
        /// <summary>
        /// Validate the input polygon
        /// </summary>
        /// <param name="polygon">The input polygon for validating</param>
        /// <returns>True if the polygon is valid, otherwise false</returns>
        public static bool IsValidPolygon(IFeature polygon)
        {
            if (polygon == null)
            {
                return false;
            }

            if (polygon.Geometry == null ||
                polygon.BoundingBox == null)
            {
                return false;
            }

            if (polygon.Geometry.IsEmpty || 
                !polygon.Geometry.IsValid)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if there is any overlapped polygons exists in the input polygon list
        /// </summary>
        /// <param name="polygons">Input polygon list</param>
        /// <returns>True if there is overlapped polygons, otherwise false</returns>
        /// <exception cref="Exception"></exception>
        public static bool ArePolygonsHaveOverlaps(List<IFeature> polygons)
        {
            var geometries = polygons.Select(poly => poly.Geometry).ToList();
            try
            {
                for (int i = 0; i < geometries.Count; i++)
                {
                    var currentGeom = geometries[i];
                    for (int j = i + 1; j < geometries.Count; j++)
                    {
                        if (currentGeom.Overlaps(geometries[j]))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // May log the operation failing error message somewhere first
                throw new Exception("Error: Failed on polygon overlap validation operation.");
            }
            
            return false;
        }

        public static List<IFeature> MergePolygonsWithOverlaps(List<IFeature> polygons)
        {
            var mergedPolygons = new List<IFeature>();

            int i = 0;
            while (i < polygons.Count)
            {
                var currentPolygon = polygons[i];
                var overlappedPolygons = new List<IFeature> { currentPolygon };

                for (var j = i + 1; j < polygons.Count; j++)
                {
                    if (currentPolygon.Geometry.Intersects(polygons[j].Geometry))
                    {
                        if (j != i + 1)
                        {
                            var temp = polygons[j];
                            polygons[j] = polygons[i + 1];
                            polygons[i + 1] = temp;
                        }
                        overlappedPolygons.Add(polygons[j]);
                        i++;
                    }
                }

                if (overlappedPolygons.Count > 1)
                {
                    var unionOp = new UnaryUnionOp(overlappedPolygons.Select(poly => poly.Geometry));
                    var mergedGeom = unionOp.Union();
                    var mergedPolygon = new Feature(mergedGeom, new AttributesTable());

                    mergedPolygons.Add(mergedPolygon);
                }
                else
                {
                    mergedPolygons.Add(currentPolygon);
                }

                i++;
            }

            return mergedPolygons;
        }
    }
}
