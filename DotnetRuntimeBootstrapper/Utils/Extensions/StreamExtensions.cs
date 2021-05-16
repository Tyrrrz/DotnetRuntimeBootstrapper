using System.Collections.Generic;
using System.IO;

namespace DotnetRuntimeBootstrapper.Utils.Extensions
{
    internal static class StreamExtensions
    {
        public static IEnumerable<string> ReadAllLines(this StreamReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}