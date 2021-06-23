using System.Collections.Generic;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Json
{
    internal class JsonArray : JsonNode
    {
        public JsonNode[] Children { get; }

        public JsonArray(JsonNode[] children) => Children = children;

        public override IEnumerable<JsonNode> EnumerateChildren() => Children;

        public override JsonNode? TryGetChild(int index) => EnumerateChildren().ElementAtOrDefault(index);
    }
}