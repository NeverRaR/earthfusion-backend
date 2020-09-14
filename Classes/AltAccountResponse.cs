using System;

namespace EarthFusion
{
    public class AltAccountResponse
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public AltAccountResult Result { get; set; }
    }
}