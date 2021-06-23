namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonNumber : JsonNode
    {
        public double Value { get; }

        public JsonNumber(double value) => Value = value;

        public override double? TryGetNumber() => Value;
    }
}