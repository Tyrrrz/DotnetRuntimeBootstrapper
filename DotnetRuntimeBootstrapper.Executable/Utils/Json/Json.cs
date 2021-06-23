namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal static class Json
    {
        public static JsonNode? TryParse(string source) => new JsonReader(source).TryReadDocument();
    }
}