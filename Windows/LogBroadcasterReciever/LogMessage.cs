using System;
using System.Collections.Generic;
using System.Text;

namespace LogBroadcasterReciever
{
    public class LogMessage
    {
        public string condition { get; set; }
        public string stackTrace { get; set; }
        public string type { get; set; }
    }
}
