using System.Collections.Generic;

namespace DotnetRuntimeBootstrapper.Executable.Utils.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> source, T item)
        {
            yield return item;

            foreach (var i in source)
                yield return i;
        }
    }
}