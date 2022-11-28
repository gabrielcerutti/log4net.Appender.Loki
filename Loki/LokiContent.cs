using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace log4net.Appender.Loki
{
    internal class LokiContent
    {
        [JsonProperty("streams")]
        public List<LokiContentStream> Streams { get; set; } = new List<LokiContentStream>();

        public string Serialize()
        {
            JsonSerializer serializer = new JsonSerializer();
            TextWriter writer = new StringWriter();
            serializer.Serialize(writer, this);
            return writer.ToString();
        }
    }
}