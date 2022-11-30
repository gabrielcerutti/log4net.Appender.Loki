using log4net.Core;

namespace Log4Net.Appender.Loki
{
    internal static class LogEventLevelExtensions
    {
        internal static string ToGrafanaLogLevel(this Level level)
        {
            string levelStr;
            if (level.Name == Level.Trace.Name) levelStr = "trace";
            else if (level.Name == Level.Debug.Name) levelStr = "debug";
            else if (level.Name == Level.Info.Name) levelStr = "info";
            else if (level.Name == Level.Warn.Name) levelStr = "warning";
            else if (level.Name == Level.Error.Name) levelStr = "error";
            else if (level.Name == Level.Critical.Name) levelStr = "critical";
            else if (level.Name == Level.Fatal.Name) levelStr = "critical";
            else levelStr = "unknown";

            return levelStr;
        }

    }
}