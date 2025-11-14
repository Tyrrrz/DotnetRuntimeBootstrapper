using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Utils.Extensions;

internal static class CollectionExtensions
{
    extension<T>(ICollection<T> source)
    {
        public void RemoveAll(Func<T, bool> predicate)
        {
            foreach (var i in source.ToArray())
            {
                if (predicate(i))
                    source.Remove(i);
            }
        }
    }
}
