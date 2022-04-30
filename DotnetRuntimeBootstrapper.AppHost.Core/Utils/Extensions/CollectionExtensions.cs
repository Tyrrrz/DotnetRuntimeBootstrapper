using System.Collections.Generic;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class CollectionExtensions
{
    public static IEnumerable<T> Enumerate<T>(this T value)
    {
        yield return value;
    }

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
    {
        yield return item;

        foreach (var i in source)
            yield return i;
    }
}