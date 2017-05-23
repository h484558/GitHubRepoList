using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace GitHubRepoList.App_Start
{
    public enum LoggerLevel
    {
        ERROR,
        WARN,
        INFO
    }

    public class Logger
    {
        public static void WriteLog(Exception ex, LoggerLevel level = LoggerLevel.ERROR)
        {
            using (var logWriter = new StreamWriter(HttpContext.Current.Server.MapPath("~") + "\\log.txt", true))
            {
                logWriter.WriteLine("[{0}] - [{1}] - {2}\r\n{3}", DateTime.Now, level, ex.Message, ex.StackTrace);
            }
        }

        public static void WriteLog(string message, LoggerLevel level = LoggerLevel.ERROR)
        {
            using (var logWriter = new StreamWriter(HttpContext.Current.Server.MapPath("~") + "\\log.txt", true))
            {
                logWriter.WriteLine("[{0}] - [{1}] - {2}", DateTime.Now, level, message);
            }
        }
    }
}