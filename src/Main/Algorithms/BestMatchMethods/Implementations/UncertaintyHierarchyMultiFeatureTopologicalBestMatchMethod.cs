using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Common.Core.Physics.CenterOfMassCalculations;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.AbstractClasses;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Implementations
{
    public class UncertaintyHierarchyMultiFeatureTopologicalBestMatchMethod : AbstractBestMatchMethod
    {

        public UncertaintyHierarchyMultiFeatureTopologicalBestMatchMethod()
            : this(100, AreaUnitType.SquareMeters)
        { }

        public UncertaintyHierarchyMultiFeatureTopologicalBestMatchMethod(double gridSize, AreaUnitType gridSizeUnit)
            : base(gridSize, gridSizeUnit)
        {
            FeatureMatchingHierarchy = FeatureMatchingSelectionMethod.UncertaintyMultiFeatureTopological;
        }


        public override IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet)
        {
            IGeocode ret = null;
            IGeocode bestUncertainty = new UncertaintyHierarchySingleFeatureAreaBestMatchMethod().GetBestMatch(geocodeResultSet);

            string topologicalRelation = "";
            string revertReason = "";

            try
            {
                double totalUncertainty = 0;

                List<double[]> xymList = new List<double[]>();
                double[] xym = null;

                if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
                {
                    // topological methods currently only works with two reference features
                    if (geocodeResultSet.GeocodeCollection.GetValidGeocodeCount() == 2)
                    {

                        List<IGeocode> validGeocodes = geocodeResultSet.GeocodeCollection.GetValidGeocodes();

                        // set one of the two geocodes to be the smaller
                        IGeocode geocodeSmaller = null;
                        IGeocode geocodeLarger = null;
                        SqlGeometry geometrySmaller = null;
                        SqlGeometry geometryLarger = null;
                        SqlGeography geographySmaller = null;
                        SqlGeography geographyLarger = null;
                        double areaSmaller = 0;
                        double areaLarger = 0;

                        // first get the individual uncertainty for each reference feature 
                        if (validGeocodes[0].GeocodedError.ErrorBounds < validGeocodes[1].GeocodedError.ErrorBounds)
                        {
                            geocodeSmaller = validGeocodes[0];
                            geocodeLarger = validGeocodes[1];
                        }
                        else
                        {
                            geocodeSmaller = validGeocodes[1];
                            geocodeLarger = validGeocodes[0];
                        }

                        if (geocodeSmaller.Valid && geocodeLarger.Valid)
                        {

                            geometrySmaller = geocodeSmaller.MatchedFeature.MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeometry;
                            geometryLarger = geocodeLarger.MatchedFeature.MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeometry;
                            geographySmaller = geocodeSmaller.MatchedFeature.MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeography;
                            geographyLarger = geocodeLarger.MatchedFeature.MatchedReferenceFeature.StreetAddressableGeographicFeature.Geometry.SqlGeography;

                            if (geometrySmaller != null && geometryLarger != null && geographySmaller != null && geographyLarger != null)
                            {
                                if (!geometrySmaller.IsNull && !geometryLarger.IsNull && !geographySmaller.IsNull && !geographyLarger.IsNull)
                                {
                                    areaSmaller = geographySmaller.STArea().Value;
                                    areaLarger = geographyLarger.STArea().Value;


                                    // set the uncertainty values for the smaller
                                    double numberOfGridCellsSmaller = (geocodeSmaller.GeocodedError.ErrorBounds / GridSize);
                                    double probabilityOfRandomPointSmaller = 1 / numberOfGridCellsSmaller;
                                    xym = new double[] { geocodeSmaller.Longitude, geocodeSmaller.Latitude, probabilityOfRandomPointSmaller };
                                    xymList.Add(xym);


                                    // set the uncertainty values of the larger
                                    double numberOfGridCellsLarger = (geocodeLarger.GeocodedError.ErrorBounds / GridSize);
                                    double probabilityOfRandomPointLarger = 1 / numberOfGridCellsLarger;
                                    xym = new double[] { geocodeLarger.Longitude, geocodeLarger.Latitude, probabilityOfRandomPointLarger };
                                    xymList.Add(xym);

                                    totalUncertainty += probabilityOfRandomPointSmaller;
                                    totalUncertainty += probabilityOfRandomPointLarger;


                                    // get the area of the union feature
                                    //SqlGeometry unionFeature = geometrySmaller.STUnion(geometryLarger);

                                    SqlGeography unionFeatureGeography = geographySmaller.STUnion(geographyLarger);
                                    double unionArea = unionFeatureGeography.STArea().Value;
                                    double unionNumberOfGridCells = (unionArea / GridSize);
                                    double unionProbabilityOfRandomPoint = 1 / unionNumberOfGridCells;

                                    // four topological conditions to deal with - disjoint, touch, overlaps, contains

                                    bool contains = geometryLarger.STContains(geometrySmaller).IsTrue;
                                    bool intersects = geometrySmaller.STIntersects(geometryLarger).IsTrue;
                                    bool disjoint = geometryLarger.STDisjoint(geometrySmaller).IsTrue;
                                    bool touches = geometryLarger.STTouches(geometrySmaller).IsTrue;

                                    if (contains)
                                    {
                                        if (!String.IsNullOrEmpty(topologicalRelation))
                                        {
                                            topologicalRelation += ";";
                                        }
                                        topologicalRelation += "Contains";
                                    }

                                    if (intersects)
                                    {
                                        if (!contains && !touches)
                                        {
                                            if (!String.IsNullOrEmpty(topologicalRelation))
                                            {
                                                topologicalRelation += ";";
                                            }
                                            topologicalRelation += "Intersects";
                                        }
                                    }

                                    if (disjoint)
                                    {
                                        if (!String.IsNullOrEmpty(topologicalRelation))
                                        {
                                            topologicalRelation += ";";
                                        }
                                        topologicalRelation += "Disjoint";
                                    }

                                    if (touches)
                                    {
                                        if (!String.IsNullOrEmpty(topologicalRelation))
                                        {
                                            topologicalRelation += ";";
                                        }
                                        topologicalRelation += "Touches";
                                    }

                                    if ((contains || intersects) && (!touches && !disjoint)) // on contain or overlap, take the centroid of the overlapped region
                                    {
                                        try
                                        {
                                            // get the area of the intersection feature - instect the smaller with the larger to ensure that the intersection is the area of the smaller when contained
                                            SqlGeometry intersectionFeatureGeometry = geometrySmaller.STIntersection(geometryLarger);
                                            SqlGeography intersectionFeatureGeography = geographySmaller.STIntersection(geographyLarger);
                                            double intersectionArea = intersectionFeatureGeography.STArea().Value;
                                            double intersectionNumberOfGridCells = (intersectionArea / GridSize);
                                            double intersectionProbabilityOfRandomPoint = 1 / intersectionNumberOfGridCells;

                                            //KMLDocument kmlDoc = new KMLDocument();
                                            //string styleName = "style";
                                            //kmlDoc.AddStyle(styleName, System.Drawing.Color.Blue, 4.0, System.Drawing.Color.Red, true, true);
                                            //kmlDoc.AddSqlGeography(intersectionFeatureGeography, "intersection", styleName, null);
                                            //string kml = kmlDoc.AsString();

                                            double intersectionWeight = totalUncertainty - unionProbabilityOfRandomPoint;


                                            SqlGeometry intersectionFeatureGeometryCentroid = intersectionFeatureGeometry.STCentroid();
                                            if (intersectionFeatureGeometryCentroid != null && !intersectionFeatureGeometryCentroid.IsNull)
                                            {

                                                double intersectionX = intersectionFeatureGeometry.STCentroid().STX.Value;
                                                double intersectionY = intersectionFeatureGeometry.STCentroid().STY.Value;

                                                double[] xymIntersection = new double[] { intersectionX, intersectionY, intersectionWeight };
                                                xymList.Add(xymIntersection);

                                            }
                                            else // if the intersection centroid is null, try to make a convex hull out of it and then get the centroid
                                            {
                                                SqlGeometry intersectionFeatureGeometryConvexHull = intersectionFeatureGeometry.STConvexHull();
                                                if (intersectionFeatureGeometryConvexHull != null && !intersectionFeatureGeometryConvexHull.IsNull)
                                                {
                                                    SqlGeometry intersectionFeatureGeometryConvexHullCentroid = intersectionFeatureGeometryConvexHull.STCentroid();
                                                    if (intersectionFeatureGeometryConvexHullCentroid != null && !intersectionFeatureGeometryConvexHullCentroid.IsNull)
                                                    {


                                                        double intersectionX = intersectionFeatureGeometryConvexHullCentroid.STX.Value;
                                                        double intersectionY = intersectionFeatureGeometryConvexHullCentroid.STY.Value;

                                                        double[] xymIntersection = new double[] { intersectionX, intersectionY, intersectionWeight };
                                                        xymList.Add(xymIntersection);

                                                    }
                                                    else
                                                    {
                                                        revertReason += "Intersection convex hull centroid is null";
                                                    }
                                                }
                                                else
                                                {
                                                    revertReason += "Intersection convex hull is null";
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception("Error occured determining topological intersection area: " + ex.Message, ex);
                                        }

                                    }
                                    else if (disjoint || touches) // on disjoint or touches, take the point on the boundary of the smaller that is the closest to the larger
                                    {
                                        try
                                        {
                                            // get the distance between them
                                            double distance = geometryLarger.STDistance(geometrySmaller).Value;

                                            // buffer by the larger by that amount, which should make it intersect with the smaller
                                            //SqlGeometry geometryLargerBuffered = geometryLarger.STBuffer(distance);
                                            SqlGeometry geometryLargerBuffered = null;

                                            if (geometryLargerBuffered != null && !geometryLargerBuffered.IsNull)
                                            {

                                                // the intersection should be the point on the smaller that is the closest to the largest
                                                SqlGeometry geometrySmallerIntersected = geometrySmaller.STIntersection(geometryLargerBuffered);

                                                if (geometrySmallerIntersected != null && !geometrySmallerIntersected.IsNull)
                                                {
                                                    SqlGeometry geometrySmallerIntersectedPoint = geometrySmallerIntersected.STStartPoint();

                                                    if (geometrySmallerIntersectedPoint != null && !geometrySmallerIntersectedPoint.IsNull)
                                                    {

                                                        // this is from http://social.msdn.microsoft.com/Forums/en-SG/sqlspatial/thread/cb094fb8-07ba-4219-8d3d-572874c271b5
                                                        double intersectionWeight = totalUncertainty - unionProbabilityOfRandomPoint;
                                                        double intersectionX = geometrySmallerIntersected.STX.Value;
                                                        double intersectionY = geometrySmallerIntersected.STY.Value;

                                                        double[] xymIntersection = new double[] { intersectionX, intersectionY, intersectionWeight };
                                                        xymList.Add(xymIntersection);
                                                    }
                                                    else
                                                    {
                                                        revertReason += "Disjoint/Touches bufferred intersected boundary first point is null";
                                                    }
                                                }
                                                else
                                                {
                                                    revertReason += "Disjoint/Touches buffered intersection is null";
                                                }
                                            }
                                            else
                                            {
                                                // revertReason += "Disjoint/Touches buffered is null";
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            throw new Exception("Error occured determining disjoint point on boundary: " + ex.Message, ex);
                                        }

                                    }
                                    else
                                    {
                                        revertReason += "Topological relation is not any of the conditions being tested for";
                                    }


                                    if (xymList.Count == 3)
                                    {
                                        double[] centerOfMass = CenterOfMassCalculator.GetCenterOfMass(xymList);

                                        ret = bestUncertainty;
                                        ((Point)ret.Geometry).X = centerOfMass[0];
                                        ((Point)ret.Geometry).Y = centerOfMass[1];

                                        ret.FM_SelectionMethod = FeatureMatchingHierarchy;

                                        ret.FM_SelectionNotes = topologicalRelation;
                                    }
                                    else
                                    {
                                        revertReason += "Topological centroid could not be computed - List of points is of size: " + xymList.Count;
                                    }
                                }
                                else
                                {
                                    revertReason += "Both geometries are not not null";
                                }
                            }
                            else
                            {
                                revertReason += "Both geometries are not not null";
                            }
                        }
                        else
                        {
                            revertReason += "Both features are not valid";
                        }
                    }
                    else
                    {
                        revertReason += "Not two features";
                    }
                }
            }
            catch (Exception ex)
            {

                revertReason += ex.Message;


                if (ret == null)
                {
                    ret = bestUncertainty;
                }

                ret.FM_SelectionNotes += topologicalRelation + " - " + revertReason + " - Reverted to " + FeatureMatchingSelectionMethod.UncertaintySingleFeatureArea;

                ret.Exception = ex;
                ret.ExceptionOccurred = true;
                ret.ErrorMessage = ex.Message;


            }

            if (ret == null)
            {
                ret = bestUncertainty;
                ret.FM_SelectionNotes += topologicalRelation + " - " + revertReason + " - Reverted to " + FeatureMatchingSelectionMethod.UncertaintySingleFeatureArea;
            }

            return ret;
        }
    }
}
