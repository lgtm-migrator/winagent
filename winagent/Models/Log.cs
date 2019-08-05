using System;

namespace Winagent.Models
{
    class Log
    {
        public string Source { set; get; }
        public long Id { set; get; }
        public string Description { set; get; }
        public DateTime Date { set; get; }
        public string Severity { set; get; }
        public string HostName { set; get; }
        public string EventLogName { set; get; }
    }
}
