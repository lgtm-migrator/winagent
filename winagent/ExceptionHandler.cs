using System;
using System.Diagnostics;
using System.Text;

namespace Winagent.ExceptionHandling
{
    public static class ExceptionHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="exception"></param>
        public static void HandleError(string errorMessage, int errorCode, Exception exception)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("Error: {0}", errorCode));
                Console.Error.WriteLine(errorMessage);
                Console.Error.WriteLine(exception.Message);

#if DEBUG
                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(exception.ToString());
#endif
            }
            else
            {
                StringBuilder message = new StringBuilder(errorMessage);
                message.Append(Environment.NewLine);
                message.Append(exception);
                message.Append(Environment.NewLine);
                message.Append(exception.Message);

                CreateLog(message.ToString(), EventLogEntryType.Error, errorCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        public static void HandleError(string errorMessage, int errorCode)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("Error: {0}", errorCode));
                Console.Error.WriteLine(errorMessage);

#if DEBUG
                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(exception.ToString());
#endif
            }
            else
            {
                StringBuilder message = new StringBuilder(errorMessage);

                CreateLog(message.ToString(), EventLogEntryType.Error, errorCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoMessage"></param>
        /// <param name="infoCode"></param>
        /// <param name="exception"></param>
        public static void HandleInformation(string infoMessage, int infoCode, Exception exception)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("Warning: {0}", infoCode));
                Console.Error.WriteLine(infoMessage);
                Console.Error.WriteLine(exception.Message);
#if DEBUG
                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(exception.ToString());
#endif
            }
            else
            {
                StringBuilder message = new StringBuilder(infoMessage);
                message.Append(Environment.NewLine);
                message.Append(exception);
                message.Append(Environment.NewLine);
                message.Append(exception.Message);

                CreateLog(message.ToString(), EventLogEntryType.Warning, infoCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="infoMessage"></param>
        /// <param name="infoCode"></param>
        public static void HandleInformation(string infoMessage, int infoCode)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("Warning: {0}", infoCode));
                Console.Error.WriteLine(infoMessage);
#if DEBUG
                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(exception.ToString());
#endif
            }
            else
            {
                StringBuilder message = new StringBuilder(infoMessage);

                CreateLog(message.ToString(), EventLogEntryType.Warning, infoCode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="entryType"></param>
        /// <param name="code"></param>
        private static void CreateLog(string message, EventLogEntryType entryType, int code)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "Winagent";
                eventLog.WriteEntry(message, entryType, code, 1);
            }
        }
    }
}
