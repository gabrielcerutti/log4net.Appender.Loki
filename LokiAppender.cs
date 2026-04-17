using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Log4Net.Appender.Loki
{
    public class LokiAppender : BufferingAppenderSkeleton
    {
        private static readonly string _processName = Process.GetCurrentProcess().ProcessName;
        private LokiHttpClient _httpClient;
        internal readonly List<LokiLabel> _labels = new List<LokiLabel>();

        public string Application { get; set; }
        public string Environment { get; set; }
        public string ServiceUrl { get; set; }
        public string BasicAuthUserName { get; set; }
        public string BasicAuthPassword { get; set; }
        public bool GZipCompression { get; set; }
        public bool TrustSelfSignedCerts { get; set; }

        /// <summary>
        /// Allows extra labels to be configured via log4net XML config. Can be specified multiple times.
        /// </summary>
        public LokiLabel Label { set { _labels.Add(value); } }

        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (string.IsNullOrEmpty(ServiceUrl))
            {
                ErrorHandler.Error("LokiAppender requires a non-empty ServiceUrl.");
                return;
            }

            LokiCredentials credentials = !string.IsNullOrEmpty(BasicAuthUserName) && !string.IsNullOrEmpty(BasicAuthPassword)
                ? new BasicAuthCredentials(ServiceUrl, BasicAuthUserName, BasicAuthPassword)
                : (LokiCredentials)new NoAuthCredentials(ServiceUrl);

            _httpClient = new LokiHttpClient(TrustSelfSignedCerts);
            _httpClient.SetAuthCredentials(credentials);
        }

        protected override void OnClose()
        {
            base.OnClose();
            _httpClient?.Dispose();
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            PostLoggingEvent(events);
        }

        private void PostLoggingEvent(LoggingEvent[] loggingEvents)
        {
            if (_httpClient == null)
                return;

            var labels = new List<LokiLabel>(_labels)
            {
                new LokiLabel("Application", Application),
                new LokiLabel("Environment", Environment)
            };
            var properties = new LokiProperty[] {
                new LokiProperty("MachineName", System.Environment.MachineName),
                new LokiProperty("ProcessName", _processName)
            };
            var formatter = new LokiBatchFormatter(labels, properties);

            var sb = new StringBuilder();
            using (var sc = new StringWriter(sb))
            {
                formatter.Format(loggingEvents, sc);
                sc.Flush();
            }
            var loggingEventsStr = sb.ToString();

            try
            {
                HttpResponseMessage response;
                if (GZipCompression)
                {
                    var compressedContent = CompressRequestContent(loggingEventsStr);
                    response = _httpClient.PostAsync(LokiRouteBuilder.BuildPostUri(ServiceUrl), compressedContent).GetAwaiter().GetResult();
                }
                else
                {
                    var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(loggingEventsStr)));
                    response = _httpClient.PostAsync(LokiRouteBuilder.BuildPostUri(ServiceUrl), content).GetAwaiter().GetResult();
                }

                if (!response.IsSuccessStatusCode)
                {
                    ErrorHandler.Error($"Failed to send log events to Loki. Status: {(int)response.StatusCode} {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.Error("Error sending log events to Loki.", ex);
            }
        }

        private static HttpContent CompressRequestContent(string content)
        {
            var compressedStream = new MemoryStream();
            using (var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                {
                    contentStream.CopyTo(gzipStream);
                }
            }

            var httpContent = new ByteArrayContent(compressedStream.ToArray());
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            httpContent.Headers.Add("Content-Encoding", "gzip");
            return httpContent;
        }
    }
}
