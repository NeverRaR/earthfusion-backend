using System;
using System.Collections.Generic;

namespace EarthFusion
{
    public class ShopSearchResponse
    {
        public ShopSearchResponse()
        {
            Contents = new List<ShopSearchResult>();
        }
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<ShopSearchResult> Contents { get; set; }
    }
}