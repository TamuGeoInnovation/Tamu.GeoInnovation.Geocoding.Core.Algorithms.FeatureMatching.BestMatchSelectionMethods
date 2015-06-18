using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Interfaces
{
    public interface IBestMatchMethod
    {

        #region Properties

        FeatureMatchingSelectionMethod FeatureMatchingHierarchy { get; set; }

        #endregion


        IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet);

    }
}
