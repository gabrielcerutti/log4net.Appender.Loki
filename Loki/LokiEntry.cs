using Newtonsoft.Json;
using System.Collections.Generic;

namespace log4net.Appender.Loki
{
    internal class LokiEntry
    {
        public LokiEntry()
        {
        }

        public string Level { get; set; }
        public string Message { get; set; }
        public string SourceContext { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}