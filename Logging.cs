using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace Utils
{
    class Logging
    {
        public static void Info(string caller, string message)
        {
            DateTime now = DateTime.Now;
            string timeString = now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            Console.WriteLine("[Info]" + "[" + timeString + "] " + caller + ": " + message);
        }
        public static void Warning(string caller, string message)
        {
            DateTime now = DateTime.Now;
            string timeString = now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[Warning]" + "[" + timeString + "] " + caller + ": " + message);
            Console.ResetColor();
        }
    }
}