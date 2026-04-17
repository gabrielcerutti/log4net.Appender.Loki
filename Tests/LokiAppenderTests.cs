using Log4Net.Appender.Loki;
using Xunit;

namespace Log4Net.Appender.Grafana.Loki.Tests
{
    public class LokiAppenderTests
    {
        [Fact]
        public void Label_SetOnce_IsAddedToLabelsList()
        {
            var appender = new LokiAppender();

            appender.Label = new LokiLabel { Key = "Team", Value = "Backend" };

            Assert.Single(appender._labels);
            Assert.Equal("Team", appender._labels[0].Key);
            Assert.Equal("Backend", appender._labels[0].Value);
        }

        [Fact]
        public void Label_SetMultipleTimes_AllLabelsAccumulated()
        {
            var appender = new LokiAppender();

            appender.Label = new LokiLabel { Key = "Team", Value = "Backend" };
            appender.Label = new LokiLabel { Key = "Region", Value = "eu-west-1" };
            appender.Label = new LokiLabel { Key = "Tier", Value = "API" };

            Assert.Equal(3, appender._labels.Count);
            Assert.Equal("Team",     appender._labels[0].Key);
            Assert.Equal("Region",   appender._labels[1].Key);
            Assert.Equal("Tier",     appender._labels[2].Key);
        }

        [Fact]
        public void Label_NotSet_LabelsListIsEmpty()
        {
            var appender = new LokiAppender();

            Assert.Empty(appender._labels);
        }

        [Fact]
        public void Label_ObjectInitializerSyntax_KeyAndValueSetCorrectly()
        {
            var label = new LokiLabel { Key = "Environment", Value = "Staging" };

            Assert.Equal("Environment", label.Key);
            Assert.Equal("Staging", label.Value);
        }

        [Fact]
        public void Label_ConstructorSyntax_KeyAndValueSetCorrectly()
        {
            var label = new LokiLabel("Environment", "Production");

            Assert.Equal("Environment", label.Key);
            Assert.Equal("Production", label.Value);
        }
    }
}
