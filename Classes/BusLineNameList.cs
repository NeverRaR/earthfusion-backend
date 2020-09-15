using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class BusLineNameList
    {
        public BusLineNameList()
        {
            names = new List<String>();
        }
        public int offset {set;get;}
        public List<String> names {set;get;}
    }
}