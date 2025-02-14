using System;
using System.Collections.Generic; 
namespace EarthFusion
{
    
    public class BussinessVitalityReport
    {
        public int userId { get; set; }
        public int reportId { get; set; }
        public double ulLongitude { get; set; }
        public double ulLatitude { get; set; }
        public double ldLongitude { get; set; }
        public double ldLatitude { get; set; }
        public DateTime date { get; set; }
        public int rowNum { get; set; }
        public int colNum { get; set; }
        public String trafficAccessibility  { get; set; }
        public String competitiveness  { get; set; }
    }
}