using System;

namespace EarthFusion
{

    public class BusStation
    {
        public BusStation(){}

        public BusStation(int seq,String name,string loc)
        {
            sequence=seq;
            statsionName=name;
            wkt=loc;
        }
        public int sequence { get; set; }
        public String statsionName { get; set; }
        public String wkt{get;set;}

    }
}