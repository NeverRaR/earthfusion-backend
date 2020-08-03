using System;


namespace Test
{
    public class GenericTestResult
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Operation { get; set; }
        public string Tester { get; set; }
        public string Message { get; set; }
        public string StringResult { get; set; }
        public int IntResult { get; set; }
        public bool BoolResult { get; set; }
    }
}