using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Utils.Extensions;

internal static class CollectionExtensions
{
    public static void RemoveAll<T>(this ICollection<T> source, Func<T, bool> predicate)
    {
        foreach (var i in source.ToArray())
        {
            if (predicate(i))
                source.Remove(i);
        }
    }
}
