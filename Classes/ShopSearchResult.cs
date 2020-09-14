using System;

namespace EarthFusion
{
    public class ShopSearchResult
    {
        // name
        public string ShopName { get; set; }
        
        // address
        public string ShopAddress { get; set; }
        
        // lat
        public double Latitude { get; set; }
        
        // lon
        public double Longitude { get; set; }
        
        // area
        public string District { get; set; }
        
        // detail
        // 457 approved that this should be removed in order to get unique result 
        // public string ShopClass { get; set; }
    }
}