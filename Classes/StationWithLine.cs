using System;
using System.Collections.Generic;
namespace EarthFusion
{

    public class StationWithLine
    {
        public StationWithLine()
        {
            allLines = new List<String>();
        }
        public String stationName { get; set; }

        public int sequence{get;set;}

        public List<String> allLines { get; set; }
    }
}