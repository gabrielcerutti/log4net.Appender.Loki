using Log4Net.Appender.Loki;
using Xunit;

namespace Log4Net.Appender.Grafana.Loki.Tests
{
    public class LokiRouteBuilderTests
    {
        [Fact]
        public void BuildPostUri_NoTrailingSlash_AppendsPath()
        {
            var uri = LokiRouteBuilder.BuildPostUri("http://localhost:3100");
            Assert.Equal("http://localhost:3100/loki/api/v1/push", uri);
        }

        [Fact]
        public void BuildPostUri_WithTrailingSlash_NormalizesAndAppendsPath()
        {
            var uri = LokiRouteBuilder.BuildPostUri("http://localhost:3100/");
            Assert.Equal("http://localhost:3100/loki/api/v1/push", uri);
        }

        [Fact]
        public void BuildPostUri_MultipleTrailingSlashes_NormalizesAndAppendsPath()
        {
            var uri = LokiRouteBuilder.BuildPostUri("http://localhost:3100///");
            Assert.Equal("http://localhost:3100/loki/api/v1/push", uri);
        }
    }
}
