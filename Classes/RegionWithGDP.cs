using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class RegionWithGDP
    {
        public RegionWithGDP()
        {
            tags = new List<GDPTag>();
        }
        public String name {set;get;}
        public List<GDPTag> tags {set;get;}
    }
}