using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class ShopTagList
    {
        public ShopTagList()
        {
            tags = new List<ShopTag>();
        }
        public int offset {set;get;}
        public List<ShopTag> tags {set;get;}
    }
}