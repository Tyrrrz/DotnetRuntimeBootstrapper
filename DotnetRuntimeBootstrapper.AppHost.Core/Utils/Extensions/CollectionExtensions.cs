using System.Collections.Generic;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(T value)
    {
        public IEnumerable<T> ToSingletonEnumerable()
        {
            yield return value;
        }
    }
}
