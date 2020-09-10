using System;

namespace EarthFusion
{

    public class ReportTag
    {
        public ReportTag(){}

        public ReportTag(int id,String time)
        {
            reportId=id;
            date=time;
        }
        public int reportId { get; set; }
        public String date { get; set; }

    }
}