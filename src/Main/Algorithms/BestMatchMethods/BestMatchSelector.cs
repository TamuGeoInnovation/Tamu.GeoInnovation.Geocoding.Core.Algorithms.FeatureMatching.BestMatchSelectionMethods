using System;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Implementations;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Interfaces;
using USC.GISResearchLab.Geocoding.Core.Configurations;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace Tamu.GeoInnovation.Geocoding.Core.Algorithms.FeatureMatching.BestMatchSelectionMethods.Algorithms.BestMatchMethods
{
    public class BestMatchSelector
    {


        public static IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet, GeocoderConfiguration configuration)
        {
            FeatureMatchingSelectionMethod featureMatchingHierarchy = configuration.OutputHierarchyConfiguration.GetFeatureMatchingSelectionMethod();
            return GetBestMatch(geocodeResultSet, featureMatchingHierarchy);
        }

        public static IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet, FeatureMatchingSelectionMethod featureMatchingHierarchy)
        {
            return GetBestMatch(geocodeResultSet, featureMatchingHierarchy, 100, AreaUnitType.SquareMeters);
        }

        public static IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet, FeatureMatchingSelectionMethod featureMatchingHierarchy, double gridSize, AreaUnitType gridUnit)
        {
            IGeocode ret = null;
            IBestMatchMethod bestMatchMethod = null;

            switch (featureMatchingHierarchy)
            {
                case FeatureMatchingSelectionMethod.FeatureClassBased:
                    bestMatchMethod = new FeatureHierarchyBestMatchMethod();
                    break;
                case FeatureMatchingSelectionMethod.UncertaintyMultiFeatureGraviational:
                    bestMatchMethod = new UncertaintyHierarchyMultiFeatureGravitationalBestMatchMethod(gridSize, gridUnit);
                    break;
                case FeatureMatchingSelectionMethod.UncertaintyMultiFeatureTopological:
                    bestMatchMethod = new UncertaintyHierarchyMultiFeatureTopologicalBestMatchMethod(gridSize, gridUnit);
                    break;
                case FeatureMatchingSelectionMethod.UncertaintySingleFeatureArea:
                    bestMatchMethod = new UncertaintyHierarchySingleFeatureAreaBestMatchMethod();
                    break;
                default:
                    throw new Exception("Unexpected or unimplmented best match method: " + featureMatchingHierarchy);
            }

            if (bestMatchMethod != null)
            {
                ret = bestMatchMethod.GetBestMatch(geocodeResultSet);
            }

            return ret;
        }
    }
}
