using System.Collections.Generic;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal abstract class JsonNode
    {
        public virtual IEnumerable<JsonNode> EnumerateChildren() => Enumerable.Empty<JsonNode>();

        public virtual IEnumerable<JsonProperty> EnumerateProperties() => Enumerable.Empty<JsonProperty>();

        public virtual JsonNode? TryGetChild(int index) => null;

        public virtual JsonNode? TryGetChild(string propertyName) => null;

        public virtual bool? TryGetBool() => null;

        public virtual double? TryGetNumber() => null;

        public virtual string? TryGetString() => null;
    }
}