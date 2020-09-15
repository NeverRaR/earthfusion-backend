using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class RegionList
    {
        public RegionList()
        {
            names = new List<String>();
        }
        public int offset {set;get;}
        public List<String> names {set;get;}
    }
}