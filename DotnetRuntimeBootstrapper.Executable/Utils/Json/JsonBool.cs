namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal partial class JsonBool : JsonNode
    {
        public bool Value { get; }

        public JsonBool(bool value) => Value = value;

        public override bool? TryGetBool() => Value;
    }

    internal partial class JsonBool
    {
        public static JsonBool True { get; } = new(true);

        public static JsonBool False { get; } = new(false);
    }
}