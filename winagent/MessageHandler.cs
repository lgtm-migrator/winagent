using System;
using System.Diagnostics;
using System.Text;

namespace Winagent.MessageHandling
{
    public static class MessageHandler
    {
        /// <summary>
        /// A simple wrapper for error messages including exception information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleError(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Error, innerException);
        }

        /// <summary>
        /// A simple wrapper for error messages.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleError(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Error);
        }

        /// <summary>
        /// A simple wrapper for warning messages including exception information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleWarning(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Warning, innerException);
        }

        /// <summary>
        /// A simple wrapper for warning messages.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleWarning(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Warning);
        }

        /// <summary>
        /// A simple wrapper for information messages including exception information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        /// <param name="innerException"></param>
        public static void HandleInformation(string message, int eventId, Exception innerException)
        {
            HandleMessage(message, eventId, EventLogEntryType.Information, innerException);
        }

        /// <summary>
        /// A simple wrapper for information messages.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventId"></param>
        public static void HandleInformation(string message, int eventId)
        {
            HandleMessage(message, eventId, EventLogEntryType.Information);
        }

        /// <summary>
        /// Prints the information or creates an EventLog with it, depending on whether the environment is interactive or not.
        /// Includes information about the inner exception.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        /// <param name="exception"></param>
        public static void HandleMessage(string message, int eventId, EventLogEntryType type, Exception innerException)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("EventID: {0}", eventId));
                Console.Error.WriteLine(message);
                Console.Error.WriteLine(innerException.Message);
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("----------");
                Console.WriteLine("DEBUG INFO");
                Console.WriteLine(innerException.ToString());
            }
            else
            {
                StringBuilder logMessage = new StringBuilder(message);
                logMessage.Append(Environment.NewLine);
                logMessage.Append(innerException.Message);
                logMessage.Append(Environment.NewLine);
                logMessage.Append(Environment.NewLine);

                logMessage.Append("----------------------");
                logMessage.Append(Environment.NewLine);
                logMessage.Append("DEBUG INFO:");
                logMessage.Append(Environment.NewLine);
                logMessage.Append(innerException);

                CreateLog(logMessage.ToString(), type, eventId);
            }
        }

        /// <summary>
        /// Prints the information or creates an EventLog with it, depending on whether the environment is interactive or not.
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="errorCode"></param>
        public static void HandleMessage(string message, int eventId, EventLogEntryType type)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("EventID: {0}", eventId));
                Console.Error.WriteLine(message);
            }
            else
            {
                CreateLog(message, type, eventId);
            }
        }

        /// <summary>
        /// Creates an EventLog with the information received
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
