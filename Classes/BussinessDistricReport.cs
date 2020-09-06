using System;

namespace EarthFusion
{
    
    public class BussinessDistrictReport
    {
        public int userId { get; set; }
        public int reportId { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public DateTime date { get; set; }
        public int trafficAccessibility  { get; set; }
        public int  competitiveness  { get; set; }
    }
}