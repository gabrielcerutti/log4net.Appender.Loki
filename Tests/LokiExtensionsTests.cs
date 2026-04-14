using log4net.Core;
using Log4Net.Appender.Loki;
using Xunit;

namespace Log4Net.Appender.Grafana.Loki.Tests
{
    public class LokiExtensionsTests
    {
        [Theory]
        [InlineData("TRACE", "trace")]
        [InlineData("DEBUG", "debug")]
        [InlineData("INFO", "info")]
        [InlineData("WARN", "warning")]
        [InlineData("ERROR", "error")]
        [InlineData("CRITICAL", "critical")]
        [InlineData("FATAL", "critical")]
        public void ToGrafanaLogLevel_MapsKnownLevels(string levelName, string expected)
        {
            var level = Level.Trace.Name == levelName ? Level.Trace
                : Level.Debug.Name == levelName ? Level.Debug
                : Level.Info.Name == levelName ? Level.Info
                : Level.Warn.Name == levelName ? Level.Warn
                : Level.Error.Name == levelName ? Level.Error
                : Level.Critical.Name == levelName ? Level.Critical
                : Level.Fatal;

            Assert.Equal(expected, level.ToGrafanaLogLevel());
        }

        [Fact]
        public void ToGrafanaLogLevel_UnknownLevel_ReturnsUnknown()
        {
            var customLevel = new Level(12345, "CUSTOM");
            Assert.Equal("unknown", customLevel.ToGrafanaLogLevel());
        }
    }
}
