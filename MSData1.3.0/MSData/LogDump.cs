using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace MSData
{
    public class LogDump
    {
        public static string LogContentString;

        public static void Log(string logMessage)
        {
            // write logMessage onto the log file with handle of w
            LogContentString += string.Format("<br>{0} {1}", Utc2Est(DateTime.UtcNow).ToLongTimeString(),
                Utc2Est(DateTime.UtcNow).ToLongDateString());
            LogContentString += string.Format(" :: {0}", logMessage);
        }
        public static void clearLog()
        {
            LogContentString = "";
        }
        public static DateTime Utc2Est(DateTime timeUtc)
        {
            TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime estTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, estZone);
            return estTime;

        }
    }
}