namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonNull : JsonNode
    {
        public static JsonNull Instance { get; } = new();
    }
}