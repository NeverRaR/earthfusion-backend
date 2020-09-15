using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class BusStationTagList
    {
        public BusStationTagList()
        {
            tags = new List<BusStationTag>();
        }
        public int offset {set;get;}
        public List<BusStationTag> tags {set;get;}
    }
}