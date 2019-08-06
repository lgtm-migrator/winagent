using System;
using System.Diagnostics;
using System.Text;

namespace Winagent.ExceptionHandling
{
    public static class ExceptionHandler
    {
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
                using (EventLog eventLog = new EventLog("Application"))
                {
                    StringBuilder message = new StringBuilder(errorMessage);
                    message.Append(Environment.NewLine);
                    message.Append(exception);
                    message.Append(Environment.NewLine);
                    message.Append(exception.Message);

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Error, errorCode, 1);
                }
            }
        }

        public static void HandleInformation(string errorMessage, int warningCode, Exception exception)
        {
            if (Environment.UserInteractive)
            {
                Console.Error.WriteLine(String.Format("Warning: {0}", warningCode));
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
                using (EventLog eventLog = new EventLog("Application"))
                {
                    StringBuilder message = new StringBuilder(errorMessage);
                    message.Append(Environment.NewLine);
                    message.Append(exception);
                    message.Append(Environment.NewLine);
                    message.Append(exception.Message);

                    eventLog.Source = "Winagent";
                    eventLog.WriteEntry(message.ToString(), EventLogEntryType.Warning, warningCode, 1);
                }
            }
        }
    }
}
