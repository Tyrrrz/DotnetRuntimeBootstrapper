using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class PathEx
{
    public static string Combine(params IEnumerable<string> paths) => paths.Aggregate(Path.Combine);
}
