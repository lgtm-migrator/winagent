using System;
using System.Diagnostics;
using System.Text;

namespace Winagent.MessageHandling
{
    public static class MessageHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleError(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Error, innerException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleError(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleWarning(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Warning, innerException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleWarning(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Warning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleInformation(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Information, innerException);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleInformation(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Information);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="exception"></param>
        private static void HandleMessage(string message, int eventId, EventLogEntryType type, Exception innerException)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("EventID: {0}", eventId));
                Console.Error.WriteLine(message);
                Console.Error.WriteLine(innerException.Message);

#if DEBUG
                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(exception.ToString());
#endif
            }
            else
            {
                StringBuilder logMessage = new StringBuilder(message);
                logMessage.Append(Environment.NewLine);
                logMessage.Append(innerException.Message);
                logMessage.Append(Environment.NewLine);
                logMessage.Append(Environment.NewLine);
                logMessage.Append("----------");
                logMessage.Append(Environment.NewLine);
                logMessage.Append("DEBUG INFO");
                logMessage.Append(innerException);

                CreateLog(message.ToString(), type, eventId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        private static void HandleMessage(string errorMessage, int eventId, EventLogEntryType type)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("EventID: {0}", eventId));
                Console.Error.WriteLine(errorMessage);
            }
            else
            {
                StringBuilder message = new StringBuilder(errorMessage);
                message.Append(Environment.NewLine);

                CreateLog(message.ToString(), type, eventId);
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
