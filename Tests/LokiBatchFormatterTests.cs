using log4net.Core;
using Log4Net.Appender.Loki;
using System;
using System.IO;
using Xunit;

namespace Log4Net.Appender.Grafana.Loki.Tests
{
    public class LokiBatchFormatterTests
    {
        private static LoggingEvent MakeEvent(Level level, string message, string logger = "TestLogger")
        {
            var data = new LoggingEventData
            {
                Level = level,
                Message = message,
                LoggerName = logger,
                TimeStampUtc = DateTime.UtcNow
            };
            return new LoggingEvent(data);
        }

        [Fact]
        public void Format_EmptyEvents_WritesNothing()
        {
            var formatter = new LokiBatchFormatter();
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(Array.Empty<LoggingEvent>(), writer);
            Assert.Empty(sb.ToString());
        }

        [Fact]
        public void Format_SingleEvent_ProducesValidJson()
        {
            var formatter = new LokiBatchFormatter();
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(new[] { MakeEvent(Level.Info, "Hello") }, writer);
            var json = sb.ToString();
            Assert.Contains("\"streams\"", json);
            Assert.Contains("\"stream\"", json);
            Assert.Contains("\"values\"", json);
            Assert.Contains("Hello", json);
        }

        [Fact]
        public void Format_EventLevel_AppearsAsLokiLabel()
        {
            var formatter = new LokiBatchFormatter();
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(new[] { MakeEvent(Level.Error, "Oops") }, writer);
            Assert.Contains("\"error\"", sb.ToString());
        }

        [Fact]
        public void Format_GlobalLabels_AppearInStream()
        {
            var labels = new[] { new LokiLabel("Application", "MyApp") };
            var properties = Array.Empty<LokiProperty>();
            var formatter = new LokiBatchFormatter(labels, properties);
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(new[] { MakeEvent(Level.Info, "test") }, writer);
            Assert.Contains("MyApp", sb.ToString());
        }

        [Fact]
        public void Format_GlobalProperties_AppearInLogLine()
        {
            var labels = Array.Empty<LokiLabel>();
            var properties = new[] { new LokiProperty("MachineName", "host01") };
            var formatter = new LokiBatchFormatter(labels, properties);
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(new[] { MakeEvent(Level.Info, "test") }, writer);
            Assert.Contains("host01", sb.ToString());
        }

        [Fact]
        public void Format_NullEvents_ThrowsArgumentNullException()
        {
            var formatter = new LokiBatchFormatter();
            using var writer = new StringWriter();
            Assert.Throws<ArgumentNullException>(() => formatter.Format(null!, writer));
        }

        [Fact]
        public void Format_NullWriter_ThrowsArgumentNullException()
        {
            var formatter = new LokiBatchFormatter();
            Assert.Throws<ArgumentNullException>(() => formatter.Format(Array.Empty<LoggingEvent>(), null!));
        }

        [Fact]
        public void Format_MultipleEvents_EachHasOwnStream()
        {
            var formatter = new LokiBatchFormatter();
            var sb = new System.Text.StringBuilder();
            using var writer = new StringWriter(sb);
            formatter.Format(new[]
            {
                MakeEvent(Level.Info, "first"),
                MakeEvent(Level.Error, "second")
            }, writer);
            var json = sb.ToString();
            Assert.Contains("first", json);
            Assert.Contains("second", json);
        }
    }
}
