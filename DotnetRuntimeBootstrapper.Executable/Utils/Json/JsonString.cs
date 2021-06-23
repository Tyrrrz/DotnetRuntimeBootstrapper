namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonString : JsonNode
    {
        public string Value { get; }

        public JsonString(string value) => Value = value;

        public override string? TryGetString() => Value;
    }
}