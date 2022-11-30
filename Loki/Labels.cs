namespace Log4Net.Appender.Loki
{
    public class LokiLabel
    {
        public LokiLabel(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }

    public class LokiProperty
    {
        public LokiProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }

        public string Value { get; }
    }
}