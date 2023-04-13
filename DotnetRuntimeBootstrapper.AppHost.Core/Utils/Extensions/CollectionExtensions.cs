using System.Collections.Generic;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<T> ToSingletonEnumerable<T>(this T value)
    {
        yield return value;
    }
}