using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Common.Geographics.Units;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Interfaces;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.AbstractClasses
{
    public abstract class AbstractBestMatchMethod : IBestMatchMethod
    {

        #region Properties

        public FeatureMatchingSelectionMethod FeatureMatchingHierarchy { get; set; }

        public double GridSize { get; set; }
        public AreaUnitType GridSizeUnit { get; set; }

        #endregion


        public AbstractBestMatchMethod()
             : this(100, AreaUnitType.SquareMeters)
        { }

        public AbstractBestMatchMethod(double gridSize, AreaUnitType gridSizeUnit)
        {
            GridSize = gridSize;
            GridSizeUnit = gridSizeUnit;
        }

        public abstract IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet);
    }
}
