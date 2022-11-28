using log4net.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace log4net.Appender.Loki
{
    internal class LokiBatchFormatter
    {
        private readonly IList<LokiLabel> _globalLabels;
        private readonly IList<LokiProperty> _globalProperties;

        public LokiBatchFormatter()
        {
            _globalLabels = new List<LokiLabel>();
            _globalProperties = new List<LokiProperty>();
        }

        public LokiBatchFormatter(IList<LokiLabel> globalLabels, IList<LokiProperty> globalProperties)
        {
            _globalLabels = globalLabels;
            _globalProperties = globalProperties;
        }

        public void Format(IEnumerable<LoggingEvent> logEvents, TextWriter output)
        {
            if (logEvents == null)
                throw new ArgumentNullException(nameof(logEvents));
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            List<LoggingEvent> logs = logEvents.ToList();
            if (!logs.Any())
                return;

            var content = new LokiContent();

            foreach (LoggingEvent logEvent in logs)
            {
                var stream = FormatLogEventToJson(logEvent);
                content.Streams.Add(stream);
            }

            if (content.Streams.Count > 0)
                output.Write(content.Serialize());
        }

        private LokiContentStream FormatLogEvent(LoggingEvent logEvent)
        {
            var stream = new LokiContentStream();

            stream.Labels.Add("level", GetLevel(logEvent.Level));
            foreach (LokiLabel globalLabel in _globalLabels)
                stream.Labels.Add(globalLabel.Key, globalLabel.Value);

            //foreach (var key in logEvent.Properties.GetKeys())
            //{
            //    // Some enrichers pass strings with quotes surrounding the values inside the string,
            //    // which results in redundant quotes after serialization and a "bad request" response.
            //    // To avoid this, remove all quotes from the value.
            //    stream.Labels.Add(new LokiLabel(key, logEvent.Properties[key].ToString().Replace("\"", "")));
            //}

            var epoch = Math.Truncate((logEvent.TimeStampUtc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString() + "000000";
            var logMessage = new LokiEntry
            {
                Level = logEvent.Level.Name,
                Message = logEvent.RenderedMessage,
                SourceContext = logEvent.LoggerName,
                Location = logEvent.LocationInformation.FullInfo
            };
            foreach (var property in _globalProperties)
            {
                logMessage.Properties.Add(property.Key, property.Value);
            }

            if (logEvent.ExceptionObject != null)
            {
                var exceptionOutput = new StringWriter();
                SerializeException(exceptionOutput, logEvent.ExceptionObject, 1);
                logMessage.Properties.Add("Exception", exceptionOutput.ToString());
            }
            var dataAsJson = JsonConvert.SerializeObject(logMessage);
            var sb = new StringBuilder();
            sb.AppendLine(dataAsJson);

            stream.Values.Add(new string[2] {
                    epoch.ToString(),
                    sb.ToString()
                });

            return stream;
        }

        private LokiContentStream FormatLogEventToJson(LoggingEvent logEvent)
        {
            if (logEvent == null)
            {
                throw new ArgumentNullException(nameof(logEvent));
            }

            var output = new StringWriter();
            var stream = new LokiContentStream();

            stream.Labels.Add("level", GetLevel(logEvent.Level));
            foreach (LokiLabel globalLabel in _globalLabels)
                stream.Labels.Add(globalLabel.Key, globalLabel.Value);

            var epoch = Math.Truncate((logEvent.TimeStampUtc - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds).ToString() + "000000";

            output.Write("{\"Message\":");
            WriteQuotedJsonString(logEvent.RenderedMessage, output);

            output.Write(",\"Level\":");
            WriteQuotedJsonString(logEvent.Level.ToString(), output);

            output.Write(",\"SourceContext\":");
            WriteQuotedJsonString(logEvent.LoggerName, output);

            output.Write(",\"Location\":");
            WriteQuotedJsonString(logEvent.LocationInformation.FullInfo, output);

            // This additional level property is for Grafana Loki to be able to parse the log level
            output.Write(",\"level\":");
            WriteQuotedJsonString(logEvent.Level.ToGrafanaLogLevel(), output);

            foreach (var property in _globalProperties)
            {
                output.Write(string.Format(",\"{0}\":\"{1}\"", property.Key, property.Value));
            }

            if (logEvent.ExceptionObject != null)
            {
                output.Write(",\"Exception\":");
                SerializeException(output, logEvent.ExceptionObject, 1);
            }

            if (logEvent.Properties.Count > 0)
            {
                output.Write(",\"Properties\":{");
                var propertiesKeys = logEvent.Properties.GetKeys();
                var count = propertiesKeys.Count();
                for (var i = 0; i < count; i++)
                {
                    var isLast = i == count - 1;
                    var key = propertiesKeys[i];
                    output.Write(string.Format("\"{0}\":{1}", key, JsonConvert.ToString(logEvent.Properties[key].ToString())));
                    if (!isLast)
                    {
                        output.Write(',');
                    }
                }
                output.Write('}');
            }

            output.Write('}');

            stream.Values.Add(new string[2] {
                    epoch.ToString(),
                    output.ToString()
                });

            return stream;
        }

        private static string GetLevel(Level level)
        {
            if (level == Level.Info)
                return "info";

            return level.ToString().ToLower();
        }

        private void SerializeException(StringWriter output, Exception exception, int level)
        {
            if (level == 4)
            {
                WriteQuotedJsonString(exception.ToString(), output);

                return;
            }

            output.Write("{\"Type\":");
            var typeNamespace = exception.GetType().Namespace;
            var typeName = typeNamespace != null && typeNamespace.StartsWith("System.")
                ? exception.GetType().Name
                : exception.GetType().ToString();
            WriteQuotedJsonString(typeName, output);

            if (!string.IsNullOrWhiteSpace(exception.Message))
            {
                output.Write(",\"Message\":");
                WriteQuotedJsonString(exception.Message, output);
            }

            if (!string.IsNullOrWhiteSpace(exception.StackTrace))
            {
                output.Write(",\"StackTrace\":");
                WriteQuotedJsonString(exception.StackTrace, output);
            }

            if (exception is AggregateException aggregateException)
            {
                output.Write(",\"InnerExceptions\":[");
                var count = aggregateException.InnerExceptions.Count;
                for (var i = 0; i < count; i++)
                {
                    var isLast = i == count - 1;
                    SerializeException(
                        output,
                        aggregateException.InnerExceptions[i],
                        level + 1);
                    if (!isLast)
                    {
                        output.Write(',');
                    }
                }

                output.Write("]");
            }
            else if (exception.InnerException != null)
            {
                output.Write(",\"InnerException\":");
                SerializeException(
                    output,
                    exception.InnerException,
                    level + 1);
            }

            output.Write('}');
        }

        private static void WriteQuotedJsonString(string str, StringWriter output)
        {
            output.Write('"');
            int num = 0;
            bool flag = false;
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c < ' ' || c == '\\' || c == '"')
                {
                    flag = true;
                    output.Write(str.Substring(num, i - num));
                    num = i + 1;
                    switch (c)
                    {
                        case '"':
                            output.Write("\\\"");
                            continue;
                        case '\\':
                            output.Write("\\\\");
                            continue;
                        case '\n':
                            output.Write("\\n");
                            continue;
                        case '\r':
                            output.Write("\\r");
                            continue;
                        case '\f':
                            output.Write("\\f");
                            continue;
                        case '\t':
                            output.Write("\\t");
                            continue;
                    }

                    output.Write("\\u");
                    int num2 = c;
                    output.Write(num2.ToString("X4"));
                }
            }

            if (flag)
            {
                if (num != str.Length)
                {
                    output.Write(str.Substring(num));
                }
            }
            else
            {
                output.Write(str);
            }

            output.Write('"');
        }
    }
}