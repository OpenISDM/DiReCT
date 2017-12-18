using System;
using System.IO;

namespace DiReCT.Logger
{
    public enum Log
    {
        GeneralEvent = 0,
        ErrorEvent
    }

    public static class Logger
    {
        private static string LogsPath = AppDomain.CurrentDomain.BaseDirectory
            + "\\Logs\\";
        private static object GeneralEventLogLock = new object();
        private static object ErrorEventLock = new object();

        /// <summary>
        /// Write message to log file.
        /// </summary>
        /// <param name="log"></param>
        /// <param name="Message"></param>
        public static void Write(this Log log, string Message)
        {
            if (log == Log.GeneralEvent)
            {
                //Prevent multiple threads simultaneously writing
                lock (GeneralEventLogLock)
                {
                    //Check if the folder exists
                    if (!Directory.Exists(LogsPath))
                        Directory.CreateDirectory(LogsPath);

                    File.AppendAllText(LogsPath + "GeneralLog.txt",
                        DateTime.Now.ToString() + ": " + Message + "\r\n");
                }
            }
            else if (log == Log.ErrorEvent)
            {
                lock (ErrorEventLock)
                {
                    if (!Directory.Exists(LogsPath))
                        Directory.CreateDirectory(LogsPath);

                    File.AppendAllText(LogsPath + "ErrorLog.txt",
                        DateTime.Now.ToString() + ": " + Message + "\r\n");
                }
            }
        }
    }
}