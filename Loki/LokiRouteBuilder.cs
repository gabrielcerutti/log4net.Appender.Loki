namespace Log4Net.Appender.Loki
{
    public static class LokiRouteBuilder
    {
        public static string BuildPostUri(string host)
        {
            return host.Substring(host.Length - 1) != "/" ? $"{host}{PostDataUri}" : $"{host.TrimEnd('/')}{PostDataUri}";
        }

        public const string PostDataUri = "/loki/api/v1/push";
    }
}