using System;
using Utils;

namespace Test
{
    public class PasswordScoreResult
    {
        public DateTime Date { get; set; }
        public int StatusCode { get; set; }
        public string Tester { get; set; }
        public string Message { get; set; }
        public PasswordScore Score { get; set; }
    }
}