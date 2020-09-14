using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class BusStationList
    {
        public BusStationList()
        {
            stations = new List<BusStation>();
        }
        public int num{set;get;}
        public List<BusStation> stations {set;get;}
    }
}