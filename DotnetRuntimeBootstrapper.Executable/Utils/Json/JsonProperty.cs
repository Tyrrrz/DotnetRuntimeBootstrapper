namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonProperty
    {
        public string Name { get; }

        public JsonNode Value { get; }

        public JsonProperty(string name, JsonNode value)
        {
            Name = name;
            Value = value;
        }
    }
}