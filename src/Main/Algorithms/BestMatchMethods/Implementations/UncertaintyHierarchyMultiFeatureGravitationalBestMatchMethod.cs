using System.Collections.Generic;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Common.Core.Physics.CenterOfMassCalculations;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.AbstractClasses;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Implementations
{
    public class UncertaintyHierarchyMultiFeatureGravitationalBestMatchMethod : AbstractBestMatchMethod
    {

        public UncertaintyHierarchyMultiFeatureGravitationalBestMatchMethod()
            : this(100, AreaUnitType.SquareMeters)
        { }

        public UncertaintyHierarchyMultiFeatureGravitationalBestMatchMethod(double gridSize, AreaUnitType gridSizeUnit)
            :base(gridSize, gridSizeUnit)
        {
            FeatureMatchingHierarchy = FeatureMatchingSelectionMethod.UncertaintyMultiFeatureGraviational;
        }


        public override IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet)
        {
            IGeocode ret = null;
            IGeocode bestUncertainty = new UncertaintyHierarchySingleFeatureAreaBestMatchMethod().GetBestMatch(geocodeResultSet);

            string revertReason = "";

            List<double[]> xymList = new List<double[]>();

            if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
            {
                foreach (IGeocode geocode in geocodeResultSet.GeocodeCollection.Geocodes)
                {
                    if (geocode.Valid == true)
                    {
                        
                        double numberOfGridCells = (geocode.GeocodedError.ErrorBounds / GridSize);
                        double probabilityOfRandomPoint = 1 / numberOfGridCells;

                        double[] xym = new double[] { geocode.Longitude, geocode.Latitude, probabilityOfRandomPoint };
                        xymList.Add(xym);
                    }
                }

                if (xymList.Count == 2)
                {
                    double[] centerOfMass = CenterOfMassCalculator.GetCenterOfMass(xymList);
                    
                    ret = bestUncertainty;
                    ((Point)ret.Geometry).X = centerOfMass[0];
                    ((Point)ret.Geometry).Y = centerOfMass[1];

                    ret.FM_SelectionMethod = FeatureMatchingHierarchy;
                }
                else
                {
                    revertReason += "Topological centroid could not be computed - List of points is of size: " + xymList.Count;
                }
            }

            if (ret == null)
            {
                ret = bestUncertainty;
                ret.FM_SelectionNotes += revertReason + " - Reverted to " + FeatureMatchingSelectionMethod.UncertaintySingleFeatureArea;
            }

            return ret;
        }
    }
}
