using System;
using System.Collections.Generic;
namespace EarthFusion
{

    public class UserIDWithAllReport
    {
        public UserIDWithAllReport()
        {
            allReports = new List<ReportTag>();
        }
        public int userId { get; set; }

        public List<ReportTag> allReports { get; set; }
    }
}