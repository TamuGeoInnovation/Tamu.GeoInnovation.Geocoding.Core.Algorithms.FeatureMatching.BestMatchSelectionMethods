using System.Collections.Generic;
using System.Linq;
using USC.GISResearchLab.Common.Core.Geocoders.FeatureMatching;
using USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.AbstractClasses;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.Algorithms.BestMatchMethods.Implementations
{
    public class FeatureHierarchyBestMatchMethod : AbstractBestMatchMethod
    {

        public FeatureHierarchyBestMatchMethod()
        {
            FeatureMatchingHierarchy = FeatureMatchingSelectionMethod.FeatureClassBased;
        }

        //PAYTON:MULTITHREADING had to adjust this after implementing MT. Results no longer in featurehierarchy order due to how threads return
        //PAYTON:MULTITHREADING new code/method (all I did here is sort the geocodes by NAACCRGISCoordinate quality prior to getting the best match
        public override IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet)
        {
            IGeocode ret = null;

            List<IGeocode> tempList = new List<IGeocode>();
            if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
            {
                List<IGeocode> geocodes = geocodeResultSet.GeocodeCollection.GetValidGeocodes();

                //This is nothing but a placeholder. It's an ok sort but I think we need to do better
                tempList = geocodes.OrderBy(d => d.NAACCRGISCoordinateQualityCode).ToList();

            }

            if (geocodeResultSet != null)
            {
                if (geocodeResultSet.GeocodeCollection != null)
                {
                    if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
                    {
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            IGeocode geocode = tempList[i];
                            if (geocode != null)
                            {
                                if (geocode.Valid == true && geocode.GeocodedError.ErrorBounds >= 0)
                                {
                                    ret = geocode;
                                    break;
                                }
                            }
                        }

                        // if the ret is null, none of the IGeocodes were valid - return the first one that was attempted
                        if (ret == null)
                        {
                            for (int i = 0; i < tempList.Count; i++)
                            {
                                IGeocode geocode = tempList[i];
                                if (geocode != null)
                                {
                                    if (geocode.Attempted)
                                    {
                                        ret = geocode;
                                        break;
                                    }
                                }
                            }
                        }

                        // if the ret is still null, none of the IGeocodes were even attempted - return the first one
                        if (ret == null)
                        {
                            IGeocode geocode = tempList[0];
                            if (geocode != null)
                            {
                                ret = tempList[0];
                            }
                        }
                    }
                }
                else
                {
                    ret = new Geocode(2.94);
                }


                if (ret == null)
                {
                    ret = new Geocode(2.94);
                }
            }


            ret.FM_SelectionMethod = FeatureMatchingHierarchy;

            return ret;
        }

        //PAYTON:MULTITHREADING old code/method
        //public override IGeocode GetBestMatch(GeocodeResultSet geocodeResultSet)
        //{
        //    IGeocode ret = null;

        //    if (geocodeResultSet != null)
        //    {
        //        if (geocodeResultSet.GeocodeCollection != null)
        //        {
        //            if (geocodeResultSet.GeocodeCollection.Geocodes.Count > 0)
        //            {
        //                for (int i = 0; i < geocodeResultSet.GeocodeCollection.Geocodes.Count; i++)
        //                {
        //                    IGeocode geocode = geocodeResultSet.GeocodeCollection.Geocodes[i];
        //                    if (geocode != null)
        //                    {
        //                        if (geocode.Valid == true && geocode.GeocodedError.ErrorBounds >= 0)
        //                        {
        //                            ret = geocode;
        //                            break;
        //                        }
        //                    }
        //                }

        //                // if the ret is null, none of the IGeocodes were valid - return the first one that was attempted
        //                if (ret == null)
        //                {
        //                    for (int i = 0; i < geocodeResultSet.GeocodeCollection.Geocodes.Count; i++)
        //                    {
        //                        IGeocode geocode = geocodeResultSet.GeocodeCollection.Geocodes[i];
        //                        if (geocode != null)
        //                        {
        //                            if (geocode.Attempted)
        //                            {
        //                                ret = geocode;
        //                                break;
        //                            }
        //                        }
        //                    }
        //                }

        //                // if the ret is still null, none of the IGeocodes were even attempted - return the first one
        //                if (ret == null)
        //                {
        //                    IGeocode geocode = geocodeResultSet.GeocodeCollection.Geocodes[0];
        //                    if (geocode != null)
        //                    {
        //                        ret = geocodeResultSet.GeocodeCollection.Geocodes[0];
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            ret = new Geocode(2.94);
        //        }


        //        if (ret == null)
        //        {
        //            ret = new Geocode(2.94);
        //        }
        //    }


        //    ret.FM_SelectionMethod = FeatureMatchingHierarchy;

        //    return ret;
        //}
    }
}
