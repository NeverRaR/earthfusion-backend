using System;

namespace EarthFusion
{
    public class Session
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string username { get; set; }
        public string sessionId { get; set; }
    }
}