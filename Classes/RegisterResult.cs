using System;

namespace EarthFusion
{
    // This class represents a generic test result, which can be use under many circumstances.
    public class RegisterResult
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public bool Result { get; set; }
    }
}