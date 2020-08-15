using System;

namespace EarthFusion
{
    // This class represents user's information
    public class UserInformationHttp
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public UserInformation userInformation { get; set; }
    }
}