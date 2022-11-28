using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace log4net.Appender.Loki
{
    internal class LokiContentStream
    {
        [JsonProperty("stream")]
        public Dictionary<string, string> Labels { get; set; } = new Dictionary<string, string>();

        [JsonProperty("values")]
        public List<string[]> Values { get; set; } = new List<string[]>();
    }
}