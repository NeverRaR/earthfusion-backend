using System;

namespace EarthFusion
{
    // This class represents user's information
    public class UserInformation
    {
        public int userId { get; set; }
        public string userName { get; set; }
        public string emailAddress { get; set; }
        public string userPasswordHashed { get; set; }
        public string accountStatus { get; set; }
        public string role { get; set; }
    }
}