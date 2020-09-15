using System;
using System.Collections.Generic;
namespace EarthFusion
{
    public class GDPReport
    {
        public int userId { get; set; }
        public int reportId { get; set; }
        public String date { get; set; }
        public String name { get;set; }
        public int bYear { get; set; }
        public int eYear { get; set; }
        public String arima { get; set; }
        public String holtWinters { get; set; }
        public String holt { get; set; }
    }
}