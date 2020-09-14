using System;
using System.Collections.Generic;
namespace EarthFusion
{

    public class BusRoute
    {
        public BusRoute()
        {
            allStations = new List<BusStationTag>();
        }
        public String lineName { get; set; }

        public List<BusStationTag> allStations { get; set; }
    }
}