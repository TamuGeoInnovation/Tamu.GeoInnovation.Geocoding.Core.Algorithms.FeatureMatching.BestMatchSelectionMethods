using System;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.AbstractClasses;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Implementations
{
    public class UncertaintyHierarchySingleFeatureAreaBestMatchMethod : AbstractBestMatchMethod
    {

        public UncertaintyHierarchySingleFeatureAreaBestMatchMethod()
        {
            FeatureMatchingHierarchy = FeatureMatchingSelectionMethod.UncertaintySingleFeatureArea;
        }


        public override IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet)
        {
            IGeocode ret = null;
            IGeocode bestHierarchy = new FeatureHierarchyBestMatchMethod().GetBestMatch(geocodeResultSet);
            if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
            {
                double leastError = Double.MaxValue;
                foreach (IGeocode geocode in geocodeResultSet.GeocodeCollection.Geocodes)
                {
                    if (geocode.Valid == true)
                    {
                        if (geocode.GeocodedError.ErrorBounds >= 0 && geocode.GeocodedError.ErrorBounds < leastError)
                        {
                            if (String.Compare(bestHierarchy.SourceType, geocode.SourceType, true) != 0) // don't compare the id's on the same geocode to itself
                            {
                                string bestHierarchyId = bestHierarchy.MatchedFeature.PrimaryIdValue;
                                string bestUncertaintyId = geocode.MatchedFeature.PrimaryIdValue;

                                if (String.Compare(bestHierarchyId, bestUncertaintyId, true) == 0) // if the uncertainty and hierarchy found the same feature, go with the hierarchy (choose tiger over USPS tiger/zip)
                                {
                                    // do nothing, will revert to hierarchy
                                }
                                    // TODO check to see if this still works
                                    // removed DG 2015-06-09
                                //else if (bestHierarchy.FM_Result.ReferenceDatasetStatistics.ReferenceSourceQueryResultSet.SawCandidate(bestUncertaintyId)) // if the hierarchy found and rejected the uncertainty, go with the hierarchy (choose tiger over USPS tiger/zip)
                                //{
                                //    // do nothing, will revert to hierarchy
                                //}
                                else
                                {
                                    leastError = geocode.GeocodedError.ErrorBounds;
                                    ret = geocode;
                                }
                            }
                            else
                            {
                                leastError = geocode.GeocodedError.ErrorBounds;
                                ret = geocode;
                            }
                        }
                    }
                }
            }

            if (ret == null)
            {
                ret = bestHierarchy;
                ret.FM_SelectionNotes += "Reverted to " + FeatureMatchingSelectionMethod.FeatureClassBased;
            }
            else
            {
                ret.FM_SelectionMethod = FeatureMatchingHierarchy;
            }

            return ret;
        }
    }
}
