namespace Log4Net.Appender.Loki
{
    public class LokiLabel
    {
        public LokiLabel() { }

        public LokiLabel(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }

    public class LokiProperty
    {
        public LokiProperty() { }

        public LokiProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}