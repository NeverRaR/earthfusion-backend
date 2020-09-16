using System;

namespace EarthFusion
{

    public class BusStationTag
    {
        public BusStationTag(){}

        public BusStationTag(int seq,String name)
        {
            sequence=seq;
            stationName=name;
        }
        public int sequence { get; set; }
        public String stationName { get; set; }

    }
}