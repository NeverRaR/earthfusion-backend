using System;
using System.Collections.Generic;

namespace EarthFusion
{
    public class UserInformationQueryResponse
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public List<UserInformation> Contents { get; set; }
    }
}