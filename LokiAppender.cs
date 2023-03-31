﻿using log4net.Appender;
using log4net.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace Log4Net.Appender.Loki
{
    public class LokiAppender : BufferingAppenderSkeleton
    {
        public string Application { get; set; }
        public string Environment { get; set; }
        public string ServiceUrl { get; set; }
        public string BasicAuthUserName { get; set; }
        public string BasicAuthPassword { get; set; }
        public bool GZipCompression { get; set; }
        public bool TrustSelfSignedCerts { get; set; }
        public LokiLabel Label { set { Labels.Add(value); } }
        private List<LokiLabel> Labels = new List<LokiLabel>();

        private void PostLoggingEvent(LoggingEvent[] loggingEvents)
        {
            var labels = new List<LokiLabel>(Labels)
            {
                new LokiLabel { Key = "Application", Value = Application },
                new LokiLabel { Key = "Environment", Value = Environment }
            };
            var properties = new LokiProperty[] {
                new LokiProperty { Key = "MachineName", Value = System.Environment.MachineName },
                new LokiProperty { Key = "ProcessName", Value = Process.GetCurrentProcess().ProcessName }
            };
            var formatter = new LokiBatchFormatter(labels, properties);
            var httpClient = new LokiHttpClient(TrustSelfSignedCerts);

            if (httpClient is LokiHttpClient c)
            {
                LokiCredentials credentials;

                if (!string.IsNullOrEmpty(BasicAuthUserName) && !string.IsNullOrEmpty(BasicAuthPassword))
                {
                    credentials = new BasicAuthCredentials(ServiceUrl, BasicAuthUserName, BasicAuthPassword);
                }
                else
                {
                    credentials = new NoAuthCredentials(ServiceUrl);
                }

                c.SetAuthCredentials(credentials);
            }

            StringBuilder sb = new StringBuilder();
            using (var sc = new StringWriter(sb))
            {
                formatter.Format(loggingEvents, sc);
                sc.Flush();
                var loggingEventsStr = sb.ToString();
                if (GZipCompression)
                {
                    var compressedContent = CompressRequestContent(loggingEventsStr);
                    httpClient.PostAsync(LokiRouteBuilder.BuildPostUri(ServiceUrl), compressedContent);
                }
                else
                {
                    var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(loggingEventsStr)));
                    //var contentStr = content.ReadAsStringAsync().Result; // TO VERIFY                
                    httpClient.PostAsync(LokiRouteBuilder.BuildPostUri(ServiceUrl), content);
                }
            }
        }

        protected override void SendBuffer(LoggingEvent[] events)
        {
            PostLoggingEvent(events);
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
            httpContent.Headers.Add("Content-encoding", "gzip");
            return httpContent;
        }
    }
}