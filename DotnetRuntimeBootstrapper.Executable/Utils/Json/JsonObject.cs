using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonObject : JsonNode
    {
        public JsonProperty[] Properties { get; }

        public JsonObject(JsonProperty[] properties) => Properties = properties;

        public override IEnumerable<JsonProperty> EnumerateProperties() => Properties;

        public override JsonNode? TryGetChild(string propertyName) => EnumerateProperties()
            .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.Ordinal))?
            .Value;
    }
}