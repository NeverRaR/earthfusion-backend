using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Utils
{
    class Logging
    {
        private static string appStartTimeString = GetDateTimeString();
        private static string logPath = "./logs/";
        private static string logNameExtension = ".log";
        public static string GetDateTimeString()
        {
            string sDate = DateTime.Now.ToString();
            DateTime datevalue = (Convert.ToDateTime(sDate.ToString()));
            string dayString = datevalue.Day.ToString();
            string monthString = datevalue.Month.ToString();
            string yearString = datevalue.Year.ToString();
            string hourString = datevalue.Hour.ToString();
            string minuteString = datevalue.Minute.ToString();
            string secondString = datevalue.Second.ToString();
            string result = yearString + "_" + monthString + "_" + dayString + "_" + hourString + "_" + minuteString + "_" + secondString; 
            return result;
        }

        public static void Info(string caller, string message)
        {
            DateTime now = DateTime.Now;
            string timeString = now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            string realMessage = "[Info]   " + "[" + timeString + "] " + caller + ": " + message;
            Console.WriteLine(realMessage);
            using (StreamWriter writer = File.AppendText(logPath + appStartTimeString + logNameExtension))
            {
                writer.WriteLine(realMessage);
            }
        }
        public static void Warning(string caller, string message)
        {
            DateTime now = DateTime.Now;
            string timeString = now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            Console.ForegroundColor = ConsoleColor.Yellow;
            string realMessage = "[Warning]" + "[" + timeString + "] " + caller + ": " + message;
            Console.WriteLine(realMessage);
            using (StreamWriter writer = File.AppendText(logPath + appStartTimeString + logNameExtension))
            {
                writer.WriteLine(realMessage);
            }
            Console.ResetColor();
        }
    }
}